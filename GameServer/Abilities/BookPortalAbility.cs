[AbilityId("book_portal_ability")]
public class BookPortalAbility : AbilityLogic
{
    public BookPortalAbility(AbilityState state) : base(state) {}

    public override void OnGain()
    {
        Bus.Subscribe<CardPlayedEvent>(DrawCardsOnPlay, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(DrawCardsOnPlay, Owner);
    }

    private void DrawCardsOnPlay(CardPlayedEvent e)
    {
        if (e.Card != Owner)
            return;
        if (!State.IntValues.TryGetValue("amount", out var amount))
            return;
        Bus.Publish(new CardDrawRequestEvent(Owner.Owner, amount));
    }

    public override bool CanBePlayed(GameContext ctx)
    {
        var faces = 0;
        foreach (var kvp in Board.FaceRotationMaps)
        {
            foreach (var value in kvp.Value)
            {
                if (ctx.GetColor(value) == StickerColor.Blue)
                {
                    faces++;
                    break;
                }
            }
        }

        return faces >= 4;
    }
    
}