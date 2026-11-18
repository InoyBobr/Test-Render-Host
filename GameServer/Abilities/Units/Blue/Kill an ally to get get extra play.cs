namespace GameServer.Abilities;

public class Kill_an_ally_to_get_get_extra_play(AbilityState state) : AbilityLogic(state)
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
        if(!OnBoardAbilityActive)
            return;
        if (e.Card != Owner)
            return;
        if (!State.CardTargets.TryGetValue("alliesTarget", out var alliesTarget))
            return;
        foreach (var target in alliesTarget)
        {
            if (target is UnitInstance unit)
            {
                Bus.Publish(new CardKillRequestEvent(unit, Owner));
                Bus.Publish(new ChangeRemainingUnitCounterRequestEvent(1, false, false, false, Owner));
            }
        }
        
    }
    
    public override List<TargetOptionGroup>? GetTargetOptions(GameContext ctx)
    {
        var allyCount = State.IntValues.GetValueOrDefault("allyCount", 1);
        var allies = ctx.GetFriendlyCards(Owner);
        var alliesInstances = allies as UnitInstance[] ?? allies.ToArray();
        if (alliesInstances.Length < allyCount)
            return null;
        
        var allyPositions = alliesInstances.Select(u => u.Position).ToList();
        
        TargetOptionGroup allytarget = new TargetOptionGroup
        {
            Key = "alliesTarget",
            Count = allyCount,
            Type = TargetType.BoardPosition,
            ValidValues = allyPositions,
            Distinct = true
        };
        
        return new List<TargetOptionGroup> { allytarget };
    }
}