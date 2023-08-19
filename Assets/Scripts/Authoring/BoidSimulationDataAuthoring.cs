using SpiritBoids.Simulation;
using Unity.Entities;
using UnityEngine;

namespace SpiritBoids.Authoring
{
    public class BoidSimulationDataAuthoring : MonoBehaviour
    {
        public float boidSpeed = 20f;
        public float boidPerceptionRadius = 50f;
        public float cageSize = 500f;
        
        public float separationWeight = 25f;
        public float cohesionWeight = 5f;
        public float alignmentWeight = 15f;

        public float avoidWallsWeight = 10f;
        public float avoidWallsTurnDist = 20f;
        
        private class BoidSimulationDataBaker : Baker<BoidSimulationDataAuthoring>
        {
            public override void Bake(BoidSimulationDataAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new BoidSimulationData
                {
                    BoidSpeed = authoring.boidSpeed,
                    BoidPerceptionRadius = authoring.boidPerceptionRadius,
                    CageSize = authoring.cageSize,
                    SeparationWeight = authoring.separationWeight,
                    CohesionWeight = authoring.cohesionWeight,
                    AlignmentWeight = authoring.alignmentWeight,
                    AvoidWallsWeight = authoring.avoidWallsWeight,
                    AvoidWallsTurnDist = authoring.avoidWallsTurnDist
                });
            }
        }
    }
}