using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ECS /////////////
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Collections;

using Random = UnityEngine.Random;

public class BoidSystem : MonoBehaviour
{
    public GameObject m_BoidPrefab;
    public Bounds m_Bounds;
    public int m_Count;
    public bool m_Collision = true;

    static int m_GroupNext;

    private int m_Group;

    ////////////////////
    // ECS /////////////
    private EntityManager m_ECSMgr;
    private Entity m_ECSPrefab;

    private void Start()
    {
        m_ECSMgr = World.DefaultGameObjectInjectionWorld.EntityManager;

        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);

        m_ECSPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(m_BoidPrefab, settings);

        m_Group = m_GroupNext++;

        m_Bounds.center = transform.position;

        Spawn();
    }

	private void Spawn()
    {
        NativeArray<Entity> enemyArray = new NativeArray<Entity>(m_Count, Allocator.Temp);

        for (int i = 0; i < enemyArray.Length; i++)
        {
            enemyArray[i] = m_ECSMgr.Instantiate(m_ECSPrefab);

            float3 pos = new float3();
            pos.x = math.lerp(m_Bounds.min.x, m_Bounds.max.x, Random.value);
            pos.y = math.lerp(m_Bounds.min.y, m_Bounds.max.y, Random.value);
            pos.z = math.lerp(m_Bounds.min.z, m_Bounds.max.z, Random.value);

            m_ECSMgr.SetComponentData(enemyArray[i], new Translation { Value = pos });
            m_ECSMgr.SetComponentData(enemyArray[i], new Rotation { Value = quaternion.EulerXYZ( Random.value * 360, Random.value * 360, Random.value * 360) });

            CBoidTag data = m_ECSMgr.GetComponentData<CBoidTag>(enemyArray[i]);
            data.m_Group = m_Group;
            data.m_Bounds = m_Bounds;
            m_ECSMgr.SetComponentData(enemyArray[i], data );

            CCollision col = m_ECSMgr.GetComponentData<CCollision>(enemyArray[i]);
            col.m_Enabled = m_Collision;
            m_ECSMgr.SetComponentData(enemyArray[i], col);
        }

        enemyArray.Dispose();
    }
}