using System.Collections;
using UnityEngine;

public class PuzzlePiecePickup :
    MonoBehaviour,
    IHandInteractable
{
    // ============================================================
    // PUZZLE PIECE
    // ============================================================

    [Header("PUZZLE PIECE")]

    [Tooltip(
        "Номер пазла / коллекции. " +
        "Для нашей первой коллекции оставляем 1."
    )]
    [SerializeField, Min(1)]
    private int puzzleId = 1;

    [Tooltip(
        "Номер конкретной детали: " +
        "1, 2, 3 или 4."
    )]
    [SerializeField, Min(1)]
    private int pieceNumber = 1;


    // ============================================================
    // PLAYER
    // ============================================================

    [Header("PLAYER")]

    [SerializeField]
    private string playerTag = "Player";

    [Tooltip(
        "Расстояние, на котором деталь " +
        "можно подобрать кнопкой руки."
    )]
    [SerializeField, Min(0f)]
    private float pickupDistance = 0.8f;


    // ============================================================
    // VISUAL
    // ============================================================

    [Header("VISUAL")]

    [SerializeField]
    private SpriteRenderer pieceRenderer;

    [SerializeField]
    private SpriteRenderer glowRenderer;

    [SerializeField]
    private Collider2D pickupCollider;


    // ============================================================
    // AUDIO
    // ============================================================

    [Header("AUDIO")]

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip pickupSound;

    [SerializeField, Range(0f, 1f)]
    private float pickupVolume = 1f;


    // ============================================================
    // HAPTICS
    // ============================================================

    [Header("HAPTICS")]

    [SerializeField]
    private bool usePickupHaptics = true;


    // ============================================================
    // PRE-FLY CHARGE
    // ============================================================

    [Header("PRE-FLY CHARGE")]

    [Tooltip(
        "Перед финальным улётом пазл сначала " +
        "слегка поднимается и начинает дрожать."
    )]
    [SerializeField]
    private bool usePreFlyCharge = true;

    [Tooltip(
        "Насколько пазл поднимется перед дрожанием."
    )]
    [SerializeField]
    private float preFlyRiseAmount = 0.12f;

    [Tooltip(
        "Сколько секунд занимает первоначальный подъём."
    )]
    [SerializeField, Min(0f)]
    private float preFlyRiseDuration = 0.30f;

    [Tooltip(
        "Сколько секунд пазл дрожит перед улётом."
    )]
    [SerializeField, Min(0f)]
    private float shakeDuration = 1.90f;

    [Tooltip(
        "Максимальная амплитуда дрожания по X."
    )]
    [SerializeField, Min(0f)]
    private float shakeAmountX = 0.035f;

    [Tooltip(
        "Максимальная амплитуда дрожания по Y."
    )]
    [SerializeField, Min(0f)]
    private float shakeAmountY = 0.025f;

    [Tooltip(
        "Скорость дрожания. Чем больше значение, " +
        "тем быстрее пазл вибрирует."
    )]
    [SerializeField, Min(0f)]
    private float shakeSpeed = 32f;

    [Tooltip(
        "Если включено, дрожание начинается мягко " +
        "и постепенно усиливается перед улётом."
    )]
    [SerializeField]
    private bool increaseShakeOverTime = true;

    [Tooltip(
        "Небольшое увеличение пазла во время зарядки. " +
        "1 = размер не меняется."
    )]
    [SerializeField, Min(0.01f)]
    private float chargeScaleMultiplier = 1.06f;


    // ============================================================
    // PICKUP ANIMATION
    // ============================================================

    [Header("PICKUP ANIMATION")]

    [Tooltip(
        "Делать финальную анимацию улёта " +
        "после подбора."
    )]
    [SerializeField]
    private bool animatePickup = true;

    [Tooltip(
        "Длительность именно финального улёта " +
        "после подъёма и дрожания."
    )]
    [SerializeField, Min(0.01f)]
    private float pickupAnimationDuration = 0.80f;

    [Tooltip(
        "Насколько высоко пазл улетает " +
        "во время финальной анимации."
    )]
    [SerializeField]
    private float pickupRiseAmount = 0.45f;

    [Tooltip(
        "Во сколько раз уменьшится деталь " +
        "к концу анимации."
    )]
    [SerializeField, Range(0f, 1f)]
    private float pickupEndScale = 0.25f;


    // ============================================================
    // PICKUP TRAIL
    // ============================================================

    [Header("PICKUP TRAIL")]

    [Tooltip(
        "Красивый шлейф во время улёта пазла."
    )]
    [SerializeField]
    private bool usePickupTrail = true;

    [SerializeField, Min(1)]
    private int trailParticlesPerSecond = 55;

    [SerializeField, Min(0.01f)]
    private float trailLifetime = 0.38f;

    [Tooltip(
        "Небольшое смещение шлейфа вниз, " +
        "чтобы он оставался позади пазла."
    )]
    [SerializeField]
    private float trailBackOffset = 0.03f;

    [SerializeField]
    private float trailSpread = 0.06f;

    [SerializeField]
    private float trailDriftX = 0.08f;

    [SerializeField]
    private float trailDriftY = 0.12f;

    [SerializeField, Min(0.001f)]
    private float trailStartSize = 0.16f;

    [SerializeField, Min(0.001f)]
    private float trailEndSize = 0.03f;

    [SerializeField]
    private Color trailOuterColor =
        new Color(1f, 0.45f, 0.08f, 0.90f);

    [SerializeField]
    private Color trailInnerColor =
        new Color(1f, 0.92f, 0.42f, 1f);

    [SerializeField]
    private int trailSortingOrderOffset = 6;


    // ============================================================
    // TRAIL BURSTS
    // ============================================================

    [Header("TRAIL BURSTS")]

    [SerializeField]
    private bool useStartBurst = true;

    [SerializeField, Min(0)]
    private int startBurstCount = 7;

    [SerializeField]
    private bool useEndBurst = true;

    [SerializeField, Min(0)]
    private int endBurstCount = 10;


    // ============================================================
    // SAVE
    // ============================================================

    [Header("SAVE")]

    [Tooltip(
        "Запоминать найденную деталь через PlayerPrefs. " +
        "Позже Gallery сможет читать это состояние."
    )]
    [SerializeField]
    private bool saveCollectedState = true;


    // ============================================================
    // DEBUG
    // ============================================================

    [Header("DEBUG")]

    [SerializeField]
    private bool debugLogs = false;


    // ============================================================
    // PRIVATE
    // ============================================================

    private Transform playerTransform;
    private bool pickedUp;

    private float trailSpawnTimer;
    private Sprite trailSprite;

    private Vector3 initialLocalScale;
    private Color initialPieceColor = Color.white;
    private Color initialGlowColor = Color.white;


    // ============================================================
    // PUBLIC
    // ============================================================

    public int PuzzleId =>
        puzzleId;

    public int PieceNumber =>
        pieceNumber;

    public bool IsPickedUp =>
        pickedUp;


    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        if (pieceRenderer == null)
        {
            pieceRenderer =
                GetComponent<SpriteRenderer>();
        }

        if (pickupCollider == null)
        {
            pickupCollider =
                GetComponent<Collider2D>();
        }

        if (audioSource == null)
        {
            audioSource =
                GetComponent<AudioSource>();
        }

        if (glowRenderer == null)
        {
            Transform glowTransform =
                transform.Find(
                    "PuzzleGlow"
                );

            if (glowTransform != null)
            {
                glowRenderer =
                    glowTransform
                        .GetComponent<SpriteRenderer>();
            }
        }

        initialLocalScale =
            transform.localScale;

        if (pieceRenderer != null)
        {
            initialPieceColor =
                pieceRenderer.color;
        }

        if (glowRenderer != null)
        {
            initialGlowColor =
                glowRenderer.color;
        }

        if (usePickupTrail ||
            useStartBurst ||
            useEndBurst)
        {
            trailSprite =
                CreateSoftCircleSprite();
        }

        FindPlayer();
    }


    // ============================================================
    // FIND PLAYER
    // ============================================================

    private void FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag(
                playerTag
            );

        if (playerObject != null)
        {
            playerTransform =
                playerObject.transform;
        }
    }


    // ============================================================
    // HAND INTERACTION
    // ============================================================

    public bool CanHandInteract
    {
        get
        {
            if (pickedUp)
            {
                return false;
            }

            if (pickupCollider == null ||
                !pickupCollider.enabled)
            {
                return false;
            }

            if (playerTransform == null)
            {
                FindPlayer();
            }

            if (playerTransform == null)
            {
                return false;
            }

            float distance =
                Vector2.Distance(
                    playerTransform.position,
                    transform.position
                );

            return
                distance <=
                pickupDistance;
        }
    }


    public void HandInteract()
    {
        if (!CanHandInteract)
        {
            return;
        }

        Pickup();
    }


    // ============================================================
    // PICKUP
    // ============================================================

    private void Pickup()
    {
        if (pickedUp)
        {
            return;
        }

        pickedUp = true;

        if (debugLogs)
        {
            Debug.Log(
                "[PUZZLE PIECE] Collected Puzzle " +
                puzzleId +
                " Piece " +
                pieceNumber,
                this
            );
        }


        // --------------------------------------------------------
        // COLLIDER
        // --------------------------------------------------------

        if (pickupCollider != null)
        {
            pickupCollider.enabled =
                false;
        }


        // --------------------------------------------------------
        // HAPTICS
        // --------------------------------------------------------

        if (usePickupHaptics)
        {
            MicroHaptics.TinyClick();
        }


        // --------------------------------------------------------
        // AUDIO
        // --------------------------------------------------------

        if (audioSource != null &&
            pickupSound != null)
        {
            audioSource.PlayOneShot(
                pickupSound,
                pickupVolume
            );
        }


        // --------------------------------------------------------
        // SAVE
        // --------------------------------------------------------

        SaveCollectedPiece();


        // --------------------------------------------------------
        // ANIMATION
        // --------------------------------------------------------

        if (animatePickup)
        {
            StartCoroutine(
                PickupSequenceRoutine()
            );
        }
        else
        {
            FinishPickup();
        }
    }


    // ============================================================
    // SAVE PIECE
    // ============================================================

    private void SaveCollectedPiece()
    {
        if (!saveCollectedState)
        {
            return;
        }

        string saveKey =
            GetSaveKey();

        PlayerPrefs.SetInt(
            saveKey,
            1
        );

        PlayerPrefs.Save();

        if (debugLogs)
        {
            Debug.Log(
                "[PUZZLE PIECE] Saved: " +
                saveKey,
                this
            );
        }
    }


    // ============================================================
    // SAVE KEY
    // ============================================================

    private string GetSaveKey()
    {
        return
            "Puzzle_" +
            puzzleId +
            "Piece" +
            pieceNumber +
            "_Collected";
    }


    // ============================================================
    // COMPLETE PICKUP SEQUENCE
    // ============================================================

    private IEnumerator PickupSequenceRoutine()
    {
        if (usePreFlyCharge)
        {
            yield return
                StartCoroutine(
                    PreFlyChargeRoutine()
                );
        }

        yield return
            StartCoroutine(
                PickupAnimationRoutine()
            );
    }


    // ============================================================
    // PRE-FLY CHARGE
    // ============================================================

    private IEnumerator PreFlyChargeRoutine()
    {
        Vector3 originalPosition =
            transform.position;

        Vector3 chargePosition =
            originalPosition +
            Vector3.up *
            preFlyRiseAmount;

        Vector3 originalScale =
            transform.localScale;

        Vector3 chargeScale =
            originalScale *
            chargeScaleMultiplier;


        // --------------------------------------------------------
        // STEP 1 — SMALL RISE
        // --------------------------------------------------------

        float riseDuration =
            Mathf.Max(
                0f,
                preFlyRiseDuration
            );

        if (riseDuration > 0f)
        {
            float timer =
                0f;

            while (timer <
                   riseDuration)
            {
                timer +=
                    Time.deltaTime;

                float t =
                    Mathf.Clamp01(
                        timer /
                        riseDuration
                    );

                float smoothT =
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        t
                    );

                transform.position =
                    Vector3.Lerp(
                        originalPosition,
                        chargePosition,
                        smoothT
                    );

                transform.localScale =
                    Vector3.Lerp(
                        originalScale,
                        chargeScale,
                        smoothT
                    );

                yield return null;
            }
        }

        transform.position =
            chargePosition;

        transform.localScale =
            chargeScale;


        // --------------------------------------------------------
        // STEP 2 — SHAKE / CHARGE
        // --------------------------------------------------------

        float safeShakeDuration =
            Mathf.Max(
                0f,
                shakeDuration
            );

        if (safeShakeDuration <= 0f)
        {
            yield break;
        }

        float shakeTimer =
            0f;

        float randomPhaseX =
            Random.Range(
                0f,
                100f
            );

        float randomPhaseY =
            Random.Range(
                0f,
                100f
            );

        while (shakeTimer <
               safeShakeDuration)
        {
            shakeTimer +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    shakeTimer /
                    safeShakeDuration
                );

            float strength =
                1f;

            if (increaseShakeOverTime)
            {
                strength =
                    Mathf.Lerp(
                        0.20f,
                        1f,
                        t * t
                    );
            }

            float time =
                shakeTimer *
                shakeSpeed;

            float shakeX =
                Mathf.Sin(
                    time +
                    randomPhaseX
                ) *
                shakeAmountX *
                strength;

            float shakeY =
                Mathf.Sin(
                    time * 1.37f +
                    randomPhaseY
                ) *
                shakeAmountY *
                strength;

            float microX =
                Mathf.Sin(
                    time * 2.17f
                ) *
                shakeAmountX *
                0.25f *
                strength;

            float microY =
                Mathf.Cos(
                    time * 1.91f
                ) *
                shakeAmountY *
                0.20f *
                strength;

            transform.position =
                chargePosition +
                new Vector3(
                    shakeX + microX,
                    shakeY + microY,
                    0f
                );

            float scalePulse =
                1f +
                Mathf.Sin(
                    time * 0.55f
                ) *
                0.012f *
                strength;

            transform.localScale =
                chargeScale *
                scalePulse;

            yield return null;
        }

        transform.position =
            chargePosition;

        transform.localScale =
            chargeScale;
    }


    // ============================================================
    // FINAL PICKUP / FLY ANIMATION
    // ============================================================

    private IEnumerator PickupAnimationRoutine()
    {
        Vector3 startPosition =
            transform.position;

        Vector3 endPosition =
            startPosition +
            Vector3.up *
            pickupRiseAmount;

        Vector3 startScale =
            transform.localScale;

        Vector3 endScale =
            startScale *
            pickupEndScale;

        Color startPieceColor =
            pieceRenderer != null
                ? pieceRenderer.color
                : initialPieceColor;

        Color startGlowColor =
            glowRenderer != null
                ? glowRenderer.color
                : initialGlowColor;

        float safeDuration =
            Mathf.Max(
                0.01f,
                pickupAnimationDuration
            );

        float timer =
            0f;

        trailSpawnTimer =
            0f;


        // --------------------------------------------------------
        // TRAIL START BURST
        // --------------------------------------------------------

        if (useStartBurst)
        {
            SpawnBurst(
                transform.position,
                startBurstCount,
                1f
            );
        }


        // --------------------------------------------------------
        // FLY
        // --------------------------------------------------------

        while (timer <
               safeDuration)
        {
            timer +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer /
                    safeDuration
                );

            float smoothT =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            transform.position =
                Vector3.Lerp(
                    startPosition,
                    endPosition,
                    smoothT
                );

            transform.localScale =
                Vector3.Lerp(
                    startScale,
                    endScale,
                    smoothT
                );

            float alpha =
                Mathf.Lerp(
                    1f,
                    0f,
                    smoothT
                );

            if (pieceRenderer != null)
            {
                Color pieceColor =
                    startPieceColor;

                pieceColor.a =
                    startPieceColor.a *
                    alpha;

                pieceRenderer.color =
                    pieceColor;
            }

            if (glowRenderer != null)
            {
                Color glowColor =
                    startGlowColor;

                glowColor.a =
                    startGlowColor.a *
                    alpha;

                glowRenderer.color =
                    glowColor;
            }

            EmitTrail(
                transform.position
            );

            yield return null;
        }


        // --------------------------------------------------------
        // TRAIL END BURST
        // --------------------------------------------------------

        if (useEndBurst)
        {
            SpawnBurst(
                transform.position,
                endBurstCount,
                1.15f
            );
        }

        FinishPickup();
    }


    // ============================================================
    // TRAIL EMISSION
    // ============================================================

    private void EmitTrail(
        Vector3 centerPosition
    )
    {
        if (!usePickupTrail)
        {
            return;
        }

        if (trailParticlesPerSecond <= 0)
        {
            return;
        }

        trailSpawnTimer +=
            Time.deltaTime;

        float interval =
            1f /
            trailParticlesPerSecond;

        while (trailSpawnTimer >=
               interval)
        {
            trailSpawnTimer -=
                interval;

            Vector3 spawnPos =
                centerPosition +
                Vector3.down *
                trailBackOffset;

            SpawnTrailParticle(
                spawnPos,
                1f
            );
        }
    }


    // ============================================================
    // TRAIL BURST
    // ============================================================

    private void SpawnBurst(
        Vector3 centerPosition,
        int count,
        float sizeMultiplier
    )
    {
        if (count <= 0)
        {
            return;
        }

        for (int i = 0;
             i < count;
             i++)
        {
            SpawnTrailParticle(
                centerPosition,
                sizeMultiplier
            );
        }
    }


    // ============================================================
    // SPAWN TRAIL PARTICLE
    // ============================================================

    private void SpawnTrailParticle(
        Vector3 centerPosition,
        float sizeMultiplier
    )
    {
        if (trailSprite == null)
        {
            trailSprite =
                CreateSoftCircleSprite();
        }

        GameObject root =
            new GameObject(
                "PuzzleTrailParticle"
            );

        Vector3 randomOffset =
            new Vector3(
                Random.Range(
                    -trailSpread,
                    trailSpread
                ),
                Random.Range(
                    -trailSpread,
                    trailSpread
                ),
                0f
            );

        root.transform.position =
            centerPosition +
            randomOffset;

        float startSize =
            trailStartSize *
            sizeMultiplier *
            Random.Range(
                0.85f,
                1.15f
            );

        root.transform.localScale =
            Vector3.one *
            startSize;

        int sortingLayerId =
            0;

        int sortingOrder =
            trailSortingOrderOffset;

        if (pieceRenderer != null)
        {
            sortingLayerId =
                pieceRenderer.sortingLayerID;

            sortingOrder =
                pieceRenderer.sortingOrder +
                trailSortingOrderOffset;
        }

        if (glowRenderer != null)
        {
            sortingLayerId =
                glowRenderer.sortingLayerID;

            sortingOrder =
                Mathf.Max(
                    sortingOrder,
                    glowRenderer.sortingOrder +
                    trailSortingOrderOffset
                );
        }

        SpriteRenderer outerRenderer =
            root.AddComponent<SpriteRenderer>();

        outerRenderer.sprite =
            trailSprite;

        outerRenderer.sortingLayerID =
            sortingLayerId;

        outerRenderer.sortingOrder =
            sortingOrder;

        outerRenderer.color =
            trailOuterColor;

        GameObject innerObject =
            new GameObject(
                "Inner"
            );

        innerObject.transform.SetParent(
            root.transform,
            false
        );

        innerObject.transform.localPosition =
            Vector3.zero;

        innerObject.transform.localScale =
            Vector3.one *
            0.55f;

        SpriteRenderer innerRenderer =
            innerObject.AddComponent<SpriteRenderer>();

        innerRenderer.sprite =
            trailSprite;

        innerRenderer.sortingLayerID =
            sortingLayerId;

        innerRenderer.sortingOrder =
            sortingOrder + 1;

        innerRenderer.color =
            trailInnerColor;

        Vector3 endPosition =
            root.transform.position +
            new Vector3(
                Random.Range(
                    -trailDriftX,
                    trailDriftX
                ),
                Random.Range(
                    -trailDriftY,
                    0.02f
                ),
                0f
            );

        float finalSize =
            Mathf.Max(
                0.001f,
                trailEndSize *
                Random.Range(
                    0.85f,
                    1.15f
                )
            );

        Vector3 endScale =
            Vector3.one *
            finalSize;

        float lifeTime =
            Mathf.Max(
                0.01f,
                trailLifetime *
                Random.Range(
                    0.9f,
                    1.15f
                )
            );

        float rotationSpeed =
            Random.Range(
                -65f,
                65f
            );

        PuzzleTrailParticleRuntime runtime =
            root.AddComponent
            <
                PuzzleTrailParticleRuntime
            >();

        runtime.Initialize(
            outerRenderer,
            innerRenderer,
            endPosition,
            endScale,
            lifeTime,
            rotationSpeed
        );
    }


    // ============================================================
    // CREATE SOFT SPRITE
    // ============================================================

    private Sprite CreateSoftCircleSprite()
    {
        const int size = 32;

        Texture2D texture =
            new Texture2D(
                size,
                size,
                TextureFormat.ARGB32,
                false
            );

        texture.wrapMode =
            TextureWrapMode.Clamp;

        texture.filterMode =
            FilterMode.Bilinear;

        Vector2 center =
            new Vector2(
                (size - 1) * 0.5f,
                (size - 1) * 0.5f
            );

        float radius =
            size * 0.5f;

        Color[] pixels =
            new Color[size * size];

        for (int y = 0;
             y < size;
             y++)
        {
            for (int x = 0;
                 x < size;
                 x++)
            {
                Vector2 point =
                    new Vector2(
                        x,
                        y
                    );

                float distance =
                    Vector2.Distance(
                        point,
                        center
                    ) /
                    radius;

                float alpha =
                    Mathf.Clamp01(
                        1f -
                        distance
                    );

                alpha =
                    alpha *
                    alpha;

                pixels[
                    y * size + x
                ] =
                    new Color(
                        1f,
                        1f,
                        1f,
                        alpha
                    );
            }
        }

        texture.SetPixels(
            pixels
        );

        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(
                0f,
                0f,
                size,
                size
            ),
            new Vector2(
                0.5f,
                0.5f
            ),
            size
        );
    }


    // ============================================================
    // FINISH PICKUP
    // ============================================================

    private void FinishPickup()
    {
        if (pieceRenderer != null)
        {
            pieceRenderer.enabled =
                false;
        }

        if (glowRenderer != null)
        {
            glowRenderer.enabled =
                false;
        }

        StartCoroutine(
            DisableAfterSound()
        );
    }


    // ============================================================
    // WAIT FOR SOUND
    // ============================================================

    private IEnumerator DisableAfterSound()
    {
        float waitTime =
            0.05f;

        if (pickupSound != null)
        {
            waitTime =
                Mathf.Max(
                    waitTime,
                    pickupSound.length +
                    0.05f
                );
        }

        yield return
            new WaitForSeconds(
                waitTime
            );

        gameObject.SetActive(
            false
        );
    }


    // ============================================================
    // PUBLIC CHECK FOR FUTURE GALLERY
    // ============================================================

    public static bool IsPieceCollected(
        int puzzleId,
        int pieceNumber
    )
    {
        string saveKey =
            "Puzzle_" +
            puzzleId +
            "Piece" +
            pieceNumber +
            "_Collected";

        return
            PlayerPrefs.GetInt(
                saveKey,
                0
            ) == 1;
    }


    // ============================================================
    // VALIDATE
    // ============================================================

    private void OnValidate()
    {
        puzzleId =
            Mathf.Max(
                1,
                puzzleId
            );

        pieceNumber =
            Mathf.Max(
                1,
                pieceNumber
            );

        pickupDistance =
            Mathf.Max(
                0f,
                pickupDistance
            );

        preFlyRiseDuration =
            Mathf.Max(
                0f,
                preFlyRiseDuration
            );

        shakeDuration =
            Mathf.Max(
                0f,
                shakeDuration
            );

        shakeAmountX =
            Mathf.Max(
                0f,
                shakeAmountX
            );

        shakeAmountY =
            Mathf.Max(
                0f,
                shakeAmountY
            );

        shakeSpeed =
            Mathf.Max(
                0f,
                shakeSpeed
            );

        chargeScaleMultiplier =
            Mathf.Max(
                0.01f,
                chargeScaleMultiplier
            );

        pickupAnimationDuration =
            Mathf.Max(
                0.01f,
                pickupAnimationDuration
            );

        pickupEndScale =
            Mathf.Clamp01(
                pickupEndScale
            );

        pickupVolume =
            Mathf.Clamp01(
                pickupVolume
            );

        trailParticlesPerSecond =
            Mathf.Max(
                1,
                trailParticlesPerSecond
            );

        trailLifetime =
            Mathf.Max(
                0.01f,
                trailLifetime
            );

        trailStartSize =
            Mathf.Max(
                0.001f,
                trailStartSize
            );

        trailEndSize =
            Mathf.Max(
                0.001f,
                trailEndSize
            );

        startBurstCount =
            Mathf.Max(
                0,
                startBurstCount
            );

        endBurstCount =
            Mathf.Max(
                0,
                endBurstCount
            );
    }
}


// ================================================================
// RUNTIME PARTICLE
// ================================================================

public class PuzzleTrailParticleRuntime :
    MonoBehaviour
{
    private SpriteRenderer outerRenderer;
    private SpriteRenderer innerRenderer;

    private Vector3 startPosition;
    private Vector3 endPosition;

    private Vector3 startScale;
    private Vector3 endScale;

    private Color outerStartColor;
    private Color innerStartColor;

    private float lifeTime;
    private float rotationSpeed;

    private float timer;


    public void Initialize(
        SpriteRenderer outer,
        SpriteRenderer inner,
        Vector3 targetPosition,
        Vector3 targetScale,
        float duration,
        float rotateSpeed
    )
    {
        outerRenderer =
            outer;

        innerRenderer =
            inner;

        startPosition =
            transform.position;

        endPosition =
            targetPosition;

        startScale =
            transform.localScale;

        endScale =
            targetScale;

        if (outerRenderer != null)
        {
            outerStartColor =
                outerRenderer.color;
        }

        if (innerRenderer != null)
        {
            innerStartColor =
                innerRenderer.color;
        }

        lifeTime =
            Mathf.Max(
                0.01f,
                duration
            );

        rotationSpeed =
            rotateSpeed;
    }


    private void Update()
    {
        timer +=
            Time.deltaTime;

        float t =
            Mathf.Clamp01(
                timer /
                lifeTime
            );

        float smoothT =
            Mathf.SmoothStep(
                0f,
                1f,
                t
            );

        transform.position =
            Vector3.Lerp(
                startPosition,
                endPosition,
                smoothT
            );

        transform.localScale =
            Vector3.Lerp(
                startScale,
                endScale,
                smoothT
            );

        transform.Rotate(
            0f,
            0f,
            rotationSpeed *
            Time.deltaTime
        );

        float outerAlpha =
            Mathf.Lerp(
                outerStartColor.a,
                0f,
                smoothT
            );

        float innerPulse =
            0.88f +
            Mathf.Sin(
                Time.time *
                30f
            ) *
            0.12f;

        float innerAlpha =
            Mathf.Lerp(
                innerStartColor.a,
                0f,
                smoothT
            ) *
            innerPulse;

        SetRendererAlpha(
            outerRenderer,
            outerStartColor,
            outerAlpha
        );

        SetRendererAlpha(
            innerRenderer,
            innerStartColor,
            innerAlpha
        );

        if (timer >=
            lifeTime)
        {
            Destroy(
                gameObject
            );
        }
    }


    private void SetRendererAlpha(
        SpriteRenderer rendererTarget,
        Color baseColor,
        float alpha
    )
    {
        if (rendererTarget == null)
        {
            return;
        }

        Color c =
            baseColor;

        c.a =
            alpha;

        rendererTarget.color =
            c;
    }
}