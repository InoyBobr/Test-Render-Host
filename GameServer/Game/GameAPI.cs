using System;
using System.Collections.Generic;
using System.Linq;


public class GameAPI
{
    // =========================
    // ПОЛЯ
    // =========================

    // Игроки
    public Player Player1 { get; private set; }
    public Player Player2 { get; private set; }

    private GameState _gameState = GameState.Start;

    // Поле
    public Board Board { get; private set; }

    // EventBus для передачи всех игровых событий
    public EventBus Bus { get; private set; }

    // Текущий игрок, чей ход
    public Player CurrentPlayer { get; private set; }

    // Счёт очков
    public int Player1Score { get; private set; }
    public int Player2Score { get; private set; }

    // Текущий раунд
    public int Round { get; private set; }

    // Количество карт, которое игрок берёт в начале игры или хода
    public int StartDrawCount { get; set; } = 4;
    
    private ChoiceContext? _pendingChoice;
    
    public event Action<ChoiceContext>? ChoiceStarted;
    public event Action<ChoiceResult>? ChoiceGetsResult;

    public event Action<PlayCardResult>? CardPlayedResult;
    

    // =========================
    // СОБЫТИЯ, КОТОРЫЕ API МОЖЕТ ПУБЛИКОВАТЬ
    // =========================
    // CardDrawnEvent
    // CardPlayedEvent
    // FaceRotatedEvent
    // DamageDealtEvent
    // PlayerScoredEvent
    // TurnStartedEvent / TurnEndedEvent
    // BattleStartedEvent / BattleEndedEvent

    // =========================
    // МЕТОДЫ
    // =========================

    // 1. Начало игры
    public void StartGame(Player player1, Player player2)
    {
        SubscribeToCardEvents();
        Round = 0;
        Player1 = player1;
        Player2 = player2;
    }

    // 2. Ход игрока
    public void StartTurn(Player player)
    {
        CurrentPlayer = player;
        Bus.Publish(new PlayerTurnStarted(player));
    }

    public void EndTurn(Player player)
    {
        CurrentPlayer = null;
        Bus.Publish(new PlayerTurnEnded(player));
    }

    // 3. Взятие карты
    private int TryDrawCards(Player player, int amount = 1)
    {
        if (player.GetDeck().Count == 0)
            return 0;
        var request = new CardDrawRequestEvent(player, amount);
        Bus.Publish(request);
        if (!request.Allowed)
            return 0;
        var actualAmount = Math.Min(amount, request.Amount);
        return actualAmount;

    } // намерение игрока взять карту

    private void DrawCards(CardDrawRequestEvent e)
    {
        for (int i = 0; i < Math.Min(e.Amount, e.Player.GetDeck().Count); i++)
        {
            var card = e.Player.DrawTopCard();
            Bus.Publish(new CardDrawnEvent(card, e.Player));
        }
        

    } // фактическое действие и событие CardDrawnEvent

    // 4. Розыгрыш карт
    
    /*public bool TryPlayCard(Player player, CardInstance card, int position)
    {
        if (player != CurrentPlayer)
            return false;
        if (!Board.IsPositionEmpty(position) || card.Color != Board.GetColor(position))
            return false;
        var e = new CardPlayRequestEvent(card, position);
        if (!e.Allowed)
            return false;
        Board.PlaceCard(card, position);
        card.Zone = CardZone.Board;
        card.Position = position;
        Bus.Publish(new CardPlayedEvent(card, position));
        return true;

    } // возвращает true, если карта сыграна */
    
    public void TryPlayCard(
    Player player,
    int handIndex,
    int? position = null,
    Dictionary<int, Dictionary<string, List<int>>>? chosenTargets = null
    )
    {
        // 1. Базовые проверки
        if (player != CurrentPlayer)
        {
            CardPlayedResult?.Invoke(new PlayCardResult { Success = false, Error = "Not your turn" });
            return;
        }

        if (handIndex < 0 || handIndex >= player.GetHand().Count)
        {
            CardPlayedResult?.Invoke(new PlayCardResult { Success = false, Error = "Invalid hand index" });
            return;
        }

        var card = player.GetHand()[handIndex];

        var baseContext = new GameContext(Board, CurrentPlayer);

        // 2. Проверка CanBePlayed у всех способностей
        foreach (var ability in card.Abilities)
        {
            if (!ability.Logic.CanBePlayed(baseContext))
            {
                CardPlayedResult?.Invoke(new PlayCardResult { Success = false, Error = "Card cannot be played now" });
                return;
            }
        }

        // 3. Если позиция не указана — вернуть доступные позиции
        if (position == null)
        {
            var playable = new List<int>();

            for (int pos = 0; pos < 24; pos++)
            {
                if (!Board.IsPositionEmpty(pos))
                    continue;

                if (card.Color != Board.GetColor(pos))
                    continue;

                playable.Add(pos);
            }
            
            CardPlayedResult?.Invoke(new PlayCardResult { Success = false, PlayablePositions = playable });
            return;
        }

        // 4. Проверка позиции
        if (!Board.IsPositionEmpty(position.Value))
        {
            CardPlayedResult?.Invoke(new PlayCardResult { Success = false, Error = "Position occupied" });
            return;
        }

        if (card.Color != Board.GetColor(position.Value))
        {
            CardPlayedResult?.Invoke(new PlayCardResult { Success = false, Error = "Wrong color" });
            return;
        }

        // 5. Preview-контекст
        var previewContext = new GameContext(Board, CurrentPlayer)
            .WithPreviewPlacement(card, position.Value);

        // 6. Сбор target request
        var targetRequest = new TargetRequest();

        for (int i = 0; i < card.Abilities.Count; i++)
        {
            var ability = card.Abilities[i];
            var groups = ability.Logic.GetTargetOptions(previewContext);

            if (groups != null && groups.Count > 0)
                targetRequest.Add(i, groups);
        }

        // 7. Если цели нужны, но не присланы
        if (!targetRequest.IsEmpty && chosenTargets == null)
        {
            CardPlayedResult?.Invoke(new PlayCardResult { Success = false, TargetsToPick = targetRequest });
            return;
        }

        // 8. Валидация целей
        if (!targetRequest.IsEmpty)
        {
            foreach (var (abilityIndex, groups) in targetRequest.AbilityTargets)
            {
                if (!chosenTargets!.TryGetValue(abilityIndex, out var abilityTargets))
                {
                    CardPlayedResult?.Invoke(new PlayCardResult { Success = false, Error = "Missing targets" });
                    return;
                }

                foreach (var group in groups)
                {
                    if (!abilityTargets.TryGetValue(group.Key, out var values))
                    {
                        CardPlayedResult?.Invoke(new PlayCardResult { Success = false, Error = "Missing target key" });
                        return;
                    }

                    if (values.Count != group.Count)
                    {
                        CardPlayedResult?.Invoke(new PlayCardResult { Success = false, Error = "Wrong target count" });
                        return;
                    }

                    foreach (var v in values)
                    {
                        if (!group.ValidValues.Contains(v))
                        {
                            CardPlayedResult?.Invoke(new PlayCardResult { Success = false, Error = "Invalid target" });
                            return;
                        }
                    }
                }
            }

            // записываем цели
            foreach (var (abilityIndex, abilityTargets) in chosenTargets!)
            {
                var ability = card.Abilities[abilityIndex];

                foreach (var (key, values) in abilityTargets)
                {
                    ability.SetInt(key, values[0]);
                }
            }
        }

        // 9. Финальное размещение
        Board.PlaceCard(card, position.Value);
        player.RemoveFromHand(handIndex);

        card.Zone = CardZone.Board;
        card.Position = position.Value;
        CardPlayedResult?.Invoke(new PlayCardResult { Success = true });
        Bus.Publish(new CardPlayedEvent(card, position.Value));
        
    }
    
    
    private void OnRequestTargetChoice(RequestTargetChoiceEvent e)
    {
        if (_pendingChoice != null)
            throw new InvalidOperationException("Choice already in progress.");

        _pendingChoice = new ChoiceContext(
            e.Source,
            e.Options,
            e.Deniable
        );

        SetGameState(GameState.WaitingForChoice);

        // Сообщаем наружу (сетевому слою)
        ChoiceStarted?.Invoke(_pendingChoice);
    }
    
    public class ChoiceResult
    {
        public bool Success;
        public string? Error;
    }

    public void SubmitChoice(
        Player player,
        List<List<int>>? selectedTargets)
    {
        if (_pendingChoice == null)
        {
            ChoiceGetsResult?.Invoke(new ChoiceResult { Success = false, Error = "No choice in progress." });
            return;
        }

        var ctx = _pendingChoice;

        if (ctx.Source.Owner.Owner != player)
        {
            ChoiceGetsResult?.Invoke(new ChoiceResult { Success = false, Error = "Not your choice." });
            return;
        }

        // отказ
        if (selectedTargets == null)
        {
            if (!ctx.Deniable)
            {
                ChoiceGetsResult?.Invoke(new ChoiceResult { Success = false, Error = "Choice cannot be skipped." });
                return;
            }

            ResolveChoice(new List<List<int>>());
            ChoiceGetsResult?.Invoke(new ChoiceResult { Success = true });
            return;
        }

        if (!ValidateChoice(ctx.Options, selectedTargets))
        {
            ChoiceGetsResult?.Invoke(new ChoiceResult { Success = false, Error = "Invalid targets." });
            return;
        }
        ChoiceGetsResult?.Invoke(new ChoiceResult { Success = true });
        ResolveChoice(selectedTargets);
    }
    
    private void ResolveChoice(List<List<int>> selected)
    {
        var ctx = _pendingChoice!;
        _pendingChoice = null;

        SetGameState(GameState.PlayPhase);

        Bus.Publish(new TargetsChosenEvent(ctx.Source, selected));
    }
    
    private bool ValidateChoice(
        List<TargetOptionGroup> groups,
        List<List<int>> selected
    )
    {
        
        // 1. Проверка количества групп
        if (groups.Count != selected.Count)
            return false;

        for (int i = 0; i < groups.Count; i++)
        {
            var group = groups[i];
            var chosen = selected[i];

            // 2. Проверка количества целей в группе
            if (chosen.Count != group.Count)
                return false;

            // 3. Проверка допустимости каждой цели
            foreach (var target in chosen)
            {
                if (!group.ValidValues.Contains(target))
                    return false;
            }
        }

        return true;
    }





    // 5. Вращение граней

    public void RotateFace(Face face, int amountOfRotations, Player player)
    {
        amountOfRotations = Math.Abs(amountOfRotations) % 4;
        if(_gameState != GameState.RotatePhase || CurrentPlayer != player || amountOfRotations == 0)
            return;
        switch (amountOfRotations)
        {
            case 1:
                Board.RotateFace(face, true);
                break;
            case 2:
                Board.RotateFace(face, true);
                Board.RotateFace(face, true);
                break;
            case 3:
                Board.RotateFace(face, false);
                break;
        }

        Bus.Publish(new FaceRotatedEvent(face, amountOfRotations, player));
    } // выполняет вращение и публикует FaceRotatedEvent

    // 6. Бой

    public void StartBattle()
    {
        _gameState = GameState.BattlePhase;
    } // включает подфазы: перед боем, во время боя, после боя
    private void PreBattlePhase() 
    {
        Bus.StartBattleMode();
        Bus.Publish(new PreBattlePhaseStarted());
        Bus.EndBattleMode();
        Bus.Publish(new PreBattlePhaseEnded());}

    private void BattlePhase()
    {
        Bus.StartBattleMode();
        Bus.Publish(new BattlePhaseStarted());
        foreach (var card in Board.GetAllCards())
        {
            var enemies = Board.GetEnemyCardsOnFace(card);
            foreach (var enemy in enemies)
            {
                //Атакует только быстрая атака и двойная атака
                if(enemy.Keywords.Contains(Keyword.QuickAttack) || enemy.Keywords.Contains(Keyword.DoubleAttack))
                    Bus.Publish(new CardCombatDamageRequestEvent(card, card.CurrentPower, enemy));
            }
        }
        Bus.EndBattleMode();
        
        Bus.StartBattleMode();
        foreach (var card in Board.GetAllCards())
        {
            var enemies = Board.GetEnemyCardsOnFace(card);
            foreach (var enemy in enemies)
            {
                // атакуют все кроме чистой быстрой атаки
                if(!enemy.Keywords.Contains(Keyword.QuickAttack) || enemy.Keywords.Contains(Keyword.DoubleAttack))
                    Bus.Publish(new CardCombatDamageRequestEvent(card, card.CurrentPower, enemy));
            }
        }
        Bus.EndBattleMode(); 
        Bus.Publish(new BattlePhaseEnded());
    }

    private void PostBattlePhase()
    {
        Bus.StartBattleMode();
        Bus.Publish(new PostBattlePhaseStarted());
        Bus.EndBattleMode();
        Bus.Publish(new PostBattlePhaseEnded());
    }

    // 7. Подсчёт очков
    private void CalculateScores()
    {
        _gameState = GameState.RewardingPhase;
        int player1Score = 0;
        int player2Score = 0;
        foreach (Face face in Enum.GetValues(typeof(Face)))
        {
            var p1 = 0;
            var p2 = 0;
            foreach (var card in Board.GetCardsOnFace(face))
            {
                if (card.Owner == Player1)
                    p1++;
                else
                    p2++;
            }

            if (p1 > p2)
                player1Score++;
            if (p2 > p1)
                player2Score++;
        }
        if(player1Score > player2Score)
            Bus.Publish(new PlayerScoreRequestEvent(1, Player1));
        if (player2Score > player1Score)
            Bus.Publish(new PlayerScoreRequestEvent(1, Player2));
    } // определяет, кто контролирует грани

    private void AwardPoints(PlayerScoreRequestEvent e)
    {
        if (!e.Allowed || e.Amount <= 0)
            return;
        if (e.Player == Player1)
        {
            Player1Score += e.Amount;
        }

        if (e.Player == Player2)
        {
            Player2Score += e.Amount;
        }
        Bus.Publish(new PlayerScoredEvent(e.Amount,e.Player));
    }   

    // 8. Управление картами на поле
    private void RemoveDeadCard(CardKilledEvent e)
    {
        Board.RemoveCard(e.Card);
        e.Card.Owner.AddToDiscard(e.Card);
    } // убирает карты с нулевым здоровьем после подфаз

    private void ApplyCombatDamage(CardCombatDamageRequestEvent e)
    {
        if (e.Card.Zone != CardZone.Board)
        {
            return;
        }

        if (e.Card.IsDead || e.Damage <= 0 || !e.Allowed)
        {
            return;
        }
        e.Card.TakeDamage(new CardCombatDamagedEvent(e.Card, e.Damage, e.Source));
    }
    
    private void ApplyNonCombatDamage(CardNonCombatDamageRequestEvent e)
    {
        if (e.Card.Zone != CardZone.Board)
        {
            return;
        }

        if (e.Card.IsDead || e.Damage <= 0 || !e.Allowed)
        {
            return;
        }
        e.Card.TakeDamage(new CardNonCombatDamagedEvent(e.Card, e.Damage, e.Source));
    }

    private void KillCard(CardKillRequestEvent e)
    {
        if (e.Card.Zone != CardZone.Board)
        {
            return;
        }
        if (e.Card.IsDead || !e.Allowed)
        {
            return;
        }
        e.Card.Kill(new CardKilledEvent(e.Card, e.Source));
    }

    private void BuffCard(CardBuffRequestEvent e)
    {
        if (e.Card.Zone is CardZone.Discard or CardZone.PlayerPlaceHolder)
            return;
        if (e.Card.IsDead || !e.Allowed || e.PowerDelta <= 0 && e.HealthDelta <= 0)
            return;
        var buffEvent = new CardBuffedEvent(e.Card, Math.Min(0, e.PowerDelta), Math.Min(0, e.HealthDelta), e.Source);
        e.Card.Buff(buffEvent);
    }

    // 9. Вспомогательные методы
    public IEnumerable<CardInstance> GetFriendlyCards(CardInstance card)
    {
        return null;
        
    }

    public IEnumerable<CardInstance> GetEnemyCards(CardInstance card)
    {
        return null;
        
    }

    private List<CardInstance> GetSelectedCards(
    CardInstance source,
    TargetSelector selector
    )
    {
        // 1. Берём все карты на поле
        IEnumerable<CardInstance> candidates = Board.GetAllCards().ToList();

        // 2. Жёсткая валидация (правила игры)
        candidates = candidates.Where(card =>
            card is { IsDead: false, Zone: CardZone.Board }
        );

        // 3. Фильтр по стороне
        if (source != null)
        {
            candidates = selector.Side switch
            {
                TargetSide.Ally =>
                    candidates.Where(c => c.Owner == source.Owner),

                TargetSide.Enemy =>
                    candidates.Where(c => c.Owner != source.Owner),

                TargetSide.Any =>
                    candidates,

                _ => candidates
            };
        }

        // 4. Фильтр по грани
        if (selector.Face == FaceConstraint.SameFace && source != null)
        {
            var sourceFace = Board.GetFaceOfSticker(source.Position);
            candidates = candidates.Where(c =>
                Board.GetFaceOfSticker(c.Position) == sourceFace
            );
        }

        // 5. Преобразуем в список (дальше порядок важен)
        var list = candidates.ToList();

        if (list.Count == 0)
            return list;

        // 6. Ограничение по стату (НЕ выбор!)
        list = selector.Stat switch
        {
            StatConstraint.Weakest =>
                list
                    .GroupBy(c => c.CurrentPower)
                    .OrderBy(g => g.Key)
                    .First()
                    .ToList(),

            StatConstraint.Strongest =>
                list
                    .GroupBy(c => c.CurrentPower)
                    .OrderByDescending(g => g.Key)
                    .First()
                    .ToList(),

            StatConstraint.Any =>
                list,

            _ => list
        };

        if (list.Count == 0)
            return list;

        // 7. Финальный выбор
        return selector.Pick switch
        {
            TargetPick.All =>
                list,

            TargetPick.First =>
                new List<CardInstance> { list[0] },

            TargetPick.Random =>
                new List<CardInstance>
                {
                    list[UnityEngine.Random.Range(0, list.Count)]
                },

            _ =>
                list
        };
    }


    // 10. Подписка на события карт/способностей
    private void SubscribeToCardEvents()
    {
        Bus.Subscribe<PlayerScoreRequestEvent>(AwardPoints, SubscriberOwnerType.API, this);
        Bus.Subscribe<CardCombatDamageRequestEvent>(ApplyCombatDamage, SubscriberOwnerType.API, this);
        Bus.Subscribe<CardNonCombatDamageRequestEvent>(ApplyNonCombatDamage, SubscriberOwnerType.API, this);
        Bus.Subscribe<CardDrawRequestEvent>(DrawCards, SubscriberOwnerType.API, this);
        Bus.Subscribe<CardKillRequestEvent>(KillCard, SubscriberOwnerType.API, this);
        Bus.Subscribe<CardKilledEvent>(RemoveDeadCard, SubscriberOwnerType.API, this);
        Bus.Subscribe<CardBuffRequestEvent>(BuffCard, SubscriberOwnerType.API, this);
        Bus.Subscribe<RequestTargetChoiceEvent>(OnRequestTargetChoice, SubscriberOwnerType.API, this);
    }
    private void UnsubscribeFromCardEvents(){}

    private void SetGameState(GameState gameState)
    {
        _gameState = gameState;
    }
}
