using UnityEngine;

[CreateAssetMenu(menuName = ProjectConstants.CommonFolder + "URL Opener")]
public class URLOpener : ScriptableObject
{
    [SerializeField] private string url;

    public void Open() => Application.OpenURL(url);
}
