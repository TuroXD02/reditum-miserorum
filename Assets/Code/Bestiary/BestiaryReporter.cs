using UnityEngine;

public class BestiaryReporter : MonoBehaviour
{
    [SerializeField] private string enemyID;

    private void Start()
    {
        if (!string.IsNullOrEmpty(enemyID))
        {
            BestiaryManager.Instance?.DiscoverEnemy(enemyID);
        }
        else
        {
            Debug.LogWarning("[BestiaryReporter] Missing enemyID on: " + gameObject.name);
        }
    }
}