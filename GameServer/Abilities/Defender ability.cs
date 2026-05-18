namespace GameServer.Abilities;
[AbilityId("defender")]
public class Defender_ability : AbilityLogic
{
    public Defender_ability(AbilityState state) : base(state) {}
    
    public override void OnGain()
    {
        Bus.Subscribe<CardCombatDamageRequestEvent>(Defend, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardCombatDamageRequestEvent>(Defend, Owner);
    }

    private void Defend(CardCombatDamageRequestEvent e)
    {
        if (!OnBoardAbilityActive)
            return;
        if (e.Card.Owner != Owner.Owner || e.Card.Definition.CardId == Owner.Definition.CardId)
            return;
        if (Board.GetFaceOfSticker(e.Unit.Position) != Board.GetFaceOfSticker(OwnerUnit.Position))
            return;
        e.Allowed = false;
    }
}