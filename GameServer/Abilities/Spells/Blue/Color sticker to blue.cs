namespace GameServer.Abilities.Spells.Blue;

[AbilityId("color_to_blue")]
public class Color_sticker_to_blue(AbilityState state) : AbilityLogic(state)
{
    public override void OnGain()
    {
        Bus.Subscribe<CardPlayedEvent>(ColorOnPlay, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(ColorOnPlay, Owner);
    }

    private void ColorOnPlay(CardPlayedEvent e)
    {
        if (e.Card != Owner)
            return;
        if(!State.IntValues.TryGetValue("stickerTarget", out var target))
            return;
        Bus.Publish(new ChangeColorRequestEvent(target, StickerColor.Blue, Owner));
    }
    
    public override bool CanBePlayed(GameContext ctx)
    {
        var blueFriends = ctx.GetFriendlyCards(Owner).Where(c => c.Color == StickerColor.Blue);
        var indexes = Enumerable.Range(0, 24).ToArray();
        var emptyNonBlue = indexes.Where(index => ctx.GetCardAt(index) == null && ctx.GetColor(index) != StickerColor.Blue).ToList();
        return blueFriends.Any() && Owner._api.Round > 1 && emptyNonBlue.Count != 0;
    }

    public override List<TargetOptionGroup>? GetTargetOptions(GameContext ctx)
    {
        var indexes = Enumerable.Range(0, 24).ToArray();
        var emptyNonBlue = indexes.Where(index => ctx.GetCardAt(index) == null && ctx.GetColor(index) != StickerColor.Blue).ToList();
        TargetOptionGroup target = new TargetOptionGroup
        {
            Key = "stickerTarget",
            Count = Math.Min(1, emptyNonBlue.Count),
            Type = TargetType.Sticker,
            ValidValues = emptyNonBlue,
            Distinct = true
        };
        return new List<TargetOptionGroup> { target };
    }
}