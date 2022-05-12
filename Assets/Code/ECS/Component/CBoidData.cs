using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[GenerateAuthoringComponent] // only ONE GenerateAuthoringComponent per file ;(
public struct CBoidData : IComponentData
{
    public float m_Count;
    public float m_CountMax;
    public float m_Radius;

    public float3 m_Alignment;
    public float3 m_Cohesion;
    public float3 m_Separation;
}