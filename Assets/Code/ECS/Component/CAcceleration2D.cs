using UnityEditor;
using UnityEngine;

using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[GenerateAuthoringComponent] // only ONE GenerateAuthoringComponent per file ;(
public struct CAcceleration2D : IComponentData
{
    public float2 m_VelocityTarget;
    public float m_Acceleration;
    public float m_Speed;
}
