namespace GameServer.Abilities.Spells.Red;

public class SetPowerToZero(AbilityState state) : AbilityLogic(state)
{
    public override void OnGain()
    {
        Bus.Subscribe<CardPlayedEvent>(DealDamageOnPlay, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(DealDamageOnPlay, Owner);
    }

    private void DealDamageOnPlay(CardPlayedEvent e)
    {
        if (e.Card != Owner)
            return;
        if (!State.CardTargets.TryGetValue("enemyTarget", out var targets))
            return;
        foreach (var target in targets)
        {
            if (target is UnitInstance unit)
            {
                Bus.Publish(new SetStatsRequestEvent(unit, 0, null, Owner, SetStatsType.Debuff));
            }
        }
    }
    
    public override List<TargetOptionGroup>? GetTargetOptions(GameContext ctx)
    {
        var enemies = ctx.GetEnemyCards(Owner).Where(c => c.CurrentPower <= 2);
        var positions = enemies.Select(u => u.Position).ToList();
        TargetOptionGroup target = new TargetOptionGroup
        {
            Key = "enemyTarget",
            Count = Math.Min(1, positions.Count),
            Type = TargetType.BoardPosition,
            ValidValues = positions,
            Distinct = true
        };
        return new List<TargetOptionGroup> { target };
    }
}