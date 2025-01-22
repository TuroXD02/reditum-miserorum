using System;
using Unity.VisualScripting;
using UnityEngine;


[Serializable] // Marks this class as serializable, enabling it to be editable in Unity's Inspector.
public class Tower
{
    // The name of the tower (e.g., "Arrow Tower", "Cannon Tower").
    public string name;

    // The in-game currency cost to build this tower (e.g., 100 coins).
    public int cost;

    // The GameObject prefab that represents this tower in the game world.
    public GameObject prefab;

    //Constructor for the Tower class: initializes a new Tower object with the specified name, cost, and prefab.
    // public Tower(string _name, int _cost, GameObject _prefab)
    // {
    //     // Assigns the provided values (_name, _cost, _prefab) to the class fields (name, cost, prefab).
    //     name = _name;
    //     cost = _cost;
    //     prefab = _prefab;
    // }

    //string[] _defences = { "Turret", "TurretSlow", "TurretLongRange", "TurretPoison" };
        
    //for (int i = 0; i < _defences.Length; i++)
   // {Debug.Log(_defences[i])}

    

    //List<Tower> towers = new List<Tower>();
    //towers.Add(new Tower("Cannon", 100, prefab));


    //A collection of key-value pairs where each key is unique.
    //Dictionary<string, int> inventory = new Dictionary<string, int>();
    //inventory["Potion"] = 5;
}

