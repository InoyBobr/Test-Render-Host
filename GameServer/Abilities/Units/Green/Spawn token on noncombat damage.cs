namespace GameServer.Abilities;
[AbilityId("slime_ability")]
public class Spawn_token_on_noncombat_damage : AbilityLogic
{
    public Spawn_token_on_noncombat_damage(AbilityState state) : base(state) {}

    private Random rnd = new Random();
    public override void OnGain()
    {
        Bus.Subscribe<CardNonCombatDamagedEvent>(OnCardNonCombatDamaged, SubscriberOwnerType.Card, Owner);
    }

    public override void OnRemove()
    {
        Bus.Unsubscribe<CardNonCombatDamagedEvent>(OnCardNonCombatDamaged, Owner);
    }

    private void OnCardNonCombatDamaged(CardNonCombatDamagedEvent e)
    {
        if (!OnBoardAbilityActive)
            return;
        if(e.Card != Owner)
            return;
        var ctx = Owner._api.GetContext(Owner.Owner);
        var face = Board.GetFaceOfSticker(Owner.Position);
        var list = Board.FaceRotationMaps[face].Where(pos => ctx.IsPositionEmpty(pos)).ToList();
        if (list.Count == 0)
            return;
        var index = rnd.Next(list.Count);
        Bus.Publish(new CreateCardRequestEvent("Slime Token", CardZone.Board, list[index], Owner, true));
    }
}