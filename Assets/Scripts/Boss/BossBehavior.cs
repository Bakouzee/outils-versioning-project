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
    public float rotationSpeed = 0f;
    public float speedShuriken = 0f;
    float timer = 0f;
    float timerAttacking = 0f;
    float timerShields = 5f;

    int randTiming = 0;
    public int randAttack = int.MinValue;
    public int bossHealth = 100;

    Vector2 movement;

    Transform player;
    Rigidbody2D rb;
    public GameObject[] shields;
    public GameObject shuriken;
    GameObject shurikenClone;


    private void Start()
    {
        for(int i = 0; i < shields.Length; i++)
        {
            shields[i].SetActive(false);
        }
        randTiming = Random.Range(5, 10);
        Debug.Log(randTiming);
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.Find("Player").GetComponent<Transform>();
        isDead = false;
        canMove = true;
        isAttacking = false;
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

        if (timer >= randTiming && !isAttacking)
        {
            StartCoroutine(ChooseAnAttack());
        }

        // Attacks
        if (randAttack == 0) // Shields
        {
            for (int i = 0; i < shields.Length; i++)
            {
                shields[i].transform.RotateAround(gameObject.transform.position, Vector3.forward, rotationSpeed * Time.deltaTime);
            }

            timerAttacking += Time.deltaTime;

            // Reinitialisation
            if (timerAttacking >= timerShields)
            {
                for (int i = 0; i < shields.Length; i++)
                {
                    shields[i].SetActive(false);
                }
                randTiming = Random.Range(5, 10);
                isAttacking = false;
                timer = 0f;
                timerAttacking = 0f;
                randAttack = 3;
            }
        }
        else if (randAttack == 1) // Shuriken
        {
            float anAngle = ((2 * Mathf.PI) / 8f);
            float nextAngle = 0f;

            for (int i = 0; i < 8; i++)
            {
                Vector2 directionShuriken = new Vector2(Mathf.Cos(nextAngle), Mathf.Sin(nextAngle)).normalized;

                shurikenClone = Instantiate(shuriken, transform.position, Quaternion.identity);
                shurikenClone.GetComponent<Rigidbody2D>().velocity = directionShuriken * speedShuriken;

                nextAngle += anAngle;
            }

            randAttack = 3;
        }
        else if (randAttack == 2) // Dash
        {
            StartCoroutine(ChooseAnAttack());

        }
        if (bossHealth <= 0)
        {
            Dead();
        }
    }

    void Shield()
    {
        Debug.Log("Shield");
        for (int i = 0; i < shields.Length; i++)
        {
            shields[i].SetActive(true);
        }
        canMove = true;
    }

    void ShurikenAOE()
    {
        Debug.Log("Shuriken");

        //StartCoroutine(ChooseAnAttack());
    }

    void Dash()
    {
        Debug.Log("Dash");
        //StartCoroutine(ChooseAnAttack());
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

    IEnumerator ChooseAnAttack()
    {
        isAttacking = true;
        canMove = false;
        randAttack = Random.Range(0, 3);
        yield return new WaitForSeconds(2.0f);
        switch (randAttack)
        {
            case 0:
                Shield();
                break;
            case 1:
                ShurikenAOE();
                break;
            case 2:
                Dash();
                break;
            default:
                break;
        }
    }

    void BossMove(Vector2 direction)
    {
        rb.MovePosition((Vector2)transform.position - (direction * speedBoss * Time.deltaTime)); // the enemy's moving to the player at a certain speed through time
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
            //envoyer raycast, choper la direction, si t'arrives à destination tu prends la nouvelle
    }
}
