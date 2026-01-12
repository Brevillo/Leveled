using UnityEngine;

[CreateAssetMenu(menuName = ProjectConstants.CommonFolder + "Cursor Template")]
public class CursorTemplate : ScriptableObject
{
    [SerializeField] private Texture2D texture;
    [SerializeField] private Vector2 hotspot;
    [SerializeField] private int priority;
    
    public Texture2D Texture => texture;
    public Vector2 Hotspot => hotspot;
    public int Priority => priority;
}