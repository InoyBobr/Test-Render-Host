namespace GameServer.Abilities.Spells.Green;

public class Damage_ally_to_buff_it(AbilityState state) : AbilityLogic(state)
{
    public override void OnGain()
    {
        Bus.Subscribe<CardPlayedEvent>(DamageOnPlay, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(DamageOnPlay, Owner);
    }

    private void DamageOnPlay(CardPlayedEvent e)
    {
        if (e.Card != Owner)
            return;
        if (!State.CardTargets.TryGetValue("damageTarget", out var targets))
            return;
        if (!State.IntValues.TryGetValue("damage", out var damage)) 
            return;
        if (!State.IntValues.TryGetValue("power", out var power)) 
            return;
        foreach (var target in targets)
        {
            if (target is UnitInstance unit)
            {
                Bus.Publish(new CardNonCombatDamageRequestEvent(unit, damage, Owner));
                Bus.Publish(new CardBuffRequestEvent(unit, power, 0, Owner));
            }
        }
    }
    
    public override List<TargetOptionGroup> GetTargetOptions(GameContext ctx)
    {
        var friends = ctx.GetFriendlyCards(Owner);
        var positions = friends.Select(u => u.Position).ToList();
        TargetOptionGroup target = new TargetOptionGroup
        {
            Key = "damageTarget",
            Count = Math.Min(1, positions.Count),
            Type = TargetType.BoardPosition,
            ValidValues = positions,
            Distinct = true
        };
        return [target];
    }
}