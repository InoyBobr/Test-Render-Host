namespace GameServer.Abilities;
[AbilityId("give_ally_quick_attack")]
public class Give_ally_quick_attack : AbilityLogic
{
    public Give_ally_quick_attack(AbilityState state) : base(state) {}
    
    public override void OnGain()
    {
        Bus.Subscribe<CardPlayedEvent>(GiveKeywordOnPlay, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(GiveKeywordOnPlay, Owner);
    }

    private void GiveKeywordOnPlay(CardPlayedEvent e)
    {
        if (e.Card != Owner)
            return;
        if (!State.CardTargets.TryGetValue("AllyTarget", out var targets))
            return;
        foreach (var target in targets)
        {
            if (target is UnitInstance unit)
            {
                Bus.Publish(new AddKeywordRequestEvent(Keyword.QuickAttack, unit, Owner));
            }
        }
    }
    
    public override List<TargetOptionGroup>? GetTargetOptions(GameContext ctx)
    {
        var allies = ctx.GetFriendlyCards(Owner);
        var positions = allies.Select(u => u.Position).ToList();
        TargetOptionGroup target = new TargetOptionGroup
        {
            Key = "AllyTarget",
            Count = Math.Min(1, positions.Count),
            Type = TargetType.BoardPosition,
            ValidValues = positions,
            Distinct = true
        };
        return new List<TargetOptionGroup> { target };
    }

}