using UnityEngine;

public class CheckpointEnjoyer : MonoBehaviour
{
    public Checkpoint Checkpoint => checkpoint;
    public bool HasCheckpoint => checkpoint != null;
    
    private Checkpoint checkpoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Checkpoint newCheckpoint)
            && checkpoint != newCheckpoint)
        {
            if (checkpoint != null)
            {
                checkpoint.Deactivate();
            }

            checkpoint = newCheckpoint;
            
            checkpoint.Activate();
        }
    }
}
