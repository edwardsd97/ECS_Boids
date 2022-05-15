using UnityEditor;
using UnityEngine;

using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[GenerateAuthoringComponent] // only ONE GenerateAuthoringComponent per file ;(
public struct CVelocity2D : IComponentData
{
    public float2 m_Velocity;
    public float2 m_Avoidance;
}
