namespace GameServer.Abilities;
[AbilityId("buff_self_on_strong_allies")]
public class Buff_self_on_strong_allies : AbilityLogic
{
    public Buff_self_on_strong_allies(AbilityState state) : base(state) {}
    
    public override void OnGain()
    {
        Bus.Subscribe<CardPlayedEvent>(BuffSelfOnDeploy, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(BuffSelfOnDeploy, Owner);
    }

    private void BuffSelfOnDeploy(CardPlayedEvent e)
    {
        if(!OnBoardAbilityActive)
            return;
        if (!State.IntValues.TryGetValue("minimum_power", out var minPow))
            return;
        var ctx = Owner._api.GetContext(Owner.Owner);
        var strongAllies = ctx.GetFriendlyCards(Owner).Count(c => c.CurrentPower >= minPow);
        Bus.Publish(new CardBuffRequestEvent(OwnerUnit, strongAllies, 0, Owner));
    }
}