[AbilityId("ogre_ability")]
public class OgreAbility : AbilityLogic
{
    public OgreAbility(AbilityState state) : base(state) {}

    private bool active;
    
    public override void OnGain()
    {
        Bus.Subscribe<RoundStarted>(OnRoundStart, SubscriberOwnerType.Card, Owner);
        Bus.Subscribe<CardCombatDamagedEvent>(OnCardCombatDamaged, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<RoundStarted>(OnRoundStart, Owner);
        Bus.Unsubscribe<CardCombatDamagedEvent>(OnCardCombatDamaged, Owner);
    }
    
    private void OnRoundStart(RoundStarted e)
    {
        if (!OnBoardAbilityActive)
            return;

        if (!active) return;
        Bus.Publish(new AddKeywordRequestEvent(Keyword.Sleeping, (UnitInstance)Owner, Owner));
        active = false;
    }

    private void OnCardCombatDamaged(CardCombatDamagedEvent e)
    {
        if (!OnBoardAbilityActive)
            return;
        if (e.Source == Owner)
        {
            active = true;
        }
    }
    
}