namespace GameServer.Abilities.Spells.Green;
[AbilityId("thorns_ability")]
public class Thorns_ability(AbilityState state) : AbilityLogic(state)
{
    public override void OnGain()
    {
        Bus.Subscribe<CardPlayedEvent>(DamageAll, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(DamageAll, Owner);
    }

    private void DamageAll(CardPlayedEvent e)
    {
        if(e.Card != Owner)
            return;
        if(!State.IntValues.TryGetValue("damage", out var damage))
            return;
        var selector = new TargetSelector(TargetSide.Any, CardZone.Board, FaceConstraint.Any, StatConstraint.Any, TargetPick.All);
        Bus.Publish(new RandomCardDamageRequestEvent(selector, damage, Owner));
    }

    public override bool CanBePlayed(GameContext ctx)
    {
        var history = Owner._api.History;
        var selfDamage = history.Where(x => x is CardNonCombatDamagedEvent e &&
                                            e.Card.Owner == Owner.Owner && e.Source?.Owner == Owner.Owner);
        var amount = State.IntValues.GetValueOrDefault("amount", 1);
        return selfDamage.Count() >= amount;
    }
}
