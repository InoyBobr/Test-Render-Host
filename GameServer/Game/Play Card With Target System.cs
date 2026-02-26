#nullable enable
using System.Collections.Generic;

public sealed class PlayCardResult
{
    public bool Success;
    public string? Error;

    public List<int>? PlayablePositions; // стадия 1
    public TargetRequest? TargetsToPick; // стадия 2
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
}


public class GameContext
{
    private readonly Board _board;
    private readonly Player _player;

    // preview-данные
    private readonly CardInstance? _previewCard;
    private readonly int _previewPosition = -1;

    public GameContext(Board board, Player player)
    {
        _board = board;
        _player = player;
    }

    private GameContext(Board board, Player player,
        CardInstance previewCard,
        int previewPosition)
    {
        _board = board;
        _player = player;
        _previewCard = previewCard;
        _previewPosition = previewPosition;
    }

    public Board Board => _board;
    public Player CurrentPlayer => _player;
    
    public GameContext WithPreviewPlacement(CardInstance card, int pos)
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
}


public sealed class TargetOptionGroup
{
    public string Key;
    public TargetType Type;
    public List<int> ValidValues = new();
    public int Count = 1;
}

public enum TargetType
{
    BoardPosition,
    HandIndex,
    Player,
    DiceSide
}

