using System.Collections.Generic;

public class UnitInstance : CardInstance
{
    public int BasePower => Definition.BasePower;
    public int BaseHealth => Definition.BaseHealth;

    private int _currentHealth;
    private int _currentPower;

    public int CurrentHealth => _currentHealth;
    public int CurrentPower => _currentPower;

    private HashSet<Keyword> _keywords = new();
    public HashSet<Keyword> Keywords => _keywords;

    public bool IsDead => CurrentHealth <= 0;

    public UnitInstance(CardData def, Player owner, GameAPI api, CardZone zone = CardZone.Deck)
        : base(def, owner, api, zone)
    {
        _currentHealth = def.BaseHealth;
        _currentPower = def.BasePower;

        foreach (var keyword in def.Keywords)
        {
            _keywords.Add(keyword);
        }
    }

    public override void Reset()
    {
        base.Reset();

        _currentHealth = Definition.BaseHealth;
        _currentPower = Definition.BasePower;

        _keywords.Clear();
        foreach (var keyword in Definition.Keywords)
            _keywords.Add(keyword);
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
        if (e.Card != this)
            return;
        
        _currentHealth = 0;
        Bus.Publish(e);
    }
    
    
}