using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;

public enum BounceSnapAxis
{
    None,
    Vertical,
    Horizontal,
    Both,
}

public readonly struct BounceParams
{
    private readonly Vector2 snapPosition;
    private readonly BounceSnapAxis snapAxis;
    public readonly Vector2 force;

    public BounceParams(Vector2 snapPosition, BounceSnapAxis snapAxis, Vector2 force)
    {
        this.snapPosition = snapPosition;
        this.snapAxis = snapAxis;
        this.force = force;
    }

    public Vector2 SnapPosition(Vector2 position) => snapAxis switch
    {
        BounceSnapAxis.None => position,
        BounceSnapAxis.Both => snapPosition,
        BounceSnapAxis.Vertical => new(position.x, snapPosition.y),
        BounceSnapAxis.Horizontal => new(snapPosition.x, position.y),
        _ => throw new InvalidEnumArgumentException(),
    };
}

public class Bounceable : MonoBehaviour
{
    [SerializeField] private UnityEvent<BounceParams> bounced;
    public event Action<BounceParams> Bounced; 

    public void Bounce(BounceParams bounceParams)
    {
        Bounced?.Invoke(bounceParams);
        bounced.Invoke(bounceParams);
    }
}
