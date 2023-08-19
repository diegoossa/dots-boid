using Unity.Entities;
using Unity.Mathematics;

namespace SpiritBoids.Simulation
{
    public struct Boid : IComponentData
    {
        public float3 Velocity;
        public float3 Position;
    }
}