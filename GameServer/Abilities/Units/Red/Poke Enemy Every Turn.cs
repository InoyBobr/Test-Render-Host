[AbilityId("poke_enemy_every_turn")]
public class Poke_Enemy_Every_Turn : AbilityLogic
{
    public Poke_Enemy_Every_Turn(AbilityState state) : base(state) {}
    
    
    public override void OnGain()
    {
        Bus.Subscribe<PlayerTurnEnded>(DealDamageAfterEndTurn, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<PlayerTurnEnded>(DealDamageAfterEndTurn, Owner);
    }
    
    private void DealDamageAfterEndTurn(PlayerTurnEnded e)
    {
        if (e.Player != Owner.Owner)
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
        var enemies = ctx.GetEnemyCards(Owner);
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