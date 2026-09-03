using System.Collections.Generic;
using System.Collections.Immutable;

public interface IGameEvent { }

public abstract class CardEvent : IGameEvent
{
    public CardInstance Card;
}

public abstract class UnitEvent : CardEvent
{
    public UnitInstance Unit => (UnitInstance)Card;
}

public class CardPlayedEvent : CardEvent
{
    public readonly int Position;

    public CardPlayedEvent(CardInstance card, int position)
    {
        Card = card;
        Position = position;
    }
}

public class CardPlayRequestEvent : CardEvent
{
    public int Position;
    public bool Allowed = true;

    public CardPlayRequestEvent(CardInstance card, int position)
    {
        Card = card;
        Position = position;
    }
}


//--- Изменения карт на столе ---

public class CardKilledEvent : UnitEvent
{
    public readonly CardInstance? Source;
    public readonly int? SourcePos;

    public CardKilledEvent(UnitInstance card, CardInstance? source = null)
    {
        Card = card;
        Source = source;
        SourcePos = Source?.Position;
    }
}

public class CardKillRequestEvent : UnitEvent
{
    public CardInstance? Source;
    public bool Allowed = true;

    public CardKillRequestEvent(UnitInstance card, CardInstance? source = null)
    {
        Card = card;
        Source = source;
    }
}

public abstract class CardDamagedEvent : UnitEvent
{
    public readonly int Damage;
    public readonly CardInstance? Source;
    public readonly int? SourcePos;
    protected CardDamagedEvent(UnitInstance card, int damage, CardInstance? source = null)
    {
        Card = card;
        Damage = damage;
        Source = source;
        SourcePos = Source?.Position;
    }
}

public class CardCombatDamagedEvent : CardDamagedEvent
{
    public CardCombatDamagedEvent(UnitInstance card, int damage, CardInstance? source) : base(card, damage, source)
    {
    }
}

public class CardNonCombatDamagedEvent : CardDamagedEvent
{
    public CardNonCombatDamagedEvent(UnitInstance card, int damage, CardInstance? source) : base(card, damage, source)
    {
    }
}

public abstract class CardDamageRequestEvent : UnitEvent
{
    public int Damage;
    public CardInstance? Source;
    public bool Allowed = true;
    
    public CardDamageRequestEvent(UnitInstance card, int damage, CardInstance? source = null)
    {
        Card = card;
        Damage = damage;
        Source = source;
    }
}

public class CardCombatDamageRequestEvent : CardDamageRequestEvent
{
    public CardCombatDamageRequestEvent(UnitInstance card, int damage, CardInstance? source) : base(card, damage, source)
    {
    }
}

public class CardNonCombatDamageRequestEvent : CardDamageRequestEvent
{
    public CardNonCombatDamageRequestEvent(UnitInstance card, int damage, CardInstance? source) : base(card, damage, source)
    {
    }
}

public class RandomCardDamageRequestEvent(TargetSelector selector, int damage, CardInstance source) : IGameEvent
{
    public TargetSelector Selector = selector;
    public int Damage = damage;
    public CardInstance Source = source;
    public bool Allowed = true;
    public readonly int SourcePos = source.Position;
    public readonly CardZone SourceZone = source.Zone;
}

public class ShieldBrokenEvent : UnitEvent
{
    public ShieldBrokenEvent(UnitInstance unit)
    {
        Card = unit;
    }
}

public class CardBuffedEvent : UnitEvent
{
    public readonly int PowerDelta;
    public readonly int HealthDelta;
    public readonly CardInstance? Source;
    public readonly int? SourcePos;

    public CardBuffedEvent(UnitInstance card, int power, int health, CardInstance? source = null)
    {
        Card = card;
        PowerDelta = power;
        HealthDelta = health;
        Source = source;
        SourcePos = Source?.Position;
    }
}

public class CardBuffRequestEvent : UnitEvent
{
    public int PowerDelta;
    public int HealthDelta;
    public CardInstance? Source;
    public bool Allowed = true;

    public CardBuffRequestEvent(UnitInstance card, int power, int health, CardInstance? source = null)
    {
        Card = card;
        PowerDelta = power;
        HealthDelta = health;
        Source = source;
    }
}

public class RandomCardBuffRequestEvent(TargetSelector selector, int powerDelta, int healthDelta, CardInstance source)
    : IGameEvent
{
    public bool Allowed = true;
    public TargetSelector Selector = selector;
    public int PowerDelta = powerDelta;
    public int HealthDelta = healthDelta;
    public CardInstance Source = source;
    public readonly int SourcePos = source.Position;
    public readonly CardZone SourceZone = source.Zone;
}

public class AddKeywordRequestEvent : UnitEvent
{
    public Keyword Keyword;
    public CardInstance? Source;
    public bool Allowed = true;

    public AddKeywordRequestEvent(Keyword keyword, UnitInstance card, CardInstance? source)
    {
        Keyword = keyword;
        Card = card;
        Source = source;
    }
}

public class KeywordAddedEvent : UnitEvent
{
    public readonly Keyword Keyword;
    public readonly CardInstance? Source;

    public KeywordAddedEvent(Keyword keyword, UnitInstance card, CardInstance? source)
    {
        Keyword = keyword;
        Card = card;
        Source = source;
    }
}

public class RemoveKeywordRequestEvent : UnitEvent
{
    public Keyword Keyword;
    public CardInstance? Source;
    public bool Allowed = true;

    public RemoveKeywordRequestEvent(Keyword keyword, UnitInstance card, CardInstance? source)
    {
        Keyword = keyword;
        Card = card;
        Source = source;
    }
}

public class RemoveAllKeywordsRequestEvent : UnitEvent
{
    public bool Allowed = true;
    public CardInstance Source;

    public RemoveAllKeywordsRequestEvent(UnitInstance unit, CardInstance source)
    {
        Card = unit;
        Source = source;
    }
}

public class KeywordRemovedEvent : UnitEvent
{
    public readonly Keyword Keyword;
    public readonly CardInstance? Source;

    public KeywordRemovedEvent(Keyword keyword, UnitInstance card, CardInstance? source)
    {
        Keyword = keyword;
        Card = card;
        Source = source;
    }
}

public class KeywordsRemovedEvent : UnitEvent
{
    public readonly ImmutableHashSet<Keyword> Keywords;
    public readonly CardInstance Source;

    public KeywordsRemovedEvent(List<Keyword> keywords, UnitInstance unit, CardInstance source)
    {
        Keywords = keywords.ToImmutableHashSet();
        Card = unit;
        Source = source;
    }
    
    public KeywordsRemovedEvent(HashSet<Keyword> keywords, UnitInstance unit, CardInstance source)
    {
        Keywords = keywords.ToImmutableHashSet();
        Card = unit;
        Source = source;
    }
}

public class AddAbilityRequestEvent : CardEvent
{
    public bool Allowed = true;
    public string AbilityId;
    public List<AbilityParameter> Parameters;

    public AddAbilityRequestEvent(CardInstance card ,string abilityId, List<AbilityParameter> parameters)
    {
        Card = card;
        AbilityId = abilityId;
        Parameters = parameters;
    }
}

public class MoveCardRequestEvent : UnitEvent
{
    public bool Allowed = true;
    public int Position;
    public bool SwapAllowed;
    public CardInstance? Source;

    public MoveCardRequestEvent(UnitInstance unit, int position, bool swapAllowed = false, CardInstance? source = null)
    {
        Card = unit;
        Position = position;
        SwapAllowed = swapAllowed;
        Source = source;
    }
}

public class CardMovedEvent : UnitEvent
{
    public readonly int From;
    public readonly int To;
    public readonly UnitInstance? AnotherUnit;
    public readonly CardInstance? Source;

    public CardMovedEvent(UnitInstance unit, int from, int to, UnitInstance? anotherUnit = null, CardInstance? source = null)
    {
        Card = unit;
        From = from;
        To = to;
        AnotherUnit = anotherUnit;
        Source = source;
    }
}

public class CreateCardRequestEvent : IGameEvent
{
    public bool Allowed = true;
    public string CardID;
    public CardZone Zone;
    public int? Position;
    public CardInstance Source;
    public bool SameOwner;

    public CreateCardRequestEvent(string cardId, CardZone zone, int? position, CardInstance source, bool sameOwner)
    {
        CardID = cardId;
        Zone = zone;
        Position = position;
        Source = source;
        SameOwner = sameOwner;
    }
}

public class CardCreatedEvent : CardEvent
{
    private CardInstance Source;
    public CardCreatedEvent(CardInstance card, CardInstance source)
    {
        Card = card;
        Source = source;
    }
}
//--- Изменение кубика ---
public class FaceRotatedEvent : IGameEvent
{
    public readonly Face Face;
    public readonly int AmountOfRotations;
    public readonly Player Player;


    public FaceRotatedEvent(Face face, int amountOfRotations, Player player)
    {
        Face = face;
        AmountOfRotations = amountOfRotations;
        Player = player;
    }
}

public class ChangeColorRequestEvent : IGameEvent
{
    public int Position;
    public StickerColor Color;
    public CardInstance Source;
    public bool Allowed = true;

    public ChangeColorRequestEvent(int position, StickerColor color, CardInstance source)
    {
        Position = position;
        Color = color;
        Source = source;
    }
}

public class ColorChangedEvent : IGameEvent
{
    public readonly int Position;
    public readonly StickerColor Color;
    public readonly CardInstance Source;

    public ColorChangedEvent(int position, StickerColor color, CardInstance source)
    {
        Position = position;
        Color = color;
        Source = source;
    }
}

//--- Фазы хода ---
public class GameStartedEvent: IGameEvent{}
public class RoundStarted : IGameEvent
{
    public readonly int Round;

    public RoundStarted(int round)
    {
        Round = round;
    }
}

public class PlayerTurnStarted : IGameEvent
{
    public readonly Player Player;
    public readonly bool BattleIsComing;

    public PlayerTurnStarted(Player player, bool battleIsComing)
    {
        Player = player;
        BattleIsComing = battleIsComing;
    }
}

public class PlayerRotationPhaseStarted : IGameEvent
{
    public readonly Player Player;

    public PlayerRotationPhaseStarted(Player player)
    {
        Player = player;
    }
}

public class PlayerTurnEnded : IGameEvent
{
    public readonly Player Player;

    public PlayerTurnEnded(Player player)
    {
        Player = player;
    }
}

public class PreBattlePhaseStarted : IGameEvent{}

public class PreBattlePhaseEnded : IGameEvent{}

public class BattlePhaseStarted : IGameEvent{}

public class BattlePhaseEnded : IGameEvent{}

public class PostBattlePhaseStarted : IGameEvent{}

public class PostBattlePhaseEnded : IGameEvent{}

public class RoundEnded(int round) : IGameEvent
{
    public readonly int Round = round;
}



//-----

public class PlayerScoreRequestEvent : IGameEvent
{
    public int Amount;
    public Player Player;
    public bool Allowed = true;

    public PlayerScoreRequestEvent(int amount, Player player)
    {
        Amount = amount;
        Player = player;
    }
}

public class PlayerScoredEvent(int amount, int score, Player player) : IGameEvent
{
    public readonly int Amount = amount;
    public readonly int FullScore = score;
    public readonly Player Player = player;
}

public class ChangeRemainingUnitCounterRequestEvent(int amount, bool isHoard, bool setValue, bool canBeNegative, CardInstance? source = null)
    : IGameEvent
{
    public bool Allowed = true;
    public int Amount = amount;
    public bool IsHoard = isHoard;
    public bool SetValue = setValue;
    public bool CanBeNegative = canBeNegative;
    public CardInstance? Source = source;
}

public class RemainingUnitCounterChangedEvent(int amount, int current, bool hoard, bool valueSet, CardInstance? source) : IGameEvent
{
    public readonly int Amount = amount;
    public readonly int Current = current;
    public readonly bool Hoard = hoard;
    public readonly bool ValueSet = valueSet;
    public readonly CardInstance? Source = source;
}

public class ChangeRemainingRotationCounterRequestEvent(int amount, bool setValue, bool canBeNegative, CardInstance? source = null) : IGameEvent
{
    public bool Allowed = true;
    public int Amount = amount;
    public bool SetValue = setValue;
    public bool CanBeNegative = canBeNegative;
    public CardInstance? Source = source;
}

public class RemainingRotationCounterChangedEvent(int amount, bool valueSet, CardInstance? source) : IGameEvent
{
    public readonly int Amount = amount;
    public readonly bool ValueSet = valueSet;
    public readonly CardInstance? Source = source;
}

//---
public class CardDrawnEvent : CardEvent
{
    public readonly Player Player;

    public CardDrawnEvent(CardInstance card, Player player)
    {
        Card = card;
        Player = player;
    }
}

public class CardDrawRequestEvent : IGameEvent
{
    public Player Player;
    public int Amount;
    public bool Allowed = true;
    public CardInstance? Source;

    public CardDrawRequestEvent(Player player, int amount, CardInstance? source = null)
    {
        Player = player;
        Amount = amount;
        Source = source;
    }
}

public class CardMovedToDiscard : CardEvent
{
    public CardMovedToDiscard(CardInstance card)
    {
        Card = card;
    }
}

//---


public class RequestTargetChoiceEvent : IGameEvent
{
    public readonly AbilityState Source;
    public List<TargetOptionGroup> Options;
    public readonly bool Deniable;

    public RequestTargetChoiceEvent(
        AbilityState source,
        List<TargetOptionGroup> options,
        bool deniable)
    {
        Source = source;
        Options = options;
        Deniable = deniable;
    }
}

public class ChoiceContext
{
    public AbilityState Source { get; }
    public List<TargetOptionGroup> Options { get; }
    public bool Deniable { get; }

    public ChoiceContext(
        AbilityState source,
        List<TargetOptionGroup> options,
        bool deniable)
    {
        Source = source;
        Options = options;
        Deniable = deniable;
    }
}

public class TargetsChosenEvent : IGameEvent
{
    public readonly AbilityState State;
    public readonly List<List<int>> Targets;


    public TargetsChosenEvent(AbilityState state, List<List<int>> targets)
    {
        State = state;
        Targets = targets;
    }
}


//==========

public class TargetSelector
{
    public TargetSide Side;
    public CardZone Zone;

    public FaceConstraint Face;
    public StatConstraint Stat;

    public TargetPick Pick;
    public int Count;
    public bool ExcludeSelf;

    public TargetSelector(TargetSide side, CardZone zone, FaceConstraint face, StatConstraint stat, TargetPick pick, int count = 1, bool excludeSelf = true)
    {
        Side = side;
        Zone = zone;
        Face = face;
        Stat = stat;
        Pick = pick;
        Count = count;
    }
}
public enum TargetSide
{
    Ally,
    Enemy,
    Any
}
public enum FaceConstraint
{
    Any,
    SameFace
}

public enum StatConstraint
{
    Any,
    Weakest,
    Strongest,
    MostHealth,
    LeastHealth
}

public enum TargetPick
{
    Random,
    All,
    First
}


