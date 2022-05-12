using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class Boid : MonoBehaviour
{
}

/*
public class BoidSysCohesion : ComponentSystem
{
    // 1
    float thresholdDistanceSq = (2f * 2f);

    protected override void OnUpdate()
    {
        Entities.WithAll<BoidDataCohesion>().ForEach((Entity boid, ref Translation boidPos) =>
        {
            // 5
            playerPosition.y = enemyPos.Value.y;

            // 6
            if (math.distancesq(enemyPos.Value, playerPosition) <= thresholdDistanceSq)
            {
                //              FXManager.Instance.CreateExplosion(enemyPos.Value);
                //              FXManager.Instance.CreateExplosion(playerPosition);
                // GameManager.EndGame();

                // 7
                PostUpdateCommands.DestroyEntity(enemy);
            }

            // 8
            float3 enemyPosition = enemyPos.Value;

            // 9
            Entities.WithAll<BulletTag>().ForEach((Entity bullet, ref Translation bulletPos) =>
            {
                // 10
                if (math.distancesq(enemyPosition, bulletPos.Value) <= thresholdDistanceSq)
                {
                    PostUpdateCommands.DestroyEntity(enemy);
                    PostUpdateCommands.DestroyEntity(bullet);

                    //11
                    //     FXManager.Instance.CreateExplosion(enemyPosition);
                    GameManager.AddScore(1);
                }
            });
        });
    }
}


// Update is called once per frame
void Update()
    {
        Vector3 direction = (m_Cohesion + m_Separation + m_Alignment) - transform.position;

        if (transform.position.x > m_BoidSys.m_ArenaSize.x || transform.position.x < -m_BoidSys.m_ArenaSize.x ||
            transform.position.y > m_BoidSys.m_ArenaSize.y || transform.position.y < -m_BoidSys.m_ArenaSize.y ||
            transform.position.z > m_BoidSys.m_ArenaSize.z || transform.position.z < -m_BoidSys.m_ArenaSize.z)
        {
            // out of bounds - face toward center of boid system
            direction = m_BoidSys.transform.position - transform.position;
        }

        if (direction != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), m_RotateSpeed * Time.deltaTime);

        transform.Translate(0, 0, m_Speed * Time.deltaTime, Space.Self);

        m_ThreadPos = transform.position;
        m_ThreadRot = transform.rotation;
    }

    public void UpdateBoidInfluences()
    {
        // Called from a separate thread than main - cannot access Unity data

        IEnumerable enumerate;

            // Iterate the list of all boids
//            enumerate = m_BoidSys.Boids;

        float separationCount = 0;
        float alignmentCount = 0;
        float cohesionCount = 0;

        float separationRadiusSq = m_BoidSys.m_SeparationRadius * m_BoidSys.m_SeparationRadius;
        float alignmentRadiusSq = m_BoidSys.m_AlignmentRadius * m_BoidSys.m_AlignmentRadius;
        float cohesionRadiusSq = m_BoidSys.m_CohesionRadius * m_BoidSys.m_CohesionRadius;

        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;

        float cohesionSpeed = m_Speed;

        foreach (Boid boid in enumerate)
        {
            if (boid == this)
                continue;

            Vector3 delta = m_ThreadPos - boid.m_ThreadPos;

            float distSq = delta.sqrMagnitude;

            if (distSq <= separationRadiusSq)
            {
                separationCount += 1.0f;
                separation += delta;
            }

            if (distSq <= alignmentRadiusSq)
            {
                alignmentCount += 1.0f;
                alignment += boid.m_ThreadRot * Vector3.forward;
            }

            if (distSq <= cohesionRadiusSq)
            {
                cohesionCount += 1.0f;
                cohesion += boid.m_ThreadPos;
                cohesionSpeed += boid.m_Speed;
            }
        }

        if (cohesionCount > 0)
        {
            m_Speed = cohesionSpeed / (cohesionCount + 1);
            m_Cohesion = (cohesion / cohesionCount);
        }
        else
        {
            m_Cohesion = m_ThreadPos;
        }

        if (separationCount > 0)
            m_Separation = separation / separationCount;
        else
            m_Separation = Vector3.zero;

        if (alignmentCount > 0)
            m_Alignment = alignment / alignmentCount;
        else
            m_Alignment = m_ThreadRot * Vector3.forward;

        if (m_BoidSys.m_UseGrid)
            m_BoidSys.m_Grid.SetPosition(this, m_ThreadPos);
    }
}
*/