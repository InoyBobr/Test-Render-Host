namespace GameServer.Abilities;
[AbilityId("curse")]
public class Curse_ability : AbilityLogic
{
    public Curse_ability(AbilityState state) : base(state) {}
    
    public override void OnGain()
    {
        Console.WriteLine("I am cursed");
    }

    public override void OnRemove()
    {
        
    }
}