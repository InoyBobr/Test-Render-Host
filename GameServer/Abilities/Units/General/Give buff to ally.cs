namespace GameServer.Abilities;
[AbilityId("buff_allies_ability")]
public class Give_buff_to_ally : AbilityLogic
{
    public Give_buff_to_ally(AbilityState state) : base(state) {}
    
    
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
                Bus.Publish(new CardBuffRequestEvent(unit, power, health, Owner));
            }
        }
    }
    
    public override List<TargetOptionGroup>? GetTargetOptions(GameContext ctx)
    {
        var count = State.IntValues.GetValueOrDefault("count", 1);
        var allies = ctx.GetFriendlyCards(Owner);
        var positions = allies.Select(u => u.Position).ToList();
        TargetOptionGroup target = new TargetOptionGroup
        {
            Key = "buffTarget",
            Count = Math.Min(count, positions.Count),
            Type = TargetType.BoardPosition,
            ValidValues = positions,
            Distinct = true
        };
        return new List<TargetOptionGroup> { target };
    }
}