using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ECS /////////////
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;

public class Attack2D : MonoBehaviour
{
    public enum Type
    {
        Sphere,
        Line
    }

    public Type m_Type;
	public float m_Radius;

    public float m_OrbitRadius;
    public float m_OrbitRate;

    public Entity m_Entity;
    public Entity m_EntityPrefab;
    public EntityManager m_Mgr;

    public static EntityArchetype m_Archetype;
    public static bool m_ArchetypeCreated;

    public static Dictionary<int, Attack2D> m_Dictionary = new Dictionary<int, Attack2D>();

    public void OnEnable()
	{
        CreateEntity();

        m_Dictionary.Add(m_Entity.Index, this);
    }

    public void OnDisable()
	{
        m_Dictionary.Remove(m_Entity.Index);

        if (m_Entity != Entity.Null && World.DefaultGameObjectInjectionWorld != null )
        {
            m_Mgr.DestroyEntity(m_Entity);
            m_Entity = Entity.Null;
        }
	}

    public void CreateEntity()
	{
        m_Mgr = World.DefaultGameObjectInjectionWorld.EntityManager;

        if (m_Entity != Entity.Null)
        {
            m_Mgr.DestroyEntity(m_Entity);
            m_Entity = Entity.Null;
        }

        if (!m_ArchetypeCreated)
        {
            m_Archetype = m_Mgr.CreateArchetype( typeof(CAttack2D) );
            m_ArchetypeCreated = true;
        }

        m_Entity = m_Mgr.CreateEntity(m_Archetype);

        CAttack2D atk = m_Mgr.GetComponentData<CAttack2D>(m_Entity);
        atk.m_LayerMask = -1;
        atk.m_Point.x = transform.position.x;
        atk.m_Point.y = transform.position.y;
        atk.m_Radius = m_Radius * transform.localScale.y;
        atk.m_Type = (int)m_Type; 
        m_Mgr.SetComponentData<CAttack2D>(m_Entity, atk);
    }

    void Update()
    {
        // Rotate around the origin for now...
        transform.position = Quaternion.Euler(0, 0, Time.time * m_OrbitRate) * Vector3.up * m_OrbitRadius;

        CAttack2D atk = m_Mgr.GetComponentData<CAttack2D>(m_Entity);
        atk.m_Point.x = transform.position.x;
        atk.m_Point.y = transform.position.y;

        if (m_Type == Type.Line)
        {
            // line to the origin
            Vector3 lineEnd = Vector3.zero;
            Vector3 delta = lineEnd - transform.position;
            atk.m_LineDir = new float2(delta.normalized.x, delta.normalized.y);
            atk.m_LineLength = delta.magnitude;
        }

        m_Mgr.SetComponentData<CAttack2D>(m_Entity, atk);
    }
}
