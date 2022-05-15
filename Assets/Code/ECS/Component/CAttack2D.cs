using UnityEditor;
using UnityEngine;

using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[GenerateAuthoringComponent] // only ONE GenerateAuthoringComponent per file ;(
public struct CAttack2D : IComponentData
{
    public int m_Type;          // Sphere, Line
    public int m_LayerMask;     // Collision layers

    public float2 m_Point;      
    public float m_Radius;

    public float2 m_LineDir;
    public float m_LineLength;
}
