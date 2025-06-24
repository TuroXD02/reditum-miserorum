using System.Collections.Generic;
using UnityEngine;

public class BestiaryManager : MonoBehaviour
{
    public static BestiaryManager Instance { get; private set; }

    [SerializeField] private List<BestiaryEntry> allEntries;

    private HashSet<string> discoveredEnemyIDs = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    

    public void DiscoverEnemy(string enemyID)
    {
        if (discoveredEnemyIDs.Add(enemyID))
        {
            Debug.Log($"Discovered enemy: {enemyID}");
        }
    }

    public bool IsDiscovered(string enemyID)
    {
        return discoveredEnemyIDs.Contains(enemyID);
    }

    public List<BestiaryEntry> GetAllEntries()
    {
        return allEntries;
    }
}
