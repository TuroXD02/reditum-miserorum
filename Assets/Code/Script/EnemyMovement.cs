using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] public float moveSpeed;

    private Transform target;
    
    private int pathIndex = 0;

    private float baseSpeed;

    

    private void Start()
    {
        baseSpeed = moveSpeed;
        target = LevelManager.main.path[pathIndex]; //inizio partita definisce il percorso seguendo i numeri sul path in unity
    }

    private void Update()
    {
        if (Vector2.Distance(target.position, transform.position) <= 0.2f)
        {
            pathIndex++; //quando il nemico va su un path, aumenta il path index creano un vettore
            //quando enemy sale su path cambia

            if (pathIndex == LevelManager.main.path.Length)
                //se il numero del path � uguaale al massimo, distruggi il game obj (enemy) e richiama on enemy detstroy
            {
                EnemySpawner.onEnemyDestroy.Invoke();
                Destroy(gameObject);
                return;
            }
            else
            {
                target = LevelManager.main.path[pathIndex];
                //negli altri casi il target viene impostato il prossimo in linea
            }
        }
    }

    private void FixedUpdate()
    {
        Vector3 RandomTarget = new Vector3(Random.Range(target.position.x - 0.4f, target.position.x + 0.4f), Random.Range(target.position.y - 1, target.position.y + 1), 0);
        Vector2 direction = (RandomTarget - transform.position).normalized;
        //crea un vettore in 2d che � composto tra posizione enemy e posizione, normalizzato in maniera tale che sia di modulo 1 ovvero che non varia velocit� 

        rb.velocity = direction * moveSpeed;
    }


    public void UpdateSpeed(float newSpeed) 
    {
        moveSpeed = newSpeed;
    }

    public void ResetSpeed()
    {
        moveSpeed = baseSpeed;
    }
}