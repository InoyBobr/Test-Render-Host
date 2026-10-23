namespace GameServer.Abilities;
[AbilityId("moss_owl_ability")]
//Damage_random_enemy_on_ally_damage_ally
public class MossOwlAbility : AbilityLogic
{
    public MossOwlAbility(AbilityState state) : base(state) {}
    
    public override void OnGain()
    {
        Bus.Subscribe<CardNonCombatDamagedEvent>(DealDamageOnAllyDamagedByAlly, SubscriberOwnerType.Card, Owner);
        Bus.Subscribe<CardCombatDamagedEvent>(DealDamageOnAllyDamagedByAlly, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardNonCombatDamagedEvent>(DealDamageOnAllyDamagedByAlly, Owner);
        Bus.Unsubscribe<CardCombatDamagedEvent>(DealDamageOnAllyDamagedByAlly, Owner);
    }

    private void DealDamageOnAllyDamagedByAlly(CardDamagedEvent e)
    {
        if(!OnBoardAbilityActive)
            return;
        if (e.Card.Owner != Owner.Owner || e.Source.Owner != Owner.Owner)
            return;
        if (!State.IntValues.TryGetValue("damage", out var damage))
            return;
        var targets = new TargetSelector(TargetSide.Enemy, CardZone.Board, FaceConstraint.Any,
            StatConstraint.LeastHealth, TargetPick.Random);
        Bus.Publish(new RandomCardDamageRequestEvent(targets, damage, Owner));
    }
}