using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class AbilityLogicRegistry
{
    private static readonly Dictionary<string, Type> map;

    static AbilityLogicRegistry()
    {
        map = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t =>
                !t.IsAbstract &&
                typeof(AbilityLogic).IsAssignableFrom(t) &&
                t.GetCustomAttribute<AbilityIdAttribute>() != null
            )
            .ToDictionary(
                t => t.GetCustomAttribute<AbilityIdAttribute>()!.AbilityId,
                t => t
            );
    }

    public static AbilityLogic Create(string abilityId, AbilityState state)
    {
        if (!map.TryGetValue(abilityId, out var type))
            throw new Exception($"AbilityLogic not found: {abilityId}");

        return (AbilityLogic)Activator.CreateInstance(type, state)!;
    }
}