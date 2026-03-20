[AbilityId("enemy_ping_on_play")]
public class DamageEnemyOnPlayTestAbility : AbilityLogic
{
    public DamageEnemyOnPlayTestAbility(AbilityState state) : base(state) {}

    public override void OnGain()
    {
        Bus.Subscribe<CardPlayedEvent>(OnCardPlayed, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(OnCardPlayed, Owner);
    }

    private void OnCardPlayed(CardPlayedEvent e)
    {
        if (!OnBoardAbilityActive)
            return;

        if (e.Card.Owner == Owner.Owner)
            return;

        if (e.Card is not UnitInstance unit)
            return;
        if (!State.IntValues.TryGetValue("damage", out var damage))
            return;
        Bus.Publish(new CardNonCombatDamageRequestEvent(unit, damage, Owner));
    }
}