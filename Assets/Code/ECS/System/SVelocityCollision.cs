using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

public partial class SVelocityCollision : SystemBase
{
    private EntityQuery m_Query;

    protected override void OnCreate()
    {
        base.OnCreate();

        m_Query = GetEntityQuery(
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<CCollision>(),
            ComponentType.ReadOnly<CBoidTag>()
            );
    }

    protected override void OnUpdate()
    {
        float dT = Time.DeltaTime;
        EntityManager mgr = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Fetch read only arrays of boid translations and rotations 
        var typeTrans = mgr.GetComponentTypeHandle<Translation>(true);
        var typeCol = mgr.GetComponentTypeHandle<CCollision>(true);
        var typeBoidTag = mgr.GetComponentTypeHandle<CBoidTag>(true);

        var chunks = m_Query.CreateArchetypeChunkArray(Allocator.TempJob);

        // Calculate collisions with read only translates and collision info writing into another buffer for results
        JobHandle finalize = 
        Entities.
            WithReadOnly(typeTrans).
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
                        var collisions = chunk.GetNativeArray(typeCol);
                        var tags = chunk.GetNativeArray(typeBoidTag);
                        for (int j = 0; j < chunk.Count; j++)
                        {
                            if (tags[j].m_ID == myTag.m_ID)
                                continue;
                            float3 delta = colResult.m_Position - translations[j].Value;
                            float distSq = math.lengthsq(delta);
                            float combinedRadius = collisions[j].m_Radius + col.m_Radius;
                            if (distSq < (combinedRadius * combinedRadius))
                            {
                                if (colResult.m_WasEverFree)
                                {
                                    colResult.m_Position = translations[j].Value + (math.normalize(delta) * combinedRadius + 0.001f);
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
