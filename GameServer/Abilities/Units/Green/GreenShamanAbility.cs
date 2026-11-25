[AbilityId("ally_buff_on_play")]
public class AllyBuffOnPlayAbility : AbilityLogic
{
    public AllyBuffOnPlayAbility(AbilityState state) : base(state) {}

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

        if (e.Card.Owner != Owner.Owner)
            return;

        if (e.Card is not UnitInstance unit)
            return;

        if (e.Card == Owner)
            return;
        if (!(State.IntValues.TryGetValue("powerBuff", out var powerBuff) && State.IntValues.TryGetValue("healthBuff", out var healthBuff)))
            return;
        Bus.Publish(new CardBuffRequestEvent(unit, powerBuff, healthBuff, Owner));
    }
}