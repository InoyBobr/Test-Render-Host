namespace GameServer.Abilities.Spells.Red;
[AbilityId("rusty_flag_ability")]
public class Buff_power_ability : AbilityLogic
{
    public Buff_power_ability(AbilityState state) : base(state) {}

    public override void OnGain()
    {
        Bus.Subscribe<CardPlayedEvent>(BuffOnPlay, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(BuffOnPlay, Owner);
    }
    
    private void BuffOnPlay(CardPlayedEvent e)
    {
        if (e.Card != Owner)
            return;
        if (!State.CardTargets.TryGetValue("buffTarget", out var targets))
            return;
        if (!State.IntValues.TryGetValue("power", out var power))
            return;
        
        foreach (var target in targets)
        {
            if (target is UnitInstance unit)
            {
                Bus.Publish(new CardBuffRequestEvent(unit, power, 0, Owner));
            }
        }
    }

    public override bool CanBePlayed(GameContext ctx)
    {
        var redFriends = ctx.GetFriendlyCards(Owner).Where(c => c.Color == StickerColor.Red);
        return redFriends.Count() >= 2 ;
    }

    public override List<TargetOptionGroup>? GetTargetOptions(GameContext ctx)
    {
        var friends = ctx.GetFriendlyCards(Owner);
        var positions = friends.Select(u => u.Position).ToList();
        TargetOptionGroup target = new TargetOptionGroup
        {
            Key = "buffTarget",
            Count = Math.Min(1, positions.Count),
            Type = TargetType.BoardPosition,
            ValidValues = positions,
            Distinct = true
        };
        return new List<TargetOptionGroup> { target };
    }
}