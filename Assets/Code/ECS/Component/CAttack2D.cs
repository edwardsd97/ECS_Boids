using UnityEditor;
using UnityEngine;

using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[InternalBufferCapacity(8)]
public struct CAttackHit : IBufferElementData
{
    // These implicit conversions are optional, but can help reduce typing.
    public static implicit operator int(CAttackHit e) { return e.m_EntityIndex; }
    public static implicit operator CAttackHit(int e) { return new CAttackHit { m_EntityIndex = e }; }

    // Actual value each buffer element will store.
    public int m_EntityIndex;
}

[GenerateAuthoringComponent] // only ONE GenerateAuthoringComponent per file ;(
public struct CAttack2D : IComponentData
{
    public int m_Type;
    public int m_LayerMask;
    public float2 m_Point;
    public float m_Radius;

    public float2 m_LineDir;
    public float m_LineLength;
}
