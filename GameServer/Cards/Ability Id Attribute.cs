using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class AbilityIdAttribute : Attribute
{
    public string AbilityId { get; }

    public AbilityIdAttribute(string abilityId)
    {
        AbilityId = abilityId;
    }
}