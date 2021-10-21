using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesBehavior : MonoBehaviour
{
    public bool withShield;
    bool canDash;

    public float timerBeforeDash = 0f;
    public float speedEnemy;

    Transform player;
    Rigidbody2D rb;
    public GameObject shield;

    Vector2 movement;

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Transform>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        shield.SetActive(false); 
    }

    void Update()
    {
        Vector2 direction = gameObject.transform.position - player.position; // gives the direction between the enemy and the player
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // gives the angle between the enemy and the player
        rb.rotation = angle;                                                 // the rotation is refreshing every frame to point to the player
        direction.Normalize();                                               
        movement = direction;                                                
                                                                             
        if (withShield)
        {
            shield.SetActive(true);
            shield.transform.RotateAround(gameObject.transform.position, shield.transform.position, 60f);
        }
    }

    private void FixedUpdate()
    {
        MoveEnemy(movement);
    }

    void MoveEnemy(Vector2 direction)
    {
        rb.MovePosition((Vector2)transform.position - (direction * speedEnemy * Time.deltaTime)); // the enemy's moving to the player at a certain speed through time
    }
}
