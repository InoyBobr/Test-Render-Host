using System.Collections.Generic;

public class AbilityState
{
    public string AbilityId { get; }
    public CardInstance? Owner { get; }

    // параметры, пришедшие из AbilityData
    public Dictionary<string, int> IntValues { get; } = new();

    public Dictionary<string, List<int>> MultipleTargets { get; } = new();

    public Dictionary<string, List<CardInstance>> CardTargets { get; } = new();

    // runtime-логика (не сериализуется)
    public AbilityLogic? Logic { get; private set; }

    public AbilityState(
        string abilityId,
        List<AbilityParameter> initialValues,
        CardInstance? owner
    )
    {
        AbilityId = abilityId;
        Owner = owner;
        foreach (var param in initialValues)
        {
            SetInt(param.key, param.value);
        }
    }

    public void AttachLogic(AbilityLogic logic)
    {
        Logic = logic;
    }

    public void SetInt(string key, int value)
    {
        IntValues[key] = value;
    }
}