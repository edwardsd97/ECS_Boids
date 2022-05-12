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

        var chunks = m_BoidQuery.CreateArchetypeChunkArray(Allocator.TempJob);

//      JobHandle boidChunksUpdate = 
        Entities.
            WithReadOnly(typeTrans).
            WithReadOnly(typeRot).
            WithReadOnly(chunks).
            ForEach((ref CBoidData data, in Translation myPos) =>
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
                for (int j = 0; j < chunk.Count; j++)
                {
                    float3 delta = myPos.Value - translations[j].Value;
                    float distSq = math.lengthsq(delta);
                    if ( data.m_Count < data.m_CountMax && distSq > 0 && distSq < radiusSq )
                    {
                        data.m_Count++;

                        data.m_Alignment += math.forward(rotations[j].Value);
                        data.m_Cohesion += translations[j].Value;                     
                        data.m_Separation += delta;
                    }
                }
            }

        }).ScheduleParallel();

  //      boidChunksUpdate.Complete();

        // Average the results when all that is done and combine
        JobHandle finalize = 
        Entities.
        //            WithDeallocateOnJobCompletion(chunks).
        ForEach((ref CBoidData data, ref Rotation rot, in Translation pos) =>
        {
            if (data.m_Count > 0)
		    {
                data.m_Alignment /= data.m_Count;
                data.m_Cohesion /= data.m_Count;
                data.m_Separation /= data.m_Count;

                float3 direction = (data.m_Alignment + data.m_Cohesion + data.m_Separation) - pos.Value;

                //            if (transform.position.x > m_BoidSys.m_ArenaSize.x || transform.position.x < -m_BoidSys.m_ArenaSize.x ||
                //                transform.position.y > m_BoidSys.m_ArenaSize.y || transform.position.y < -m_BoidSys.m_ArenaSize.y ||
                //                transform.position.z > m_BoidSys.m_ArenaSize.z || transform.position.z < -m_BoidSys.m_ArenaSize.z)
                //            {
                //                // out of bounds - face toward center of boid system
                //                direction = m_BoidSys.transform.position - transform.position;
                //            }

                if (math.lengthsq(direction) > 0)
                    rot.Value = quaternion.LookRotation(direction, math.up());//  quaternion. Slerp(transform.rotation, quaternion.LookRotation(direction), 5.0f * dT);
            }

        }).ScheduleParallel(this.Dependency);

        finalize.Complete();

        chunks.Dispose();
    }
}
