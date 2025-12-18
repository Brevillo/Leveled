using System.Collections;
using UnityEngine;

public class EndGoal : MonoBehaviour
{
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private ParticleSystem victoryParticles;
    [SerializeField] private CanvasGroup winScreen;
    [SerializeField] private float endDelay;
    [SerializeField] private float fadeInDuration;
    [SerializeField] private SoundEffect winSound;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerMovement playerMovement))
        {
            StartCoroutine(EndSequence(playerMovement));
        }
    }

    private IEnumerator EndSequence(PlayerMovement playerMovement)
    {
        playerMovement.Immobilize(true);
        playerMovement.GetComponent<Rigidbody2D>().linearVelocityX = 0f;
        
        winSound.Play();
        
        victoryParticles.Play();

        winScreen.alpha = 0f;
        
        for (float percent = 0f; percent < 1f; percent += Time.deltaTime / fadeInDuration)
        {
            winScreen.alpha = percent;
            
            yield return null;
        }

        yield return new WaitForSeconds(endDelay);
        
        playerMovement.Immobilize(false);
        
        gameStateManager.GameState = GameState.Editing;
    }
}
