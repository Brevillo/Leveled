using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Physics2DUtilities
{
    public static ContactFilter2D Normal(this ContactFilter2D contactFilter2D, Vector2 anchor, float range, bool useOutsideNormalAngle = false)
    {
        float angle = Vector2.SignedAngle(Vector2.right, anchor);

        contactFilter2D.useNormalAngle = true;
        contactFilter2D.maxNormalAngle = angle + range / 2f;
        contactFilter2D.minNormalAngle = angle - range / 2f;
        contactFilter2D.useOutsideNormalAngle = useOutsideNormalAngle;

        return contactFilter2D;
    }

    public static ContactFilter2D LayerMask(this ContactFilter2D contactFilter2D, LayerMask layerMask)
    {
        contactFilter2D.useLayerMask = true;
        contactFilter2D.layerMask = layerMask;

        return contactFilter2D;
    }
    
    
    public static bool WithinDistance(this BoxCast2D boxCast2D, Transform transform, float distance) =>
        boxCast2D.Hits
            .Where(hit => hit.transform != transform)
            .Any(hit => (hit.point - (Vector2)transform.position).sqrMagnitude < distance * distance);
}

public class BoxCast2D
{
    public Vector2 origin;
    public Transform originTransform;
    public Vector2 size;
    public Vector2 direction;
    public float angle;
    public ContactFilter2D contactFilter;
    public float distance;
    public bool useOffsetsAsEdges;
    public Vector2 startEndOffsetAdjustment;
   
    private readonly List<RaycastHit2D> hits;

    public BoxCast2D(
        Vector2 origin,
        Vector2 size,
        float angle,
        Vector2 direction,
        ContactFilter2D contactFilter,
        float? distance = null,
        Transform originTransform = null,
        bool useOffsetsAsEdges = false)
    {
        this.origin = origin;
        this.size = size;
        this.angle = angle;
        this.direction = direction.normalized;
        this.contactFilter = contactFilter;
        this.distance = distance ?? float.MaxValue;
        this.originTransform = originTransform;
        this.useOffsetsAsEdges = true;
        this.startEndOffsetAdjustment = Vector2.zero;
        
        this.hits = new();

        if (useOffsetsAsEdges)
        {
            Vector2 startOffset = this.origin;
            Vector2 endOffset = this.origin + this.direction * this.distance;

            var rotation = Quaternion.AngleAxis(this.angle, Vector3.forward);
            Vector2 offsetDelta = Quaternion.Inverse(rotation) * (startOffset - endOffset);
            Vector2 squared = rotation * offsetDelta * Mathf.Min(
                this.size.x / Mathf.Abs(offsetDelta.x),
                this.size.y / Mathf.Abs(offsetDelta.y));
            
            startEndOffsetAdjustment = -squared / 2f;
            
            this.origin += startEndOffsetAdjustment;
            
            Vector2 end = endOffset - startEndOffsetAdjustment;
            this.distance = (end - origin).magnitude;
        }
    }
    
    public static BoxCast2D StartEnd(Vector2 startOffset,
        Vector2 endOffset,
        Transform originTransform,
        Vector2 size,
        float angle,
        ContactFilter2D contactFilter,
        bool offsetAsEdges) =>
        new(startOffset,
            size,
            angle,
            endOffset - startOffset,
            contactFilter,
            (endOffset - startOffset).magnitude,
            originTransform,
            offsetAsEdges);
    
    public IReadOnlyList<RaycastHit2D> Hits => hits;

    public bool IsHitting => Hits.Count > 0;

    public Vector2 HitPoint => Hits.FirstOrDefault().point;

    public void Update() => Physics2D.BoxCast(Origin, size, angle, direction, contactFilter, hits, distance);

    private Vector2 Origin => originTransform != null ? (Vector2)originTransform.position + origin : origin;

    public void DrawGizmos()
    {
        Gizmos.color = IsHitting ? Color.red : Color.green;

        var rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Vector2 size = this.size;
        
        Vector3[] BoxPoints(Vector3 origin) => new[]
        {
            origin + rotation * size / 2f,
            origin + rotation * new Vector2(-size.x, size.y) / 2f,
            origin + rotation * -size / 2f,
            origin + rotation * new Vector2(size.x, -size.y) / 2f,
        };

        Vector2 endPosition = Origin + direction * distance;
        
        var start = BoxPoints(Origin);
        var end = BoxPoints(endPosition);
        
        Gizmos.DrawLineStrip(start, true);
        Gizmos.DrawLineStrip(end, true);
        Gizmos.DrawLineList(Enumerable.Range(0, 4).SelectMany(i => new[] { start[i], end[i] }).ToArray());
        
        if (useOffsetsAsEdges)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(Origin, -startEndOffsetAdjustment);
            Gizmos.DrawRay(endPosition, startEndOffsetAdjustment);
        }
            
        float radius = 0.03f;
        Gizmos.DrawSphere(Origin - startEndOffsetAdjustment, radius);
        Gizmos.DrawSphere(endPosition + startEndOffsetAdjustment, radius);

        foreach (var hit in hits)
        {
            Gizmos.DrawRay(hit.point, hit.normal);
        }
    }
}

public class BoxCaster2D : MonoBehaviour
{
    public Vector2 size = Vector2.one;
    public Vector2 startOffset;
    public Vector2 endOffset = Vector2.right * 2f;
    public bool offsetsAsEdges;
    public float angle;
    public ContactFilter2D contactFilter;

    private BoxCast2D boxCast;
    private BoxCast2D BoxCast => boxCast ?? GetNewBoxCast();

    private BoxCast2D GetNewBoxCast() =>
        boxCast = BoxCast2D.StartEnd(startOffset,
            endOffset,
            transform,
            size,
            angle,
            contactFilter,
            offsetsAsEdges);

    public IReadOnlyList<RaycastHit2D> Hits => BoxCast.Hits;

    public bool IsHitting => BoxCast.IsHitting;

    public Vector2 HitPoint => BoxCast.HitPoint;

    public bool WithinDistance(Transform transform, float distance) => BoxCast.WithinDistance(transform, distance);
    
    private void FixedUpdate()
    {
        GetNewBoxCast();
        BoxCast.Update();
    }
    
    private void OnValidate()
    {
        angle %= 360f;
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.IsPlaying(this))
        {
            GetNewBoxCast();
        }
        
        BoxCast.DrawGizmos();
    }
}
