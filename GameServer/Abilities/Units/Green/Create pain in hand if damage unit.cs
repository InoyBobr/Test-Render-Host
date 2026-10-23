[AbilityId("pain_creator_ability")]
public class Create_pain_in_hand_if_damaged_unit(AbilityState state) : AbilityLogic(state)
{
    public override void OnGain()
    {
        Bus.Subscribe<RoundEnded>(OnRoundEnd, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<RoundEnded>(OnRoundEnd, Owner);
    }

    private void OnRoundEnd(RoundEnded e)
    {
        if(!OnBoardAbilityActive)
            return;
        var ctx = Owner._api.GetContext(Owner.Owner);
        var allies = ctx.GetFriendlyCards(Owner);
        if (!allies.Any(c => c.CurrentHealth < c.BaseHealth))
            return;
        Bus.Publish(new CreateCardRequestEvent("Pain", CardZone.Hand, -1, Owner, true));
    }
}