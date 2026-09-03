namespace GameServer.Abilities;

public class Leech_ability : AbilityLogic
{
    public Leech_ability(AbilityState state) : base(state) {}
    
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
        if(!OnBoardAbilityActive)
            return;
        if(e.Card.Owner == Owner.Owner)
            return;
        if(Board.GetFaceOfSticker(e.Position) != Board.GetFaceOfSticker(Owner.Position))
            return;
    }
}