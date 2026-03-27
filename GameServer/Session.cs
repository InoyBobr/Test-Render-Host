using System.Net.WebSockets;
using System.Text.Json;

public class Session
{
    private readonly Connection connA;
    private readonly Connection connB;
    private readonly Action<Session> onEnded;

    private Player playerA;
    private Player playerB;

    private GameAPI api;

    private int ended = 0;

    public Session(Connection a, Connection b, Action<Session> onEnded)
    {
        this.connA = a;
        this.connB = b;
        this.onEnded = onEnded;

        a.Session = this;
        b.Session = this;

        InitGame();
        Subscribe();
    }

    // =============================================================================================
    // INIT
    // =============================================================================================

    private void InitGame()
    {
        var deckA = connA.DeckIds!
            .Select(id => CardDatabase.Get(id))
            .ToList();

        var deckB = connB.DeckIds!
            .Select(id => CardDatabase.Get(id))
            .ToList();

        api = new GameAPI();

        playerA = new Player(deckA, api);
        playerB = new Player(deckB, api);
    }
    
    public async Task Start()
    {
        await Task.WhenAll(
            connA.Send(new { type = "match_found", you = true }),
            connB.Send(new { type = "match_found", you = true })
        );

        api.StartGame(playerA, playerB);
    }

    // =============================================================================================
    // INPUT FROM CLIENT
    // =============================================================================================

    public void HandleMessage(Connection from, string text)
    {
        if (ended == 1) return;

        using var doc = JsonDocument.Parse(text);

        if (!doc.RootElement.TryGetProperty("type", out var t))
            return;

        var type = t.GetString();
        var player = from == connA ? playerA : playerB;

        try
        {
            switch (type)
            {
                case "play_card":
                {
                    if (!doc.RootElement.TryGetProperty("hand_index", out var handIndexEl) ||
                        handIndexEl.ValueKind != JsonValueKind.Number)
                    {
                        _ = from.Send(new
                        {
                            type = "card_played_result",
                            success = false,
                            error = "Invalid hand index type"

                        });
                        return;
                    }

                    int handIndex = handIndexEl.GetInt32();

                    int? position = null;
                    if (doc.RootElement.TryGetProperty("position", out var posEl) &&
                        posEl.ValueKind == JsonValueKind.Number)
                    {
                        position = posEl.GetInt32();
                    }

                    Dictionary<int, Dictionary<string, List<int>>>? targets = null;
                    if (doc.RootElement.TryGetProperty("targets", out var targetsEl))
                    {
                        targets = JsonSerializer.Deserialize<
                            Dictionary<int, Dictionary<string, List<int>>>>(targetsEl.GetRawText());
                    }

                    api.TryPlayCard(player, handIndex, position, targets);
                    break;
                }

                case "end_turn":
                    api.EndTurn(player);
                    break;

                case "rotate_face":
                {
                    var face = Enum.Parse<Face>(
                        doc.RootElement.GetProperty("face").GetString()!,
                        true);

                    int dir = doc.RootElement.GetProperty("dir").GetInt32();

                    api.RotateFace(face, dir, player);
                    break;
                }
                case "submit_choice":
                {
                    List<List<int>>? targets = null;

                    if (doc.RootElement.TryGetProperty("targets", out var targetsEl))
                    {
                        targets = JsonSerializer.Deserialize<List<List<int>>>(targetsEl.GetRawText());
                    }

                    api.SubmitChoice(player, targets);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Client message error: " + ex.Message);
        }
    }

    // =============================================================================================
    // SEND HELPERS
    // =============================================================================================

    private async Task SendTo(Player p, object payload)
    {
        var conn = p == playerA ? connA : connB;
        await conn.Send(payload);
    }

    private async Task Broadcast(Func<Player, object?> build)
    {
        var aData = build(playerA);
        var bData = build(playerB);

        var tasks = new List<Task>();

        if (aData != null)
            tasks.Add(connA.Send(aData));

        if (bData != null)
            tasks.Add(connB.Send(bData));

        await Task.WhenAll(tasks);
    }

    // =============================================================================================
    // GAME EVENTS → NETWORK
    // =============================================================================================

    private void Subscribe()
    {
        api.GameStarted += () =>
            _ = Broadcast(_ => new { type = "game_started" });

        api.RoundStarted += e =>
            _ = Broadcast(_ => new { type = "round_started", round = e.Round });

        api.TurnStarted += e =>
            _ = Broadcast(p => new
            {
                type = "turn_started",
                you = (p == e.Player)
            });

        api.TurnEnded += e =>
            _ = Broadcast(p => new
            {
                type = "turn_ended",
                you = (p == e.Player)
            });

        api.BattleStarted += () =>
            _ = Broadcast(_ => new { type = "battle_started" });

        api.BattleEnded += () =>
            _ = Broadcast(_ => new { type = "battle_ended" });

        api.PlayerScored += e =>
            _ = Broadcast(p => new
            {
                type = "player_scored",
                you = (p == e.Player),
                gain = e.Amount,
                score = e.FullScore
            });

        api.CardDrawn += e =>
            _ = Broadcast(p =>
            {
                if (p == e.Player)
                {
                    return new
                    {
                        type = "card_drawn",
                        you = true,
                        card = CardMapper.ToDto(e.Card)
                    };
                }

                return new
                {
                    type = "card_drawn",
                    you = false
                };
            });

        api.CardDamaged += e =>
            _ = Broadcast(_ => new
            {
                type = "card_damaged",
                target_pos = e.Card.Position,
                damage = e.Damage,
                source_pos = e.SourcePos
            });

        api.CardBuffed += e =>
            _ = Broadcast(_ => new
            {
                type = "card_buffed",
                pos = e.Card.Position,
                power = e.PowerDelta,
                health = e.HealthDelta
            });

        api.CardKilled += e =>
            _ = Broadcast(_ => new
            {
                type = "card_killed",
                pos = e.Card.Position,
                killer = e.SourcePos
            });

        api.ShieldBroken += e =>
            _ = Broadcast(_ => new
            {
                type = "shield_broken",
                pos = e.Unit.Position
            });

        api.ChoiceStarted += e =>
            _ = Broadcast(p =>
            {
                bool you = p == e.Source.Owner?.Owner;

                if (!you)
                {
                    return new
                    {
                        type = "choice_started",
                        you = false
                    };
                }

                return new
                {
                    type = "choice_started",
                    you = true,
                    card_pos = e.Source.Owner.Position,
                    deniable = e.Deniable,
                    options = e.Options
                };
            });

        api.ChoiceGetsResult += e =>
            _ = Broadcast(p =>
            {
                if (p != e.Player) return null;

                return new
                {
                    type = "choice_result",
                    success = e.Success,
                    error = e.Error
                };
            });

        api.FaceRotatedResult += e =>
            _ = Broadcast(p =>
            {
                if (p != e.Player) return null;

                return new
                {
                    type = "face_rotated_result",
                    success = e.Success,
                    error = e.Error
                };
            });

        api.CardPlayedResult += result =>
            _ = Broadcast(p =>
            {
                if (p != result.Player) return null;

                return new
                {
                    type = "card_played_result",
                    success = result.Success,
                    error = result.Error,
                    playable_positions = result.PlayablePositions,
                    targets = result.TargetsToPick?.AbilityTargets
                };
            });

        api.CardPlayed += e =>
            _ = Broadcast(_ => new
            {
                type = "card_played",
                card = CardMapper.ToDto(e.Card),
                position = e.Position
            });
    }

    // =============================================================================================
    // END
    // =============================================================================================

    public async void End(string reason)
    {
        if (Interlocked.Exchange(ref ended, 1) == 1)
            return;

        try
        {
            await Task.WhenAll(
                connA.Send(new { type = "session_end", reason }),
                connB.Send(new { type = "session_end", reason })
            );
        }
        catch
        {
            // игнор — кто-то мог уже отвалиться
        }

        await Task.WhenAll(
            SafeClose(connA),
            SafeClose(connB)
        );

        onEnded(this);
    }
    
    private async Task SafeClose(Connection c)
    {
        try
        {
            var socket = c.Socket;

            if (socket.State == WebSocketState.Open ||
                socket.State == WebSocketState.CloseReceived)
            {
                await socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "session_end",
                    CancellationToken.None
                );
            }
        }
        catch
        {
            // сокет уже умер — ок
        }
    }
}