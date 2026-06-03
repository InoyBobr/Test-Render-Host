namespace GameServer.Abilities;
[AbilityId("damage_all_enemies_after_death")]
public class Damage_All_enemies_after_Death : AbilityLogic
{
    public Damage_All_enemies_after_Death(AbilityState state) : base(state) {}
    
    public override void OnGain()
    {
        Bus.Subscribe<CardKilledEvent>(OnDeath, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardKilledEvent>(OnDeath, Owner);
    }

    private void OnDeath(CardKilledEvent e)
    {
        if (!OnBoardAbilityActive)
            return;
        if(e.Card != Owner)
            return;
        if (!State.IntValues.TryGetValue("damage", out var damage))
            return;
        //var selector = new TargetSelector(TargetSide.Enemy, CardZone.Board, FaceConstraint.Any, StatConstraint.Any, TargetPick.All);
        var ctx = Owner._api.GetContext(Owner.Owner);
        var enemies = ctx.GetEnemyCardsOnFace(OwnerUnit);
        foreach (var enemy in enemies)
        {
            Bus.Publish(new CardNonCombatDamageRequestEvent(enemy, damage, Owner));
        }
        //Bus.Publish(new RandomCardDamageRequestEvent(selector, damage, Owner));
    }
}