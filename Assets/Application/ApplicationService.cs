using UnityEngine;

[CreateAssetMenu(menuName = "Leveled/Application/Application Service")]
public class ApplicationService : GameService
{
    [SerializeField] private int targetFrameRate;
    
    protected override void Initialize()
    {
        Application.targetFrameRate = targetFrameRate;
    }
}
