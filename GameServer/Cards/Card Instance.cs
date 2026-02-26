using System;
using System.Collections.Generic;
using System.Linq;

public class CardInstance
{
    public CardData Definition { get; }
    public Player Owner { get; }

    public int BasePower => Definition.basePower;
    public int BaseHealth => Definition.baseHealth;
    public StickerColor Color => Definition.color;

    private int _currentHealth;
    private int _currentPower;
    public int CurrentHealth => _currentHealth;
    public int CurrentPower => _currentPower;

    public List<AbilityState> Abilities = new();
    public List<AbilityState> AddedAbilities = new();

    private HashSet<Keyword> _keywords = new ();
    public HashSet<Keyword> Keywords => _keywords;
    

    public int Position;  // индекс на кубе в руке или колоде
    public CardZone Zone;
    public bool IsDead => CurrentHealth <= 0;
    
    private GameAPI _api;
    public EventBus Bus { get; }

    public CardInstance(CardData def, Player owner, GameAPI api, CardZone zone = CardZone.Deck)
    {
        Definition = def;
        Owner = owner;
        _currentHealth = def.baseHealth;
        _currentPower = def.basePower;
        Zone = zone;
        _api = api;
        Bus = api.Bus;

        foreach (var abilityDef in def.abilities)
        {
            AbilityState state = new AbilityState(abilityDef.AbilityId, abilityDef.Parameters, this);
            var logic = AbilityLogicRegistry.Create(state.AbilityId, state);
            state.AttachLogic(logic);
            Abilities.Add(state);
            logic.OnGain();
            
        }

        foreach (var keyword in def.keywords)
        {
            _keywords.Add(keyword);
        }
        
    }

    public void Reset()
    {
        _currentHealth = Definition.baseHealth;
        _currentPower = Definition.basePower;
        foreach (var keyword in Definition.keywords)
        {
            _keywords.Add(keyword);
        }

        foreach (var abilityState in AddedAbilities)
        {
            RemoveAbility(abilityState);
        }
        
    }

    public void Buff(CardBuffedEvent e)
    {
        if (e.Card != this)
            return;
        _currentHealth += e.HealthDelta;
        _currentPower += e.PowerDelta;
        Bus.Publish(e);
    }
    
    public void TakeDamage(CardDamagedEvent e)
    {
        if (e.Card != this)
            return;
        _currentHealth -= e.Damage;
        
        Bus.Publish(e);
            
        if (IsDead)
        {
            Bus.Publish(new CardKilledEvent(this, e.Source));
        }
    }

    public void Kill(CardKilledEvent e)
    {
        _currentHealth = 0;
        Bus.Publish(e);
    }

    public void AddAbility(AbilityState abilityState)
    {
        if (AddedAbilities.Contains(abilityState))
            throw new ArgumentException();
        AddedAbilities.Add(abilityState);
        abilityState.Logic.OnGain();
    }

    public void RemoveAbility(AbilityState abilityState)
    {
        if (!AddedAbilities.Contains(abilityState))
            throw new ArgumentException();
        abilityState.Logic.OnRemove();
        Abilities.Remove(abilityState);
    }
    
}

public enum CardZone
{
    Deck,
    Hand,
    Board,
    Discard,
    PlayerPlaceHolder
}

public enum Keyword
{
    QuickAttack,
    DoubleAttack,
    Hoard,
    Flying,
    Taunt,
    Silenced
}
