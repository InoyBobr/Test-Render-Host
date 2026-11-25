namespace GameServer.Abilities;

public class Draw_Card_On_Destroy(AbilityState state) : AbilityLogic(state)
{
    public override void OnGain()
    {
        Bus.Subscribe<CardKilledEvent>(DrawCardOnDestroy, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardKilledEvent>(DrawCardOnDestroy, Owner);
    }

    private void DrawCardOnDestroy(CardKilledEvent e)
    {
        if (e.Card != Owner || e.Unit.Keywords.Contains(Keyword.Silenced))
            return;
        if (!State.IntValues.TryGetValue("amount", out var amount))
            return;
        Bus.Publish(new CardDrawRequestEvent(Owner.Owner, amount, Owner));
    }
}