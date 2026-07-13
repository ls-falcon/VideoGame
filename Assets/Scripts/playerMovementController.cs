using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementController : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip swordSound;

    private PlayerInputSystem playerInput;
    private Rigidbody2D rb;

    // Agregamos estas dos referencias
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool isGrounded;
    private bool jumpPressed = false;
    private float originalGravity;

    [Header("Configuraci�n de Movimiento")]
    [SerializeField] private float playerSpeed = 4f;
    [SerializeField] private float jumpForce = 6f;

    [Header("Detecci�n de Suelo")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.2f;
    [SerializeField] private LayerMask groundMask;

    [Header("Ataque a enemigos")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 0.5f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask bossLayer;
    private Vector3 originalAttackPointPos;
    private bool isAttacking = false;

    public PlayerInputSystem Input => playerInput;
    private float moveX;

    [Header("Sword Throw")]
    [SerializeField] private ThrownSword sword;
    [SerializeField] private Transform swordHolder;
    [SerializeField] private float pullSpeed = 15f;
    [SerializeField] private float momentumDecay = 15f; // QuÃ© tan rÃ¡pido pierde el impulso en el aire
    [SerializeField] private SwordAimUI swordAimUI;
    [SerializeField] private float minThrowForce = 2f;
    [SerializeField] private float maxThrowForce = 12f;
    [SerializeField] private float chargeTimeToMax = 1.2f;

    
    private bool pullingToSword = false;
    private bool hasPullMomentum = false; // <--- NUEVA VARIABLE
    private bool isAiming = false;
    private Vector2 aimDirection = Vector2.right;
    private float aimCharge = 0f;

    [HideInInspector] public float MeleeAttackSpeedMultiplier = 1f;
    [HideInInspector] public float SwordAttackSpeedMultiplier = 1f;

    [HideInInspector] public int MeleeDamage = 1;
    [HideInInspector] public int SwordDamage = 1;
    [HideInInspector] public float SwordThrowForceMultiplier = 1f;

    private bool canDoubleJump = false;
    private bool hasUsedDoubleJump = false;

    public enum ActiveBlessingType { None, DragonFire, CelestialStorm, WindBoots }
    [HideInInspector] public ActiveBlessingType activeBlessing = ActiveBlessingType.None;

    [HideInInspector] public bool UnlockedDragonFire = false;
    [HideInInspector] public bool UnlockedCelestialStorm = false;
    [HideInInspector] public bool UnlockedWindBoots = false;

    public bool HasDragonFire => UnlockedDragonFire && activeBlessing == ActiveBlessingType.DragonFire;
    public bool HasCelestialStorm => UnlockedCelestialStorm && activeBlessing == ActiveBlessingType.CelestialStorm;
    public bool HasWindBoots => UnlockedWindBoots && activeBlessing == ActiveBlessingType.WindBoots;

    private TMPro.TextMeshProUGUI blessingUIText;

    private bool hasDashedInAir = false;

    private void Awake()
    {
        playerInput = new PlayerInputSystem();
        playerInput.Player.Enable();

        playerInput.Player.Jump.performed += OnJump;
        playerInput.Player.Attack.performed += OnAttack;
        playerInput.Player.ThrowSword.performed += OnThrowSwordPressed;
        playerInput.Player.ThrowSword.canceled += OnThrowSwordReleased;

        // Inicializamos los componentes
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnDestroy()
    {
        if (playerInput == null) return;

        playerInput.Player.Jump.performed -= OnJump;
        playerInput.Player.Attack.performed -= OnAttack;
        playerInput.Player.ThrowSword.performed -= OnThrowSwordPressed;
        playerInput.Player.ThrowSword.canceled -= OnThrowSwordReleased;

        playerInput.Player.Disable();
        playerInput.Dispose();
        playerInput = null;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        originalGravity = rb.gravityScale;
        sword.SetPlayerCollider(GetComponent<Collider2D>());

        // Asegurar que el jugador se dibuje delante del follaje
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 18;
        }

        originalAttackPointPos = attackPoint.localPosition;
        SetupSwordAimUI();

        // Configurar la interfaz de habilidad activa
        SetupBlessingUI();
    }

    private void Update()
    {
        moveX = playerInput.Player.HorizontalMovement.ReadValue<float>();

        // --- Lgica de Animacin y Flip ---

        // Usamos Mathf.Abs para que si moveX es -1, se convierta en 1.
        // As el parmetro "Movs" siempre refleja si hay movimiento.
        float movsValue = Mathf.Abs(moveX);
        animator.SetFloat("Movs", movsValue);

        // Voltear el sprite segn la direccin
        if (moveX > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (moveX < 0)
        {
            spriteRenderer.flipX = true;
        }

        // Ajusta el punto de ataque segun la direccion del jugador
        float direction = spriteRenderer.flipX ? -1f : 1f;

        attackPoint.localPosition = new Vector3(
            direction * Mathf.Abs(originalAttackPointPos.x),
            originalAttackPointPos.y,
            originalAttackPointPos.z
        );

        if (isAiming)
        {
            UpdateAim();
        }

        // Cambiar de habilidad con la tecla Q
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            CycleActiveBlessing();
        }

        // Actualizar UI de habilidad
        UpdateBlessingUI();

        // Resetear air-dash al tocar el suelo
        if (isGrounded)
        {
            hasDashedInAir = false;
        }

        // Habilidad de Air-Dash con Botas de Viento (Usa Shift o la tecla F)
        if (HasWindBoots && !isGrounded && !hasDashedInAir && Keyboard.current != null && (Keyboard.current.leftShiftKey.wasPressedThisFrame || Keyboard.current.fKey.wasPressedThisFrame))
        {
            float dashDir = spriteRenderer.flipX ? -1f : 1f;
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(dashDir * playerSpeed * 2.5f, rb.linearVelocity.y * 0.5f);
                hasDashedInAir = true;
                CreateWindDashEffect();
            }
        }
    }

    private void FixedUpdate()
    {
        if (pullingToSword)
        {
            Vector2 direction = (sword.transform.position - transform.position).normalized;
            rb.linearVelocity = direction * pullSpeed;

            // Daño eléctrico en la trayectoria de atracción (Celestial Storm)
            if (HasCelestialStorm)
            {
                if (Random.value < 0.4f)
                {
                    CreateLightningSpark();
                }

                Collider2D[] hitDuringPull = Physics2D.OverlapCircleAll(transform.position, 1.2f, enemyLayer | bossLayer);
                foreach (Collider2D col in hitDuringPull)
                {
                    EnemyHealth eh = col.GetComponent<EnemyHealth>();
                    if (eh != null)
                    {
                        eh.TakeDamage(1);
                    }
                    else
                    {
                        BossHealth bh = col.GetComponent<BossHealth>();
                        if (bh != null)
                        {
                            bh.TakeDamage(1);
                        }
                    }
                }
            }

            float distance = Vector2.Distance(transform.position, sword.transform.position);
            if (distance < 1f)
            {
                RecoverSword();
            }
            return;
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask);

        // Si toca el suelo, el momentum se detiene inmediatamente
        if (isGrounded)
        {
            hasPullMomentum = false;
        }

        // --- MANEJO DE VELOCIDAD HORIZONTAL ---
        if (hasPullMomentum)
        {
            // El impulso del pull decae gradualmente hacia la velocidad objetivo del jugador (moveX * playerSpeed)
            float targetX = moveX * playerSpeed;
            float newX = Mathf.MoveTowards(rb.linearVelocity.x, targetX, momentumDecay * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
        }
        else
        {
            // Movimiento normal en suelo o aire sin momentum
            rb.linearVelocity = new Vector2(moveX * playerSpeed, rb.linearVelocity.y);
        }

        // --- LGICA DE SALTO ---
        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpPressed = false;
        }
    }

    public void EnableDoubleJump()
    {
        canDoubleJump = true;
    }

    public void AddMoveSpeed(float amount)
    {
        playerSpeed += amount;
    }

    void OnThrowSwordPressed(InputAction.CallbackContext ctx)
    {
        if (pullingToSword)
        {
            CancelPull();
            return;
        }

        if (!sword.IsThrown)
        {
            StartAiming();
        }
        else
        {
            PullPlayerToSword();
        }
    }

    void OnThrowSwordReleased(InputAction.CallbackContext ctx)
    {
        if (isAiming && sword != null)
        {
            ThrowSword();
        }
    }

    void CancelPull()
    {
        if (HasWindBoots)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }

        pullingToSword = false;
        rb.gravityScale = originalGravity;

        // Activamos el momentum para que no frene en seco
        hasPullMomentum = true;

        // BORRAMOS O COMENTAMOS ESTA LNA:
        // rb.linearVelocity = savedVelocity; 
    }

    void RecoverSword()
    {
        if (HasWindBoots)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }

        pullingToSword = false;
        rb.gravityScale = originalGravity;
        animator.SetBool("NoSword", false);
        sword.AttachToPlayer(swordHolder);

        // OPCIONAL: Si tambin quieres que salga disparado con inercia 
        // al llegar con xito a la espada, activa esto aqu tambin:
        hasPullMomentum = true;
    }

    void ThrowSword()
    {
        if (HasWindBoots)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }

        if (sword == null)
        {
            isAiming = false;
            if (swordAimUI != null)
            {
                swordAimUI.Show(false);
            }
            return;
        }

        float force =
        Mathf.Lerp(minThrowForce, maxThrowForce, aimCharge)
        * SwordThrowForceMultiplier;

        sword.Throw(aimDirection, force);
        animator.SetBool("NoSword", true);
        isAiming = false;

        if (swordAimUI != null)
        {
            swordAimUI.Show(false);
        }
    }

    void StartAiming()
    {
        isAiming = true;
        aimCharge = 0f;
        aimDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;

        if (HasWindBoots)
        {
            Time.timeScale = 0.35f;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }

        if (swordAimUI != null)
        {
            swordAimUI.Show(true);
        }

        UpdateAim();
    }

    void UpdateAim()
    {
        aimCharge = Mathf.Min(
            aimCharge + Time.deltaTime / Mathf.Max(chargeTimeToMax, 0.01f),
            1f
        );

        Vector2 startPosition = swordHolder.position;
        Vector2 targetPosition = GetMouseWorldPosition();
        Vector2 direction = targetPosition - startPosition;

        // Evita direcciones vacÃ­as si el cursor estÃ¡ justo encima del jugador.
        if (direction.sqrMagnitude > 0.01f)
        {
            aimDirection = direction.normalized;
        }

        if (swordAimUI != null)
        {
            float force =
            Mathf.Lerp(minThrowForce, maxThrowForce, aimCharge)
            * SwordThrowForceMultiplier;

            swordAimUI.UpdateAim(
                startPosition,
                targetPosition,
                force,
                sword.GravityScale,
                aimCharge
            );
        }
    }

    Vector2 GetMouseWorldPosition()
    {
        if (Camera.main == null || Mouse.current == null)
        {
            return (Vector2)swordHolder.position + aimDirection;
        }

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        return new Vector2(mouseWorld.x, mouseWorld.y);
    }

    void SetupSwordAimUI()
    {
        if (swordAimUI == null)
        {
            Transform aimIndicator = transform.Find("AimIndicator");
            if (aimIndicator != null)
            {
                swordAimUI = aimIndicator.GetComponent<SwordAimUI>();
                if (swordAimUI == null)
                {
                    swordAimUI = aimIndicator.gameObject.AddComponent<SwordAimUI>();
                }
            }
        }

        if (swordAimUI != null)
        {
            swordAimUI.Show(false);
        }
    }

    void PullPlayerToSword()
    {
        if (sword == null || !sword.IsThrown) return;

        

        pullingToSword = true;

        rb.gravityScale = 0;
    }



    void OnJump(InputAction.CallbackContext ctx)
    {
        if (pullingToSword) return;

        if (isGrounded)
        {
            jumpPressed = true;
            hasUsedDoubleJump = false;
        }
        else if (canDoubleJump && !hasUsedDoubleJump)
        {
            rb.linearVelocity =
                new Vector2(rb.linearVelocity.x, jumpForce);

            hasUsedDoubleJump = true;
        }
    }

    void OnAttack(InputAction.CallbackContext ctx)
    {
        //if (pullingToSword) return;
        Attack();
    }

    public void DealDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRadius,
            enemyLayer
        );

        Collider2D hitBoss = Physics2D.OverlapCircle(
            attackPoint.position,
            attackRadius,
            bossLayer
        );

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            EnemyHealth enemy = enemyCollider.GetComponent<EnemyHealth>();

            if (enemy != null)
            {
                if (HasDragonFire)
                {
                    BurnEffect burn = enemyCollider.GetComponent<BurnEffect>();
                    if (burn == null)
                    {
                        enemyCollider.gameObject.AddComponent<BurnEffect>();
                    }
                }
                else
                {
                    enemy.TakeDamage(MeleeDamage);
                }
            }
        }

        if (hitBoss != null)
        {
            BossHealth boss = hitBoss.GetComponent<BossHealth>();
            if (boss != null)
            {
                boss.TakeDamage(MeleeDamage);
                if (HasDragonFire)
                {
                    BurnEffect burn = hitBoss.GetComponent<BurnEffect>();
                    if (burn == null)
                    {
                        hitBoss.gameObject.AddComponent<BurnEffect>();
                    }
                }
            }
        }
    }

    void Attack()
    {
        if (isAttacking) return;

        isAttacking = true;

        animator.speed = MeleeAttackSpeedMultiplier;

        animator.SetTrigger("Attack");

        audioSource.PlayOneShot(swordSound);
    }

    public void EndAttack()
    {
        isAttacking = false;

        animator.speed = 1f;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }

    private void CreateWindDashEffect()
    {
        GameObject effect = new GameObject("WindDashEffect");
        effect.transform.position = transform.position;
        SpriteRenderer er = effect.AddComponent<SpriteRenderer>();
        
        Texture2D tex = new Texture2D(8, 8);
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                tex.SetPixel(x, y, new Color(0.8f, 1f, 0.9f, 0.5f));
        tex.Apply();
        
        er.sprite = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f));
        er.sortingOrder = 9;
        
        Destroy(effect, 0.2f);
        StartCoroutine(ExpandAndFadeEffect(effect.transform, er));
    }

    private System.Collections.IEnumerator ExpandAndFadeEffect(Transform t, SpriteRenderer sr)
    {
        float elapsed = 0f;
        while (elapsed < 0.2f && t != null)
        {
            t.localScale += Vector3.one * 5f * Time.deltaTime;
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.5f - (elapsed / 0.2f) * 0.5f);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void CreateLightningSpark()
    {
        GameObject spark = new GameObject("LightningSpark");
        LineRenderer lr = spark.AddComponent<LineRenderer>();
        lr.startWidth = 0.04f;
        lr.endWidth = 0.04f;
        
        if (spriteRenderer != null)
        {
            lr.sharedMaterial = spriteRenderer.sharedMaterial;
        }
        
        lr.startColor = Color.cyan;
        lr.endColor = Color.white;
        lr.positionCount = 3;

        Vector3 start = transform.position + new Vector3(Random.Range(-0.4f, 0.4f), Random.Range(-0.4f, 0.4f), 0);
        Vector3 end = start + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
        Vector3 mid = Vector3.Lerp(start, end, 0.5f) + new Vector3(Random.Range(-0.15f, 0.15f), Random.Range(-0.15f, 0.15f), 0);

        lr.SetPosition(0, start);
        lr.SetPosition(1, mid);
        lr.SetPosition(2, end);

        Destroy(spark, 0.08f);
    }

    private void SetupBlessingUI()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject uiObj = new GameObject("ActiveBlessingUI");
        uiObj.transform.SetParent(canvas.transform, false);
        
        RectTransform rect = uiObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f); // Top-Left
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.sizeDelta = new Vector2(300f, 30f);
        rect.anchoredPosition = new Vector2(20f, -80f); // Debajo de los corazones de vida

        blessingUIText = uiObj.AddComponent<TMPro.TextMeshProUGUI>();
        blessingUIText.fontSize = 13;
        blessingUIText.alignment = TMPro.TextAlignmentOptions.Left;
        blessingUIText.color = Color.white;
    }

    private void UpdateBlessingUI()
    {
        if (blessingUIText == null) return;

        string status = "Habilidad: Ninguna";
        if (activeBlessing == ActiveBlessingType.DragonFire)
        {
            status = "Habilidad: <color=#FFA500>Fuego de Dragón</color>";
        }
        else if (activeBlessing == ActiveBlessingType.CelestialStorm)
        {
            status = "Habilidad: <color=#00FFFF>Tormenta Celestial</color>";
        }
        else if (activeBlessing == ActiveBlessingType.WindBoots)
        {
            status = "Habilidad: <color=#00FF7F>Botas de Viento</color>";
        }

        // Listar las otras habilidades desbloqueadas
        System.Collections.Generic.List<string> others = new System.Collections.Generic.List<string>();
        if (UnlockedDragonFire && activeBlessing != ActiveBlessingType.DragonFire) others.Add("Fuego");
        if (UnlockedCelestialStorm && activeBlessing != ActiveBlessingType.CelestialStorm) others.Add("Tormenta");
        if (UnlockedWindBoots && activeBlessing != ActiveBlessingType.WindBoots) others.Add("Viento");

        if (others.Count > 0)
        {
            status += " <color=#FFD700>(Presiona Q para cambiar)</color>";
        }

        blessingUIText.text = status;
    }

    private void CycleActiveBlessing()
    {
        System.Collections.Generic.List<ActiveBlessingType> unlockedList = new System.Collections.Generic.List<ActiveBlessingType>();
        unlockedList.Add(ActiveBlessingType.None);
        
        if (UnlockedDragonFire) unlockedList.Add(ActiveBlessingType.DragonFire);
        if (UnlockedCelestialStorm) unlockedList.Add(ActiveBlessingType.CelestialStorm);
        if (UnlockedWindBoots) unlockedList.Add(ActiveBlessingType.WindBoots);

        if (unlockedList.Count <= 1) return;

        int currentIndex = unlockedList.IndexOf(activeBlessing);
        int nextIndex = (currentIndex + 1) % unlockedList.Count;
        activeBlessing = unlockedList[nextIndex];
        
        Debug.Log($"[PlayerMovementController] Habilidad activa cambiada a: {activeBlessing}");
    }
}
