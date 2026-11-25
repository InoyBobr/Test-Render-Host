namespace GameServer.Abilities.Spells.Green;

public class Rise_stats_to_value_ability(AbilityState state) : AbilityLogic(state)
{
    public override void OnGain()
    {
        Bus.Subscribe<CardPlayedEvent>(BuffOnPlay, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(BuffOnPlay, Owner);
    }

    private void BuffOnPlay(CardPlayedEvent e)
    {
        if (e.Card != Owner)
            return;
        if (!State.CardTargets.TryGetValue("buffTarget", out var targets))
            return;
        if (!State.IntValues.TryGetValue("power", out var power) || !State.IntValues.TryGetValue("health", out var health))
            return;
        foreach (var target in targets)
        {
            if (target is UnitInstance unit)
            {
                Bus.Publish(new SetStatsRequestEvent(unit, power, health, Owner, SetStatsType.Buff));
            }
        }
    }

    
    public override List<TargetOptionGroup>? GetTargetOptions(GameContext ctx)
    {
        var friends = ctx.GetFriendlyCards(Owner);
        var positions = friends.Select(u => u.Position).ToList();
        TargetOptionGroup target = new TargetOptionGroup
        {
            Key = "buffTarget",
            Count = Math.Min(1, positions.Count),
            Type = TargetType.BoardPosition,
            ValidValues = positions,
            Distinct = true
        };
        return new List<TargetOptionGroup> { target };
    }
}