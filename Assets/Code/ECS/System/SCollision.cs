using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

public partial class SCollision : SystemBase
{
    private EntityQuery m_Query;

    protected override void OnCreate()
    {
        base.OnCreate();

        m_Query = GetEntityQuery(
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<CCollision>()
            );
    }

    protected override void OnUpdate()
    {
        float dT = Time.DeltaTime;
        EntityManager mgr = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Fetch read only arrays of boid translations and rotations 
        var typeTrans = mgr.GetComponentTypeHandle<Translation>(true);
        var typeCol = mgr.GetComponentTypeHandle<CCollision>(true);

        var chunks = m_Query.CreateArchetypeChunkArray(Allocator.TempJob);

        // Calculate collisions with read only translates and collision info writing into another buffer for results
        JobHandle finalize = 
        Entities.
            WithReadOnly(typeTrans).
            WithReadOnly(typeCol).
            WithReadOnly(chunks).
            ForEach((ref CCollisionResult colResult, in Translation myPos, in CCollision col) =>
            {
                colResult.m_Position = myPos.Value;

                if (!col.m_Enabled)
                    return;

                bool collided = false;

                for (int i = 0; i < chunks.Length && !collided; i++)
                {
                    var chunk = chunks[i];
                    var translations = chunk.GetNativeArray(typeTrans);
                    var collisions = chunk.GetNativeArray(typeCol);
                    for (int j = 0; j < chunk.Count; j++)
                    {
                        float3 delta = colResult.m_Position - translations[j].Value;
                        float distSq = math.lengthsq(delta);
                        if (distSq == 0)
                            continue;
                        float combinedRadius = collisions[j].m_Radius + col.m_Radius;
                        if (distSq < (combinedRadius * combinedRadius) )
                        {
                            colResult.m_Position = translations[j].Value + (math.normalize(delta) * combinedRadius);
                            collided = true;
                            break;
                        }
                    }
                }

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
