using Unity.Entities;
using UnityEngine;

namespace SpiritBoids.Spawner
{
    public partial struct ReadSpawnEventsSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BoidSpawner>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // TODO: Use UI to spawn boids
            if (Input.GetButtonDown("Jump"))
            {
                if (SystemAPI.TryGetSingleton<BoidSpawner>(out var spawner))
                {
                    spawner.ShouldSpawn = true;
                    SystemAPI.SetSingleton(spawner);
                }
            }
        }
    }
}