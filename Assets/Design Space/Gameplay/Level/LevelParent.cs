using UnityEngine;

public class LevelParent : MonoBehaviour
{
    [SerializeField] private LevelService LevelService;

    private void Awake()
    {
        LevelService.RegisterLevel(this);
    }
}