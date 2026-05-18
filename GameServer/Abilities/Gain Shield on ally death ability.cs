namespace GameServer.Abilities;
[AbilityId("gain_shield_on_ally_death")]
public class GainShieldOnAllyDeathAbility : AbilityLogic
{
    public GainShieldOnAllyDeathAbility(AbilityState state) : base(state) {}
    
    public override void OnGain()
    {
        Bus.Subscribe<CardKilledEvent>(OnAllyDeath, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardKilledEvent>(OnAllyDeath, Owner);
    }

    private void OnAllyDeath(CardKilledEvent e)
    {
        if(!OnBoardAbilityActive)
            return;
        if(e.Card.Owner != Owner.Owner)
            return;
        Bus.Publish(new AddKeywordRequestEvent(Keyword.Shield, OwnerUnit, Owner));
    }
}