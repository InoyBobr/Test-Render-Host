namespace GameServer.Abilities.Spells.Blue;

public class Keep_alive_strongest_card_on_each_face_of_each_player(AbilityState state) : AbilityLogic(state)
{
    public override void OnGain()
    {
        Bus.Subscribe<CardPlayedEvent>(IncreaseCounter, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(IncreaseCounter, Owner);
    }

    private void IncreaseCounter(CardPlayedEvent e)
    {
        var ctx = Owner._api.GetContext(Owner.Owner);
        foreach (var face in Board.FaceRotationMaps.Keys)
        {
            var enemies = ctx.GetCardsOnFace(face).Where(c => c.Owner != Owner.Owner).ToList();
            var strongestEnemy = enemies.MaxBy(c => (c.CurrentPower, c.CurrentHealth));
            var allies = ctx.GetCardsOnFace(face).Where(c => c.Owner == Owner.Owner).ToList();
            var strongestAlly = allies.MaxBy(c => (c.CurrentPower, c.CurrentHealth));
            var toKill = enemies.Except(new[] { strongestEnemy }).Concat(allies.Except(new[] { strongestAlly }));
            foreach (var unit in toKill)
            {
                Bus.Publish(new CardKillRequestEvent(unit!, Owner));
            }
        }
    }
}