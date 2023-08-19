using Unity.Entities;

namespace SpiritBoids.Simulation
{
    public struct BoidSimulationData : IComponentData
    {
        public float BoidSpeed;
        public float BoidPerceptionRadius;
        public float CageSize;

        public float SeparationWeight;
        public float CohesionWeight;
        public float AlignmentWeight;

        public float AvoidWallsWeight;
        public float AvoidWallsTurnDist;
    }
}