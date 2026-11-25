namespace GameServer.Abilities;
[AbilityId("death_mark_ability")]
public class Play_to_kill_ally_if_in_hand_kill_all_allies(AbilityState state) : AbilityLogic(state)
{
    public override void OnGain()
    {
        Bus.Subscribe<CardPlayedEvent>(KillTarget, SubscriberOwnerType.Card, Owner);
        Bus.Subscribe<PlayerTurnEnded>(KillAll, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(KillTarget, Owner);
        Bus.Unsubscribe<PlayerTurnEnded>(KillAll, Owner);
    }

    private void KillTarget(CardPlayedEvent e)
    {
        if(e.Card != Owner)
            return;
        if (!State.CardTargets.TryGetValue("killTarget", out var targets))
            return;
        foreach (var cardInstance in targets)
        {
            var unit = (UnitInstance)cardInstance;
            Bus.Publish(new CardKillRequestEvent(unit, Owner));
        }
    }
    
    private void KillAll(PlayerTurnEnded e)
    {
        if(e.Player != Owner.Owner)
            return;
        if(Owner.Zone != CardZone.Hand)
            return;
        var ctx = Owner._api.GetContext(Owner.Owner);
        var allies = ctx.GetFriendlyCards(Owner);
        foreach (var target in allies)
        {
            Bus.Publish(new CardKillRequestEvent(target, Owner));
        }
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