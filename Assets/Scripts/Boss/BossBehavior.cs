using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBehavior : MonoBehaviour
{
    bool isDead;
    bool canMove;
    bool isAttacking;

    float distance;
    public float speedBoss;
    float timer = 0f;

    int randTiming = 0;
    int randAttack;
    public int bossHealth = 100;

    Vector2 movement;

    Transform player;
    Rigidbody2D rb;

    private void Start()
    {
        randTiming = Random.Range(5, 10);
        Debug.Log(randTiming);
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.Find("Player").GetComponent<Transform>();
        canMove = true;
    }

    private void Update()
    {
        distance = Vector2.Distance(gameObject.transform.position, player.position);
        Vector2 direction = gameObject.transform.position - player.position; // gives the direction between the enemy and the player
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // gives the angle between the enemy and the player
        rb.rotation = angle;                                                 // the rotation is refreshing every frame to point to the player
        direction.Normalize();                                               // the length of the vector will always be 1 and always pointed to the same direction
        movement = direction;

        if (canMove)
        {
            BossMove(movement);
        }

        timer += Time.deltaTime;
        

        if (timer >= randTiming && (!isDead && !isAttacking))
        {            
            StartCoroutine(BossTakingABreath());            
        }

        if (isAttacking)
        {
            randAttack = Random.Range(0, 3);
            Debug.Log(randAttack);
            switch (randAttack)
            {
                case 0:
                    Dash();
                    break;
                case 1:
                    Shield();
                    break;
                case 2:
                    ShurikenAOE();
                    break;
                default:
                    break;
            }
        }
    }
    
    void Dash()
    {
        Debug.Log("Dash");
        canMove = true;
        isAttacking = false;
        timer = 0f;
        randTiming = Random.Range(5, 10);
        Debug.Log(randTiming);
    }

    void Shield()
    {
        Debug.Log("Shield");
        canMove = true;
        isAttacking = false;
        timer = 0f;
        randTiming = Random.Range(5, 10);
        Debug.Log(randTiming);
    }

    void ShurikenAOE()
    {
        Debug.Log("Shuriken");
        canMove = true;
        isAttacking = false;
        timer = 0f;
        randTiming = Random.Range(5, 10);
        Debug.Log(randTiming);
    }

    public void TakeDamage()
    {
        // Anim TakeDamage
        bossHealth -= 5;
    }

    void Dead()
    {
        // Anim death
        isDead = true;
    }

    IEnumerator BossTakingABreath()
    {
        canMove = false;
        yield return new WaitForSeconds(5.0f);
        isAttacking = true;
    }

    void BossMove(Vector2 direction)
    {
        rb.MovePosition((Vector2)transform.position - (direction * speedBoss * Time.deltaTime)); // the enemy's moving to the player at a certain speed through time
    }
}
