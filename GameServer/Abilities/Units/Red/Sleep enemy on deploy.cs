namespace GameServer.Abilities;

public class Sleep_enemy_on_deploy(AbilityState state) : AbilityLogic(state)
{
    public override void OnGain()
    {
        Bus.Subscribe<CardPlayedEvent>(DealDamageOnPlay, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(DealDamageOnPlay, Owner);
    }

    private void DealDamageOnPlay(CardPlayedEvent e)
    {
        if (e.Card != Owner)
            return;
        if (!State.CardTargets.TryGetValue("sleepTarget", out var targets))
            return;
        foreach (var target in targets)
        {
            if (target is UnitInstance unit)
            {
                Bus.Publish(new AddKeywordRequestEvent(Keyword.Sleeping, unit, Owner));
            }
        }
    }
    
    public override List<TargetOptionGroup>? GetTargetOptions(GameContext ctx)
    {
        var count = State.IntValues.GetValueOrDefault("count", 1);
        var enemies = ctx.GetEnemyCards(Owner);
        var positions = enemies.Select(u => u.Position).ToList();
        TargetOptionGroup target = new TargetOptionGroup
        {
            Key = "sleepTarget",
            Count = Math.Min(count, positions.Count),
            Type = TargetType.BoardPosition,
            ValidValues = positions,
            Distinct = true
        };
        return new List<TargetOptionGroup> { target };
    }
}