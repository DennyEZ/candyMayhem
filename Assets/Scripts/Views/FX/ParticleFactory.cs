using UnityEngine;

namespace Match3.Views.FX
{
    /// <summary>
    /// Generates procedural particle effects for the game.
    /// This avoids the need for prefabs and allows dynamic coloring.
    /// </summary>
    public static class ParticleFactory
    {
        private static Material _particleMaterial;

        /// <summary>
        /// Gets or creates a default particle material.
        /// </summary>
        private static Material GetParticleMaterial()
        {
            if (_particleMaterial == null)
            {
                // Find the default sprite shader
                var shader = Shader.Find("Sprites/Default");
                if (shader == null) shader = Shader.Find("Mobile/Particles/Alpha Blended");
                
                _particleMaterial = new Material(shader);
            }
            return _particleMaterial;
        }

        /// <summary>
        /// Plays an explosion effect at the given position with the specified color.
        /// </summary>
        public static void PlayExplosion(Vector3 position, Color color, float scale = 1f)
        {
            var go = new GameObject($"Explosion_{color}");
            go.transform.position = position;
            
            var particles = go.AddComponent<ParticleSystem>();
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            
            // Configure Renderer
            renderer.material = GetParticleMaterial();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = 20; // Ensure in front of candies
            
            // Configure Main Module
            var main = particles.main;
            main.loop = false;
            main.startLifetime = 3f; // Last long enough to fall off screen
            main.startSpeed = new ParticleSystem.MinMaxCurve(3f * scale, 8f * scale); // Slightly less explosive speed
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f * scale, 0.15f * scale); // Smaller bits
            main.startColor = color;
            main.gravityModifier = 3f; // Stronger gravity to ensure they fall
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.Destroy;
            
            // Configure Emission
            var emission = particles.emission;
            emission.enabled = true;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 8, 12) }); // Reduced intensity
            
            // Configure Shape
            var shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f * scale;
            
            // Play
            particles.Play();
        }
    }
}
