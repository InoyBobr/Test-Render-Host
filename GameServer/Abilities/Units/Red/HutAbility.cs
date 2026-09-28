[AbilityId("hut_ability")]
public class HutAbility(AbilityState state) : AbilityLogic(state)
{
    public override void OnGain()
    {
        Bus.Subscribe<CardPlayedEvent>(OnAllyPlayed, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(OnAllyPlayed, Owner);
    }

    private void OnAllyPlayed(CardPlayedEvent e)
    {
        if(!OnBoardAbilityActive)
            return;
        if(e.Card == Owner || e.Card.Owner != Owner.Owner)
            return;
        var ownerFace = Board.GetFaceOfSticker(Owner.Position);
        var playedCardFace = Board.GetFaceOfSticker(e.Position);
        if(ownerFace != playedCardFace)
            return;
        Bus.Publish(new ChangeRemainingUnitCounterRequestEvent(1, true, false, false, Owner));
    }
}