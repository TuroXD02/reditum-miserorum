using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager main;
    public static LevelManager instance;

    public Transform startPoint;
    public Transform[] path;

    public int currency;

    
    private void Start()
    {
        currency = 1000;
        GetComponent<PlayerHealthSystem>().Init();
    }
    private void Awake()
    {
        main = this;
        instance = this;
    }

    
//serve anche quando vendi una torre
    public void IncreaseCurrency(int amount) //soldi totali aumentano
    {
        currency += amount;
        
    }

    public bool SpendCurrency(int amount) //diminuiscono
    {
        if (amount <= currency)
        {
            currency -= amount;
            return true;
        }
        else
        {
            Debug.Log("You don't have enough money.");
            return false;
        }
    }

    // New AddCurrency method 
    public void AddCurrency(int amount)
    {
        IncreaseCurrency(amount);
        
    }
}
