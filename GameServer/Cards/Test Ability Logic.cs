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
        
    }
    

    private void PrintTextOnGetDamage(CardCombatDamagedEvent e)
    {
        if (e.Card == Owner)
        {
            Console.WriteLine("I get damage");
        }
    }

    private void TakeDamageOnDeploy(CardPlayedEvent e)
    {
        if (!OnBoardAbilityActive)
            return;
        if (e.Card != Owner)
            return;
        State.IntValues.TryGetValue("damage", out var damage);
        if (damage > 0 && OwnerUnit is not null)
           Bus.Publish(new CardNonCombatDamageRequestEvent(OwnerUnit, damage, Owner));
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
