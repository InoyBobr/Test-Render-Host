namespace GameServer.Abilities;
[AbilityId("damage_all_enemies_on_face_after_death")]
public class DamageAllEnemiesOnFaceAfterDeath : AbilityLogic
{
    public DamageAllEnemiesOnFaceAfterDeath(AbilityState state) : base(state) {}
    
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
        var selector = new TargetSelector(TargetSide.Enemy, CardZone.Board, FaceConstraint.SameFace, StatConstraint.Any, TargetPick.All);
        Bus.Publish(new RandomCardDamageRequestEvent(selector, damage, Owner));
    }
}