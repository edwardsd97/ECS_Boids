using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

public partial class SVelocityCollision : SystemBase
{
    private EntityQuery m_Query;

    private static float s_AccumulatedTime;

    protected override void OnCreate()
    {
        base.OnCreate();

        m_Query = GetEntityQuery(
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<CVelocity>(),
            ComponentType.ReadOnly<CCollision>(),
            ComponentType.ReadOnly<CBoidTag>()
            );
    }

    protected override void OnUpdate()
    {
        s_AccumulatedTime += Time.DeltaTime;

        if (s_AccumulatedTime < 0.0167f)
            return;

        float dT = s_AccumulatedTime;
        s_AccumulatedTime = 0.0f;
        EntityManager mgr = World.DefaultGameObjectInjectionWorld.EntityManager;

        var world = World.Unmanaged;

        // Fetch read only arrays of boid translations and rotations 
        var typeTrans = mgr.GetComponentTypeHandle<Translation>(true);
        var typeVel = mgr.GetComponentTypeHandle<CVelocity>(true);
        var typeCol = mgr.GetComponentTypeHandle<CCollision>(true);
        var typeBoidTag = mgr.GetComponentTypeHandle<CBoidTag>(true);

        /*
        // Populates a hash map, where each bucket contains the indices of all Boids whose positions quantize
        // to the same value for a given cell radius 
        // This is useful in terms of the algorithm because it limits the number of comparisons that will
        // actually occur between the different boids. Instead of for each boid, searching through all
        // boids for those within a certain radius, this limits those by the hash-to-bucket simplification.
        var boidCount = m_Query.CalculateEntityCount();
        var hashMap = new NativeMultiHashMap<int, int>(boidCount, world.UpdateAllocator.ToAllocator);
        var parallelHashMap = hashMap.AsParallelWriter();
        var hashPositionsJobHandle = Entities
            .WithName("HashPositionsJob")
            .WithAll<Boid>()
            .ForEach((int entityInQueryIndex, in Translation pos) =>
            {
                var hash = (int)math.hash(new int3(math.floor(pos.Value / 5.0f)));
                parallelHashMap.Add(hash, entityInQueryIndex);
            })
            .ScheduleParallel(this.Dependency);

		[BurstCompile]
        struct MergeCells : IJobNativeMultiHashMapMergedSharedKeyIndices
        { 
        }
    */

        var chunks = m_Query.CreateArchetypeChunkArray(Allocator.TempJob);

        // Calculate collisions with read only translates and collision info writing into another buffer for results
        JobHandle finalize = 
        Entities.
            WithReadOnly(typeTrans).
            WithReadOnly(typeVel).
            WithReadOnly(typeCol).
            WithReadOnly(typeBoidTag).
            WithReadOnly(chunks).
            //WithoutBurst().
            ForEach((ref CCollisionResult colResult, in Translation myPos, in CVelocity vel, in CCollision col, in CBoidTag myTag ) =>
            {
                colResult.m_Position = myPos.Value + vel.Value * dT;

                if (!col.m_Enabled)
                    return;

                int collisionCount = 0;
                int maxCollisions = 2;

                for ( int collideTest = 0; collideTest < maxCollisions; collideTest++ )
				{
                    bool testRestart = false;

                    for (int i = 0; i < chunks.Length && !testRestart; i++)
                    {
                        var chunk = chunks[i];
                        var translations = chunk.GetNativeArray(typeTrans);
                        var velocities = chunk.GetNativeArray(typeVel);
                        var collisions = chunk.GetNativeArray(typeCol);
                        var tags = chunk.GetNativeArray(typeBoidTag);
                        for (int j = 0; j < chunk.Count; j++)
                        {
                            if (tags[j].m_ID == myTag.m_ID)
                                continue;

                            float3 jPos = translations[j].Value + velocities[j].Value * dT;

                            float3 delta = colResult.m_Position - jPos;

                            float distSq = math.lengthsq(delta);
                            float combinedRadius = collisions[j].m_Radius + col.m_Radius;
                            if (distSq < (combinedRadius * combinedRadius))
                            {
                                if (math.dot(delta, vel.Value) > 0.0f)
                                    continue; // moving away from the sphere I am colliding with

                                if (colResult.m_WasEverFree)
                                {
                                    colResult.m_Position = jPos + (math.normalize(delta) * combinedRadius);
                                    testRestart = true;
                                }
                                collisionCount++;
                                break;
                            }
                        }
                    }
                }

                if ( collisionCount == maxCollisions && colResult.m_WasEverFree )
                    colResult.m_Position = myPos.Value;
                else if ( collisionCount == 0 )
                    colResult.m_WasEverFree = true;

            }).ScheduleParallel(this.Dependency);

        finalize.Complete();

        // Pull the collision results back into the Translation of each entity
        Entities.
            ForEach((ref Translation myPos, in CCollisionResult colResult ) =>
            {
                myPos.Value = colResult.m_Position;

            }).ScheduleParallel();

        chunks.Dispose();
    }
}
