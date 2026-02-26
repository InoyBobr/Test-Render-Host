[AbilityId("test_ability")]
public class TestAbilityLogic : AbilityLogic
{
    public TestAbilityLogic(AbilityState state) : base(state)
    {
    }
    public override void OnGain()
    {
        Bus.Subscribe<CardPlayedEvent>(TakeDamageOnDeploy, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(TakeDamageOnDeploy, Owner);
    }

    private void TakeDamageOnDeploy(CardPlayedEvent e)
    {
        if (!OnBoardAbilityActive)
            return;
        if (e.Card.Owner != Owner.Owner || e.Card == Owner)
        {
            State.IntValues.TryGetValue("damage", out var damage);
            if (damage > 0)
                Bus.Publish(new CardNonCombatDamageRequestEvent(Owner, damage, Owner));
        }
        
    }
}
