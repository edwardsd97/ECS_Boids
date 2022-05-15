using UnityEditor;
using UnityEngine;

using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

/*
// FIXME - experimented with this approach but it had a write buffer conflict that I coudnt figure out how to get around
[InternalBufferCapacity(8)]
public struct CAttackHit : IBufferElementData
{
    // These implicit conversions are optional, but can help reduce typing.
    public static implicit operator int(CAttackHit e) { return e.m_EntityIndex; }
    public static implicit operator CAttackHit(int e) { return new CAttackHit { m_EntityIndex = e }; }

    // Actual value each buffer element will store.
    public int m_EntityIndex;
}
*/

[GenerateAuthoringComponent] // only ONE GenerateAuthoringComponent per file ;(
public struct CAttackHits : IComponentData
{
    // This is confusing, but to avoid having to pull two different components off of an entity every frame,
    //  The CTranslation2D component holds the hitcount for CAttackHits

    public int4 m_Hits; // up to 4 attack entity indexes that hit this component this frame
}
