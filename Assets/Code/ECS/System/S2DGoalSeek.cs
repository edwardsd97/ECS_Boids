using UnityEditor;
using UnityEngine;

using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public partial class S2DGoalSeek : SystemBase
{
    protected override void OnUpdate()
    {
        float dT = Time.DeltaTime;

        Entities.ForEach((ref CAcceleration2D acc, in CTranslation2D trans, in CGoal2D goal ) =>
        {
            acc.m_VelocityTarget = math.normalizesafe(goal.m_Position - trans.m_Translation) * acc.m_Speed;

        }).ScheduleParallel();

        Entities.ForEach((ref CVelocity2D vel, ref CTranslation2D trans, in CAcceleration2D acc) =>
        {
            vel.m_Velocity = math.lerp(vel.m_Velocity, acc.m_VelocityTarget, dT * acc.m_Acceleration);

        }).ScheduleParallel();
    }
}
