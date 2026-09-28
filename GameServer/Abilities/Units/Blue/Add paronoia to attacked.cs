namespace GameServer.Abilities;

public class AddParanoiaToAttacked : AbilityLogic
{
    public AddParanoiaToAttacked(AbilityState state) : base(state) {}
    
    public override void OnGain()
    {
        Bus.Subscribe<CardCombatDamagedEvent>(AddParanoia, SubscriberOwnerType.Card, Owner);
        Bus.Subscribe<CardNonCombatDamagedEvent>(AddParanoia, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardCombatDamagedEvent>(AddParanoia, Owner);
        Bus.Unsubscribe<CardNonCombatDamagedEvent>(AddParanoia, Owner);
    }

    private void AddParanoia(CardDamagedEvent e)
    {
        if (!OnBoardAbilityActive)
            return;
        if (e.Source != Owner)
            return;
        Bus.Publish(new AddKeywordRequestEvent(Keyword.Paranoia, e.Unit, Owner));
    }
}