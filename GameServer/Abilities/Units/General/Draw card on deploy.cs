namespace GameServer.Abilities;
[AbilityId("draw_card_on_deploy")]
public class DrawCardOnDeploy : AbilityLogic
{
    public DrawCardOnDeploy(AbilityState state) : base(state) {}
    
    public override void OnGain()
    {
        Bus.Subscribe<CardPlayedEvent>(DrawCardOnPlay, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(DrawCardOnPlay, Owner);
    }

    private void DrawCardOnPlay(CardPlayedEvent e)
    {
        if (e.Card != Owner)
            return;
        if (!State.IntValues.TryGetValue("amount", out var amount))
            return;
        Bus.Publish(new CardDrawRequestEvent(Owner.Owner, amount, Owner));
    }
}