namespace GameServer.Abilities.Spells.Blue;

public class Deal1DmgMultipleTimes(AbilityState state) : AbilityLogic(state)
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
        if (!State.CardTargets.TryGetValue("damageTarget", out var targets))
            return;
        if (!State.IntValues.TryGetValue("count", out var count))
            return;
        foreach (var target in targets)
        {
            if (target is UnitInstance unit)
            {
                for (int i = 0; i < count; i++)
                {
                    Bus.Publish(new CardNonCombatDamageRequestEvent(unit, 1, Owner));
                }
                
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