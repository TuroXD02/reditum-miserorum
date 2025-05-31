using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthMonitor : MonoBehaviour
{
    private EnemyHealth enemyHealth;
    private LussuriaHealth lussuriaHealth;
    private AbaddonEnemyHealth abaddonHealth;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth == null)
        {
            lussuriaHealth = GetComponent<LussuriaHealth>();
        }
        if (enemyHealth == null && lussuriaHealth == null)
        {
            abaddonHealth = GetComponent<AbaddonEnemyHealth>();
        }
    }

    private void Update()
    {
        int currentHP = 0;
        string enemyType = "";

        if (enemyHealth != null)
        {
            currentHP = enemyHealth.HitPoints;
            enemyType = "EnemyHealth";
        }
        else if (lussuriaHealth != null)
        {
            currentHP = lussuriaHealth.HitPoints;
            enemyType = "LussuriaHealth";
        }
        else if (abaddonHealth != null)
        {
            currentHP = abaddonHealth.HitPoints;
            enemyType = "AbaddonEnemyHealth";
        }
        else
        {
            Debug.LogWarning("EnemyHealthMonitor: No health component found on " + gameObject.name);
            return;
        }

       
    }
}