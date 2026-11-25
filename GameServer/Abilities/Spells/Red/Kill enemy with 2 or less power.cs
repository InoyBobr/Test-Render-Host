namespace GameServer.Abilities.Spells.Red;

public class Kill_enemy_with_2_or_less_power(AbilityState state) : AbilityLogic(state)
{
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
        if (e.Card != Owner)
            return;
        if (!State.CardTargets.TryGetValue("killTarget", out var targets))
            return;
        if (!State.IntValues.TryGetValue("count", out var count))
            return;
        foreach (var target in targets)
        {
            if (target is UnitInstance unit)
            {
                Bus.Publish(new CardKillRequestEvent(unit, Owner));
            }
        }
    }
    
    public override List<TargetOptionGroup>? GetTargetOptions(GameContext ctx)
    {
        var enemies = ctx.GetEnemyCards(Owner).Where(c => c.CurrentPower <= 2);
        var positions = enemies.Select(u => u.Position).ToList();
        TargetOptionGroup target = new TargetOptionGroup
        {
            Key = "killTarget",
            Count = Math.Min(1, positions.Count),
            Type = TargetType.BoardPosition,
            ValidValues = positions,
            Distinct = true
        };
        return new List<TargetOptionGroup> { target };
    }
}