using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesBehavior : MonoBehaviour
{
    public bool withShield;
    bool isDashing;

    public float timerBeforeDash = 0f;
    public float distance = 2.0f;
    float timer = 0f;

    Transform player;
    public GameObject shield;

    void Start()
    {
        timer = Random.Range(1.5f, 3.5f);
        player = GameObject.Find("Player").GetComponent<Transform>();
        withShield = false;
        isDashing = false;
        shield.SetActive(false); 
    }

    void Update()
    {
        timerBeforeDash += Time.deltaTime;

       // float distance = Vector2.Distance(gameObject.transform.position, player.position);

        /*if(distance >= 0 && isDashing == false)
        {
            gameObject.transform.position = Vector2.MoveTowards(gameObject.transform.position, player.position, distance) * 0.5f;
        }*/

       /* if(timerBeforeDash >= timer)
        {
            isDashing = true;
            timer = Random.Range(1.5f, 3.5f);
        }

        if(isDashing == true)
        {
            gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.MoveTowards(gameObject.transform.position, player.position, distance) * 2.0f;
            timer = 0f;
        }*/

        if(withShield == true)
        {
            shield.SetActive(true);
            shield.transform.RotateAround(gameObject.transform.position, shield.transform.position, 60f);
        }
    }
}
