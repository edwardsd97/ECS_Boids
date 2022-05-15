using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

public partial class S2DVelocityCollision : SystemBase
{
    private EntityQuery m_Query;

    private static float s_AccumulatedTime;

    protected override void OnCreate()
    {
        base.OnCreate();

        m_Query = GetEntityQuery(
            ComponentType.ReadOnly<CTranslation2D>(),
            ComponentType.ReadOnly<CVelocity2D>(),
            ComponentType.ReadOnly<CCollision>(),
            ComponentType.ReadOnly<CAvoidance2D>()
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
        var typeTrans = mgr.GetComponentTypeHandle<CTranslation2D>(true);
        var typeVel = mgr.GetComponentTypeHandle<CVelocity2D>(true);
        var typeCol = mgr.GetComponentTypeHandle<CCollision>(true);
        var typeAvd = mgr.GetComponentTypeHandle<CAvoidance2D>(true);

        var chunks = m_Query.CreateArchetypeChunkArray(Allocator.TempJob);

        JobHandle avoidanceCalc =
            Entities.
            WithReadOnly(typeTrans).
            WithReadOnly(typeAvd).
            WithReadOnly(chunks).
            WithReadOnly(typeCol).
            ForEach((ref CAvoidanceTarget2D avdTgt, in CAvoidance2D avd, in CTranslation2D myPos) =>
			{
                avdTgt.m_AvoidanceTarget = float2.zero;
                avdTgt.m_Count = 0;

                for (int i = 0; i < chunks.Length; i++)
				{
					var chunk = chunks[i];
					var translations = chunk.GetNativeArray(typeTrans);
                    var avoidance = chunk.GetNativeArray(typeAvd);
                    var collision = chunk.GetNativeArray(typeCol);
					for (int j = 0; j < chunk.Count; j++)
					{
                        if (avoidance[j].m_ID == avd.m_ID)
                            continue;

                        if ((avoidance[j].m_LayerMask & avd.m_LayerMask) == 0)
                            continue;

                        float2 delta = myPos.m_Translation - translations[j].m_Translation;

                        float distSq = math.lengthsq(delta);
                        float combinedRadius = avd.m_Radius + collision[j].m_Radius;
                        if (distSq < (combinedRadius * combinedRadius))
                        {
                            avdTgt.m_AvoidanceTarget += delta * avoidance[j].m_Weight;
                            avdTgt.m_Count++;
                        }
                    }
                }

                if ( avdTgt.m_Count > 0 )
                    avdTgt.m_AvoidanceTarget = avdTgt.m_AvoidanceTarget / avdTgt.m_Count;

            }).ScheduleParallel(this.Dependency);

        // Calculate collisions with read only translates and collision info writing into another buffer for results
        JobHandle collision = 
            Entities.
            WithReadOnly(typeTrans).
            WithReadOnly(typeVel).
            WithReadOnly(typeCol).
            WithReadOnly(chunks).
            //WithoutBurst().
            ForEach((ref CCollisionResult2D colResult, in CTranslation2D myPos, in CVelocity2D vel, in CCollision col ) =>
            {
                colResult.m_Position = myPos.m_Translation + vel.m_Velocity * dT + vel.m_Avoidance * dT;

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
                        for (int j = 0; j < chunk.Count; j++)
                        {
                            if (collisions[j].m_ID == col.m_ID)
                                continue;

                            if ((collisions[j].m_LayerMask & col.m_LayerMask) == 0)
                                continue;

                            float2 jPos = translations[j].m_Translation + velocities[j].m_Velocity * dT + velocities[j].m_Avoidance * dT;

                            float2 delta = colResult.m_Position - jPos;

                            float distSq = math.lengthsq(delta);
                            float combinedRadius = collisions[j].m_Radius + col.m_Radius;
                            if (distSq < (combinedRadius * combinedRadius))
                            {
                                if (math.dot(delta, vel.m_Velocity) > 0.0f)
                                    continue; // moving away from the sphere I am colliding with

                                colResult.m_Position = jPos + (math.normalize(delta) * combinedRadius);
                                testRestart = true;

                                collisionCount++;
                                break;
                            }
                        }
                    }
                }

                if ( collisionCount == maxCollisions )
                    colResult.m_Position = myPos.m_Translation;

            }).ScheduleParallel(this.Dependency);

        JobHandle avoidanceAndCollision = JobHandle.CombineDependencies(collision, avoidanceCalc);

        avoidanceAndCollision.Complete();

        // Pull the collision results back into the Translation of each entity
        JobHandle posUpdate = 
        Entities.
            ForEach((ref CTranslation2D myPos, in CCollisionResult2D colResult ) =>
            {
                myPos.m_Translation = colResult.m_Position;

            }).ScheduleParallel(this.Dependency);


        // Lerp avoidance for next frame
        JobHandle avoidUpdate =
        Entities.
            ForEach((ref CVelocity2D vel, in CAvoidanceTarget2D avdTgt) =>
            {
                vel.m_Avoidance = math.lerp( vel.m_Avoidance, avdTgt.m_AvoidanceTarget, dT );
            }).ScheduleParallel(this.Dependency);

        JobHandle finalize = JobHandle.CombineDependencies(posUpdate, avoidUpdate);

        finalize.Complete();

        chunks.Dispose();
    }
}
