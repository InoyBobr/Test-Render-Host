[AbilityId("buff_weakest_ally_after_death")]
public class BuffWeakestAllyAfterDeath : AbilityLogic
{
    public BuffWeakestAllyAfterDeath(AbilityState state) : base(state) {}
    
    public override void OnGain()
    {
        Bus.Subscribe<CardKilledEvent>(OnDeath, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardKilledEvent>(OnDeath, Owner);
    }

    private void OnDeath(CardKilledEvent e)
    {
        if (!OnBoardAbilityActive)
            return;
        if(e.Card != Owner)
            return;
        if (!(State.IntValues.TryGetValue("powerBuff", out var powerBuff) && State.IntValues.TryGetValue("healthBuff", out var healthBuff)))
            return;
        var selector = new TargetSelector(TargetSide.Ally, CardZone.Board, FaceConstraint.Any, StatConstraint.Weakest, TargetPick.Random);
        Bus.Publish(new RandomCardBuffRequestEvent(selector, powerBuff, healthBuff, Owner));
    }
}