using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

public partial class SBoidSystem : SystemBase
{
    private EntityQuery m_BoidQuery;

    protected override void OnCreate()
    {
        base.OnCreate();

        m_BoidQuery = GetEntityQuery(
            ComponentType.ReadOnly<Rotation>(),
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<CBoidTag>()
            );
    }

    protected override void OnUpdate()
    {
        float dT = Time.DeltaTime;
        EntityManager mgr = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Fetch read only arrays of boid translations and rotations 
        var typeRot = mgr.GetComponentTypeHandle<Rotation>(true);
        var typeTrans = mgr.GetComponentTypeHandle<Translation>(true);
        var typeTag = mgr.GetComponentTypeHandle<CBoidTag>(true);

        var chunks = m_BoidQuery.CreateArchetypeChunkArray(Allocator.TempJob);

        Entities.
            WithReadOnly(typeTrans).
            WithReadOnly(typeRot).
            WithReadOnly(typeTag).
            WithReadOnly(chunks).
            ForEach((ref CBoidData data, in Translation myPos, in CBoidTag tag) =>
        {
            data.m_Count = 0;
            data.m_Alignment = math.float3(0.0f);
            data.m_Cohesion = math.float3(0.0f);
            data.m_Separation = math.float3(0.0f);

            float radiusSq = data.m_Radius * data.m_Radius;

            for ( int i = 0; i < chunks.Length; i++)
			{
                var chunk = chunks[i];
                var translations = chunk.GetNativeArray(typeTrans);
                var rotations = chunk.GetNativeArray(typeRot);
                var tags = chunk.GetNativeArray(typeTag);
                for (int j = 0; j < chunk.Count; j++)
                {
                    if (tags[j].m_Group != tag.m_Group)
                        continue;

                    float3 delta = myPos.Value - translations[j].Value;
                    float distSq = math.lengthsq(delta);
                    if ( distSq > 0 && distSq < radiusSq )
                    {
                        data.m_Count++;

                        data.m_Alignment += math.forward(rotations[j].Value);
                        data.m_Cohesion += translations[j].Value;
                        data.m_Separation += delta;
                    }
                }
            }

        }).ScheduleParallel();

        // Average the results when all that is done and combine
        JobHandle finalize = 
        Entities.
        //            WithDeallocateOnJobCompletion(chunks).
        ForEach((ref CBoidData data, ref Rotation rot, in Translation pos, in CBoidTag tag ) =>
        {
            float3 direction;

            Bounds bounds = tag.m_Bounds;

            if (pos.Value.x > bounds.max.x || pos.Value.x < bounds.min.x ||
                pos.Value.y > bounds.max.y || pos.Value.y < bounds.min.y ||
                pos.Value.z > bounds.max.z || pos.Value.z < bounds.min.z)
            {
                // out of bounds - face toward center of boid system
                direction.x = bounds.center.x - pos.Value.x;
                direction.y = bounds.center.y - pos.Value.y;
                direction.z = bounds.center.z - pos.Value.z;
            }
            else if (data.m_Count > 0)
            {
                data.m_Alignment /= data.m_Count;
                data.m_Cohesion /= data.m_Count;
                data.m_Separation /= data.m_Count;

                direction = (data.m_Alignment + data.m_Cohesion + data.m_Separation * 1.5f) - pos.Value;
            }
            else
            {
                direction = math.forward(rot.Value);
            }

            if (math.lengthsq(direction) > 0)
                rot.Value =  math.slerp(rot.Value, quaternion.LookRotation(direction, math.up()), dT);

        }).ScheduleParallel(this.Dependency);

        finalize.Complete();

        chunks.Dispose();
    }
}
