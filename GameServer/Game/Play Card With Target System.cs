using System.Collections.Generic;
using System.Text;

public sealed class PlayCardResult
{
    public bool Success;
    public string? Error;

    public List<int>? PlayablePositions; // стадия 1
    public TargetRequest? TargetsToPick; // стадия 2

    public Player Player;
}

public sealed class RotateFaceResult
{
    public bool Success;
    public string? Error;
    public Player Player;
}

public sealed class TargetRequest
{
    public Dictionary<int, List<TargetOptionGroup>> AbilityTargets
        = new();

    public bool IsEmpty => AbilityTargets.Count == 0;

    public void Add(int abilityIndex, List<TargetOptionGroup> groups)
    {
        AbilityTargets[abilityIndex] = groups;
    }
    
    public override string ToString()
    {
        var sb = new StringBuilder();

        foreach (var (abilityIndex, groups) in AbilityTargets)
        {
            sb.AppendLine($"Ability {abilityIndex}:");

            foreach (var g in groups)
            {
                sb.AppendLine(
                    $"  Key={g.Key}, Type={g.Type}, Count={g.Count}, Distinct={g.Distinct}, Values=[{string.Join(",", g.ValidValues)}]"
                );
            }
        }

        return sb.ToString();
    }
}


public class GameContext
{
    private readonly Board _board;
    private readonly Player _player;

    // preview-данные
    private readonly CardInstance? _previewCard;
    private readonly int _previewPosition = -1;
    
    public Board Board => _board;
    public Player CurrentPlayer => _player;
    
    public CardInstance? PreviewCard => _previewCard;

    public int? PreviewPosition =>
        _previewCard != null ? _previewPosition : null;

    public GameContext(Board board, Player player)
    {
        _board = board;
        _player = player;
    }

    private GameContext(Board board, Player player,
        CardInstance? previewCard,
        int previewPosition)
    {
        _board = board;
        _player = player;
        _previewCard = previewCard;
        _previewPosition = previewPosition;
    }
    
    public GameContext WithPreviewPlacement(CardInstance? card, int pos)
    {
        return new GameContext(_board, _player, card, pos);
    }
    
    public CardInstance? GetCardAt(int position)
    {
        // если это preview позиция
        if (_previewCard != null && position == _previewPosition)
            return _previewCard;

        return _board.GetCard(position);
    }
    
    public bool IsPositionEmpty(int position)
    {
        if (_previewCard != null && position == _previewPosition)
            return false;

        return _board.IsPositionEmpty(position);
    }
    
    public IEnumerable<CardInstance> GetCurrentPlayerHand()
    {
        foreach (var card in _player.GetHand())
        {
            if (_previewCard != null && card == _previewCard)
                continue;

            yield return card;
        }
    }
    
    public IEnumerable<UnitInstance> GetCardsOnFace(Face face)
    {
        foreach (int index in Board.FaceRotationMaps[face])
        {
            var card = GetCardAt(index);

            if (card is UnitInstance unit)
                yield return unit;
        }
    }

    public IEnumerable<UnitInstance> GetAllCards()
    {
        for (int i = 0; i < 24; i++)
        {
            var card = GetCardAt(i);

            if (card is UnitInstance unit)
                yield return unit;
        }
    }

    public IEnumerable<UnitInstance> GetFriendlyCards(CardInstance card)
    {
        return GetAllCards().Where(c => c.Owner == card.Owner && c != card);
    }
    
    public IEnumerable<UnitInstance> GetFriendlyCardsOnFace(UnitInstance card)
    {
        int? pos = GetCardPosition(card);
        if (pos == null)
            yield break;

        Face f = Board.GetFaceOfSticker(pos.Value);

        foreach (var c in GetCardsOnFace(f))
            if (c.Owner == card.Owner && c != card)
                yield return c;
    }
    
    public IEnumerable<UnitInstance> GetEnemyCards(CardInstance card)
    {
        return GetAllCards().Where(c => c.Owner != card.Owner);
    }
    public IEnumerable<UnitInstance> GetEnemyCardsOnFace(UnitInstance card)
    {
        int? pos = GetCardPosition(card);
        if (pos == null)
            yield break;

        Face f = Board.GetFaceOfSticker(pos.Value);

        foreach (var c in GetCardsOnFace(f))
            if (c.Owner != card.Owner)
                yield return c;
    }
    
    public int? GetCardPosition(CardInstance card)
    {
        if (_previewCard == card)
            return _previewPosition;

        if (card is UnitInstance unit)
            return Board.GetPosition(unit);

        return null;
    }

    public StickerColor GetColor(int pos)
    {
        return Board.GetColor(pos);
    }
}


public sealed class TargetOptionGroup
{
    public string Key { get; init; }
    public TargetType Type{ get; init; }
    public List<int> ValidValues { get; init; } = new();
    public int Count { get; init; } = 1;
    public bool Distinct { get; init; } = true;
}

public enum TargetType
{
    BoardPosition,
    HandIndex,
    Player,
    CubeSide,
    Sticker
}

