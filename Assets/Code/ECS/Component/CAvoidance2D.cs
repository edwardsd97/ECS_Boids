using UnityEditor;
using UnityEngine;

using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[GenerateAuthoringComponent] // only ONE GenerateAuthoringComponent per file ;(
public struct CAvoidance2D : IComponentData
{
    public int m_ID;
    public int m_LayerMask;
    public float m_Radius;
    public float m_Weight;
}
