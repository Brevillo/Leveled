using UnityEngine;

public enum UILayer
{
    None = -1,
    Default = 0,
    Viewport = 1,
}

public class OnUILayer : MonoBehaviour
{
    public UILayer layer;
}
