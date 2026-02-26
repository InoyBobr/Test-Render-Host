using System;
using System.Collections.Generic;
using System.Linq;

public enum SubscriberOwnerType
{
    Card,
    PlayerPassive,
    API
}


public class EventBus
{
    private class Subscriber
    {
        public SubscriberOwnerType OwnerType;
        public object Owner; // CardInstance, Player, API
        public Delegate RawCallback; // оригинальный делегат
        public Action<IGameEvent> WrappedCallback; // обёртка
        public Type EventType;
    }

    private Queue<IGameEvent> _eventQueue = new();
    private Queue<IGameEvent> _delayedEventQueue = new();
    private bool _processing = false;
    private bool _battleMode = false;

    private Dictionary<Type, List<Subscriber>> _subscribers = new();
    
    
    // =============================================================================================
    // БОЕВОЙ РЕЖИМ
    // =============================================================================================

    public void StartBattleMode()
    {
        _battleMode = true;
    }

    public void EndBattleMode()
    {
        _battleMode = false;
        ProcessDelayedQueue();
    }

    // =============================================================================================
    // ПОДПИСКА
    // =============================================================================================
    public void Subscribe<TEvent>(
        Action<TEvent> callback,
        SubscriberOwnerType ownerType,
        object ownerReference
    ) where TEvent : IGameEvent
    {
        Type eventType = typeof(TEvent);

        if (!_subscribers.TryGetValue(eventType, out var list))
        {
            list = new List<Subscriber>();
            _subscribers[eventType] = list;
        }

        list.Add(new Subscriber
        {
            OwnerType = ownerType,
            Owner = ownerReference,
            RawCallback = callback,
            EventType = eventType,
            WrappedCallback = e => callback((TEvent)e)
        });
    }

    // =============================================================================================
    // ОТПИСКА
    // =============================================================================================
    public void Unsubscribe<TEvent>(
        Action<TEvent> callback,
        object ownerReference
    ) where TEvent : IGameEvent
    {
        Type eventType = typeof(TEvent);

        if (!_subscribers.TryGetValue(eventType, out var list))
            return;

        list.RemoveAll(s =>
            s.Owner == ownerReference &&
            s.EventType == eventType &&
            s.RawCallback == (Delegate)callback
        );
    }

    // =============================================================================================
    // ПУБЛИКАЦИЯ
    // =============================================================================================
    public void Publish(IGameEvent gameEvent)
    {
        if (gameEvent is CardKilledEvent ||
            _battleMode && gameEvent is CardDamagedEvent)
        {
            _delayedEventQueue.Enqueue(gameEvent);
            return;
        }
        _eventQueue.Enqueue(gameEvent);
        ProcessQueue();
    }

    // =============================================================================================
    // ОБРАБОТКА
    // =============================================================================================
    private void ProcessQueue()
    {
        if (_processing) return;
        _processing = true;

        while (_eventQueue.Count > 0)
        {
            var e = _eventQueue.Dequeue();
            Type eventType = e.GetType();

            if (!_subscribers.TryGetValue(eventType, out var list))
                continue;

            var sorted = SortSubscribers(list, e);

            foreach (var sub in sorted)
            {
                try
                {
                    sub.WrappedCallback(e);
                }
                catch (Exception ex)
                {
                    throw new Exception($"EventBus callback error: {ex}");
                }
            }
        }

        _processing = false;
        if(!_battleMode)
            ProcessDelayedQueue();
    }

    private void ProcessDelayedQueue()
    {
        if (_delayedEventQueue.Count == 0)
        {
            return;
        }

        while (_delayedEventQueue.Count > 0)
        {
            _eventQueue.Enqueue(_delayedEventQueue.Dequeue());
        }
        ProcessQueue();
    }

    // =============================================================================================
    // СОРТИРОВКА
    // =============================================================================================
    private List<Subscriber> SortSubscribers(List<Subscriber> original, IGameEvent e)
    {
        // Если событие не относится к карте — только API ставим последним
        if (e is not CardEvent cardEvent)
        {
            return original
                .OrderBy(sub => sub.OwnerType switch
                {
                    SubscriberOwnerType.PlayerPassive => 0,
                    SubscriberOwnerType.Card => 1,
                    SubscriberOwnerType.API => 2
                })
                .ToList();
        }

        CardInstance eventCard = cardEvent.Card;

        return original
            .OrderBy(sub => ResolvePriority(sub, eventCard))
            .ToList();
    }

    private int ResolvePriority(Subscriber sub, CardInstance eventCard)
    {
        if (sub.OwnerType == SubscriberOwnerType.API)
            return 999;

        if (sub.OwnerType == SubscriberOwnerType.PlayerPassive)
        {
            var player = (Player)sub.Owner;
            return player == eventCard.Owner ? 1 : 20;
        }

        // Карта
        var card = sub.Owner as CardInstance;

        bool sameOwner = card.Owner == eventCard.Owner;
        bool sameFace = Board.GetFaceOfSticker(card.Position) == Board.GetFaceOfSticker(eventCard.Position) && card.Zone == CardZone.Board;

        if (sameOwner)
        {
            if (card == eventCard) return 0;
            if (sameFace) return 2;
            return 3;
        }
        else
        {
            if (sameFace) return 30;
            return 40;
        }
    }
}

