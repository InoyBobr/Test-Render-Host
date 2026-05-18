namespace GameServer.Abilities;

public class Silence_on_play_ability : AbilityLogic
{
    public Silence_on_play_ability(AbilityState state) : base(state) {}
    
    
    public override void OnGain()
    {
        Bus.Subscribe<CardPlayedEvent>(SilenceOnPlay, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(SilenceOnPlay, Owner);
    }

    private void SilenceOnPlay(CardPlayedEvent e)
    {
        if (e.Card != Owner)
            return;
        if (!State.CardTargets.TryGetValue("silenceTarget", out var targets))
            return;
        foreach (var target in targets)
        {
            if (target is UnitInstance unit)
            {
                Bus.Publish(new AddKeywordRequestEvent(Keyword.Silenced, unit, Owner));
            }
        }
    }
    
    public override List<TargetOptionGroup> GetTargetOptions(GameContext ctx)
    {
        var enemies = ctx.GetEnemyCards(Owner);
        var positions = enemies.Select(u => u.Position).ToList();
        TargetOptionGroup target = new TargetOptionGroup
        {
            Key = "silenceTarget",
            Count = Math.Min(1, positions.Count),
            Type = TargetType.BoardPosition,
            ValidValues = positions,
            Distinct = true
        };
        return new List<TargetOptionGroup> { target };
    }
}