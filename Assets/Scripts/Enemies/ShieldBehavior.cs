using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldBehavior : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            /*sStun = true; // Add bool isStun to the player and if it's true then he can't move for a certain amount of time then isStun = false
            GameObject.Find("Player").GetComponent<Rigidbody2D>().velocity = Vector2.zero;*/
        }
    }
}
