namespace GameServer.Abilities;

public class Buff_on_card_draw : AbilityLogic
{
    public Buff_on_card_draw(AbilityState state) : base(state) {}
    
    public override void OnGain()
    {
        Bus.Subscribe<CardDrawnEvent>(BuffOnDraw, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardDrawnEvent>(BuffOnDraw, Owner);
    }

    private void BuffOnDraw(CardDrawnEvent e)
    {
        if(!OnBoardAbilityActive)
            return;
        if (e.Player != Owner.Owner)
            return;
        if (!(State.IntValues.TryGetValue("powerBuff", out var powerBuff) && State.IntValues.TryGetValue("healthBuff", out var healthBuff)))
            return;
        Bus.Publish(new CardBuffRequestEvent(OwnerUnit, powerBuff, healthBuff, Owner));    
    }
}