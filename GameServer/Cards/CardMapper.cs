public static class CardMapper
{
    public static CardDto ToDto(CardInstance card)
    {
        var dto = new CardDto
        {
            InstanceId = card.InstanceId,
            CardId = card.Definition.CardId,
            Color = card.Color,
            Position = card.Position
        };

        if (card is UnitInstance unit)
        {
            dto.Keywords = unit.Keywords;
            dto.Attack = unit.CurrentPower;
            dto.Health = unit.CurrentHealth;
            dto.BaseAttack = unit.BasePower;
            dto.BaseHealth = unit.BaseHealth;
        }
        else
        {
            dto.Keywords = card.Definition.Keywords;
        }

        return dto;
    }
}