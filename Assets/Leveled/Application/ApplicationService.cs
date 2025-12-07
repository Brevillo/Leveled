using UnityEngine;

[CreateAssetMenu(menuName = ProjectConstants.ServicesFolder + "Application Service")]
public class ApplicationService : GameService
{
    [SerializeField] private int targetFrameRate;
    
    protected override void Initialize()
    {
        Application.targetFrameRate = targetFrameRate;
    }
}
