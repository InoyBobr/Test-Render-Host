namespace GameServer.Abilities.Spells.Green;
[AbilityId("no_combat_this_round")]
public class Disable_Combat_Damage_This_Round(AbilityState state) : AbilityLogic(state)
{
    private bool _active = false;
    public override void OnGain()
    {
        Bus.Subscribe<CardPlayedEvent>(ActivateCard, SubscriberOwnerType.Card, Owner);
        Bus.Subscribe<CardCombatDamageRequestEvent>(DisableDamage, SubscriberOwnerType.Card, Owner);
        Bus.Subscribe<RoundEnded>(DisableCard, SubscriberOwnerType.Card, Owner);
        
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(ActivateCard, Owner);
        Bus.Unsubscribe<CardCombatDamageRequestEvent>(DisableDamage, Owner);
        Bus.Unsubscribe<RoundEnded>(DisableCard, Owner);
        
    }

    private void ActivateCard(CardPlayedEvent e)
    {
        if (e.Card != Owner)
            return;
        _active = true;
    }

    private void DisableDamage(CardCombatDamageRequestEvent e)
    {
        if(!_active)
            return;
        e.Allowed = false;
    }

    private void DisableCard(RoundEnded e)
    {
        _active = false;
    }

    public override bool CanBePlayed(GameContext ctx)
    {
        if (Owner.Owner == Owner._api.SecondPlayer)
            return false;
        if (Owner._api.Round < 4)
            return false;
        var greenAllies = ctx.GetFriendlyCards(Owner).Where(c => c.Color == StickerColor.Green);
        if (greenAllies.Count() < 2)
            return false;
        return true;
    }
}