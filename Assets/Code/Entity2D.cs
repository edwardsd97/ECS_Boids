using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ECS /////////////
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;

public class Entity2D : MonoBehaviour
{
    public float m_Speed;
    public float m_Acceleration;
    public float m_Radius;

    float2 m_LastTranslation;
    AnimSimple m_AnimSimple;

    public Entity m_Entity;
    public Entity m_EntityPrefab;
    public EntityManager m_Mgr;
    int m_EntityID;

    public static EntityArchetype m_Archetype;
    public static bool m_ArchetypeCreated;

    public float m_RespawnTime;

	public void OnEnable()
	{
        CreateEntity();
    }

    public void OnDisable()
	{
        DestroyEntity();
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
            m_Archetype = m_Mgr.CreateArchetype(
                typeof(CTranslation2D), typeof(CAcceleration2D), typeof(CVelocity2D), 
                typeof(CCollisionResult2D), typeof(CCollision), typeof(CGoal2D), 
                typeof(CAvoidance2D), typeof(CAvoidanceTarget2D),
                typeof(CAttackHits) );
            m_ArchetypeCreated = true;
        }

        float scale = transform.localScale.y;

        m_Entity = m_Mgr.CreateEntity(m_Archetype);

        m_EntityID = m_Entity.Index | (int)((m_Entity.Version << 16) & 0xFFFF0000);

        CTranslation2D trans = m_Mgr.GetComponentData<CTranslation2D>(m_Entity);
        trans.m_Translation.x = transform.position.x;
        trans.m_Translation.y = transform.position.y;
        m_Mgr.SetComponentData<CTranslation2D>(m_Entity, trans);

        CAcceleration2D acc = m_Mgr.GetComponentData<CAcceleration2D>(m_Entity);
        acc.m_Acceleration = m_Acceleration;
        acc.m_Speed = m_Speed;
        m_Mgr.SetComponentData<CAcceleration2D>(m_Entity, acc);

        CCollision col = m_Mgr.GetComponentData<CCollision>(m_Entity);
        col.m_ID = m_EntityID;
        if (MonsterSystem.Singleton.m_Collision)
            col.m_LayerMask = 1 << gameObject.layer;
        else
            col.m_LayerMask = 0;
        col.m_Radius = m_Radius * scale;
        col.m_Enabled = true;
        m_Mgr.SetComponentData<CCollision>(m_Entity, col);

        CAvoidance2D avd = m_Mgr.GetComponentData<CAvoidance2D>(m_Entity);
        avd.m_ID = m_EntityID;
        avd.m_LayerMask = 1 << gameObject.layer;
        avd.m_Radius = m_Radius * 1.5f * scale;
        avd.m_Weight = 1.0f;
        m_Mgr.SetComponentData<CAvoidance2D>(m_Entity, avd);

        CGoal2D goal = m_Mgr.GetComponentData<CGoal2D>(m_Entity);
        goal.m_Position = float2.zero;
        m_Mgr.SetComponentData<CGoal2D>(m_Entity, goal);

        m_AnimSimple = gameObject.GetComponent<AnimSimple>();
    }

    public void DestroyEntity()
    {
        if (m_Entity != Entity.Null && World.DefaultGameObjectInjectionWorld != null)
        {
            m_Mgr.DestroyEntity(m_Entity);
            m_Entity = Entity.Null;
        }
    }

    public void Die()
    {
        DestroyEntity();

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        sprite.enabled = false;

        m_RespawnTime = Time.time + 1.0f;
    }

    public void Respawn()
	{
        MonsterSystem.Singleton.Respawn(gameObject);

        CreateEntity();

        CTranslation2D trans = m_Mgr.GetComponentData<CTranslation2D>(m_Entity);
        trans.m_Translation.x = transform.position.x;
        trans.m_Translation.y = transform.position.y;
        m_Mgr.SetComponentData<CTranslation2D>(m_Entity, trans);

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        sprite.enabled = true;

        m_RespawnTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_RespawnTime > 0.0f)
        {
            // Dead
            if (Time.time >= m_RespawnTime)
                Respawn();
        }
        else
        {
            // Alive
            CTranslation2D trans = m_Mgr.GetComponentData<CTranslation2D>(m_Entity);
            if (m_LastTranslation.x != trans.m_Translation.x || m_LastTranslation.y != trans.m_Translation.y)
            {
                transform.position = new Vector3(trans.m_Translation.x, trans.m_Translation.y, 0);
            }
            m_LastTranslation = trans.m_Translation;

            if ( trans.m_HitCount > 0 )
			{
                CAttackHits hits = m_Mgr.GetComponentData<CAttackHits>(m_Entity);
                for (int i = 0; i < trans.m_HitCount; i++)
                {
                    // Process hit from an attack matching that entity id
                    Attack2D atk;
                    if (Attack2D.m_Dictionary.TryGetValue(hits.m_Hits[i], out atk))
                    {
                        // Process damage from the Attack2D gameobject
                        Die();
                        return;
                    }
                }

                trans.m_HitCount = 0;
                m_Mgr.SetComponentData<CTranslation2D>(m_Entity, trans );
            }

            if (m_AnimSimple != null)
            {
                CVelocity2D vel = m_Mgr.GetComponentData<CVelocity2D>(m_Entity);
                m_AnimSimple.Play(new Vector3(vel.m_Velocity.x, vel.m_Velocity.y, 0));
            }
        }
    }
}
