using SpiritBoids.Spawner;
using Unity.Entities;
using UnityEngine;

namespace SpiritBoids.Authoring
{
    public class BoidSpawnerAuthoring : MonoBehaviour
    {
        public GameObject prefab;
        public int count;
        public float radius;

        public class BoidSpawnerBaker : Baker<BoidSpawnerAuthoring>
        {
            public override void Bake(BoidSpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new BoidSpawner
                {
                    Prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
                    Count = authoring.count,
                    Radius = authoring.radius,
                    ShouldSpawn = true
                });
            }
        }
    }
}