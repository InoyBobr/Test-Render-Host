using System;
using System.Collections.Generic;

public class AbilityDefinition
{
    public string AbilityId;
    public List<AbilityParameter> Parameters;
}


[Serializable]
public class AbilityParameter
{
    public string key;
    public int value;
}
