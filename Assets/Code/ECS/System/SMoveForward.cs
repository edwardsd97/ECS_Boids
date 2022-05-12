using UnityEditor;
using UnityEngine;

using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public partial class SMoveForward : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.WithAll<CMoveFoward>().ForEach((ref CVelocity vel, ref CMoveFoward moveFwd, ref Rotation rot ) =>
        {
            vel.Value = moveFwd.Value * math.forward(rot.Value);
        }).ScheduleParallel();
    }
}
