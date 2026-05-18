namespace GameServer.Abilities;
[AbilityId("buff_if_wins_gain_keywords_if_lose")]
public class Buff_if_wins_Gain_keywords_if_lose: AbilityLogic
{
    public Buff_if_wins_Gain_keywords_if_lose(AbilityState state) : base(state) {}
    
    public override void OnGain()
    {
        Bus.Subscribe<CardPlayedEvent>(OnDeploy, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(OnDeploy, Owner);
    }

    private void OnDeploy(CardPlayedEvent e)
    {
        if(!OnBoardAbilityActive)
            return;
        if (e.Card != Owner)
            return;
        var ctx = Owner._api.GetContext(Owner.Owner);
        if (ctx.PlayerScore <= ctx.EnemyScore)
        {
            Bus.Publish(new AddKeywordRequestEvent(Keyword.QuickAttack, OwnerUnit, Owner));
        }
        else
        {
            var delta = ctx.PlayerScore - ctx.EnemyScore;
            Bus.Publish(new CardBuffRequestEvent(OwnerUnit, delta, delta, Owner));
        }
    }
}