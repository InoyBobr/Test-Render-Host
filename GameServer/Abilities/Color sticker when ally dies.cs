namespace GameServer.Abilities;
[AbilityId("color_sticker_green_when_ally_dies")]
public class ColorStickerWhenAllyDies : AbilityLogic
{
    public ColorStickerWhenAllyDies(AbilityState state) : base(state) {}

    private Random _random;
    public override void OnGain()
    {
        _random = new Random();
        Bus.Subscribe<CardKilledEvent>(ChangeColorOnAllyDeath, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardKilledEvent>(ChangeColorOnAllyDeath, Owner);
    }

    private void ChangeColorOnAllyDeath(CardKilledEvent e)
    {
        if (!OnBoardAbilityActive)
            return;
        if (e.Card.Owner != Owner.Owner)
            return;
        
        var ctx = Owner._api.GetContext(Owner.Owner);
        if (ctx == null)
        {
            return;
        }
        var indexes = Enumerable.Range(0, 24).ToArray();
        var emptyNonGreen = indexes.Where(index => ctx.GetCardAt(index) == null && ctx.GetColor(index) != StickerColor.Green).ToList();
        if (emptyNonGreen.Count == 0)
        {
            return;
        }

        var sticker = emptyNonGreen[_random.Next(emptyNonGreen.Count)];
        Bus.Publish(new ChangeColorRequestEvent(sticker, StickerColor.Green, Owner));
    }
}