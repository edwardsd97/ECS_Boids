using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[GenerateAuthoringComponent] // only ONE GenerateAuthoringComponent per file ;(
public struct CBoidTag : IComponentData
{
	public int m_Group;
	public int m_ID;
	public Bounds m_Bounds;
}