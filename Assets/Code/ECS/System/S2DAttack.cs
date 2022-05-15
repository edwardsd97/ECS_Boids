using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

public partial class S2DAttack : SystemBase
{
    private EntityQuery m_Query;
    
    protected override void OnCreate()
    {
        base.OnCreate();

        m_Query = GetEntityQuery( ComponentType.ReadOnly<CAttack2D>() );
    }

    protected override void OnUpdate()
    {
        EntityManager mgr = World.DefaultGameObjectInjectionWorld.EntityManager;

        var world = World.Unmanaged;

        // Fetch read only arrays of boid translations and rotations 
        var typeAtk = mgr.GetComponentTypeHandle<CAttack2D>(true);
        var typeEntity = mgr.GetEntityTypeHandle();

        var chunks = m_Query.CreateArchetypeChunkArray(Allocator.TempJob);
        
//        JobHandle attacks = 
        Entities.
        WithReadOnly(typeAtk).
        WithReadOnly(typeEntity).
        WithReadOnly(chunks).
        ForEach((Entity ent, ref CAttackHits hits, ref CTranslation2D trans, in CCollision col) =>
		{
            for (int i = 0; i < chunks.Length && trans.m_HitCount < 4; i++)
			{
				var chunk = chunks[i];
				var attacks = chunk.GetNativeArray(typeAtk);
                var attackEnts = chunk.GetNativeArray(typeEntity);
                for (int j = 0; j < chunk.Count && trans.m_HitCount < 4; j++)
				{
                    if ((col.m_LayerMask & attacks[j].m_LayerMask) == 0)
                        continue;

                    float combinedRadius = attacks[j].m_Radius + col.m_Radius;
                    float combinedRadiusSq = combinedRadius * combinedRadius;

                    float2 delta = trans.m_Translation - attacks[j].m_Point;

                    switch ( (Attack2D.Type) attacks[j].m_Type )
					{
                        case Attack2D.Type.Sphere:
                            {
                                if (math.lengthsq(delta) <= combinedRadiusSq)
                                {
                                    hits.m_Hits[trans.m_HitCount++] = attackEnts[j].Index;
                                }
                            }
                            break;

                        case Attack2D.Type.Line:
                            {
                                float clampedDot = math.clamp(math.dot(delta, attacks[j].m_LineDir), 0.0f, attacks[j].m_LineLength );

                                float2 closestPointOnLine = attacks[j].m_Point + attacks[j].m_LineDir * clampedDot;

                                if (math.lengthsq(trans.m_Translation - closestPointOnLine) <= combinedRadiusSq)
                                {
                                    hits.m_Hits[trans.m_HitCount++] = attackEnts[j].Index;
                                }
                            }
                            break;
                    }
                }
            }

        }).WithDisposeOnCompletion(chunks).ScheduleParallel();
    }
}
