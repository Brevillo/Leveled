using System;
using UnityEngine;

[Serializable]
public class BufferTimer
{
    private readonly float duration;

    private float Timer
    {
        get => timer;
        set
        {
            timer = value;
            isBuffered = timer <= duration;
        }
    }
    
    private float timer = Mathf.Infinity;
    private bool isBuffered;

    public BufferTimer(float duration)
    {
        this.duration = duration;
    }
    
    public bool IsBuffered => isBuffered;

    public void Buffer(bool triggered)
    {
        if (triggered)
        {
            Triggered();
        }
    }
    
    public void Triggered()
    {
        Timer = 0f;
    }
    
    public void Update()
    {
        Timer += Time.deltaTime;
    }
    
    public bool BufferUpdate(bool triggered)
    {
        if (triggered)
        {
            Triggered();
        }
        else
        {
            Update();
        }

        return isBuffered;
    }

    public void Reset()
    {
        timer = Mathf.Infinity;
    }
}