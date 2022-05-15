using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ECS /////////////
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;

using Random = UnityEngine.Random;

public class BoidSystem : MonoBehaviour
{
    public List<GameObject> m_BoidPrefabs = new List<GameObject>();
    public Bounds m_Bounds;
    public int m_Count;
    public bool m_Collision = true;

    ////////////////////
    // ECS /////////////
    private EntityManager m_ECSMgr;
    private List<Entity> m_ECSPrefabs = new List<Entity>();

    private static int s_BoidIdNext = 1;

    private void Start()
    {
        m_ECSMgr = World.DefaultGameObjectInjectionWorld.EntityManager;

        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);

        foreach( GameObject prefab in m_BoidPrefabs )
		{
            m_ECSPrefabs.Add( GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, settings) );
        }

        m_Bounds.center = transform.position;

        Spawn();
    }

    private bool TouchesAnother( float3[] list, int listCount, float3 test )
	{
        float radiusSq = 0.5f * 0.5f;
        for ( int i = 0; i < listCount; i++)
        {
            if (math.distancesq( list[i], test ) <= radiusSq)
                return true;
        }
        return false;                
	}

	private void Spawn()
    {
        NativeArray<Entity> enemyArray = new NativeArray<Entity>(m_Count, Allocator.Temp);

        float3[] spawnPositions = new float3[enemyArray.Length];

        // Get set of spawn positions that are not touching eachother
        for ( int i = 0; i < enemyArray.Length; i++ )
		{
            float3 pos = new float3();

            int failSafe = 0;
            do
            {
                pos.x = math.lerp(m_Bounds.min.x, m_Bounds.max.x, Random.value);
                pos.y = math.lerp(m_Bounds.min.y, m_Bounds.max.y, Random.value);
                pos.z = math.lerp(m_Bounds.min.z, m_Bounds.max.z, Random.value);
                failSafe++;

            } while ( failSafe < 20 && TouchesAnother(spawnPositions, i - 1, pos ) );

            spawnPositions[i] = pos;
        }

        for (int i = 0; i < enemyArray.Length; i++)
        {
            int groupId = Random.Range(0, m_ECSPrefabs.Count);
            Entity entityPrefab = m_ECSPrefabs[groupId];

            enemyArray[i] = m_ECSMgr.Instantiate(entityPrefab);

            m_ECSMgr.SetComponentData(enemyArray[i], new Translation { Value = spawnPositions[i] });
            m_ECSMgr.SetComponentData(enemyArray[i], new Rotation { Value = quaternion.EulerXYZ( Random.value * 360, Random.value * 360, Random.value * 360) });

            CBoidTag data = m_ECSMgr.GetComponentData<CBoidTag>(enemyArray[i]);
            data.m_Group = groupId;
            data.m_Bounds = m_Bounds;
            data.m_ID = s_BoidIdNext++;
            m_ECSMgr.SetComponentData(enemyArray[i], data );

            CCollision col = m_ECSMgr.GetComponentData<CCollision>(enemyArray[i]);
            col.m_Enabled = m_Collision;
            m_ECSMgr.SetComponentData(enemyArray[i], col);
        }

        enemyArray.Dispose();
    }
}