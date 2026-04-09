using System;
using System.Collections.Generic;
using System.Linq;


public class GameAPI
{
    // =========================
    // ПОЛЯ
    // =========================

    // Игроки
    public Player Player1 { get; private set; }
    public Player Player2 { get; private set; }

    private GameState _gameState = GameState.Start;

    // Поле
    public Board Board { get; private set; } = new Board();

    // EventBus для передачи всех игровых событий
    public EventBus Bus { get; private set; } = new EventBus();

    // Текущий игрок, чей ход
    public Player? CurrentPlayer { get; private set; }

    // Счёт очков
    public int Player1Score { get; private set; }
    public int Player2Score { get; private set; }

    // Текущий раунд
    public int Round { get; private set; } = 1;

    // Количество карт, которое игрок берёт в начале игры или хода
    public int StartDrawCount { get; set; } = 4;
    
    private ChoiceContext? _pendingChoice;
    private readonly Dictionary<UnitInstance, List<CardCombatDamageRequestEvent>> _pendingCombatDamage = new();
    
    public event Action? GameStarted;
    public event Action<RoundStarted>? RoundStarted;
    public event Action<PlayerTurnStarted>? TurnStarted;
    public event Action<PlayerRotationPhaseStarted>? RotationPhaseStarted; 
    public event Action<PlayerTurnEnded>? TurnEnded;
    public event Action? BattleStarted;
    public event Action? BattleEnded;
    public event Action<PlayerScoredEvent>? PlayerScored;
    public event Action<CardDrawnEvent>? CardDrawn;
    public event Action<CardDamagedEvent>? CardDamaged;
    public event Action<CardBuffedEvent>? CardBuffed;
    public event Action<CardKilledEvent>? CardKilled;
    public event Action<ShieldBrokenEvent>? ShieldBroken; 
    public event Action<ChoiceContext>? ChoiceStarted;
    public event Action<ChoiceResult>? ChoiceGetsResult;
    public event Action<PlayCardResult>? CardPlayedResult;
    public event Action<CardPlayedEvent>? CardPlayed; 
    public event Action<RotateFaceResult>? FaceRotatedResult;
    public event Action<FaceRotatedEvent>? FaceRotated;
    public event Action<KeywordAddedEvent>? KeywordAdded;
    public event Action<KeywordRemovedEvent>? KeywordRemoved;
    public event Action<ColorChangedEvent>? ColorChanged;
    



    private Random _random = new();
    // =========================
    // СОБЫТИЯ, КОТОРЫЕ API МОЖЕТ ПУБЛИКОВАТЬ
    // =========================
    // CardDrawnEvent
    // CardPlayedEvent
    // RoRotatedEvent
    // DamageDealtEvent
    // PlayerScoredEvent
    // TurnStartedEvent / TurnEndedEvent
    // BattleStartedEvent / BattleEndedEvent

    // =========================
    // МЕТОДЫ
    // =========================

    // 1. Начало игры
    public void StartGame(Player player1, Player player2)
    {
        SubscribeToCardEvents();
        Round = 1;
        Player1 = player1;
        Player2 = player2;
        Bus.Publish(new GameStartedEvent());
        GameStarted?.Invoke();
        var p1 =TryDrawCards(player1, StartDrawCount);
        var p2 =TryDrawCards(player2, StartDrawCount);
        StartTurn(player1);
    }

    // 2. Ход игрока
    private void StartTurn(Player player)
    {
        SetGameState(GameState.PlayPhase);
        CurrentPlayer = player;
        var e = new PlayerTurnStarted(player);
        TurnStarted?.Invoke(e);
        Bus.Publish(e);
        TryDrawCards(player, 1);
    }

    public void EndTurn(Player player)
    {
        if (player != CurrentPlayer)
            return;
        Bus.Publish(new PlayerTurnEnded(player));
        ProcessState();
    }

    // 3. Взятие карты
    private int TryDrawCards(Player player, int amount = 1)
    {
        if (player.GetDeck().Count == 0)
            return 0;
        var request = new CardDrawRequestEvent(player, amount);
        Bus.Publish(request);
        if (!request.Allowed)
            return 0;
        var actualAmount = Math.Min(amount, request.Amount);
        return actualAmount;

    } // намерение игрока взять карту

    private void DrawCards(CardDrawRequestEvent e)
    {
        var minimum = Math.Min(e.Amount, e.Player.GetDeck().Count);
        for (int i = 0; i < minimum; i++)
        {
            var card = e.Player.DrawTopCard();
            var drawEvent = new CardDrawnEvent(card, e.Player);
            CardDrawn?.Invoke(drawEvent);
            Bus.Publish(drawEvent);
        }
        

    } // фактическое действие и событие CardDrawnEvent

    // 4. Розыгрыш карт

    public void TryPlayCard(
    Player player,
    int handIndex,
    int? position = null,
    Dictionary<int, Dictionary<string, List<int>>>? chosenTargets = null
    )
    {
        // 1. Базовые проверки
        if (player != CurrentPlayer)
        {
            CardPlayedResult?.Invoke(new PlayCardResult { Success = false, Error = "Not your turn", Player = player});
            return;
        }

        if (_gameState != GameState.PlayPhase)
        {
            CardPlayedResult?.Invoke(new PlayCardResult { Success = false, Error = "Not play phase", Player = player});
            return;
        }

        if (handIndex < 0 || handIndex >= player.GetHand().Count)
        {
            CardPlayedResult?.Invoke(new PlayCardResult { Success = false, Error = "Invalid hand index", Player = player});
            return;
        }

        var card = player.GetHand()[handIndex];
        bool isUnit = card is UnitInstance;

        var baseContext = new GameContext(Board, CurrentPlayer);

        // 2. Проверка CanBePlayed
        foreach (var ability in card.Abilities)
        {
            if (!ability.Logic.CanBePlayed(baseContext))
            {
                CardPlayedResult?.Invoke(new PlayCardResult { Success = false, Error = "Card cannot be played now", Player = player});
                return;
            }
        }

        // 3. PREVIEW
        if (position == null)
        {
            var playable = new List<int>();
            TargetRequest? targetRequest = null;

            if (isUnit)
            {
                for (int pos = 0; pos < 24; pos++)
                {
                    if (!Board.IsPositionEmpty(pos))
                        continue;

                    if (card.Color != Board.GetColor(pos))
                        continue;

                    playable.Add(pos);
                }
            }
            else
            {
                playable.Add(-1);

                targetRequest = new TargetRequest();

                for (int i = 0; i < card.Abilities.Count; i++)
                {
                    var ability = card.Abilities[i];
                    var groups = ability.Logic.GetTargetOptions(baseContext);

                    if (groups != null && groups.Count > 0)
                        targetRequest.Add(i, groups);
                }
            }

            if (playable.Count == 0)
            {
                CardPlayedResult?.Invoke(new PlayCardResult
                {
                    Success = false,
                    Error = "No free space"
                });
            }
            CardPlayedResult?.Invoke(new PlayCardResult
            {
                Success = false,
                PlayablePositions = playable,
                TargetsToPick = targetRequest,
                Player = player
            });

            return;
        }

        // 4. Проверка позиции (только для юнита)
        if (isUnit)
        {
            if (!Board.IsPositionEmpty(position.Value))
            {
                CardPlayedResult?.Invoke(new PlayCardResult { Success = false, Error = "Position occupied", Player = player});
                return;
            }

            if (card.Color != Board.GetColor(position.Value))
            {
                CardPlayedResult?.Invoke(new PlayCardResult { Success = false, Error = "Wrong color", Player = player});
                return;
            }
        }
        else
        {
            if (position.Value != -1)
            {
                CardPlayedResult?.Invoke(new PlayCardResult { Success = false, Error = "Spell must be played with -1 position", Player = player});
                return;
            }
        }

        // 5. PreviewContext

        var previewContext = isUnit ? new GameContext(Board, CurrentPlayer).WithPreviewPlacement(card, position.Value) : baseContext;

        // 6. Сбор target request
        var targetRequestFinal = new TargetRequest();

        for (int i = 0; i < card.Abilities.Count; i++)
        {
            var ability = card.Abilities[i];
            var groups = ability.Logic.GetTargetOptions(previewContext);

            if (groups != null)
            {
                groups.RemoveAll(g => g.ValidValues.Count == 0);
                if (groups.Count > 0)
                    targetRequestFinal.Add(i, groups);
            }
        }

        // 7. Если цели нужны
        if (!targetRequestFinal.IsEmpty && chosenTargets == null)
        {
            CardPlayedResult?.Invoke(new PlayCardResult { Success = false, TargetsToPick = targetRequestFinal, Player = player});
            return;
        }

        // 8. Валидация целей
        if (!targetRequestFinal.IsEmpty)
        {
            foreach (var (abilityIndex, groups) in targetRequestFinal.AbilityTargets)
            {
                if (!chosenTargets!.TryGetValue(abilityIndex, out var abilityTargets))
                {
                    CardPlayedResult?.Invoke(new PlayCardResult { Success = false, Error = "Missing targets", Player = player});
                    return;
                }

                foreach (var group in groups)
                {
                    if (!abilityTargets.TryGetValue(group.Key, out var values))
                    {
                        CardPlayedResult?.Invoke(new PlayCardResult { Success = false, Error = "Missing target key", Player = player});
                        return;
                    }

                    if (group.Distinct && values.Count != values.Distinct().Count())
                    {
                        CardPlayedResult?.Invoke(new PlayCardResult { Success = false, Error = "Targets should be distinct", Player = player });
                        return;
                    }

                    if (values.Count != group.Count)
                    {
                        CardPlayedResult?.Invoke(new PlayCardResult { Success = false, Error = "Wrong target count", Player = player });
                        return;
                    }

                    foreach (var v in values)
                    {
                        if (!group.ValidValues.Contains(v))
                        {
                            CardPlayedResult?.Invoke(new PlayCardResult { Success = false, Error = "Invalid target", Player = player });
                            return;
                        }
                    }
                }
            }

            foreach (var (abilityIndex, abilityTargets) in chosenTargets!)
            {
                var ability = card.Abilities[abilityIndex];

                foreach (var group in targetRequestFinal.AbilityTargets[abilityIndex])
                {
                    var key = group.Key;
                    var values = abilityTargets[key];

                    if (group.Count == 1)
                    {
                        int v = values[0];

                        switch (group.Type)
                        {
                            case TargetType.BoardPosition:
                                ability.CardTargets[key] = new List<CardInstance>
                                {
                                    Board.GetCard(v)!
                                };
                                break;

                            case TargetType.HandIndex:
                                ability.CardTargets[key] = new List<CardInstance>
                                {
                                    player.GetHand()[v]
                                };
                                break;

                            default:
                                ability.IntValues[key] = v;
                                break;
                        }
                    }
                    else
                    {
                        switch (group.Type)
                        {
                            case TargetType.BoardPosition:
                                ability.CardTargets[key] =
                                    new List<CardInstance>(values.Select(v => Board.GetCard(v)!).ToList());
                                break;

                            case TargetType.HandIndex:
                                ability.CardTargets[key] =
                                    values.Select(v => player.GetHand()[v]).ToList();
                                break;

                            default:
                                ability.MultipleTargets[key] = values;
                                break;
                        }
                    }
                }
            }
        }

        // 9. Финал
        player.RemoveFromHand(handIndex);

        if (isUnit)
        {
            Board.PlaceCard((UnitInstance)card, position.Value);

            card.Zone = CardZone.Board;
            card.Position = position.Value;
        }
        else
        {
            player.AddToDiscard(card);
            Bus.Publish(new CardMovedToDiscard(card));
        }

        CardPlayedResult?.Invoke(new PlayCardResult { Success = true, Player = player });
        var e = new CardPlayedEvent(card, position.Value);
        CardPlayed?.Invoke(e);
        Bus.Publish(e);
    }
    
    
    private void OnRequestTargetChoice(RequestTargetChoiceEvent e)
    {
        if (_pendingChoice != null)
            throw new InvalidOperationException("Choice already in progress.");

        _pendingChoice = new ChoiceContext(
            e.Source,
            e.Options,
            e.Deniable
        );

        SetGameState(GameState.WaitingForChoice);

        // Сообщаем наружу (сетевому слою)
        ChoiceStarted?.Invoke(_pendingChoice);
    }
    
    public class ChoiceResult
    {
        public bool Success;
        public string? Error;
        public Player Player;
    }

    public void SubmitChoice(
        Player player,
        List<List<int>>? selectedTargets)
    {
        if (_pendingChoice == null)
        {
            ChoiceGetsResult?.Invoke(new ChoiceResult { Success = false, Error = "No choice in progress.", Player = player});
            return;
        }

        var ctx = _pendingChoice;

        if (ctx.Source.Owner.Owner != player)
        {
            ChoiceGetsResult?.Invoke(new ChoiceResult { Success = false, Error = "Not your choice.", Player = player });
            return;
        }

        // отказ
        if (selectedTargets == null)
        {
            if (!ctx.Deniable)
            {
                ChoiceGetsResult?.Invoke(new ChoiceResult { Success = false, Error = "Choice cannot be skipped.", Player = player });
                return;
            }

            ResolveChoice(new List<List<int>>());
            ChoiceGetsResult?.Invoke(new ChoiceResult { Success = true, Player = player });
            return;
        }

        if (!ValidateChoice(ctx.Options, selectedTargets))
        {
            ChoiceGetsResult?.Invoke(new ChoiceResult { Success = false, Error = "Invalid targets.", Player = player });
            return;
        }
        ChoiceGetsResult?.Invoke(new ChoiceResult { Success = true, Player = player });
        ResolveChoice(selectedTargets);
    }
    
    private void ResolveChoice(List<List<int>> selected)
    {
        var ctx = _pendingChoice!;
        _pendingChoice = null;

        SetGameState(GameState.PlayPhase);

        Bus.Publish(new TargetsChosenEvent(ctx.Source, selected));
    }
    
    private bool ValidateChoice(
        List<TargetOptionGroup> groups,
        List<List<int>> selected
    )
    {
        
        // 1. Проверка количества групп
        if (groups.Count != selected.Count)
            return false;

        for (int i = 0; i < groups.Count; i++)
        {
            var group = groups[i];
            var chosen = selected[i];

            // 2. Проверка количества целей в группе
            if (chosen.Count != group.Count)
                return false;

            // 3. Проверка допустимости каждой цели
            foreach (var target in chosen)
            {
                if (!group.ValidValues.Contains(target))
                    return false;
            }
        }

        return true;
    }

    
    // 5. Вращение граней

    public void RotateFace(Face face, int amountOfRotations, Player player)
    {
        amountOfRotations = Math.Abs(amountOfRotations) % 4;
        if(CurrentPlayer != player)
        {
            FaceRotatedResult?.Invoke(new RotateFaceResult{Success=false, Error="Not your turn", Player = player});
            return;
        }
        if (_gameState != GameState.RotatePhase)
        {
            FaceRotatedResult?.Invoke(new RotateFaceResult{Success=false, Error="Wrong phase", Player = player});
            return;
        }

        if (amountOfRotations == 0)
        {
            FaceRotatedResult?.Invoke(new RotateFaceResult{Success=false, Error="Zero rotation", Player = player});
        }

        switch (amountOfRotations)
        {
            case 1:
                Board.RotateFace(face, true);
                break;
            case 2:
                Board.RotateFace(face, true);
                Board.RotateFace(face, true);
                break;
            case 3:
                Board.RotateFace(face, false);
                break;
        }
        
        FaceRotatedResult?.Invoke(new RotateFaceResult{Success=true, Player = player});
        var e = new FaceRotatedEvent(face, amountOfRotations, player);
        FaceRotated?.Invoke(e);
        Bus.Publish(e);
    } // выполняет вращение и публикует FaceRotatedEvent

    // 6. Бой

    private void StartBattle()
    {
         Console.WriteLine("Battle");
         SetGameState(GameState.BattlePhase);
         PreBattlePhase();
         BattlePhase();
         PostBattlePhase();
         ProcessState();
    } // включает подфазы: перед боем, во время боя, после боя
    private void PreBattlePhase() 
    {
        BattleStarted?.Invoke();
        Bus.StartBattleMode();
        Bus.Publish(new PreBattlePhaseStarted());
        Bus.EndBattleMode();
        Bus.Publish(new PreBattlePhaseEnded());
        
    }

    private void BattlePhase()
    {
        Bus.StartBattleMode();
        Bus.Publish(new BattlePhaseStarted());
        foreach (var card in Board.GetAllCards())
        {
            //Атакует только быстрая атака и двойная атака
            if (card.Keywords.Contains(Keyword.Sleeping))
                continue;
            if (!card.Keywords.Contains(Keyword.QuickAttack) && !card.Keywords.Contains(Keyword.DoubleAttack))
                continue;
            var enemies = Board.GetEnemyCardsOnFace(card);
            foreach (var enemy in enemies)
            {
                {
                    Console.WriteLine("Quick attack " + enemy.InstanceId + " " + card.InstanceId);
                    Bus.Publish(new CardCombatDamageRequestEvent(enemy, card.CurrentPower, card));
                }
            }
        }
        ResolveCombatDamage();
        Bus.EndBattleMode();
        
        Bus.StartBattleMode();
        
        foreach (var card in Board.GetAllCards())
        {
            //Атакует только обычная атака и двойная атака
            if (card.Keywords.Contains(Keyword.Sleeping))
                continue;
            if (card.Keywords.Contains(Keyword.QuickAttack) && !card.Keywords.Contains(Keyword.DoubleAttack)) continue;
            var enemies = Board.GetEnemyCardsOnFace(card);
            foreach (var enemy in enemies)
            {
                {
                    Console.WriteLine("Normal attack " + enemy.InstanceId + " " + card.InstanceId);
                    Bus.Publish(new CardCombatDamageRequestEvent(enemy, card.CurrentPower, card));
                }
            }
        }
        Bus.Publish(new BattlePhaseEnded());
        ResolveCombatDamage();
        Bus.EndBattleMode(); 
    }

    private void PostBattlePhase()
    {
        Bus.StartBattleMode();
        Bus.Publish(new PostBattlePhaseStarted());
        Bus.EndBattleMode();
        Bus.Publish(new PostBattlePhaseEnded());
        BattleEnded?.Invoke();
    }

    // 7. Подсчёт очков
    private void CalculateScores()
    {
        SetGameState(GameState.RewardingPhase); 
        int player1Score = 0;
        int player2Score = 0;
        foreach (Face face in Enum.GetValues(typeof(Face)))
        {
            var p1 = 0;
            var p2 = 0;
            foreach (var card in Board.GetCardsOnFace(face))
            {
                if (card.Owner == Player1)
                    p1++;
                else
                    p2++;
            }

            if (p1 > p2)
                player1Score++;
            if (p2 > p1)
                player2Score++;
        }
        if(player1Score > player2Score)
            Bus.Publish(new PlayerScoreRequestEvent(1, Player1));
        if (player2Score > player1Score)
            Bus.Publish(new PlayerScoreRequestEvent(1, Player2));
        ProcessState();
    } // определяет, кто контролирует грани

    private void AwardPoints(PlayerScoreRequestEvent e)
    {
        if (!e.Allowed || e.Amount <= 0)
            return;
        if (e.Player == Player1)
        {
            Player1Score += e.Amount;
            var scoredEvent = new PlayerScoredEvent(e.Amount, Player1Score, e.Player);
            PlayerScored?.Invoke(scoredEvent);
            Bus.Publish(scoredEvent);
        }

        if (e.Player == Player2)
        {
            Player2Score += e.Amount;
            var scoredEvent = new PlayerScoredEvent(e.Amount, Player2Score, e.Player);
            PlayerScored?.Invoke(scoredEvent);
            Bus.Publish(scoredEvent);
        }
        
    }   

    // 8. Управление картами на поле
    private void KillCard(CardKillRequestEvent e)
    {
        if (e.Card.Zone != CardZone.Board)
        {
            return;
        }
        if (e.Unit.IsDead || !e.Allowed)
        {
            return;
        }
        e.Unit.Kill(new CardKilledEvent(e.Unit, e.Source));
    }
    private void RemoveDeadCard(CardKilledEvent e)
    {
        CardKilled?.Invoke(e);
        Board.RemoveCard(e.Unit);
        e.Card.Owner.AddToDiscard(e.Card);
        Bus.Publish(new CardMovedToDiscard(e.Card));
    } // убирает карты с нулевым здоровьем после подфаз

    private void ApplyCombatDamage(CardCombatDamageRequestEvent e)
    {
        if (e.Card.Zone != CardZone.Board)
        {
            return;
        }

        if (e.Unit.IsDead || e.Damage <= 0 || !e.Allowed)
        {
            return;
        }
        if (!_pendingCombatDamage.TryGetValue(e.Unit, out var list))
        {
            list = new List<CardCombatDamageRequestEvent>();
            _pendingCombatDamage[e.Unit] = list;
        }
        list.Add(e);
        Console.WriteLine("Combat damage");
    }
    
    private void ApplyNonCombatDamage(CardNonCombatDamageRequestEvent e)
    {
        if (e.Card.Zone != CardZone.Board)
        {
            return;
        }

        if (e.Unit.IsDead || e.Damage <= 0 || !e.Allowed)
        {
            return;
        }
        if (e.Unit.Keywords.Contains(Keyword.Shield))
        {
            e.Unit.Keywords.Remove(Keyword.Shield);
            var shieldEvent = new ShieldBrokenEvent(e.Unit);
            ShieldBroken?.Invoke(shieldEvent);
            Bus.Publish(shieldEvent);
            return;
        }
        e.Unit.TakeDamage(new CardNonCombatDamagedEvent(e.Unit, e.Damage, e.Source));
    }
    
    private void ResolveCombatDamage()
    {
        foreach (var (unit, hits) in _pendingCombatDamage)
        {
            if (unit.IsDead)
                continue;

            if (unit.Keywords.Contains(Keyword.Shield))
            {
                unit.Keywords.Remove(Keyword.Shield);

                var shieldEvent = new ShieldBrokenEvent(unit);
                ShieldBroken?.Invoke(shieldEvent);
                Bus.Publish(shieldEvent);

                continue;
            }

            var damageEvents = hits
                .Select(h => new CardCombatDamagedEvent(unit, h.Damage, h.Source))
                .ToList();

            unit.TakeDamage(damageEvents);
        }

        _pendingCombatDamage.Clear();
    }
    
    private void BuffCard(CardBuffRequestEvent e)
    {
        if (e.Card.Zone is CardZone.Discard or CardZone.PlayerPlaceHolder)
            return;
        if (e.Unit.IsDead || !e.Allowed || e.PowerDelta <= 0 && e.HealthDelta <= 0)
            return;
        var buffEvent = new CardBuffedEvent(e.Unit, Math.Max(0, e.PowerDelta), Math.Max(0, e.HealthDelta), e.Source);
        e.Unit.Buff(buffEvent);
    }

    private void AddKeyword(AddKeywordRequestEvent e)
    {
        if (!e.Allowed)
            return;
        if (e.Unit.Keywords.Contains(e.Keyword)) return;
        e.Unit.Keywords.Add(e.Keyword);
        var ev = new KeywordAddedEvent(e.Keyword, e.Unit, e.Source);
        KeywordAdded?.Invoke(ev);
        Bus.Publish(ev);
    }

    private void RemoveKeyword(RemoveKeywordRequestEvent e)
    {
        if (!e.Allowed)
            return;
        if (e.Unit.Keywords.Contains(e.Keyword)) return;
        e.Unit.Keywords.Remove(e.Keyword);
        var ev = new KeywordRemovedEvent(e.Keyword, e.Unit, e.Source);
        KeywordRemoved?.Invoke(ev);
        Bus.Publish(ev);
    }

    //Методы куба

    private void ChangeColor(ChangeColorRequestEvent e)
    {
        if (!e.Allowed)
            return;
        Board.ChangeColor(e.Position, e.Color);
        var ev = new ColorChangedEvent(e.Position, e.Color, e.Source);
        ColorChanged?.Invoke(ev);
        Bus.Publish(ev);
    }

    // 9. Вспомогательные методы

    public GameContext GetContext(Player player)
    {
        return new GameContext(Board, player);
    }
    
    //10. Методы уведомлений
    private void BuffNotify(CardBuffedEvent e)
    {
        CardBuffed?.Invoke(e);
    }

    private void DamageNotify(CardDamagedEvent e)
    {
        CardDamaged?.Invoke(e);
    }
    
    private IEnumerable<CardInstance> GetZoneCards(CardZone zone, Player? player)
    {
        if (player == null)
            return Enumerable.Empty<CardInstance>();
        return zone switch
        {
            CardZone.Board => Board.GetAllCards(),
            CardZone.Hand => player.GetHand(),
            CardZone.Deck => player.GetDeck(),
            CardZone.Discard => player.GetDiscard(),
            _ => Enumerable.Empty<CardInstance>()
        };
    }
    
    private List<CardInstance> GetSelectedCards(
    CardInstance? source,
    TargetSelector selector)
    {
        IEnumerable<CardInstance> candidates = GetZoneCards(selector.Zone, source.Owner);

        // 1 Side filter
        if (source != null)
        {
            candidates = selector.Side switch
            {
                TargetSide.Ally =>
                    candidates.Where(c => c.Owner == source.Owner),

                TargetSide.Enemy =>
                    candidates.Where(c => c.Owner != source.Owner),

                _ => candidates
            };
        }

        // 2 Face filter (ONLY BOARD)
        if (selector.Zone == CardZone.Board &&
            selector.Face == FaceConstraint.SameFace &&
            source is UnitInstance sourceUnit)
        {
            var sourceFace = Board.GetFaceOfSticker(sourceUnit.Position);

            candidates = candidates.Where(c =>
                c is UnitInstance u &&
                Board.GetFaceOfSticker(u.Position) == sourceFace);
        }

        // 3 Stat filter (ONLY UNITS)
        if (selector.Stat != StatConstraint.Any)
        {
            var units = candidates.OfType<UnitInstance>().ToList();

            if (units.Count == 0)
                return new List<CardInstance>();
            
            List<UnitInstance> GetWeakest(List<UnitInstance> units)
            {
                int min = units.Min(u => u.CurrentPower);

                return units
                    .Where(u => u.CurrentPower == min)
                    .ToList();
            }
            List<UnitInstance> GetStrongest(List<UnitInstance> units)
            {
                int max = units.Max(u => u.CurrentPower);

                return units
                    .Where(u => u.CurrentPower == max)
                    .ToList();
            }
            
            units = selector.Stat switch
            {
                StatConstraint.Weakest => GetWeakest(units),

                StatConstraint.Strongest => GetStrongest(units),

                _ => units
            };

            candidates = units;
        }

        var list = candidates.ToList();

        if (list.Count == 0)
            return list;

        // 4 Final pick
        return selector.Pick switch
        {
            TargetPick.All =>
                list,

            TargetPick.First =>
                list.Take(selector.Count).ToList(),

            TargetPick.Random =>
                list
                    .OrderBy(_ => _random.Next())
                    .Take(selector.Count)
                    .ToList(),

            _ => list
        };
    }


    // 10. Подписка на события карт/способностей
    private void SubscribeToCardEvents()
    {
        Bus.Subscribe<PlayerScoreRequestEvent>(AwardPoints, SubscriberOwnerType.API, this);
        Bus.Subscribe<CardCombatDamageRequestEvent>(ApplyCombatDamage, SubscriberOwnerType.API, this);
        Bus.Subscribe<CardNonCombatDamageRequestEvent>(ApplyNonCombatDamage, SubscriberOwnerType.API, this);
        Bus.Subscribe<CardDrawRequestEvent>(DrawCards, SubscriberOwnerType.API, this);
        Bus.Subscribe<CardKillRequestEvent>(KillCard, SubscriberOwnerType.API, this);
        Bus.Subscribe<CardKilledEvent>(RemoveDeadCard, SubscriberOwnerType.API, this);
        Bus.Subscribe<CardBuffRequestEvent>(BuffCard, SubscriberOwnerType.API, this);
        Bus.Subscribe<RequestTargetChoiceEvent>(OnRequestTargetChoice, SubscriberOwnerType.API, this);
        Bus.Subscribe<CardBuffedEvent>(BuffNotify, SubscriberOwnerType.API, this);
        Bus.Subscribe<CardCombatDamagedEvent>(DamageNotify, SubscriberOwnerType.API, this);
        Bus.Subscribe<CardNonCombatDamagedEvent>(DamageNotify, SubscriberOwnerType.API, this);
        Bus.Subscribe<AddKeywordRequestEvent>(AddKeyword, SubscriberOwnerType.API, this);
        Bus.Subscribe<RemoveKeywordRequestEvent>(RemoveKeyword, SubscriberOwnerType.API, this);
        Bus.Subscribe<ChangeColorRequestEvent>(ChangeColor, SubscriberOwnerType.API, this);
        
    }
    private void UnsubscribeFromCardEvents(){}

    private void SetGameState(GameState gameState)
    {
        _gameState = gameState;
    }

    private void ProcessState()
    {
        var firstPlayer = Round % 2 == 1 ? Player1 : Player2;
        var secondPlayer = Round % 2 == 0 ? Player1 : Player2;
        switch (_gameState)
        {
            case GameState.Start:
                StartTurn(Player1);
                return;
            case GameState.PlayPhase:
            {
                SetGameState(GameState.RotatePhase);
                var e = new PlayerRotationPhaseStarted(CurrentPlayer);
                RotationPhaseStarted?.Invoke(e);
                Bus.Publish(e);
                return;
            }
            case GameState.RotatePhase:
            {
                var e = new PlayerTurnEnded(CurrentPlayer);
                TurnEnded?.Invoke(e);
                Bus.Publish(e);
                if (CurrentPlayer == firstPlayer)
                {
                    
                    StartTurn(secondPlayer);
                }
                else
                {
                    StartBattle();
                }
                return;
            }
            case GameState.BattlePhase:
                CalculateScores();
                return;
            case GameState.RewardingPhase:
                Bus.Publish(new RoundEnded(Round));
                Round += 1;
                var roundStartEvent = new RoundStarted(Round);
                RoundStarted?.Invoke(roundStartEvent);
                Bus.Publish(roundStartEvent);
                StartTurn(secondPlayer);
                break;
        }
    }
}
