using System;
using System.Collections.Generic;
using System.Linq;

using System;
using System.Collections.Generic;

public abstract class CardInstance
{
    private static int idCount = 0;

    public int InstanceId { get; }
    public CardData Definition { get; }
    public Player Owner { get; }

    public StickerColor Color => Definition.Color;

    public List<AbilityState> Abilities = new();
    public List<AbilityState> AddedAbilities = new();

    public int Position;
    public CardZone Zone;

    protected GameAPI _api;
    public EventBus Bus { get; }

    protected CardInstance(CardData def, Player owner, GameAPI api, CardZone zone = CardZone.Deck)
    {
        InstanceId = idCount++;
        Definition = def;
        Owner = owner;
        Zone = zone;

        _api = api;
        Bus = api.Bus;

        foreach (var abilityDef in def.Abilities)
        {
            AbilityState state = new AbilityState(
                abilityDef.AbilityId,
                abilityDef.Parameters,
                this);

            var logic = AbilityLogicRegistry.Create(state.AbilityId, state);

            state.AttachLogic(logic);
            Abilities.Add(state);

            logic.OnGain();
        }
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

    public virtual void Reset()
    {
        // удаляем временные способности
        foreach (var abilityState in AddedAbilities.ToArray())
        {
            RemoveAbility(abilityState);
        }

        AddedAbilities.Clear();
    }
}




