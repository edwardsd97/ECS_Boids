using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[GenerateAuthoringComponent] // only ONE GenerateAuthoringComponent per file ;(
public struct CCollision : IComponentData
{
    public int m_ID;
    public float m_Radius;
    public int m_LayerMask;
    public bool m_Enabled;
}
