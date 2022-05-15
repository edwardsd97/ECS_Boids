using UnityEditor;
using UnityEngine;

using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[GenerateAuthoringComponent] // only ONE GenerateAuthoringComponent per file ;(
public struct CTranslation2D : IComponentData
{
    public float2 m_Translation;

    // This is confusing, but to avoid having to pull two different components off of an entity every frame,
    //  The translation component holds the hitcount for CAttackHits
    public int m_HitCount;
}
