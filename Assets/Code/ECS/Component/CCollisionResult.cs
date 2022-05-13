using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[GenerateAuthoringComponent] // only ONE GenerateAuthoringComponent per file ;(
public struct CCollisionResult : IComponentData
{
    public float3 m_Position;
    public bool m_WasEverFree; // dont start preventing collision until the object has been free of collision the first time
}
