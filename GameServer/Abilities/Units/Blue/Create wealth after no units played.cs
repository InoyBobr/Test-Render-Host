namespace GameServer.Abilities;

[AbilityId("financier_ability")]
public class Create_wealth_after_no_units_played(AbilityState state) : AbilityLogic(state)
{ 
      private bool _isActive = false;

      public override void OnGain()
      {
         Bus.Subscribe<CardPlayedEvent>(DisableOnCardPlayed, SubscriberOwnerType.Card, Owner);
      }

      public override void OnRemove()
      {
         Bus.Unsubscribe<CardPlayedEvent>(DisableOnCardPlayed, Owner);
      }

      private void DisableOnCardPlayed(CardPlayedEvent e)
      {
         if (e.Card.Owner != Owner.Owner)
            return;
         if (e.Card is UnitInstance)
            _isActive = false;
      }

      private void EnableOnRoundStart(RoundStarted e)
      {
         _isActive = true;
      }

      private void CreateWealthAtRoundEnd(RoundEnded e)
      {
         if (!OnBoardAbilityActive || !_isActive)
            return;
         Bus.Publish(new CreateCardRequestEvent("Wealth", CardZone.Hand, 0, Owner, true));
      }
}