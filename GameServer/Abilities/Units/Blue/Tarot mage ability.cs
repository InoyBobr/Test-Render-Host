namespace GameServer.Abilities;

[AbilityId("tarot_mage_ability")]

public class Tarot_mage_ability : AbilityLogic
{
    public Tarot_mage_ability(AbilityState state) : base(state) {}

    private bool active = true;
    public override void OnGain()
    {
        Bus.Subscribe<CardDrawRequestEvent>(IncreaseDraw, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardDrawRequestEvent>(IncreaseDraw, Owner);
    }

    private void IncreaseDraw(CardDrawRequestEvent e)
    {
        if(!OnBoardAbilityActive || !active)
            return;
        if (e.Player != Owner.Owner || e.Source == null)
            return;
        if (!State.IntValues.TryGetValue("draw", out var draw))
            return;
        e.Amount += draw;
    }
}