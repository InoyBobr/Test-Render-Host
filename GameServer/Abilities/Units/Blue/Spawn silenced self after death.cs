namespace GameServer.Abilities;
[AbilityId("spawn_silenced_self_after_death")]
public class Spawn_silenced_self_after_death: AbilityLogic
{
    public Spawn_silenced_self_after_death(AbilityState state) : base(state) {}
    
    public override void OnGain()
    {
        Bus.Subscribe<CardKilledEvent>(OnDeath, SubscriberOwnerType.Card, Owner);
        Bus.Subscribe<CardCreatedEvent>(SilenceCreatedCard, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardKilledEvent>(OnDeath, Owner);
        Bus.Unsubscribe<CardCreatedEvent>(SilenceCreatedCard, SubscriberOwnerType.Card);
    }

    private void OnDeath(CardKilledEvent e)
    {
        if (!OnBoardAbilityActive)
            return;
        if(e.Card != Owner)
            return;
        Bus.Publish(new CreateCardRequestEvent(Owner.Definition.CardId, CardZone.Board, Owner.Position, Owner, true));
    }

    private void SilenceCreatedCard(CardCreatedEvent e)
    {
        if(e.Source != Owner)
            return;
        if(e.Card is not UnitInstance unit)
            return;
        Bus.Publish(new AddKeywordRequestEvent(Keyword.Silenced, unit, Owner));
    }
}