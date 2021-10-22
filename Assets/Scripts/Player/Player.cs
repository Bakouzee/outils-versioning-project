using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(PlayerController))]
public class Player : MonoBehaviour
{
    PlayerController controller;
    [SerializeField] SpriteRenderer spRenderer;
    new CircleCollider2D collider;
    Camera cam;

    [Header("Health"), SerializeField] float maxHealth;
    float health;
    bool dead;
    [Header("Invulnerability and knockback"), SerializeField] float invulnerabilityTimer = 0.5f;
    bool invulnerable;
    [SerializeField] float knockBackSpeed = 1f;
    [SerializeField] float knockBackDuration = 0.5f;
    bool stagger;

    [Header("Stamina"), SerializeField] float staminaMax;
    [SerializeField] float staminaGainBySec;
    float stamina;

    [Header("Fury"), SerializeField] float furyAmountMax = 100f;
    [SerializeField] float furyGainByAttack;
    [SerializeField] float furyDecreaseRate;
    float furyAmount;
    bool furyMode;

    [Header("Dash attack")]
    [SerializeField] float dashSpeed;
    [SerializeField] float dashDuration;
    [SerializeField] float dashStaminaCost;
    bool dashing;
    List<EnemiesBehavior> enemiesTouched = new List<EnemiesBehavior>();

    [Header("Shurikens attack"), SerializeField] GameObject shurikenPrefab;
    [SerializeField] int shurikenLaunchedByAttack;
    [SerializeField] float timeBetweenTwoShurikens;
    [Space(5), SerializeField] float shurikensAttackCooldownDuration;
    float shurikensAttackTime;
    [Space(5), SerializeField] float shurikensStaminaCost;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<PlayerController>();
        collider = GetComponent<CircleCollider2D>();
        cam = Camera.main;

        health = maxHealth;
        HealthDisplayer.StartDisplay(maxHealth);

        stamina = staminaMax;
        StaminaDisplayer.StartDisplay(staminaMax);

        furyAmount = 0;
        FuryDisplayer.StartDisplay(furyAmount, furyAmountMax);
    }

    // Update is called once per frame
    void Update()
    {
        if (!dead)
        {
            // Gain stamina
            if (stamina < staminaMax)
            {
                stamina = Mathf.Min(staminaMax, stamina + staminaGainBySec * Time.deltaTime);

                // Update UI
                StaminaDisplayer.UpdateDisplay(stamina);
            }

            if (!stagger)
            {
                // Test furymode
                if (Input.GetKeyDown(KeyCode.E))
                {
                    SetFuryMode(!furyMode);
                }

                // Dash
                if (Input.GetMouseButtonDown(0) && (furyMode || stamina >= dashStaminaCost))
                {
                    if(!furyMode) stamina -= dashStaminaCost;
                    StartDashAttack();
                }
                // Launch Shuriken
                else if (Input.GetMouseButtonDown(1) && shurikensAttackTime <= Time.time && (furyMode || stamina >= shurikensStaminaCost))
                {
                    if(!furyMode) stamina -= shurikensStaminaCost;
                    LaunchShurikens();
                }
            }

            // Decrease constantly fury
            if (furyAmount > 0)
            {
                furyAmount -= Time.deltaTime * furyDecreaseRate;

                // Update UI
                FuryDisplayer.UpdateDisplay(furyAmount);
            }
            if (furyAmount <= 0)
            {
                if (furyAmount != 0)
                {
                    furyAmount = 0;

                    // Update UI
                    FuryDisplayer.UpdateDisplay(furyAmount);
                }

                // Disable fury mode if needed
                if (furyMode) SetFuryMode(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (dashing)
        {
            Debug.Log(collision.tag);
            switch (collision.tag)
            {
                case "Wall":
                    // Stop dash
                    StopDashAttack();
                    break;
                case "Enemy":
                    // Get the enemy
                        Debug.Log("?");
                    EnemiesBehavior enemy;
                    if(collision.TryGetComponent(out enemy))
                    {
                        Debug.Log("Componenet found");
                        // Check if this enemy already has been touched
                        if (!enemiesTouched.Contains(enemy))
                        {
                            // Add enemy to the list of enemies touched
                            enemiesTouched.Add(enemy);

                            // Deal damage to the enemy touched
                            enemy.TakeDamage();

                            // Add fury points if not already in fury
                            if (!furyMode)
                            {
                                furyAmount = Mathf.Min(furyAmountMax, furyAmount + furyGainByAttack);
                                if (furyAmount == furyAmountMax) SetFuryMode(true);
                            }
                        }
                    }
                    break;
                case "EnemyShield":
                    // Stop dash
                    StopDashAttack();

                    // Get stunned with knockback
                    StartCoroutine(StartKnockback((transform.position - collision.bounds.center).normalized));
                    break;
            }
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("EnemyShield"))
        {
            // Stop dash
            if(dashing) StopDashAttack();

            // Get stunned with knockback
            StartCoroutine(StartKnockback((transform.position - collision.collider.bounds.center).normalized));
        }
    }

    //private void OnGUI()
    //{
    //    GUILayout.Label("Stamina: " + ((int)stamina).ToString());
    //}

    #region Get mouse direction from player
    Vector2 GetMouseDirectionFromPlayer(ref Vector2 mousePos)
    {
        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        return (mousePos - (Vector2)transform.position).normalized;
    }
    Vector2 GetMouseDirectionFromPlayer()
    {
        return (cam.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
    }
    #endregion

    #region Attacks
    private void StartDashAttack()
    {
        Vector2 mouseDir = GetMouseDirectionFromPlayer();

        dashing = true;
        collider.isTrigger = true;

        StartCoroutine(DashAttackCoroutine(mouseDir));
    }
    IEnumerator DashAttackCoroutine(Vector2 direction)
    {
        controller.SetFreeMovement(false);
        controller.SetRbVelocity(direction * dashSpeed);

        float timer = 0;
        while(timer < dashDuration)
        {
            if (!dashing) break;

            timer += Time.deltaTime;
            yield return null;
        }

        StopDashAttack();
    }
    void StopDashAttack()
    {
        dashing = false;
        collider.isTrigger = false;
        controller.ResetRbVelocity();
        controller.SetFreeMovement(true);

        enemiesTouched.Clear();
    }
    private void LaunchShurikens()
    {
        shurikensAttackTime = Time.time + shurikensAttackCooldownDuration;
        StartCoroutine(LaunchShurikensCoroutine());
    }
    IEnumerator LaunchShurikensCoroutine()
    {
        Vector2 dir = GetMouseDirectionFromPlayer();
        for (int i = 0; i < shurikenLaunchedByAttack; i++)
        {
            GameObject shuriken = Instantiate(shurikenPrefab, transform.position, Quaternion.identity);

            // Temp
            shuriken.GetComponent<Rigidbody2D>().velocity = dir;

            Destroy(shuriken, 20f);

            yield return new WaitForSeconds(timeBetweenTwoShurikens);
        }
    }
    #endregion

    #region Heal methods
    public bool CanHeal => health < maxHealth;
    void Heal()
    {
        if (!CanHeal) return;

        // Heal
        health++;

        // Update UI
        HealthDisplayer.UpdateDisplay(health);
    }
    void TakeDamage(bool becomeInvulnerable = true, Vector2? knockBackDir = null)
    {
        if (dead || invulnerable) return;

        // Take Damage
        health--;
        if (health <= 0) Die();
        else
        {
            // Make the player invulnerable for a period of time
            if (becomeInvulnerable)
            {
                StartCoroutine(BecomeTemporaryInvulnerable());
            }

            // Give a knock back to the player if we want to 
            if(knockBackDir != null)
            {
                StartCoroutine(StartKnockback((Vector2)knockBackDir));
            }
        }

        // Update UI
        HealthDisplayer.UpdateDisplay(health);
    }
    void Die()
    {
        if (dead) return;
        // Die
        dead = true;
    }
    #endregion

    IEnumerator BecomeTemporaryInvulnerable()
    {
        invulnerable = true;
        yield return new WaitForSeconds(invulnerabilityTimer);
        invulnerable = false;
    }
    IEnumerator StartKnockback(Vector2 direction)
    {
        stagger = true;
        controller.SetFreeMovement(false);
        controller.SetRbVelocity(direction * knockBackSpeed);
        yield return new WaitForSeconds(knockBackDuration);
        controller.ResetRbVelocity();
        controller.SetFreeMovement(true);
        stagger = false;
    }

    void SetFuryMode(bool furyMode)
    {
        if (this.furyMode == furyMode) return;
        this.furyMode = furyMode;

        controller.SetSpeed(furyMode);
        spRenderer.color = furyMode ? Color.red : Color.white;
    }
}
