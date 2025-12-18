using UnityEngine;

public class PrefabSpawner : MonoBehaviour
{
    public GameObject prefab;
    [SerializeField] private Sprite previewSprite;
    [Space]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameStateManager gameStateManager;
    
    private GameObject prefabInstance;

    public Sprite PreviewSprite
    {
        get => previewSprite;
        set
        {
            previewSprite = value;
            spriteRenderer.sprite = value;
        }
    }

    private void OnEnable()
    {
        gameStateManager.GameStateChanged += OnGameStateChanged;

        spriteRenderer.sprite = previewSprite;
    }

    private void OnDisable()
    {
        gameStateManager.GameStateChanged -= OnGameStateChanged;
    }

    public void OnGameStateChanged(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.Editing:
                
                if (prefabInstance != null)
                {
                    Destroy(prefabInstance);
                }

                spriteRenderer.enabled = true;
                
                break;
            
            case GameState.Playing:
                
                spriteRenderer.enabled = false;
                
                prefabInstance = Instantiate(prefab, transform);
                
                break;
        }
    }
}
