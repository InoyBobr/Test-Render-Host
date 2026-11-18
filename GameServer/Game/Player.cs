using System.Collections.Generic;
using System.Linq;
using Random = System.Random;

public class Player
{
    private List<CardInstance> _deck;
    private List<CardInstance> _hand;
    private List<CardInstance> _discard;
    private CardInstance _playerPlaceholder;
    private GameAPI _api;

    public Player(List<CardData> cards, GameAPI api)
    {
        _hand = new List<CardInstance>();
        _discard = new List<CardInstance>();
        //_playerPlaceholder = new CardInstance(null, this, api);
        _api = api;

        _deck = new List<CardInstance>();
        foreach (var card in cards)
        {
            _deck.Add(CardFactory.Create(card, this, api));
        }
        _deck = _deck.OrderBy(_ => new Random().Next()).ToList();
    }

    public List<CardInstance> GetDeck()
    {
        return _deck.ToList();
    }
    
    public List<CardInstance> GetHand()
    {
        return _hand.ToList();
    }

    public List<CardInstance> GetDiscard()
    {
        return _discard.ToList();
    }

    public CardInstance DrawTopCard()
    {
        var card = _deck[0];
        card.Zone = CardZone.Hand;
        _hand.Add(card);
        _deck.RemoveAt(0);
        card.Position = _hand.IndexOf(card);
        return card;
    }

    public bool DrawExactCard(CardInstance card)
    {
        if (!_deck.Contains(card))
            return false;
        card.Zone = CardZone.Hand;
        _hand.Add(card);
        _deck.Remove(card);
        card.Position = _hand.IndexOf(card);
        for (int i = 0; i < _deck.Count; i++)
        {
            _deck[i].Position = i;
        }
        return true;
    }

    public void AddToDiscard(CardInstance card)
    {
        card.Zone = CardZone.Discard;
        _discard.Add(card);
        card.Reset();
        card.Position = _discard.IndexOf(card);
    }
    
    public void RemoveFromHand(CardInstance card)
    {
        _hand.Remove(card);

        foreach (var c in _hand)
        {
            c.Position = _hand.IndexOf(c);
        }
    }
    
    public void RemoveFromHand(int card)
    {
        _hand.RemoveAt(card);

        foreach (var c in _hand)
        {
            c.Position = _hand.IndexOf(c);
        }
    }

    public void AddCard(CardInstance card, int? position, CardZone zone)
    {
        var rnd = new Random();
        card.Zone = zone;
        switch (zone)
        {
            case CardZone.Deck:
            {
                position ??= rnd.Next(_deck.Count + 1);
                if(position < 0 || position > _deck.Count)
                    _deck.Add(card);
                else
                    _deck.Insert(position.Value, card);
                for (int i = 0; i < _deck.Count; i++)
                {
                    _deck[i].Position = i;
                }
                break;
            }
            case CardZone.Discard:
            {
                position ??= rnd.Next(_discard.Count + 1);
                if(position < 0 || position > _discard.Count)
                    _discard.Add(card);
                else
                    _discard.Insert(position.Value, card);
                for (int i = 0; i < _discard.Count; i++)
                {
                    _discard[i].Position = i;
                }
                break;
            }
            case CardZone.Hand:
            {
                position ??= -1;
                if(position < 0 || position > _hand.Count)
                    _hand.Add(card);
                else
                    _hand.Insert(position.Value, card);
                for (int i = 0; i < _hand.Count; i++)
                {
                    _hand[i].Position = i;
                }
                break;
            }
        }
        
    }
}

