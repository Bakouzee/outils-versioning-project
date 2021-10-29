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

    #region Combos
    public enum DashDirection { None, Right, Left, Up, Down, UpRight, UpLeft, DownRight, DownLeft};
    public static DashDirection VectorToDashDirection(Vector2 vectorDir)
    {
        Vector2[] vectors = new Vector2[8] { Vector2.right, Vector2.left, Vector2.up, Vector2.down, (Vector2.up + Vector2.right).normalized, (Vector2.up + Vector2.left).normalized, (Vector2.down + Vector2.right).normalized, (Vector2.down + Vector2.left).normalized };
        float bestAngle = Vector2.Angle(vectorDir, vectors[0]);
        int bestVectorIndex = 0;
        for (int i = 1; i < vectors.Length; i++)
        {
            float angle = Vector2.Angle(vectorDir, vectors[i]);
            if(angle < bestAngle)
            {
                bestAngle = angle;
                bestVectorIndex = i;
            }
        }
        return (DashDirection)(bestVectorIndex+1);
    }
    [System.Serializable] struct PlayerCombo
    {
        public string name;
        public int damage;
        public DashDirection[] moves;
    }
    [Header("Combos")]
    [SerializeField] PlayerCombo[] combos;
    [System.Serializable] struct PlayerAttack
    {
        public DashDirection dir;
        public bool touched;
        public EnemiesBehavior[] enemiesTouched;

        public PlayerAttack(Vector2 vectorDir, List<EnemiesBehavior> enemiesTouched)
        {
            this.dir = VectorToDashDirection(vectorDir);
            this.touched = enemiesTouched.Count > 0;
            this.enemiesTouched = enemiesTouched.ToArray();
        }
    }
    List<PlayerAttack> currentCombo = new List<PlayerAttack>();
    float currentComboTimer;
    [SerializeField] float timeToCancelCombo = 1f;
    #endregion

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

            // If we're doing a combo --> wait to cancel it
            if(currentCombo.Count > 0)
            {
                currentComboTimer -= Time.deltaTime;
                if (currentComboTimer <= 0) StopCurrentCombo();
            }

            // If the player is not staggered/stunned
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
            //Debug.Log(collision.tag);
            switch (collision.tag)
            {
                case "Wall":
                    // Stop dash
                    StopDashAttack();
                    break;
                case "Enemy":
                    // Get the enemy
                        //Debug.Log("?");
                    EnemiesBehavior enemy;
                    if(collision.TryGetComponent(out enemy))
                    {
                        //Debug.Log("Componenet found");
                        // Check if this enemy already has been touched
                        if (!enemiesTouched.Contains(enemy))
                        {
                            // Add enemy to the list of enemies touched
                            enemiesTouched.Add(enemy);

                            // Deal damage to the enemy touched
                            enemy.TakeDamage();

                            // Reset combo timer
                            ResetComboTimer();

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
        // Create a dash attack and add it to the current combo
        PlayerAttack dash = new PlayerAttack(controller.GetRbDirection(), enemiesTouched);
        //Debug.Log(dash.touched);
        AddAttackToCurrentCombo(dash);

        dashing = false;
        // Change back collider
        collider.isTrigger = false;
        // Stop player and set it movable 
        controller.ResetRbVelocity();
        controller.SetFreeMovement(true);

        // Remove all enemies touched during the dash
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

    #region Combos functions
    void AddAttackToCurrentCombo(PlayerAttack attack)
    {
        // Check if the two last dash didn't touch an enemy --> stop current combo if yes
        if(currentCombo.Count > 0)
        {
            if(!attack.touched && !currentCombo[currentCombo.Count - 1].touched)
            {
                StopCurrentCombo();
                return;
            }
        }

        // Add the attack to the current combo and reset the timer to cancel the combo
        currentCombo.Add(attack);
        ResetComboTimer();

        // Check if the current combo is valid
        CheckCurrentCombo();
    }
    void CheckCurrentCombo()
    {
        // Translate current combos in direction
        DashDirection[] currentComboMoves = CurrentComboToDashDirections();

        // Compare each possible combos directions with the current combo directions
        foreach (PlayerCombo combo in combos)
        {
            // Fast check to gain some time
            // If the last dashdirection is not the same --> not this combo
            //Debug.Log($"{currentComboMoves[currentComboMoves.Length - 1]} == {combo.moves[combo.moves.Length - 1]}");
            if (currentComboMoves[currentComboMoves.Length - 1] != combo.moves[combo.moves.Length - 1]) continue;

            // Long check
            bool sameDirection = true;
            for (int i = 1; i < combo.moves.Length; i++)
            {
                //Debug.Log($"{currentComboMoves[currentComboMoves.Length - i - 1]} == {combo.moves[combo.moves.Length - i - 1]}");
                if(currentComboMoves[currentComboMoves.Length - i - 1] != combo.moves[combo.moves.Length - i - 1])
                {
                    sameDirection = false;
                    break;
                }
            }
            // If not exactly the same --> not this combo
            if (!sameDirection) continue;

            // We assume that only the good combo will read the code below
            Debug.Log($"{combo.name.ToUpper()}!");

            // Deal combos damage
            foreach (PlayerAttack attack in currentCombo) 
            {
                if (attack.touched) 
                {
                    foreach (EnemiesBehavior enemy in attack.enemiesTouched)
                    {
                        //Debug.Log($"Tried to deal damage to {enemy.name}");
                        enemy.TakeDamage();
                    }
                }
            }

            StopCurrentCombo();
        }
    }
    void StopCurrentCombo()
    {
        Debug.Log($"Current combo stopped after {currentCombo.Count} dashes");
        currentCombo.Clear();
    }

        #region Under da hood funcs
    void ResetComboTimer() => currentComboTimer = timeToCancelCombo;
    DashDirection[] CurrentComboToDashDirections()
    {
        DashDirection[] result = new DashDirection[currentCombo.Count];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = currentCombo[i].dir;
        }
        return result;
    }
        #endregion

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
