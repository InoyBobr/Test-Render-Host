[AbilityId("orc_king_ability")]
public class OrcKingAbility : AbilityLogic
{
    public OrcKingAbility(AbilityState state) : base(state) {}

    private Random _random;
    public override void OnGain()
    {
        _random = new Random();
        Bus.Subscribe<CardPlayedEvent>(ChangeColorOnAllyRedPlay, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(ChangeColorOnAllyRedPlay, Owner);
    }

    private void ChangeColorOnAllyRedPlay(CardPlayedEvent e)
    {
        if (!OnBoardAbilityActive)
            return;
        if (e.Card.Owner != Owner.Owner || e.Card.Color != StickerColor.Red )
            return;
        
        var ctx = Owner._api.GetContext(Owner.Owner);
        if (ctx == null)
        {
            Console.WriteLine("No context for orc king");
            return;
        }
        var face = Board.GetFaceOfSticker(Owner.Position);
        var indexes = Board.FaceRotationMaps[face];
        var emptyNonRed = indexes.Where(index => ctx.GetCardAt(index) == null && ctx.GetColor(index) != StickerColor.Red).ToList();
        if (emptyNonRed.Count == 0)
        {
            Console.WriteLine("No actual stickers for orc king");
            return;
        }

        var sticker = emptyNonRed[_random.Next(emptyNonRed.Count)];
        Bus.Publish(new ChangeColorRequestEvent(sticker, StickerColor.Red, Owner));
    }
}