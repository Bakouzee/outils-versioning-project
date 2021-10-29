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

    Vector2 input;
    bool freeMovement;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Initialize current speed
        currentSpeed = normalSpeed;

        freeMovement = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameHandler.Paused)
        {
            input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        }
    }
    private void FixedUpdate()
    {
        if (freeMovement) rb.velocity = input * currentSpeed;
    }

    public void SetSpeed(bool furyMode) // Called from Player scrip => change speed
    {
        currentSpeed = furyMode ? furySpeed : normalSpeed;
    }

    public Vector2 GetRbDirection() => rb.velocity.normalized;
    public void SetRbVelocity(Vector2 velocity)
    {
        rb.velocity = velocity;
    }
    public void ResetRbVelocity() { rb.velocity = Vector2.zero; }
    public void SetFreeMovement(bool freeMovement)
    {
        this.freeMovement = freeMovement;
    }
}
