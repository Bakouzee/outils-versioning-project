using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class Player : MonoBehaviour
{
    PlayerController controller;
    [SerializeField] SpriteRenderer spRenderer;
    new CircleCollider2D collider;
    Camera cam;

    [Header("Stamina"), SerializeField] float staminaMax;
    [SerializeField] float staminaGainBySec;
    float stamina;

    [Header("Dash attack")]
    [Range(2, 10), SerializeField] float dashRange;
    [SerializeField] float dashStaminaCost;

    [Header("Shurikens attack"), SerializeField] GameObject shurikenPrefab;
    [SerializeField] int shurikenLaunchedByAttack;
    [SerializeField] float timeBetweenTwoShurikens;
    [Space(5), SerializeField] float shurikensAttackCooldownDuration;
    float shurikensAttackTime;
    [Space(5), SerializeField] float shurikensStaminaCost;

    bool furyMode;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<PlayerController>();
        collider = GetComponent<CircleCollider2D>();
        cam = Camera.main;

        stamina = staminaMax;
    }

    // Update is called once per frame
    void Update()
    {
        // Gain stamina
        stamina = Mathf.Min(staminaMax, stamina + staminaGainBySec * Time.deltaTime);

        // Test furymode
        if (Input.GetKeyDown(KeyCode.E))
        {
            SetFuryMode(!furyMode);
        }

        // Dash
        if (Input.GetMouseButtonDown(0) && stamina >= dashStaminaCost)
        {
            stamina -= dashStaminaCost;
            AttackDash();
        }
        // Launch Shuriken
        else if (Input.GetMouseButtonDown(1) && shurikensAttackTime <= Time.time && stamina >= shurikensStaminaCost)
        {
            stamina -= shurikensStaminaCost;
            LaunchShurikens();
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Stamina: " + ((int)stamina).ToString());
    }

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

    private void AttackDash()
    {
        Vector2 mousePos = Vector2.zero;
        Vector2 mouseDir = GetMouseDirectionFromPlayer(ref mousePos);

        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, collider.radius, mouseDir, dashRange);
        foreach (RaycastHit2D hit in hits)
        {
            // Check if hit is an enemy

            // Deal damage
        }

        Vector2 dashPos = mousePos;
        if (Vector2.Distance((Vector2)transform.position, dashPos) > dashRange)
        {
            dashPos = (Vector2)transform.position + mouseDir * dashRange;
        }
        transform.position = dashPos;
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

    void SetFuryMode(bool furyMode)
    {
        if (this.furyMode == furyMode) return;
        this.furyMode = furyMode;

        controller.SetSpeed(furyMode);
        spRenderer.color = furyMode ? Color.red : Color.white;
    }
}
