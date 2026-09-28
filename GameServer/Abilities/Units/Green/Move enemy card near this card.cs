namespace GameServer.Abilities;

public class Move_enemy_card_near_this_card(AbilityState state) : AbilityLogic(state)
{
    public override void OnGain()
    {
        Bus.Subscribe<CardPlayedEvent>(MoveCardOnPlay, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardPlayedEvent>(MoveCardOnPlay, Owner);
    }

    private void MoveCardOnPlay(CardPlayedEvent e)
    {
        if (e.Card != Owner)
            return;
        if (!State.CardTargets.TryGetValue("enemyTarget", out var targets))
            return;
        if (!State.IntValues.TryGetValue("positionTarget", out var pos))
            return;
        foreach (var target in targets)
        {
            if (target is UnitInstance unit)
            {
                Bus.Publish(new MoveCardRequestEvent(unit, pos, false, Owner));
            }
        }
    }
    
    public override List<TargetOptionGroup>? GetTargetOptions(GameContext ctx)
    {
        var enemies = ctx.GetEnemyCards(Owner);
        var enemiesPositions = enemies.Select(u => u.Position).ToList();
        if (enemiesPositions.Count == 0)
            return null;
        TargetOptionGroup enemyTarget = new TargetOptionGroup
        {
            Key = "enemyTarget",
            Count = Math.Min(1, enemiesPositions.Count),
            Type = TargetType.BoardPosition,
            ValidValues = enemiesPositions,
            Distinct = true
        };

        var face = Board.GetFaceOfSticker((int)ctx.GetCardPosition(Owner)!);
        var possiblePos = Board.FaceRotationMaps[face].Where(pos => ctx.IsPositionEmpty(pos)).ToList();
        if (possiblePos.Count == 0)
            return null;
        TargetOptionGroup positionTarget = new TargetOptionGroup
        {
            Key = "positionTarget",
            Count = Math.Min(1, possiblePos.Count),
            Type = TargetType.Sticker,
            ValidValues = possiblePos,
            Distinct = true
        };
        return new List<TargetOptionGroup> { enemyTarget, positionTarget };
    }
}