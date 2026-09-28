namespace GameServer.Abilities.Spells.Red;
[AbilityId("color_to_red")]
public class Color_sticker_to_red(AbilityState state) : AbilityLogic(state)
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
        Bus.Publish(new ChangeColorRequestEvent(target, StickerColor.Red, Owner));
    }
    
    public override bool CanBePlayed(GameContext ctx)
    {
        var redFriends = ctx.GetFriendlyCards(Owner).Where(c => c.Color == StickerColor.Red);
        var indexes = Enumerable.Range(0, 24).ToArray();
        var emptyNonRed = indexes.Where(index => ctx.GetCardAt(index) == null && ctx.GetColor(index) != StickerColor.Red).ToList();
        return redFriends.Any() && Owner._api.Round > 1 && emptyNonRed.Count != 0;
    }

    public override List<TargetOptionGroup>? GetTargetOptions(GameContext ctx)
    {
        var indexes = Enumerable.Range(0, 24).ToArray();
        var emptyNonRed = indexes.Where(index => ctx.GetCardAt(index) == null && ctx.GetColor(index) != StickerColor.Red).ToList();
        TargetOptionGroup target = new TargetOptionGroup
        {
            Key = "stickerTarget",
            Count = Math.Min(1, emptyNonRed.Count),
            Type = TargetType.Sticker,
            ValidValues = emptyNonRed,
            Distinct = true
        };
        return new List<TargetOptionGroup> { target };
    }
}