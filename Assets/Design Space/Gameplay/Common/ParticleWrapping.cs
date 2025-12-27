using UnityEngine;

public class ParticleWrapping : MonoBehaviour 
{
    [SerializeField] private ParticleSystem system;
    [SerializeField] private Transform pivot;
    [SerializeField] private Vector2 wrappingRect;

    private void Update() 
    {
        var particles = new ParticleSystem.Particle[system.particleCount];
        system.GetParticles(particles);

        var rect = new Rect
        {
            size = wrappingRect,
            center = pivot.position,
        };
        
        for (int i = 0; i < particles.Length; i++) 
        {
            var position = particles[i].position;
            
            position.x = (position.x - rect.xMin + rect.width) % rect.width + rect.xMin;
            position.y = (position.y - rect.yMin + rect.height) % rect.height + rect.yMin;
            
            particles[i].position = position;
        }

        system.SetParticles(particles);
    }
}
