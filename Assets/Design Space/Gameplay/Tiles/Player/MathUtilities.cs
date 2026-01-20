using System;

public static class MathUtilities
{
    public static int Sign0(this float f) => f > 0 ? 1 : f < 0 ? -1 : 0;

    public static T IncrementEnum<T>(this T value, int increment = 1) where T : Enum =>
        (T)(object)(((int)(object)value + increment) % Enum.GetValues(typeof(T)).Length);
}