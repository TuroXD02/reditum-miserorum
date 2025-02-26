using UnityEngine;

public class EnemyHealthMonitor : MonoBehaviour
{
    private EnemyHealth enemyHealth;
    private LussuriaHealth lussuriaHealth;

    private void Awake()
    {
        // Try to get an EnemyHealth component.
        enemyHealth = GetComponent<EnemyHealth>();
        // If not found, try to get a LussuriaHealth component.
        if (enemyHealth == null)
        {
            lussuriaHealth = GetComponent<LussuriaHealth>();
        }
    }

    private void Update()
    {
        int currentHP = 0;
        string enemyType = "";

        if (enemyHealth != null)
        {
            currentHP = enemyHealth.hitPoints; // EnemyHealth's hitPoints is public.
            enemyType = "EnemyHealth";
        }
        else if (lussuriaHealth != null)
        {
            currentHP = lussuriaHealth.HitPoints; // Uses the public property in LussuriaHealth.
            enemyType = "LussuriaHealth";
        }
        else
        {
            Debug.LogWarning("EnemyHealthMonitor: No health component found on " + gameObject.name);
            return;
        }

        // Display the current health in the console.
      

        // Optionally, you could check for death (though both health scripts already handle destruction).
        if (currentHP <= 0)
        {
            Debug.Log($"{gameObject.name} is dead.");
        }
    }
}