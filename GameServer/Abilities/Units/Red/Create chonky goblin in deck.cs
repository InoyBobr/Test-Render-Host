namespace GameServer.Abilities;

public class CreateChonkyGoblinInDeck(AbilityState state) : AbilityLogic(state)
{
    public override void OnGain()
    {
        Bus.Subscribe<CardPlayedEvent>(OnCardNonCombatDamaged, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(OnCardNonCombatDamaged, Owner);
    }

    private void OnCardNonCombatDamaged(CardPlayedEvent e)
    {
        if(e.Card != Owner)
            return;
        Bus.Publish(new CreateCardRequestEvent("Chonky Goblin", CardZone.Deck, null, Owner, true));
    }
}
