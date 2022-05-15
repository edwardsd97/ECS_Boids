using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSystem : MonoBehaviour
{
    static MonsterSystem s_MonsterSystem;

    public GameObject m_Prefab;
    public int m_Count;
    public float m_Radius;
    public float m_MinScale = 1.0f;
    public float m_MaxScale = 1.0f;
    public bool m_Collision = true;

    List<GameObject> m_Instances = new List<GameObject>();

    static public MonsterSystem Singleton { get { return s_MonsterSystem; } }

    // Start is called before the first frame update
    void Start()
    {
        s_MonsterSystem = this;

        for (int i = 0; i < m_Count; i++ )
        {
            GameObject go = GameObject.Instantiate(m_Prefab);

            m_Instances.Add(go);

            Respawn(go);

            Entity2D ent2D = go.GetComponent<Entity2D>();
            if (ent2D != null)
            {
                ent2D.CreateEntity();
            }
        }
    }

    public float Respawn( GameObject go )
	{
        float scale = Mathf.Lerp(m_MinScale, m_MaxScale, UnityEngine.Random.value);
        go.transform.localScale = Vector3.one * scale;
        go.transform.position = transform.position + Quaternion.Euler(0, 0, UnityEngine.Random.value * 360.0f) * Vector3.up * m_Radius * (1.0f + UnityEngine.Random.value * 0.5f);
        return scale;
    }
}
