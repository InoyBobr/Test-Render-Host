[AbilityId("test_ability")]
public class TestAbilityLogic : AbilityLogic
{
    public TestAbilityLogic(AbilityState state) : base(state)
    {
    }
    public override void OnGain()
    {   
        Bus.Subscribe<CardPlayedEvent>(DealDamageOnDeploy, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(DealDamageOnDeploy, Owner);
    }
    
    
    private void DealDamageOnDeploy(CardPlayedEvent e)
    {
        if (e.Card != Owner)
            return;
        if (!State.CardTargets.TryGetValue("damageTarget", out var targets))
            return;
        if (!State.IntValues.TryGetValue("damage", out var damage))
            return;
        foreach (var target in targets)
        {
            if (target is UnitInstance unit)
            {
                Bus.Publish(new CardNonCombatDamageRequestEvent(unit, damage, Owner));
            }
        }
    }

    public override List<TargetOptionGroup>? GetTargetOptions(GameContext ctx)
    {
        var enemies = ctx.GetEnemyCardsOnFace(OwnerUnit);
        var positions = enemies.Select(u => u.Position).ToList();
        TargetOptionGroup target = new TargetOptionGroup
        {
            Key = "damageTarget",
            Count = Math.Min(1, positions.Count),
            Type = TargetType.BoardPosition,
            ValidValues = positions,
            Distinct = true
        };
        return new List<TargetOptionGroup> { target };
    }
}
