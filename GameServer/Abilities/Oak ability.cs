[AbilityId("oak_ability")]
public class OakAbility : AbilityLogic
{
    public OakAbility(AbilityState state) : base(state) {}

    private bool active;
    
    public override void OnGain()
    {
        Bus.Subscribe<PostBattlePhaseStarted>(OnBattleEnd, SubscriberOwnerType.Card, Owner);
        Bus.Subscribe<CardCombatDamagedEvent>(OnCardCombatDamaged, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<PostBattlePhaseStarted>(OnBattleEnd, Owner);
        Bus.Unsubscribe<CardCombatDamagedEvent>(OnCardCombatDamaged, Owner);
    }
    
    private void OnBattleEnd(PostBattlePhaseStarted e)
    {
        if (!OnBoardAbilityActive)
            return;

        if (!active) return;
        if (!(State.IntValues.TryGetValue("powerBuff", out var powerBuff) && State.IntValues.TryGetValue("healthBuff", out var healthBuff)))
            return;
        Bus.Publish(new CardBuffRequestEvent((UnitInstance)Owner,powerBuff,healthBuff, Owner));
        active = false;
    }

    private void OnCardCombatDamaged(CardCombatDamagedEvent e)
    {
        if (!OnBoardAbilityActive)
            return;
        if (e.Card == Owner || e.Source == Owner)
        {
            active = true;
        }
    }
    
}