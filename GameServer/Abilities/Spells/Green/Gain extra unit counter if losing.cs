namespace GameServer.Abilities.Spells.Green;

public class Gain_extra_unit_counter_if_losing(AbilityState state) : AbilityLogic(state)
{
    public override void OnGain()
    {
        Bus.Subscribe<CardPlayedEvent>(IncreaseCounter, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(IncreaseCounter, Owner);
    }

    private void IncreaseCounter(CardPlayedEvent e)
    {
        if (e.Card != Owner)
            return;
        if(!State.IntValues.TryGetValue("amount", out var amount))
        {
            Console.WriteLine("unit_difference key not found");
            return;
        }
        Bus.Publish(new ChangeRemainingUnitCounterRequestEvent(amount, false, false, false, Owner));
    }

    public override bool CanBePlayed(GameContext ctx)
    {
        var scoreDiff = ctx.EnemyScore - ctx.PlayerScore;
        var unitsDiff = ctx.GetEnemyCards(Owner).Count() - ctx.GetFriendlyCards(Owner).Count();
        if (!State.IntValues.TryGetValue("score_difference", out var minScoreDiff))
        {
            Console.WriteLine("score_difference key not found");
            return false;
        }
        if(!State.IntValues.TryGetValue("unit_difference", out var minUnitDiff))
        {
            Console.WriteLine("unit_difference key not found");
            return false;
        }
        
        return scoreDiff > minScoreDiff && unitsDiff > minUnitDiff;
    }
}