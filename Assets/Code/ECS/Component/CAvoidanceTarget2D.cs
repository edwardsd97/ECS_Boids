using UnityEditor;
using UnityEngine;

using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[GenerateAuthoringComponent] // only ONE GenerateAuthoringComponent per file ;(
public struct CAvoidanceTarget2D : IComponentData
{
    public float2 m_AvoidanceTarget;
    public int m_Count;
}
