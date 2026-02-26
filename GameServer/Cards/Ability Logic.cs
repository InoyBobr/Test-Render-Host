using System;
using System.Collections.Generic;

public abstract class AbilityLogic
{
    protected AbilityState State { get; }
    protected CardInstance Owner => State.Owner;
    protected EventBus Bus => Owner.Bus;

    protected bool OnBoardAbilityActive => Owner.Zone == CardZone.Board && !Owner.Keywords.Contains(Keyword.Silenced);

    protected AbilityLogic(AbilityState state)
    {
        State = state;
    }

    // Сюда подписываемся на события
    public virtual void OnGain() {}

    // Сюда отписываемся
    public virtual void OnRemove() {}

    public virtual bool CanBePlayed(GameContext ctx) => true;
    
    public virtual List<TargetOptionGroup>? GetTargetOptions(GameContext ctx)
        => null;    

}
