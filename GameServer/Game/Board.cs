using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public enum StickerColor
{
    Red,
    Green,
    Blue,
    Black
}


public class Board
{
    // --- ПОЛЯ ---
    private UnitInstance?[] _slots = new UnitInstance?[24];
    private StickerColor[] _stickerColors = new StickerColor[24];

    // Карта -> позиция на поле
    private readonly Dictionary<UnitInstance, int> _cardPositions = new();
    

    // --- КОНСТРУКТОР ---
    public Board(StickerColor[]? initialColors = null)
    {
        if (initialColors == null)
        {
            for (int i = 0; i < 24; i++)
            {
                var face = GetFaceOfSticker(i);
                _stickerColors[i] = face switch
                {
                    Face.Front or Face.Back => StickerColor.Blue,
                    Face.Bottom or Face.Top => StickerColor.Green,
                    Face.Left or Face.Right => StickerColor.Red,
                    _ => StickerColor.Black
                };

            }
            return;
        }
        if (initialColors.Length != 24)
            throw new Exception("Board must be initialized with exactly 24 sticker colors.");

        Array.Copy(initialColors, _stickerColors, 24);
    }

    // --- ПРОВЕРКИ ---
    public bool IsValidPosition(int pos) => pos is >= 0 and < 24;

    public bool IsPositionEmpty(int pos)
    {
        if (!IsValidPosition(pos))
            return false;
        return _slots[pos] == null;
    }

    // --- ОПЕРАЦИИ ---
    public void PlaceCard(UnitInstance card, int pos)
    {
        if (!IsValidPosition(pos))
            throw new Exception("Invalid board position");

        if (!IsPositionEmpty(pos))
            throw new Exception("Position is already occupied");

        _slots[pos] = card;
        _cardPositions[card] = pos;
    }

    public void RemoveCard(UnitInstance card)
    {
        if (!_cardPositions.TryGetValue(card, out int pos))
            return;

        _slots[pos] = null;
        _cardPositions.Remove(card);
    }

    public UnitInstance? GetCard(int pos)
    {
        if (!IsValidPosition(pos))
            return null;
        return _slots[pos];
    }

    public int? GetPosition(UnitInstance card)
    {
        if (!_cardPositions.TryGetValue(card, out int pos))
            return null;
        return pos;
    }
    
    public StickerColor GetColor(int pos)
    {
        if (!IsValidPosition(pos))
            throw new IndexOutOfRangeException();
        return _stickerColors[pos];
    }

    // --- ПОЛУЧЕНИЕ КАРТ ПО ГРАНИ ---

    // карта → союзные карты на той же грани
    public IEnumerable<UnitInstance> GetFriendlyCardsOnFace(UnitInstance card)
    {
        if (!_cardPositions.TryGetValue(card, out int pos))
            yield break;

        Face f = GetFaceOfSticker(pos);

        foreach (var c in GetCardsOnFace(f))
            if (c?.Owner == card.Owner && c != card)
                yield return c;
    }

    // карта → вражеские карты на той же грани
    public IEnumerable<UnitInstance> GetEnemyCardsOnFace(UnitInstance card)
    {
        if (!_cardPositions.TryGetValue(card, out int pos))
            yield break;

        Face f = GetFaceOfSticker(pos);

        foreach (var c in GetCardsOnFace(f))
            if (c.Owner != card.Owner)
                yield return c;
    }

    // все карты на определённой грани
    public IEnumerable<UnitInstance> GetCardsOnFace(Face face)
    {
        foreach (int index in FaceRotationMaps[face])
            if (_slots[index] != null)
                yield return _slots[index]!;
    }

    public IEnumerable<UnitInstance> GetAllCards()
    {
        foreach (var card in _slots)
        {
            if (card != null)
                yield return card;
        }
    }

    // --- ВРАЩЕНИЕ ГРАНИ ---
    public void RotateFace(Face face, bool clockwise)
    {
        // (1) делаем копию текущих карт и цветов
        UnitInstance[] oldSlots = (UnitInstance[])_slots.Clone();
        StickerColor[] oldColors = (StickerColor[])_stickerColors.Clone();

        // (2) для всех 24 стикеров вычисляем новое положение
        for (int oldIndex = 0; oldIndex < 24; oldIndex++)
        {
            int newIndex = RotateStickerIndex(oldIndex, face, clockwise);
            // перенос цвета
            _stickerColors[newIndex] = oldColors[oldIndex];

            // перенос карты (если была)
            var card = oldSlots[oldIndex];
            _slots[newIndex] = card;
            if (card != null)
            {
                _cardPositions[card] = newIndex;
                card.Position = newIndex;
                Console.WriteLine("old: " + oldIndex + " new: " + newIndex + " pos: " + card.Position);
                
            }
        }
    }

    private static int RotateStickerIndex(int index, Face face, bool clockwise)
    {
        bool affected = false;
        foreach (var i in FaceRotationMaps[face])
        {
            if (index / 3 == i / 3)
            {
                affected = true;
                break;
            }
        }

        if (!affected)
            return index;
        if (FaceRotationMaps[face].Contains(index))
        {
            var curIndex = FaceRotationMaps[face].IndexOf(index);
            return clockwise ? FaceRotationMaps[face][(curIndex + 1) % 4] : FaceRotationMaps[face][(curIndex + 3) % 4];
        }
        var cubelet = index / 3;
        var rotation = index % 3;
        foreach (var i in FaceRotationMaps[face])
        {
            if (cubelet != i / 3)
                continue;
            var curIndex = FaceRotationMaps[face].IndexOf(i);
            if (clockwise)
            {
                cubelet = FaceRotationMaps[face][(curIndex + 1) % 4] / 3;
                break;
            }
                
            cubelet = FaceRotationMaps[face][(curIndex + 3) % 4] / 3;
            break;
        }

        rotation = face switch
        {
            Face.Front or Face.Back => 1 - rotation,
            Face.Bottom or Face.Top => 2 - rotation,
            Face.Left or Face.Right => 3 - rotation,
            _ => rotation
        };
        
        return cubelet * 3 + rotation;
    }

    // --- ДАННЫЕ О ГРАНЯХ ---
    // каждая грань → массив из четырёх индексов стикеров
    public static readonly ImmutableDictionary<Face, ImmutableArray<int>> FaceRotationMaps =
        new Dictionary<Face, ImmutableArray<int>>
        {
            { Face.Front,  ImmutableArray.Create(2, 8, 11, 5) },
            { Face.Back,   ImmutableArray.Create(17, 23, 20, 14) },
            { Face.Left,   ImmutableArray.Create(12, 18, 6, 0) },
            { Face.Right,  ImmutableArray.Create(3, 9, 21, 15) },
            { Face.Top,    ImmutableArray.Create(7, 19, 22, 10) },
            { Face.Bottom, ImmutableArray.Create(13, 1, 4, 16) }
        }.ToImmutableDictionary();

    // --- Помощь: какой Face у стикера? ---
    public static Face GetFaceOfSticker(int sticker)
    {
        foreach (var kvp in FaceRotationMaps)
            if (kvp.Value.Any(s => s == sticker))
            {
                return kvp.Key;
            }

        throw new Exception($"Sticker {sticker} does not belong to any face.");
    }
    
}

