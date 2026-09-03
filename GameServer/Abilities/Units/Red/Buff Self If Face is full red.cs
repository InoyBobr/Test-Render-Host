namespace GameServer.Abilities;
[AbilityId("buff_self_if_face_is_full_red")]
public class Buff_Self_If_Face_is_full_red : AbilityLogic
{
    public Buff_Self_If_Face_is_full_red(AbilityState state) : base(state) {}
    
    public override void OnGain()
    {
        Bus.Subscribe<CardPlayedEvent>(BuffSelfOnCondition, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(BuffSelfOnCondition, Owner);
    }

    private void BuffSelfOnCondition(CardPlayedEvent e)
    {
        if(!OnBoardAbilityActive)
            return;
        if (e.Card != Owner)
            return;
        if (!(State.IntValues.TryGetValue("powerBuff", out var powerBuff) && State.IntValues.TryGetValue("healthBuff", out var healthBuff)))
            return;
        var ctx = Owner._api.GetContext(Owner.Owner);
        var face = Board.GetFaceOfSticker(Owner.Position);
        foreach (var sticker in Board.FaceRotationMaps[face])
        {
            if(ctx.GetColor(sticker) != StickerColor.Red)
                return;
        }
        Bus.Publish(new CardBuffRequestEvent(OwnerUnit, powerBuff, healthBuff, Owner));
    }
}