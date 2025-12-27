using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TimeModifier
{
    public float Multiplier
    {
        get => multiplier;
        set
        {
            multiplier = value;
            Updated?.Invoke();
        }
    }

    public void Freeze() => Multiplier = 0f;
    public void UnFreeze() => Multiplier = 1f;
    
    public static TimeModifier New()
    {
        var timeModifier = new TimeModifier();
        
        AddModifier(timeModifier);

        return timeModifier;
    }
    
    public void Destroy()
    {
        RemoveModifier(this);
    }
    
    #region Instance
    
    private event Action Updated;

    private float multiplier;
    
    private TimeModifier()
    {
        multiplier = 1f;
    }
    
    #endregion
    
    #region Management
    
    private static readonly List<TimeModifier> modifiers = new();

    private static void CalculateTimeScale()
    {
        float timeScale = modifiers.Count > 0
            ? modifiers.Aggregate(1f, (product, modifier) => product * modifier.Multiplier)
            : 1f;

        Time.timeScale = Mathf.Max(timeScale, 0);
    }
    
    private static void AddModifier(TimeModifier modifier)
    {
        modifiers.Add(modifier);
        modifier.Updated += CalculateTimeScale;
        CalculateTimeScale();
    }
    
    private static void RemoveModifier(TimeModifier modifier)
    {
        if (modifiers.Remove(modifier))
        {
            modifier.Updated -= CalculateTimeScale;
            CalculateTimeScale();
        }
    }
    
    #endregion
}

public static class TimeUtility
{
    public static Coroutine TimeFreeze(float duration, MonoBehaviour host, TimeModifier modifier) =>
        TimeSlow(0f, duration, host, modifier);

    public static Coroutine TimeSlow(float multiplier, float duration, MonoBehaviour host, TimeModifier modifier)
    {
        return host.StartCoroutine(Routine());

        IEnumerator Routine()
        {
            modifier.Multiplier = multiplier;

            yield return new WaitForSecondsRealtime(duration);

            modifier.Multiplier = 1f;
        }
    }
}
