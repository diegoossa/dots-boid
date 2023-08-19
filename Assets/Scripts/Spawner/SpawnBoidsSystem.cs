using SpiritBoids.Simulation;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

namespace SpiritBoids.Spawner
{
    /// <summary>
    /// Boid Spawner System
    /// </summary>
    public partial struct SpawnBoidsSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<BoidSpawner>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            
            if (SystemAPI.TryGetSingleton<BoidSpawner>(out var spawner))
            {
                if (!spawner.ShouldSpawn)
                    return;
                
                var random = new Random((uint)SystemAPI.Time.ElapsedTime + 1000);
                
                for (var i = 0; i < spawner.Count; i++)
                {
                    var instance = ecb.Instantiate(spawner.Prefab);
                    // Use square root to get a more even distribution
                    var position = random.NextFloat3Direction() * math.sqrt(random.NextFloat(-spawner.Radius, spawner.Radius));
                    ecb.SetComponent(instance, new LocalTransform
                    {
                        Position = position,
                        Rotation = quaternion.identity,
                        Scale = 1f
                    });
                    
                    ecb.SetComponent(instance, new Boid
                    {
                        //Velocity = random.NextFloat3Direction(),
                        Position = position
                    });
                }

                spawner.ShouldSpawn = false;
                SystemAPI.SetSingleton(spawner);
            }
        }
    }
}