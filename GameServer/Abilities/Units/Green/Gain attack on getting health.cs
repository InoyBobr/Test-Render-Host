namespace GameServer.Abilities;

public class Gain_attack_on_getting_health: AbilityLogic
{
    public Gain_attack_on_getting_health(AbilityState state) : base(state) {}
    
    public override void OnGain()
    {
        Bus.Subscribe<CardBuffedEvent>(OnBuffed, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardBuffedEvent>(OnBuffed, Owner);
    }

    private void OnBuffed(CardBuffedEvent e)
    {
        if(!OnBoardAbilityActive)
            return;
        if (e.Card != Owner || e.HealthDelta <= 0 || e.Source == Owner)
            return;
        Bus.Publish(new CardBuffRequestEvent(OwnerUnit, e.HealthDelta, 0, Owner));
    }
}