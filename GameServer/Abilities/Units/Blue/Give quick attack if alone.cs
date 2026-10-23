namespace GameServer.Abilities;

public class Give_quick_attack_if_alone(AbilityState state) : AbilityLogic(state)
{
    public override void OnGain()
    {
        Bus.Subscribe<PreBattlePhaseStarted>(AddKeywordBeforeBattleIfAlone, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<PreBattlePhaseStarted>(AddKeywordBeforeBattleIfAlone, Owner);
    }

    private void AddKeywordBeforeBattleIfAlone(PreBattlePhaseStarted e)
    {
        if (!OnBoardAbilityActive)
            return;
        var ctx = Owner._api.GetContext(Owner.Owner);
        var allies = ctx.GetFriendlyCardsOnFace(OwnerUnit);
        if(allies.Any())
            return;
        if (!OwnerUnit.Keywords.Contains(Keyword.QuickAttack))
            Bus.Publish(new AddKeywordRequestEvent(Keyword.QuickAttack, OwnerUnit, Owner));
        else
        {
            Bus.Publish(new AddKeywordRequestEvent(Keyword.DoubleAttack, OwnerUnit, Owner));
        }
    }
}