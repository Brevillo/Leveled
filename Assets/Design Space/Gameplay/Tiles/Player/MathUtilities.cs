using System;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtilities
{
    public static int Sign0(this float f) => f > 0 ? 1 : f < 0 ? -1 : 0;

    public static T IncrementEnum<T>(this T value, int increment = 1) where T : Enum =>
        (T)(object)(((int)(object)value + increment) % Enum.GetValues(typeof(T)).Length);

    public static IEnumerable<Vector2Int> GetAllPositionsWithin(this RectInt rectInt)
    {
        var positions = new List<Vector2Int>();

        foreach (var position in rectInt.allPositionsWithin)
        {
            positions.Add(position);
        }

        return positions;
    }
}