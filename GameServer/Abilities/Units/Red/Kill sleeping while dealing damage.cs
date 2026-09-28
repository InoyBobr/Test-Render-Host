namespace GameServer.Abilities;
[AbilityId("kill_sleeping_while_deal_damage")]
public class Kill_sleeping_while_dealing_damage : AbilityLogic
{
    public Kill_sleeping_while_dealing_damage(AbilityState state) : base(state) {}
    
    public override void OnGain()
    {
        Bus.Subscribe<CardCombatDamagedEvent>(KillSleeping, SubscriberOwnerType.Card, Owner);
        Bus.Subscribe<CardNonCombatDamagedEvent>(KillSleeping, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardCombatDamagedEvent>(KillSleeping, Owner);
        Bus.Unsubscribe<CardNonCombatDamagedEvent>(KillSleeping, Owner);
    }

    private void KillSleeping(CardDamagedEvent e)
    {
        if (!OnBoardAbilityActive)
            return;
        if (e.Source != Owner)
            return;
        if (e.Unit.Keywords.Contains(Keyword.Sleeping))
        {
            Bus.Publish(new CardKillRequestEvent(e.Unit, Owner));
        }
    }
}