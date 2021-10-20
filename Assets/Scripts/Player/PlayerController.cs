using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    float currentSpeed;
    [SerializeField] float normalSpeed;
    [SerializeField] float furySpeed;

    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Initialize current speed
        currentSpeed = normalSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        // Movement
        rb.velocity = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized * currentSpeed;
    }

    public void SetSpeed(bool furyMode) // Called from Player scrip => change speed
    {
        currentSpeed = furyMode ? furySpeed : normalSpeed;
    }
}
