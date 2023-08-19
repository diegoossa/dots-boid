using SpiritBoids.Simulation;
using Unity.Entities;
using UnityEngine;

namespace SpiritBoids.Authoring
{
    public class BoidAuthoring : MonoBehaviour
    {
        public float maxSpeed = 30f;
        
        public class BoidBaker : Baker<BoidAuthoring>
        {
            public override void Bake(BoidAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Boid
                {
                    //MaxSpeed = authoring.maxSpeed
                });
            }
        }
    }
}