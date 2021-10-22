using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesBehavior : MonoBehaviour
{
    [Header("Enemy Settings")]
    public bool withShield;
    public bool closeCombat;

    [Header("Enemy Health")]
    public int health = 25;

    [Header("Informations")]
    bool canDash;
    bool isReloadingTheDash;
    bool playerLoseHealth;
    bool canMove;

    public float timer= 0f;
    public float speedEnemy = 0f;
    public float rotationShieldSpeed = 0f;
    float distance = 0f;
    public float distanceSeeingByEnemy = 0f;
    public float timerBeforeDash = 5f;
    public float range = 2.0f;
    float attackRate = 0f;
    [SerializeField, Range(0f, 15f)] float attackRadius;


    Vector2 movement;
    Vector2 attackRange = new Vector2(2.0f, -2.0f);

    public LayerMask playerLayer;

    Transform player;
    public Transform attack;
    Rigidbody2D rb;
    public GameObject shield;


    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Transform>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        shield.SetActive(false);
        isReloadingTheDash = false;
        playerLoseHealth = false;
        canMove = true;
    }

    void Update()
    {
        distance = Vector2.Distance(gameObject.transform.position, player.position);
        Vector2 direction = gameObject.transform.position - player.position; // gives the direction between the enemy and the player
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // gives the angle between the enemy and the player
        rb.rotation = angle;                                                 // the rotation is refreshing every frame to point to the player
        direction.Normalize();                                               // the length of the vector will always be 1 and always pointed to the same direction
        movement = direction;

        if (closeCombat)
        {
            if(distance <= range && playerLoseHealth == false)
            {
                Attack();
            }
            if (playerLoseHealth)
            {
                attackRate += Time.deltaTime;
                if(attackRate >= 1.5f)
                {
                    playerLoseHealth = false;
                    canMove = true;
                    attackRate = 0f;
                }
            }
        }
        else
        {
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
        }

        // If the shield is activated so the enemy have a shield rotating around him with a speed
        if (withShield)
        {
            shield.SetActive(true);
            shield.transform.RotateAround(gameObject.transform.position, Vector3.forward, rotationShieldSpeed * Time.deltaTime);
        }

        if(health <= 0)
        {
            Dead();
        }
    }

    private void FixedUpdate()
    {
        if (!canDash && canMove)
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

    void Attack()
    {
        canMove = false;
        Collider2D[] hit = Physics2D.OverlapCircleAll(attack.position, attackRadius, playerLayer);
        // Attack animation
        foreach (Collider2D player in hit)
        {
            //GameObject.Find("Player").GetComponent<HealthDisplayer>().health -= 2; // when the player hits by the collider then he's loosing 2hp
            playerLoseHealth = true;
        }
    }

    void TakeDamage()
    {
        // Anim TakeDamage
        health -= 5;
    }

    void Dead()
    {
        this.enabled = false;        // script disable to avoid problems
        gameObject.SetActive(false); // gameObject disable
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(attack.position, attackRadius);
    }
}
