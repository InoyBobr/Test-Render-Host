public class CardDto
{
    public int InstanceId { get; set; }
    public string CardId { get; set; }
    public StickerColor Color { get; set; }
    public IEnumerable<Keyword> Keywords { get; set; }
    public int Position { get; set; }

    public int? Attack { get; set; }
    public int? Health { get; set; }

    public int? BaseAttack { get; set; }
    public int? BaseHealth { get; set; }
}