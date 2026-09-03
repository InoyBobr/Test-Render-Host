[AbilityId("vulture_ability")]
public class VultureAbility : AbilityLogic
{
    public VultureAbility(AbilityState state) : base(state) {}
    
    public override void OnGain()
    {
        Bus.Subscribe<CardKilledEvent>(OnEnemyCardDies, SubscriberOwnerType.Card, Owner);
        Bus.Subscribe<RoundStarted>(FlyOnRoundStart, SubscriberOwnerType.Card, Owner);
        
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardKilledEvent>(OnEnemyCardDies, Owner);
        Bus.Unsubscribe<RoundStarted>(FlyOnRoundStart, Owner);
    }

    private void OnEnemyCardDies(CardKilledEvent e)
    {
        if(!OnBoardAbilityActive)
            return;
        if (!State.IntValues.TryGetValue("powerBuff", out var powerBuff))
            return;
        if (e.Card.Owner == Owner.Owner)
            return;
        Bus.Publish(new RemoveKeywordRequestEvent(Keyword.Flying, (UnitInstance)Owner, Owner));
        Bus.Publish(new CardBuffRequestEvent((UnitInstance)Owner, powerBuff, 0, Owner));
    }

    private void FlyOnRoundStart(RoundStarted e)
    {
        if(!OnBoardAbilityActive)
            return;
        Bus.Publish(new AddKeywordRequestEvent(Keyword.Flying, (UnitInstance)Owner, Owner));
    }
}