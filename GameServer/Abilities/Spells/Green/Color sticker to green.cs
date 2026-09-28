namespace GameServer.Abilities.Spells.Green;

[AbilityId("color_to_green")]
public class Color_sticker_to_green(AbilityState state) : AbilityLogic(state)
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
        Bus.Publish(new ChangeColorRequestEvent(target, StickerColor.Green, Owner));
    }
    
    public override bool CanBePlayed(GameContext ctx)
    {
        var greenFriends = ctx.GetFriendlyCards(Owner).Where(c => c.Color == StickerColor.Green);
        var indexes = Enumerable.Range(0, 24).ToArray();
        var emptyNonGreen = indexes.Where(index => ctx.GetCardAt(index) == null && ctx.GetColor(index) != StickerColor.Green).ToList();
        return greenFriends.Any() && Owner._api.Round > 1 && emptyNonGreen.Count != 0;
    }

    public override List<TargetOptionGroup>? GetTargetOptions(GameContext ctx)
    {
        var indexes = Enumerable.Range(0, 24).ToArray();
        var emptyNonGreen = indexes.Where(index => ctx.GetCardAt(index) == null && ctx.GetColor(index) != StickerColor.Green).ToList();
        TargetOptionGroup target = new TargetOptionGroup
        {
            Key = "stickerTarget",
            Count = Math.Min(1, emptyNonGreen.Count),
            Type = TargetType.Sticker,
            ValidValues = emptyNonGreen,
            Distinct = true
        };
        return new List<TargetOptionGroup> { target };
    }
};