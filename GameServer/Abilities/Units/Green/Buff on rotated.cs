namespace GameServer.Abilities;

public class Buff_on_rotated(AbilityState state) : AbilityLogic(state)
{
    public override void OnGain()
    {
        Bus.Subscribe<FaceRotatedEvent>(BuffWhenRotated, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<FaceRotatedEvent>(BuffWhenRotated, Owner);
    }

    private void BuffWhenRotated(FaceRotatedEvent e)
    {
        if (!OnBoardAbilityActive)
            return;
        if (e.Face != Board.GetFaceOfSticker(Owner.Position))
            return;
        if (!State.IntValues.TryGetValue("power", out var power) || !State.IntValues.TryGetValue("health", out var health))
            return;
        Bus.Publish(new CardBuffRequestEvent(OwnerUnit, power, health, Owner));
    }
}