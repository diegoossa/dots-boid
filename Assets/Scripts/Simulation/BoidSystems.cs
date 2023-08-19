using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpiritBoids.Simulation
{
    public partial struct BoidSystems : ISystem
    {
        private EntityQuery _boidQuery;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BoidSimulationData>();
            
            _boidQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Boid>()
                .WithAllRW<LocalToWorld>()
                .Build(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var boidSimulationData = SystemAPI.GetSingleton<BoidSimulationData>();
            
            var boidCount = _boidQuery.CalculateEntityCount();
            
            var cellIndices = new NativeArray<int>(boidCount, Allocator.TempJob);
            var cellBoidCount = new NativeArray<int>( boidCount, Allocator.TempJob);
            var boidPositions = new NativeArray<float3>(boidCount, Allocator.TempJob);
            var boidHeadings = new NativeArray<float3>(boidCount, Allocator.TempJob);
            var hashMap = new NativeParallelMultiHashMap<int, int>(boidCount, Allocator.TempJob);
            
            var random = new Random((uint)SystemAPI.Time.ElapsedTime + 1000);
            
            var copyPositionAndHeadingJobHandle = new CopyPositionAndHeadingJob
            {
                BoidPositions = boidPositions,
                BoidHeadings = boidHeadings
            }.ScheduleParallel(_boidQuery, state.Dependency);

            var randomHashRotation = random.NextQuaternionRotation();

            var offsetRange = boidSimulationData.BoidPerceptionRadius / 2f;
            var randomHashOffset = random.NextFloat3(-offsetRange, offsetRange);
            
            var hashPositionsJobHandle = new HashPositionsToHashMapJob
            {
                HashMap = hashMap.AsParallelWriter(),
                CellRotationVary = randomHashRotation,
                PositionOffsetVary = randomHashOffset,
                CellRadius = boidSimulationData.BoidPerceptionRadius
            }.ScheduleParallel(_boidQuery, copyPositionAndHeadingJobHandle);
            
            // Proceed when these two jobs have been completed
            var copyAndHashJobHandle = JobHandle.CombineDependencies(
                copyPositionAndHeadingJobHandle,
                hashPositionsJobHandle
            );
            
            var mergeCellsJobHandle = new MergeCellsJob
            {
                indicesOfCells = cellIndices,
                cellPositions = boidPositions,
                cellHeadings = boidHeadings,
                cellCount = cellBoidCount,
            }.Schedule(hashMap, 64, copyAndHashJobHandle);
            
            var moveJobHandle = new MoveBoidsJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                BoidSpeed = boidSimulationData.BoidSpeed,

                SeparationWeight = boidSimulationData.SeparationWeight,
                AlignmentWeight = boidSimulationData.AlignmentWeight,
                CohesionWeight = boidSimulationData.CohesionWeight,

                CageSize = boidSimulationData.CageSize,
                CageAvoidDist = boidSimulationData.AvoidWallsTurnDist,
                CageAvoidWeight = boidSimulationData.AvoidWallsWeight,

                CellSize = boidSimulationData.BoidPerceptionRadius,
                CellIndices = cellIndices,
                PositionSumsOfCells = boidPositions,
                HeadingSumsOfCells = boidHeadings,
                CellBoidCount = cellBoidCount,
            }.Schedule(_boidQuery, mergeCellsJobHandle);
            
            moveJobHandle.Complete();
            hashMap.Dispose();
        }
    }
    
    /// <summary>
    /// Copies all boid positions and headings into buffers
    /// </summary>
    [BurstCompile]
    public partial struct CopyPositionAndHeadingJob : IJobEntity
    {
        public NativeArray<float3> BoidPositions;
        public NativeArray<float3> BoidHeadings;

        public void Execute([EntityIndexInQuery] int index, in LocalToWorld localToWorld)
        {
            BoidPositions[index] = localToWorld.Position;
            BoidHeadings[index] = localToWorld.Forward;
        }
    }
    
    /// <summary>
    /// Assigns each boid to a cell. Each boid index is stored in the hashMap. Each hash corresponds to a cell.
    /// The cell grid has a random offset and rotation each frame to remove artefacts.
    /// </summary>
    [BurstCompile]
    public partial struct HashPositionsToHashMapJob : IJobEntity
    {
        public NativeParallelMultiHashMap<int, int>.ParallelWriter HashMap;
        [ReadOnly] public quaternion CellRotationVary;
        [ReadOnly] public float3 PositionOffsetVary;
        [ReadOnly] public float CellRadius;

        public void Execute([EntityIndexInQuery] int index, in LocalToWorld localToWorld)
        {
            var hash = (int) math.hash(new int3(math.floor(math.mul(CellRotationVary, localToWorld.Position + PositionOffsetVary) / CellRadius)));
            HashMap.Add(hash, index);
        }
    }
    
    /// <summary>
    /// Sums up positions and headings of all boids of each cell. These sums are stored in the
    /// same array as before (cellPositions and cellHeadings), so that there is no need for
    /// a new array. The index of each cell is set to the index of the first boid.
    /// With the array indicesOfCells each boid can find the index of its cell.
    /// This way every boid knows the sum of all the positions (and headings) of all the other
    /// boids in the same cell -> no nested loop required -> massive performance boost
    /// </summary>
    [BurstCompile]
    public struct MergeCellsJob : IJobNativeParallelMultiHashMapMergedSharedKeyIndices
    {
        public NativeArray<int> indicesOfCells;
        public NativeArray<float3> cellPositions;
        public NativeArray<float3> cellHeadings;
        public NativeArray<int> cellCount;
    
        public void ExecuteFirst(int firstBoidIndexEncountered)
        {
            indicesOfCells[firstBoidIndexEncountered] = firstBoidIndexEncountered;
            cellCount[firstBoidIndexEncountered] = 1;
            float3 positionInThisCell = cellPositions[firstBoidIndexEncountered] / cellCount[firstBoidIndexEncountered];
        }
    
        public void ExecuteNext(int firstBoidIndexAsCellKey, int boidIndexEncountered)
        {
            cellCount[firstBoidIndexAsCellKey] += 1;
            cellHeadings[firstBoidIndexAsCellKey] += cellHeadings[boidIndexEncountered];
            cellPositions[firstBoidIndexAsCellKey] += cellPositions[boidIndexEncountered];
            indicesOfCells[boidIndexEncountered] = firstBoidIndexAsCellKey;
        }
    }
    
    
    [BurstCompile]
    public partial struct MoveBoidsJob : IJobEntity
    {
        [ReadOnly] public float DeltaTime;
        [ReadOnly] public float BoidSpeed;

        [ReadOnly] public float SeparationWeight;
        [ReadOnly] public float AlignmentWeight;
        [ReadOnly] public float CohesionWeight;

        [ReadOnly] public float CageSize;
        [ReadOnly] public float CageAvoidDist;
        [ReadOnly] public float CageAvoidWeight;

        [ReadOnly] public float CellSize;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<int> CellIndices;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float3> PositionSumsOfCells;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float3> HeadingSumsOfCells;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<int> CellBoidCount;

        public void Execute([EntityIndexInQuery] int boidIndex, ref LocalToWorld localToWorld, in Boid boid)
        {
            var boidPosition = localToWorld.Position;
            var cellIndex = CellIndices[boidIndex];

            var nearbyBoidCount = CellBoidCount[cellIndex] - 1;
            var positionSum = PositionSumsOfCells[cellIndex] - localToWorld.Position;
            var headingSum = HeadingSumsOfCells[cellIndex] - localToWorld.Forward;
            var force = float3.zero;

            if (nearbyBoidCount > 0)
            {
                var averagePosition = positionSum / nearbyBoidCount;

                var distToAveragePositionSq = math.lengthsq(averagePosition - boidPosition);
                var maxDistToAveragePositionSq = CellSize * CellSize;

                var distanceNormalized = distToAveragePositionSq / maxDistToAveragePositionSq;
                var needToLeave = math.max(1 - distanceNormalized, 0f);

                var toAveragePosition = math.normalizesafe(averagePosition - boidPosition);
                var averageHeading = headingSum / nearbyBoidCount;

                force += -toAveragePosition * SeparationWeight * needToLeave;
                force += toAveragePosition * CohesionWeight;
                force += averageHeading * AlignmentWeight;
            }

            if (math.min(math.min((CageSize / 2f) - math.abs(boidPosition.x), (CageSize / 2f) - math.abs(boidPosition.y)), (CageSize / 2f) - math.abs(boidPosition.z)) < CageAvoidDist)
            {
                force += -math.normalize(boidPosition) * CageAvoidWeight;
            }

            var velocity = localToWorld.Forward * BoidSpeed;
            velocity += force * DeltaTime;
            velocity = math.normalize(velocity) * BoidSpeed;

            localToWorld.Value = float4x4.TRS(
                localToWorld.Position + velocity * DeltaTime,
                quaternion.LookRotationSafe(velocity, localToWorld.Up),
                new float3(1f)
            );
        }
    }

}