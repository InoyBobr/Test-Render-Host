[AbilityId("saplings_ability")]
public class SaplingsAbility : AbilityLogic
{
    public SaplingsAbility(AbilityState state) : base(state) {}

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
        var ctx = Owner._api.GetContext(Owner.Owner);
        var greenAllies = ctx.GetFriendlyCards(Owner).Count(c => c.Color == StickerColor.Green);
        foreach (var target in targets)
        {
            if (target is UnitInstance unit)
            {
                Bus.Publish(new CardBuffRequestEvent(unit, greenAllies, greenAllies, Owner));
            }
        }
    }

    public override bool CanBePlayed(GameContext ctx)
    {
        var greenFriends = ctx.GetFriendlyCards(Owner).Where(c => c.Color == StickerColor.Green);
        return greenFriends.Count() >=2 ;
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