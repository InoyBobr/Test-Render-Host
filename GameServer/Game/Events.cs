using System.Collections.Generic;

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

public class RandomCardDamageRequestEvent : IGameEvent
{
    public TargetSelector Selector;
    public int Damage;
    public CardInstance Source;
    public bool Allowed = true;

    public RandomCardDamageRequestEvent(TargetSelector selector, int damage, CardInstance source = null)
    {
        Selector = selector;
        Damage = damage;
        Source = source;
    }
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
    public CardInstance Source;
    public bool Allowed = true;

    public CardBuffRequestEvent(UnitInstance card, int power, int health, CardInstance source = null)
    {
        Card = card;
        PowerDelta = power;
        HealthDelta = health;
        Source = source;
    }
}

public class RandomCardBuffRequestEvent : IGameEvent
{
    public TargetSelector Selector;
    public int PowerDelta;
    public int HealthDelta;
    public CardInstance? Source;

    public RandomCardBuffRequestEvent(TargetSelector selector, int powerDelta, int healthDelta, CardInstance source = null)
    {
        Selector = selector;
        PowerDelta = powerDelta;
        HealthDelta = healthDelta;
        Source = source;
    }
}

//--- Вращение кубика ---
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

    public PlayerTurnStarted(Player player)
    {
        Player = player;
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

    public CardDrawRequestEvent(Player player, int amount)
    {
        Player = player;
        Amount = amount;
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

public class GetContextEvent : IGameEvent
{
    public CardInstance Card;
    public GameContext Ctx;

    public GetContextEvent(CardInstance card, GameContext ctx)
    {
        Card = card;
        Ctx = ctx;
    }
}

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
    public int Count = 1; // NEW
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
    Strongest
}

public enum TargetPick
{
    Random,
    All,
    First
}


