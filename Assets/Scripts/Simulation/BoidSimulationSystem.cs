// using System;
// using Unity.Burst;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Mathematics;
// using Unity.Transforms;
//
// namespace SpiritBoids.Simulation
// {
//     public partial struct BoidSimulationSystem : ISystem
//     {
//         private EntityQuery _boidQuery;
//         
//         
//         [BurstCompile]
//         public void OnCreate(ref SystemState state)
//         {
//             state.RequireForUpdate<BoidSimulationData>();
//             _boidQuery = new EntityQueryBuilder(Allocator.TempJob)
//                 .WithAll<Boid>()
//                 .WithAll<LocalToWorld>()
//                 .Build(ref state);
//         }
//
//         [BurstCompile]
//         public void OnUpdate(ref SystemState state)
//         {
//             var boidSimulationData = SystemAPI.GetSingleton<BoidSimulationData>();
//             
//             var entityArray = _boidQuery.ToEntityArray(Allocator.TempJob);
//             var localToWorldArray = _boidQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
//         }
//     }
//
//
//     [BurstCompile]
//     public partial struct BoidJob : IJobEntity
//     {
//         [ReadOnly] public float BoidPerceptionRadius;
//         [ReadOnly] public float SeparationWeight;
//         [ReadOnly] public float CohesionWeight;
//         [ReadOnly] public float AlignmentWeight;
//         [ReadOnly] public float CageSize;
//         [ReadOnly] public float AvoidWallsTurnDist;
//         [ReadOnly] public float AvoidWallsWeight;
//         [ReadOnly] public float BoidSpeed;
//         [ReadOnly] public float DeltaTime;
//         
//         public void Execute(Entity entity, [EntityIndexInQuery] int index, ref Boid boid, ref LocalToWorld localToWorld)
//         {
//             var position = localToWorld.Position;
//             var separationSum = float3.zero;
//             var positionSum = float3.zero;
//             var headingSum = float3.zero;
//             
//             var boidsNearby = 0;
//             
//             for (int otherBoidIndex = 0; otherBoidIndex < otherBoids.Length; otherBoidIndex++) {
//                 if (boid != otherBoids[otherBoidIndex].entity) {
//                     
//                     float3 otherPosition = otherBoids[otherBoidIndex].localToWorld.Position;
//                     float distToOtherBoid = math.length(boidPosition - otherPosition);
//
//                     if (distToOtherBoid < boidPerceptionRadius) {
//
//                         seperationSum += -(otherPosition - boidPosition) * (1f / math.max(distToOtherBoid, .0001f));
//                         positionSum += otherPosition;
//                         headingSum += otherBoids[otherBoidIndex].localToWorld.Forward;
//
//                         boidsNearby++;
//                     }
//                 }
//             } 
//             
//             
//         }
//         
//     }
//     
//     
//     
//     
//     
//     
//     
//     
//     
//     
//     
//     
//     
//     // public partial struct BoidSimulationSystem : ISystem
//     // {
//     //     [BurstCompile]
//     //     public void OnCreate(ref SystemState state)
//     //     {
//     //         
//     //     }
//     //
//     //     [BurstCompile]
//     //     public void OnUpdate(ref SystemState state)
//     //     {
//     //         var deltaTime = SystemAPI.Time.DeltaTime;
//     //         
//     //         var jobHandle = new AvoidBoundsJob
//     //         {
//     //             Radius = 5f,
//     //             DeltaTime = deltaTime
//     //         }.ScheduleParallel(state.Dependency);
//     //         
//     //         jobHandle = new UpdateBoidTransformJob
//     //         {
//     //             DeltaTime = deltaTime
//     //         }.ScheduleParallel(jobHandle);
//     //         
//     //         jobHandle.Complete();
//     //     }
//     // }
//     //
//     // /// <summary>
//     // /// Keep the boids within the bounds of the world
//     // /// </summary>
//     // [BurstCompile]
//     // public partial struct AvoidBoundsJob : IJobEntity
//     // {
//     //     public float Radius;
//     //     public float DeltaTime;
//     //     
//     //     private void Execute(ref Boid boid)
//     //     {
//     //         var distanceToCenter = math.length(boid.Position);
//     //         if (distanceToCenter > Radius)
//     //         {
//     //             var desiredDirection = math.normalize(-boid.Position);
//     //             var overshoot = distanceToCenter - Radius;
//     //             var steeringForce = desiredDirection * overshoot * DeltaTime;
//     //             boid.Velocity += steeringForce;
//     //         }
//     //     }
//     // }
//     //
//     //         
//     //
//     // /// <summary>
//     // /// Update the boid transform
//     // /// </summary>
//     // [BurstCompile]
//     // public partial struct UpdateBoidTransformJob : IJobEntity
//     // {
//     //     public float DeltaTime;
//     //     
//     //     private void Execute(ref Boid boid, ref LocalTransform localTransform)
//     //     {
//     //         // Update the boid position
//     //         boid.Position += boid.Velocity * DeltaTime;
//     //         localTransform.Position = boid.Position;
//     //         
//     //         // Calculate the rotation based on the velocity
//     //         var targetRotation = quaternion.LookRotationSafe(boid.Velocity, math.up());
//     //
//     //         // Use Slerp to smoothly interpolate between the current rotation and target rotation
//     //         const float rotationSpeed = 2f; // TODO: Make this configurable
//     //         var newRotation = math.slerp(localTransform.Rotation, targetRotation, rotationSpeed * DeltaTime);
//     //
//     //         // Update the boid rotation
//     //         localTransform.Rotation = newRotation;
//     //     }
//     // }
// }