using Unity.Entities;

namespace SpiritBoids.Spawner
{
    public struct BoidSpawner : IComponentData
    {
        public Entity Prefab;
        public int Count;
        public float Radius;
        public bool ShouldSpawn;
    }
}