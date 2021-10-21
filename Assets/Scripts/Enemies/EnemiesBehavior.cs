using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesBehavior : MonoBehaviour
{
    public bool withShield;
    bool canDash, isReloadingTheDash;

    public float timer= 0f;
    public float speedEnemy = 0f;
    public float rotationShieldSpeed = 0f;
    float distance = 0f;
    public float distanceSeeingByEnemy = 0f;
    public float timerBeforeDash = 5f;

    Transform player;
    Rigidbody2D rb;
    public GameObject shield;

    Vector2 movement;

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Transform>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        shield.SetActive(false);
        isReloadingTheDash = false;
    }

    void Update()
    {
        distance = Vector2.Distance(gameObject.transform.position, player.position);
        Vector2 direction = gameObject.transform.position - player.position; // gives the direction between the enemy and the player
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // gives the angle between the enemy and the player
        rb.rotation = angle;                                                 // the rotation is refreshing every frame to point to the player
        direction.Normalize();                                               
        movement = direction;                                                
                                
        // if the enemy and the player are at a certain distance and the enemy can't dash, the enemy can dash
        if(distance <= distanceSeeingByEnemy && !canDash)
        {
            canDash = true;            
        }

        // time to reload before the enemy will dash again
        if (isReloadingTheDash)
        {
            timer += Time.deltaTime;
            if(timer >= timerBeforeDash)
            {
                timer = 0f;
                isReloadingTheDash = false;
            }

        }
        // If the shield is activated so the enemy have a shield rotating around him with a speed
        if (withShield)
        {
            shield.SetActive(true);
            shield.transform.RotateAround(gameObject.transform.position, Vector3.forward, rotationShieldSpeed * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (!canDash)
        {
            MoveEnemy(movement);
        } else if(canDash && !isReloadingTheDash)
        {
            StartCoroutine(IsDashing(movement));
        }
    }

    void MoveEnemy(Vector2 direction)
    {
        rb.MovePosition((Vector2)transform.position - (direction * speedEnemy * Time.deltaTime)); // the enemy's moving to the player at a certain speed through time
    }

    IEnumerator IsDashing(Vector2 direction)
    {
        rb.velocity = Vector2.zero;                                                         // stopped his speed to prepare the dash
        // Dash Animation 
        yield return new WaitForSeconds(1.5f);
        rb.MovePosition((Vector2)transform.position - (direction * 125f * Time.deltaTime)); // the enemy's dashing to the player
        isReloadingTheDash = true;                                                          // the enemy's reloading his dash
        timer = 0f;
        canDash = false;                                                                    // can't dash
    }
}
