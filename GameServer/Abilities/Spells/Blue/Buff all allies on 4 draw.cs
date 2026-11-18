namespace GameServer.Abilities.Spells.Blue;

public class Buff_all_allies_on_some_draw(AbilityState state) : AbilityLogic(state)
{
    private bool _isActive = false;
    private int _counter;
    public override void OnGain()
    {
        Bus.Subscribe<CardPlayedEvent>(ActivateOnPlay, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(ActivateOnPlay, Owner);
    }

    private void ActivateOnPlay(CardPlayedEvent e)
    {
        if (e.Card != Owner)
            return;
        _isActive = true;
        _counter = State.IntValues.GetValueOrDefault("count", 4);
    }

    private void OnDraw(CardDrawnEvent e)
    {
        if (e.Player != Owner.Owner || e.Source == null)
            return;
        _counter--;
        if (_counter <= 0)
        {
            _counter = State.IntValues.GetValueOrDefault("count", 4);
            if (!State.IntValues.TryGetValue("power", out var power) || !State.IntValues.TryGetValue("health", out var health))
                return;
            Bus.Publish(new RandomCardBuffRequestEvent(new TargetSelector(TargetSide.Ally, CardZone.Board, FaceConstraint.Any, StatConstraint.Any, TargetPick.All), power, health, Owner));
        }
    }
}