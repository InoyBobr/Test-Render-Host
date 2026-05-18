namespace GameServer.Abilities;

public class Kill_allies_to_kill_enemies : AbilityLogic
{
    public Kill_allies_to_kill_enemies(AbilityState state) : base(state) {}
    
    public override void OnGain()
    {
        Bus.Subscribe<CardPlayedEvent>(KillOnPlay, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(KillOnPlay, Owner);
    }

    private void KillOnPlay(CardPlayedEvent e)
    {
        if(!OnBoardAbilityActive)
            return;
        if (e.Card != Owner)
            return;
        if (!State.CardTargets.TryGetValue("alliesTarget", out var alliesTarget) || !State.CardTargets.TryGetValue("enemiesTarget", out var enemiesTarget))
            return;
        foreach (var target in alliesTarget)
        {
            if (target is UnitInstance unit)
            {
                Bus.Publish(new CardKillRequestEvent(unit, Owner));
            }
        }
        foreach (var target in enemiesTarget)
        {
            if (target is UnitInstance unit)
            {
                Bus.Publish(new CardKillRequestEvent(unit, Owner));
            }
        }
        
    }
    
    public override List<TargetOptionGroup>? GetTargetOptions(GameContext ctx)
    {
        var allyCount = State.IntValues.GetValueOrDefault("allyCount", 1);
        var enemyCount = State.IntValues.GetValueOrDefault("enemyCount", 1);
        var allies = ctx.GetFriendlyCards(Owner);
        var alliesInstances = allies as UnitInstance[] ?? allies.ToArray();
        var enemies = ctx.GetEnemyCards(Owner);
        var enemiesInstances = enemies as UnitInstance[] ?? enemies.ToArray();
        if (alliesInstances.Length < allyCount || enemiesInstances.Length == 0)
            return null;
        
        var allyPositions = enemiesInstances.Select(u => u.Position).ToList();
        var enemyPositions = enemiesInstances.Select(u => u.Position).ToList();
        
        TargetOptionGroup allytarget = new TargetOptionGroup
        {
            Key = "alliesTarget",
            Count = allyCount,
            Type = TargetType.BoardPosition,
            ValidValues = allyPositions,
            Distinct = true
        };
        
        TargetOptionGroup enemyTarget = new TargetOptionGroup
        {
            Key = "enemiesTarget",
            Count = Math.Min(enemyCount, enemiesInstances.Length),
            Type = TargetType.BoardPosition,
            ValidValues = enemyPositions,
            Distinct = true
        };

        
        return new List<TargetOptionGroup> { allytarget, enemyTarget };
    }

}