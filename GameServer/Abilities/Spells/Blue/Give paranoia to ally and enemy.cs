namespace GameServer.Abilities.Spells.Blue;

public class Give_paranoia_to_ally_and_enemy(AbilityState state) : AbilityLogic(state)
{
    public override void OnGain()
    {
        Bus.Subscribe<CardPlayedEvent>(GiveParanoia, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(GiveParanoia, Owner);
    }

    private void GiveParanoia(CardPlayedEvent e)
    {
        if(e.Card != Owner)
            return;
        if (!State.CardTargets.TryGetValue("alliesTarget", out var alliesTarget) || !State.CardTargets.TryGetValue("enemiesTarget", out var enemiesTarget))
            return;
        foreach (var target in alliesTarget)
        {
            if (target is UnitInstance unit)
            {
                Bus.Publish(new AddKeywordRequestEvent(Keyword.Paranoia, unit, Owner));
            }
        }
        foreach (var target in enemiesTarget)
        {
            if (target is UnitInstance unit)
            {
                Bus.Publish(new AddKeywordRequestEvent(Keyword.Paranoia, unit, Owner));
            }
        }
    }

    public override List<TargetOptionGroup>? GetTargetOptions(GameContext ctx)
    {
        var allyCount = State.IntValues.GetValueOrDefault("allyCount", 1);
        var enemyCount = State.IntValues.GetValueOrDefault("enemyCount", 1);
        var nonParanoiaAllies = ctx.GetFriendlyCards(Owner).Where(x => !x.Keywords.Contains(Keyword.Paranoia));
        var nonParanoiaEnemies = ctx.GetEnemyCards(Owner).Where(x => !x.Keywords.Contains(Keyword.Paranoia));
        
        var allyPositions = nonParanoiaAllies.Select(u => u.Position).ToList();
        var nonParanoiaEnemiesArr = nonParanoiaEnemies as UnitInstance[] ?? nonParanoiaEnemies.ToArray();
        var enemyPositions = nonParanoiaEnemiesArr.Select(u => u.Position).ToList();
        
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
            Count = Math.Min(enemyCount, nonParanoiaEnemiesArr.Length),
            Type = TargetType.BoardPosition,
            ValidValues = enemyPositions,
            Distinct = true
        };

        
        return new List<TargetOptionGroup> { allytarget, enemyTarget };
    }
    public override bool CanBePlayed(GameContext ctx)
    {
        var nonParanoiaAllies = ctx.GetFriendlyCards(Owner).Where(x => !x.Keywords.Contains(Keyword.Paranoia));
        var nonParanoiaEnemies = ctx.GetEnemyCards(Owner).Where(x => !x.Keywords.Contains(Keyword.Paranoia));
        return nonParanoiaAllies.Any() && nonParanoiaEnemies.Any();
    }
}