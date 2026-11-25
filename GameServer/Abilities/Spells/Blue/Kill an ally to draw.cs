namespace GameServer.Abilities.Spells.Blue;

public class Kill_an_ally_to_draw(AbilityState state) : AbilityLogic(state)
{
    public override void OnGain()
    {
        Bus.Subscribe<CardPlayedEvent>(KillOnPlay, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(KillOnPlay, Owner);
    }

    private void KillOnPlay(CardPlayedEvent e)
    {
        if (e.Card != Owner)
            return;
        if (!State.CardTargets.TryGetValue("killTarget", out var targets))
            return;
        if (!State.IntValues.TryGetValue("amount", out var amount))
            return;
        foreach (var target in targets)
        {
            if (target is UnitInstance unit)
            {
                Bus.Publish(new CardKillRequestEvent(unit, Owner));
            }
        }
        Bus.Publish(new CardDrawRequestEvent(Owner.Owner,amount, Owner ));
    }
    
    public override List<TargetOptionGroup>? GetTargetOptions(GameContext ctx)
    {
        var allies = ctx.GetFriendlyCards(Owner);
        var positions = allies.Select(u => u.Position).ToList();
        TargetOptionGroup target = new TargetOptionGroup
        {
            Key = "killTarget",
            Count = Math.Min(1, positions.Count),
            Type = TargetType.BoardPosition,
            ValidValues = positions,
            Distinct = true
        };
        return new List<TargetOptionGroup> { target };
    }
}