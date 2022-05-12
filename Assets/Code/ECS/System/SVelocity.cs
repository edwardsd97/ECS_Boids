using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public partial class SVelocity : SystemBase
{
    protected override void OnUpdate()
    {
        float dT = Time.DeltaTime;

        Entities.WithAll<CVelocity>().ForEach((ref Translation trans, ref CVelocity vel ) =>
        {
            trans.Value += vel.Value * dT;
        }).ScheduleParallel();
    }
}
