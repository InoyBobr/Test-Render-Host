using System.Text.Json;
using System.Text.Json.Serialization;

public class TestingClass
{
    public void Main()
    {
        CardDatabase.LoadFromFolder("CardsData");
        var f = CardDatabase.Get("greenShaman");
        Console.WriteLine(f.CardId);
        //TestBuffOrder();
        TestSpell();
    }
    
    
    
    private void TestBuffOrder()
    {
        Console.WriteLine("===== TEST: Buff before enemy damage =====");

        var buffer = CardDatabase.Get("greenShaman");
        var fragile = CardDatabase.Get("fragile");
        var pinger = CardDatabase.Get("redShaman");

        var api = new GameAPI();

        var player1 = new Player(
            new List<CardData>
            {
                buffer,
                fragile,
                fragile,
                fragile,
                fragile
            },
            api);

        var player2 = new Player(
            new List<CardData>
            {
                pinger,
                fragile,
                fragile,
                fragile,
                fragile
            },
            api);

        SubscribeToAllEventsWithJsonPrint(api, player1);

        api.StartGame(player1, player2);
        

        // ставим баффера
        var pos = player1.GetHand().MinBy(c => c.InstanceId).Position;
        api.TryPlayCard(player1, pos, 4);
        api.EndTurn(player1);
        api.EndTurn(player1);
        // ставим пингера
        pos = player2.GetHand().MinBy(c => c.InstanceId).Position;
        api.TryPlayCard(player2, pos, 12);
        api.EndTurn(player2);
        api.EndTurn(player2);
        //ход игрока 2 раунд 2
        api.TryPlayCard(player2, 0, 11);
        api.EndTurn(player2);
        api.EndTurn(player2);
        // ставим fragile 1/1
        var c = player1.GetHand()[0] as UnitInstance;
        api.TryPlayCard(player1, 0, 8);
        Console.WriteLine($"Power {c.CurrentPower} Health {c.CurrentHealth}");
        api.EndTurn(player1);
        api.RotateFace(Face.Top, 1, player1);
        api.EndTurn(player1);
        //

        //
    }

    private void TestSpell()
    {
        Console.WriteLine("===== TEST: Spell requires units to be played =====");
        
        var fragile = CardDatabase.Get("fragile");
        var fireball = CardDatabase.Get("fireBall");

        var api = new GameAPI();

        var player1 = new Player(
            new List<CardData>
            {
                fragile,
                fragile,
                fragile,
                fragile
            },
            api);

        var player2 = new Player(
            new List<CardData>
            {
                fireball,
                fragile,
                fragile,
                fragile,
                fragile
            },
            api);

        SubscribeToAllEventsWithJsonPrint(api, player1);

        api.StartGame(player1, player2);
        api.TryPlayCard(player1, 0, 2);
        api.TryPlayCard(player1, 0, 8);
        api.TryPlayCard(player1, 0, 11);
        api.EndTurn(player1);
        api.EndTurn(player1);
        var pos = player2.GetHand().MinBy(c => c.InstanceId).Position;
        api.TryPlayCard(player2, pos);
        Thread.Sleep(100);
        api.TryPlayCard(player2, (pos + 1) % player2.GetHand().Count, 5);
        pos = player2.GetHand().MinBy(c => c.InstanceId).Position;
        var l = new List<int>() { 11, 8 };
        var d = new Dictionary<string, List<int>>()
        {
            { "damageTarget", l },
        };
        var r = new Dictionary<int, Dictionary<string, List<int>>> { { 0, d } };
        api.TryPlayCard(player2, pos, -1);
        api.TryPlayCard(player2, pos, -1, r);

    }

    private void FirstTest()
    {
        var json = """
                   {
                     "CardId": "fire_elemental",
                     "Type": "Unit",
                     "basePower": 2,
                     "baseHealth": 8,
                     "color": "Red",
                     "keywords": ["Hoard", "DoubleAttack"],
                     "abilities": [
                     {
                        "AbilityId" : "test_ability",
                        "Parameters" : [
                            {"key": "damage", "value": 1}
                        ]
                     }
                     ]
                   }
                   """;

        var json2 = """
                    {
                      "CardId": "frost_elemental",
                      "Type": "Unit",
                      "basePower": 4,
                      "baseHealth": 5,
                      "color": "Red",
                      "keywords": ["QuickAttack", "Shield"],
                      "abilities": []
                    }
                    """;

        var data1 = CardDataLoader.FromJson(json);
        var data2 = CardDataLoader.FromJson(json2);
        var api = new GameAPI();
        /*api.ChoiceStarted += OnChoiceStarted;
        api.ChoiceGetsResult += OnChoiceGetsResult;
        api.CardPlayedResult += OnCardPlayedResult;*/
        
        var player1 = new Player(new List<CardData>() {data2, data2, data2, data2, data2}, api);
        var player2 = new Player(new List<CardData>() {data1, data1}, api);
        SubscribeToAllEvents(api, player1);
        api.StartGame(player1, player2);
        var c1 = (UnitInstance)player1.GetHand()[0];
        var c2 = (UnitInstance)player2.GetHand()[0];
        Console.WriteLine(c1.InstanceId + " pos: " + c1.Position + " pow: " + c1.CurrentPower + " hp: " + c1.CurrentHealth + " zone: " + c1.Zone);
        Console.WriteLine(c2.InstanceId + " pos: " + c2.Position + " pow: " + c2.CurrentPower + " hp: " + c2.CurrentHealth + " zone: " + c2.Zone);
        api.TryPlayCard(player1, 0, 6);
        api.TryPlayCard(player1, 0, 0);
        api.EndTurn(player1);
        //api.RotateFace(Face.Front, 1, player1);
        api.EndTurn(player1);
        var l = new List<int>() { 6 };
        var d = new Dictionary<string, List<int>>()
        {
            { "damageTarget", l },
        };
        var r = new Dictionary<int, Dictionary<string, List<int>>>() { { 0, d } };
        api.TryPlayCard(player2, 0, 18, r);
        Console.WriteLine(c1.InstanceId + " pos: " + c1.Position + " pow: " + c1.CurrentPower + " hp: " + c1.CurrentHealth + " zone: " + c1.Zone);
        Console.WriteLine(c2.InstanceId + " pos: " + c2.Position + " pow: " + c2.CurrentPower + " hp: " + c2.CurrentHealth + " zone: " + c2.Zone);
        api.EndTurn(player2);
        api.EndTurn(player2);
        Console.WriteLine(c1.InstanceId + " pos: " + c1.Position + " pow: " + c1.CurrentPower + " hp: " + c1.CurrentHealth + " zone: " + c1.Zone);
        Console.WriteLine(c2.InstanceId + " pos: " + c2.Position + " pow: " + c2.CurrentPower + " hp: " + c2.CurrentHealth + " zone: " + c2.Zone);
    }
    
    private void SubscribeToAllEvents(GameAPI api, Player player1)
    {
        string P(Player p) => p == player1 ? "Player1" : "Player2";

        api.GameStarted += () =>
            Console.WriteLine("[GameStarted]");
        api.RoundStarted += e =>
            Console.WriteLine($"[Round {e.Round} started]");

        api.TurnStarted += e =>
            Console.WriteLine($"[TurnStarted] {P(e.Player)}");

        api.TurnEnded += e =>
            Console.WriteLine($"[TurnEnded] {P(e.Player)}");

        api.BattleStarted += () =>
            Console.WriteLine("[BattleStarted]");

        api.BattleEnded += () =>
            Console.WriteLine("[BattleEnded]");

        api.PlayerScored += e =>
            Console.WriteLine($"[PlayerScored] {P(e.Player)} Gain: {e.Amount} Score: {e.FullScore} ");

        api.CardDrawn += e =>
            Console.WriteLine($"[CardDrawn] {P(e.Player)} drew card {e.Card.InstanceId}");

        api.CardDamaged += e =>
            Console.WriteLine($"[CardDamaged] TargetPos={e.Card.Position} Damage={e.Damage} SourcePos={e.SourcePos}");

        api.CardBuffed += e =>
            Console.WriteLine($"[CardBuffed] TargetPos={e.Card.Position} Power+={e.PowerDelta} Health+={e.HealthDelta}");

        api.CardKilled += e =>
            Console.WriteLine($"[CardKilled] UnitPos={e.Card.Position} KillerPos={e.SourcePos}");

        api.ShieldBroken += e =>
            Console.WriteLine($"[ShieldBroken] UnitPos={e.Unit.Position}");

        api.ChoiceStarted += e =>
        {
            var owner = e.Source.Owner?.Owner;
            var pos = e.Source.Owner.Position;

            Console.WriteLine($"[ChoiceStarted] Owner={P(owner)} CardPos={pos} Deniable={e.Deniable}");

            if (e.Options != null)
            {
                foreach (var g in e.Options)
                {
                    Console.WriteLine(
                        $"  Key={g.Key}, Type={g.Type}, Count={g.Count}, Distinct={g.Distinct}, Values=[{string.Join(",", g.ValidValues)}]"
                    );
                }
            }
        };

        api.ChoiceGetsResult += e =>
            Console.WriteLine($"[ChoiceResult] Success={e.Success} Error={e.Error}");

        api.CardPlayedResult += PrintCardPlayedResult;

        api.FaceRotatedResult += e =>
            Console.WriteLine($"[FaceRotated] Success={e.Success} Error={e.Error}");
    }
    
    private void SubscribeToAllEventsWithJsonPrint(GameAPI api, Player player1)
    {
        bool You(Player p) => p == player1;

        api.GameStarted += () =>
            Send(JsonEvent.Build("game_started", new { }));

        api.RoundStarted += e =>
            Send(JsonEvent.Build("round_started", new { round = e.Round }));

        api.TurnStarted += e =>
            Send(JsonEvent.Build("turn_started", new { you = You(e.Player) }));

        api.TurnEnded += e =>
            Send(JsonEvent.Build("turn_ended", new { you = You(e.Player) }));

        api.BattleStarted += () =>
            Send(JsonEvent.Build("battle_started", new { }));

        api.BattleEnded += () =>
            Send(JsonEvent.Build("battle_ended", new { }));

        api.PlayerScored += e =>
            Send(JsonEvent.Build("player_scored", new
            {
                you = You(e.Player),
                gain = e.Amount,
                score = e.FullScore
            }));

        api.CardDrawn += e =>
        {
            if (You(e.Player))
            {
                Send(JsonEvent.Build("card_drawn", new
                {
                    you = true,
                    card = CardMapper.ToDto(e.Card)
                }));
            }
            else
            {
                Send(JsonEvent.Build("card_drawn", new { you = false }));
            }
        };

        api.CardDamaged += e =>
            Send(JsonEvent.Build("card_damaged", new
            {
                target_pos = e.Card.Position,
                damage = e.Damage,
                source_pos = e.SourcePos
            }));

        api.CardBuffed += e =>
            Send(JsonEvent.Build("card_buffed", new
            {
                pos = e.Card.Position,
                power = e.PowerDelta,
                health = e.HealthDelta
            }));

        api.CardKilled += e =>
            Send(JsonEvent.Build("card_killed", new
            {
                pos = e.Card.Position,
                killer = e.SourcePos
            }));

        api.ShieldBroken += e =>
            Send(JsonEvent.Build("shield_broken", new
            {
                pos = e.Unit.Position
            }));

        api.ChoiceStarted += e =>
            Send(JsonEvent.Build("choice_started", new
            {
                owner = You(e.Source.Owner.Owner),
                card_pos = e.Source.Owner.Position,
                deniable = e.Deniable,
                options = e.Options
            }));

        api.ChoiceGetsResult += e =>
            Send(JsonEvent.Build("choice_result", new
            {
                success = e.Success,
                error = e.Error
            }));

        api.FaceRotatedResult += e =>
            Send(JsonEvent.Build("face_rotated_result", new
            {
                success = e.Success,
                error = e.Error
            }));

        api.CardPlayedResult += result =>
            Send(JsonEvent.Build("card_played_result", new
            {
                success = result.Success,
                error = result.Error,
                playable_positions = result.PlayablePositions,
                targets = result.TargetsToPick?.AbilityTargets
            }));
        api.CardPlayed += e =>
            Send(JsonEvent.Build("card_played", new
            {
                card = CardMapper.ToDto(e.Card),
                position = e.Position
            }));
    }
    
    private void PrintCardPlayedResult(PlayCardResult result)
    {
        var success = "Success: " + result.Success;
        var error = "";
        if (result.Error is not null)
            error = ", Error: " + result.Error + ",";
        var positions = "";
        if (result.PlayablePositions != null)
            positions = ", Playable positions: " + String.Join(", ", result.PlayablePositions);
        var targets = "";
        var j = "";
        if (result.TargetsToPick is not null)
        {
            targets = ", Targets to Pick: " + result.TargetsToPick;
            var options = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
            j = JsonSerializer.Serialize(result.TargetsToPick.AbilityTargets, options);
        }

        Console.WriteLine(success + error + positions + targets);
        Console.WriteLine(j);
    }

    private void Send(string json)
    {
        Console.WriteLine(json);
    }
}