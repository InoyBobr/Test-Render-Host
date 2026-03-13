
using System.Text.Json;
using System.Text.Json.Serialization;

public class TestingClass
{
    public void Main()
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
        api.StartTurn(player1);
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

    private void OnChoiceStarted(ChoiceContext context)
    { 
        Console.WriteLine(context.Deniable + " " + context.Source.Owner.Position + " " + context.Source.Owner.Zone);
    }

    private void OnChoiceGetsResult(GameAPI.ChoiceResult context)
    {
        Console.WriteLine(context.Success + " " + context.Error);
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
            Console.WriteLine($"[CardDamaged] TargetPos={e.Card.Position} Damage={e.Damage} SourcePos={e.Source?.Position}");

        api.CardBuffed += e =>
            Console.WriteLine($"[CardBuffed] TargetPos={e.Card.Position} Power+={e.PowerDelta} Health+={e.HealthDelta}");

        api.CardKilled += (unit, killer) =>
            Console.WriteLine($"[CardKilled] UnitPos={unit.Position} KillerPos={killer?.Position}");

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
}