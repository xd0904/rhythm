using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Player : MonoBehaviour
{
    public float speed = 5f; // 이동 속도
    public float acceleration = 20f; // 가속도 (빠르게 반응)
    public float deceleration = 15f; // 감속도 (약간 미끄러짐)
    
    [Header("대쉬 설정")]
    public float dashDistance = 3f; // 순간이동 거리
    public float dashCooldown = 0.5f; // 대쉬 쿨타임
    public int dashAfterimageCount = 3; // 잔상 개수 (줄임)
    public float afterimageDuration = 0.2f; // 잔상 지속 시간 (짧게)
    public Color dashStartColor = new Color(0.267f, 0.886f, 0.576f, 0.6f); // #44E293 투명도 60%
    public Color dashEndColor = new Color(0.267f, 0.886f, 0.576f, 0.6f); // #44E293 투명도 60%
    
    [Header("개발자 설정")]
    public bool godMode = false; // 무적 모드 (F1 토글)
    
    [Header("충돌 태그 설정")]
    public string[] dangerousTags = { "Triangle", "Circle", "Wave", "WaveAlt" }; // 플레이어를 죽이는 태그 리스트
    
    [Header("경계 설정")]
    [Tooltip("게임 창 오브젝트 (경계 계산용)")]
    public Transform gameWindow;
    
    [Tooltip("기준 창 크기 (128x128)")]
    public Vector2 referenceWindowSize = new Vector2(128f, 128f);
    
    [Tooltip("기준 창 크기일 때의 경계값")]
    public Vector2 referenceMinBounds = new Vector2(-1.47f, -1.33f);
    public Vector2 referenceMaxBounds = new Vector2(1.47f, 1.33f);
    
    [Header("Window Split 설정")]
    [Tooltip("Window Split 후 대쉬로 창 간 이동 가능 여부")]
    public bool canDashBetweenWindows = false;
    
    [Tooltip("대쉬 시 경계 무시 시간 (초)")]
    public float dashBoundaryIgnoreDuration = 0.2f;
    
    // 동적으로 계산되는 경계값
    private float minX;
    private float maxX;
    private float minY;
    private float maxY;
    
    // Window Split 관련
    private bool isWindowSplit = false;
    private GameObject[,] splitWindows;
    private bool isDashingBetweenWindows = false;
    
    // 대쉬 애니메이션 중 플래그
    private bool isDashing = false;
    
    [Header("죽음 설정")]
    public Color deathColor = Color.red; // 죽을 때 색상
    public float slowMotionDuration = 0.8f; // 슬로우모션 시간
    public float minTimeScale = 0.1f; // 최소 시간 배율 (거의 정지)
    public float colorChangeDuration = 0.5f; // 색 변화 시간
    public float glitchDuration = 0.3f; // 글리치 효과 시간
    public Material glitchMaterial; // 글리치 셰이더 머티리얼
    public string gameOverSceneName = "GameOver"; // GameOver 씬 이름
    public float pitchChangeSpeed = 1.5f; // 피치 변화 속도
    public float targetPitch = 0.3f; // 목표 피치 (낮고 기분 나쁜 소리)
    public float screenShakeIntensity = 0.3f; // 화면 흔들림 강도
    public float screenShakeDuration = 0.2f; // 화면 흔들림 시간
    public float flashDuration = 0.1f; // 화면 플래시 시간
    public int deathPulseCount = 3; // 빨간색 펄스 횟수
    public float pulseSpeed = 0.15f; // 펄스 속도

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 currentVelocity;
    private SpriteRenderer spriteRenderer;
    private Material originalMaterial;
    private Color originalColor;
    private bool isDead = false;
    private float dashCooldownLeft = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 원래 머티리얼과 색상 저장
        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.material;
            originalColor = spriteRenderer.color;
        }
        
        // Rigidbody2D 설정 강제 조정
        if (rb != null)
        {
            rb.gravityScale = 0f;        // 중력 완전히 끄기
            rb.linearDamping = 0f;       // 선형 감쇠 끄기
            rb.angularDamping = 0f;      // 회전 감쇠 끄기
            rb.constraints = RigidbodyConstraints2D.FreezeRotation; // 회전 고정
        }
        
        // 게임 창 자동 찾기
        if (gameWindow == null)
        {
            GameObject windowObj = GameObject.Find("GameWindow");
            if (windowObj == null) windowObj = GameObject.Find("Window");
            if (windowObj == null) windowObj = GameObject.Find("Game Window");
            
            if (windowObj != null)
            {
                gameWindow = windowObj.transform;
                Debug.Log($"[Player] 게임 창 자동 발견: {windowObj.name}");
            }
            else
            {
                Debug.LogWarning("[Player] 게임 창을 찾을 수 없습니다. 기본 경계값을 사용합니다.");
            }
        }
        
        // 초기 경계 계산
        UpdateBounds();
    }

    void Update()
    {
        // 죽었으면 입력 무시
        if (isDead) return;
        
        // 매 프레임마다 경계 업데이트 (창이 움직일 수 있으므로)
        UpdateBounds();
        
        // F1 키로 무적 모드 토글
        if (Input.GetKeyDown(KeyCode.F1))
        {
            godMode = !godMode;
            Debug.Log($"[Player] 무적 모드: {(godMode ? "ON" : "OFF")}");
            
            // 무적 모드 시각적 피드백 (색상 변경)
            if (spriteRenderer != null)
            {
                spriteRenderer.color = godMode ? Color.cyan : originalColor;
            }
        }
        
        // 쿨타임 감소
        if (dashCooldownLeft > 0)
        {
            dashCooldownLeft -= Time.deltaTime;
        }
        
        // 입력 받기
        float moveX = Input.GetAxisRaw("Horizontal"); // A, D
        float moveY = Input.GetAxisRaw("Vertical");   // W, S

        moveInput = new Vector2(moveX, moveY).normalized; // 대각선 이동 시 속도 보정
        
        // 스페이스바로 순간이동 대쉬
        if (Input.GetKeyDown(KeyCode.Space) && dashCooldownLeft <= 0 && moveInput.magnitude > 0.1f)
        {
            PerformDash();
        }
    }
    
    /// <summary>
    /// 게임 창의 크기와 위치에 따라 플레이어 경계를 동적으로 계산
    /// </summary>
    void UpdateBounds()
    {
        // Window Split 후에는 현재 플레이어가 있는 창의 경계 사용
        if (isWindowSplit && splitWindows != null)
        {
            CalculateSplitWindowBounds(false); // 현재 창 경계 (가장 가까운 창)
            return;
        }
        
        // 일반 경계 계산
        if (gameWindow == null)
        {
            // 게임 창이 없으면 기준값 사용
            minX = referenceMinBounds.x;
            maxX = referenceMaxBounds.x;
            minY = referenceMinBounds.y;
            maxY = referenceMaxBounds.y;
            return;
        }
        
        // 게임 창의 현재 스케일 가져오기
        Vector3 currentScale = gameWindow.localScale;
        Vector3 currentPosition = gameWindow.position;
        
        // 스케일 비율 계산 (기준 크기 대비)
        float scaleRatioX = currentScale.x / referenceWindowSize.x;
        float scaleRatioY = currentScale.y / referenceWindowSize.y;
        
        // 경계값 계산 (스케일에 비례 + 창의 중심 위치 반영)
        float halfWidth = (referenceMaxBounds.x - referenceMinBounds.x) * 0.5f * scaleRatioX;
        float halfHeight = (referenceMaxBounds.y - referenceMinBounds.y) * 0.5f * scaleRatioY;
        
        minX = currentPosition.x - halfWidth;
        maxX = currentPosition.x + halfWidth;
        minY = currentPosition.y - halfHeight;
        maxY = currentPosition.y + halfHeight;
    }
    
    /// <summary>
    /// Window Split 후 경계 계산 (중앙 창 또는 전체 창)
    /// </summary>
    void CalculateSplitWindowBounds(bool useAllWindows)
    {
        if (splitWindows == null || splitWindows.Length == 0)
        {
            Debug.LogWarning("[Player] splitWindows가 null이거나 비어있습니다!");
            return;
        }
        
        if (useAllWindows)
        {
            // 전체 창의 경계 계산 (모든 창 포함)
            float minXTemp = float.MaxValue;
            float maxXTemp = float.MinValue;
            float minYTemp = float.MaxValue;
            float maxYTemp = float.MinValue;
            
            int validWindowCount = 0;
            
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    GameObject window = splitWindows[row, col];
                    if (window != null)
                    {
                        Transform windowTransform = window.transform;
                        Vector3 pos = windowTransform.position;
                        
                        // NaN 체크
                        if (float.IsNaN(pos.x) || float.IsNaN(pos.y))
                        {
                            Debug.LogError($"[Player] 창[{row},{col}] 위치가 NaN입니다!");
                            continue;
                        }
                        
                        float halfWidth = (referenceMaxBounds.x - referenceMinBounds.x) * 0.5f;
                        float halfHeight = (referenceMaxBounds.y - referenceMinBounds.y) * 0.5f;
                        
                        minXTemp = Mathf.Min(minXTemp, pos.x - halfWidth);
                        maxXTemp = Mathf.Max(maxXTemp, pos.x + halfWidth);
                        minYTemp = Mathf.Min(minYTemp, pos.y - halfHeight);
                        maxYTemp = Mathf.Max(maxYTemp, pos.y + halfHeight);
                        
                        validWindowCount++;
                    }
                }
            }
            
            // 유효한 창이 있는 경우만 업데이트
            if (validWindowCount > 0)
            {
                minX = minXTemp;
                maxX = maxXTemp;
                minY = minYTemp;
                maxY = maxYTemp;
                
                Debug.Log($"[Player] 전체 창 경계 ({validWindowCount}개): X({minX:F2}~{maxX:F2}), Y({minY:F2}~{maxY:F2})");
            }
            else
            {
                Debug.LogError("[Player] 유효한 창이 없습니다! 경계값 업데이트 실패");
            }
        }
        else
        {
            // 현재 플레이어와 가장 가까운 창의 경계 사용
            Vector3 currentPos = transform.position;
            GameObject closestWindow = null;
            float minDistance = float.MaxValue;
            
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    GameObject window = splitWindows[row, col];
                    if (window != null)
                    {
                        Transform windowTransform = window.transform;
                        Vector3 pos = windowTransform.position;
                        
                        float distance = Vector3.Distance(currentPos, pos);
                        
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestWindow = window;
                        }
                    }
                }
            }
            
            if (closestWindow != null)
            {
                Transform windowTransform = closestWindow.transform;
                Vector3 pos = windowTransform.position;
                
                float halfWidth = (referenceMaxBounds.x - referenceMinBounds.x) * 0.5f;
                float halfHeight = (referenceMaxBounds.y - referenceMinBounds.y) * 0.5f;
                
                minX = pos.x - halfWidth;
                maxX = pos.x + halfWidth;
                minY = pos.y - halfHeight;
                maxY = pos.y + halfHeight;
            }
        }
    }
    
    /// <summary>
    /// Window Split 알림 받기 (WindowSplitEffect에서 호출)
    /// </summary>
    public void OnWindowSplit(GameObject[,] windows)
    {
        isWindowSplit = true;
        splitWindows = windows;
        canDashBetweenWindows = true;
        
        Debug.Log("[Player] Window Split 감지! 대쉬로 창 간 이동 가능");
    }
    
    /// <summary>
    /// 현재 경계값을 반환 (외부 스크립트에서 사용)
    /// </summary>
    public Vector4 GetBounds()
    {
        return new Vector4(minX, maxX, minY, maxY);
    }
    
    /// <summary>
    /// 개별 경계값 반환
    /// </summary>
    public float GetMinX() { return minX; }
    public float GetMaxX() { return maxX; }
    public float GetMinY() { return minY; }
    public float GetMaxY() { return maxY; }

    void FixedUpdate()
    {
        // 죽었으면 물리 업데이트 무시
        if (isDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        
        Vector3 pos = transform.position;
        
        // 일반 이동
        // 목표 속도 계산
        Vector2 targetVelocity = moveInput * speed;
        
        // 부드러운 가속/감속 적용
        if (moveInput.magnitude > 0.1f)
        {
            // 입력이 있으면 빠르게 가속
            currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            // 입력이 없으면 천천히 감속 (미끄러지는 느낌)
            currentVelocity = Vector2.MoveTowards(currentVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
        }
        
        // 속도 적용
        rb.linearVelocity = currentVelocity;
        
        // 대쉬 중이 아닐 때만 위치 제한 (창 밖으로 못 나가게)
        if (!isDashing)
        {
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            transform.position = pos;
            
            // 경계에 부딪혔을 때 속도 초기화 (벽에서 미끄러지지 않게)
            if (pos.x <= minX || pos.x >= maxX)
            {
                currentVelocity.x = 0;
            }
            if (pos.y <= minY || pos.y >= maxY)
            {
                currentVelocity.y = 0;
            }
        }
    }
    
    void PerformDash()
    {
        // Window Split 후에는 전체 창 경계로 대쉬 가능
        if (isWindowSplit && canDashBetweenWindows)
        {
            StartCoroutine(DashBetweenWindows());
        }
        else
        {
            // 일반 대쉬
            Vector3 currentPos = transform.position;
            Vector3 dashDirection = new Vector3(moveInput.x, moveInput.y, 0f).normalized;
            Vector3 targetPos = currentPos + dashDirection * dashDistance;
            
            // 경계 내로 제한
            targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
            targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
            
            // 순간이동 애니메이션 시작
            StartCoroutine(DashAnimation(currentPos, targetPos));
            
            // 쿨타임 시작
            dashCooldownLeft = dashCooldown;
            
            Debug.Log($"[Player] 대쉬! {currentPos} → {targetPos}");
        }
    }
    
    /// <summary>
    /// Window Split 후 창 간 대쉬 (대쉬 방향에 있는 창으로 이동)
    /// </summary>
    IEnumerator DashBetweenWindows()
    {
        isDashingBetweenWindows = true;
        
        Vector3 currentPos = transform.position;
        Vector3 dashDirection = new Vector3(moveInput.x, moveInput.y, 0f).normalized;
        
        // 입력이 없으면 대쉬 취소
        if (dashDirection.magnitude < 0.1f)
        {
            isDashingBetweenWindows = false;
            yield break;
        }
        
        // 현재 어느 창에 있는지 찾기 (가장 가까운 창)
        int currentRow = 1, currentCol = 1; // 기본값: 중앙
        float minDistance = float.MaxValue;
        
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                GameObject window = splitWindows[row, col];
                if (window != null)
                {
                    Transform windowTransform = window.transform;
                    Vector3 pos = windowTransform.position;
                    
                    // 현재 위치와 창 중심 사이의 거리
                    float distance = Vector3.Distance(currentPos, pos);
                    
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        currentRow = row;
                        currentCol = col;
                    }
                }
            }
        }
        
        Debug.Log($"[Player] 현재 창: ({currentRow},{currentCol})");
        
        // 대쉬 방향에 따라 목표 창 결정 (대각선 포함)
        int targetRow = currentRow;
        int targetCol = currentCol;
        
        // X축 방향
        if (Mathf.Abs(dashDirection.x) > 0.3f) // 임계값 0.3
        {
            if (dashDirection.x > 0) targetCol = Mathf.Min(2, currentCol + 1);
            else targetCol = Mathf.Max(0, currentCol - 1);
        }
        
        // Y축 방향
        if (Mathf.Abs(dashDirection.y) > 0.3f) // 임계값 0.3
        {
            if (dashDirection.y > 0) targetRow = Mathf.Max(0, currentRow - 1);
            else targetRow = Mathf.Min(2, currentRow + 1);
        }
        
        Debug.Log($"[Player] 창 이동: ({currentRow},{currentCol}) → ({targetRow},{targetCol})");
        
        // 원래 대쉬하려던 위치 계산
        Vector3 targetPos = currentPos + dashDirection * dashDistance;
        
        // 목표 창이 현재 창과 다르면, 목표 창의 경계로 업데이트
        if (targetRow != currentRow || targetCol != currentCol)
        {
            GameObject targetWindow = splitWindows[targetRow, targetCol];
            if (targetWindow == null)
            {
                Debug.LogWarning($"[Player] 목표 창({targetRow},{targetCol})이 없습니다. 대쉬 취소");
                isDashingBetweenWindows = false;
                yield break;
            }
            
            // 목표 창의 경계로 업데이트
            Transform targetTransform = targetWindow.transform;
            Vector3 windowCenter = targetTransform.position;
            
            float newHalfWidth = (referenceMaxBounds.x - referenceMinBounds.x) * 0.5f;
            float newHalfHeight = (referenceMaxBounds.y - referenceMinBounds.y) * 0.5f;
            
            minX = windowCenter.x - newHalfWidth;
            maxX = windowCenter.x + newHalfWidth;
            minY = windowCenter.y - newHalfHeight;
            maxY = windowCenter.y + newHalfHeight;
        }
        
        // 목표 위치를 새 경계 내로 제한
        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
        
        // 순간이동 애니메이션 시작
        StartCoroutine(DashAnimation(currentPos, targetPos));
        
        Debug.Log($"[Player] 창 간 대쉬! {currentPos} → {targetPos}, 새 경계: X({minX:F2}~{maxX:F2}), Y({minY:F2}~{maxY:F2})");
        
        // 쿨타임 시작
        dashCooldownLeft = dashCooldown;
        
        // 짧은 시간 동안 경계 무시 상태 유지
        yield return new WaitForSeconds(dashBoundaryIgnoreDuration);
        
        isDashingBetweenWindows = false;
    }
    
    IEnumerator DashAnimation(Vector3 startPos, Vector3 endPos)
    {
        isDashing = true; // 대쉬 시작
        
        Vector3 originalScale = transform.localScale;
        
        // 1. 시작 위치에 파티클 효과 + 플레이어 스케일 줄이기 (사라지는 효과)
        float disappearDuration = 0.08f;
        float elapsed = 0f;
        
        while (elapsed < disappearDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / disappearDuration;
            
            // 플레이어가 점점 작아지면서 사라짐 (1.0 → 0)
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
            
            yield return null;
        }
        
        transform.localScale = Vector3.zero; // 완전히 사라짐
        
        // 시작 위치에 파티클 폭발
        for (int i = 0; i < 8; i++)
        {
            GameObject particle = new GameObject($"DashParticle_{i}");
            SpriteRenderer particleSR = particle.AddComponent<SpriteRenderer>();
            
            // 매우 작은 사각형 (1x1)
            Texture2D particleTex = new Texture2D(1, 1);
            particleTex.SetPixel(0, 0, dashStartColor);
            particleTex.Apply();
            
            particleSR.sprite = Sprite.Create(particleTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 20f);
            particleSR.color = Color.white;
            particleSR.sortingOrder = spriteRenderer.sortingOrder - 1;
            
            particle.transform.position = startPos;
            
            // 랜덤 방향으로 튕기기
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            StartCoroutine(AnimateParticle(particle, randomDir));
        }
        
        // 2. 순간이동 실행 (완전히 사라진 상태에서)
        transform.position = endPos;
        
        // 3. 도착 위치에 파티클 먼저 생성
        for (int i = 0; i < 8; i++)
        {
            GameObject particle = new GameObject($"ArrivalParticle_{i}");
            SpriteRenderer particleSR = particle.AddComponent<SpriteRenderer>();
            
            // 매우 작은 사각형 (1x1)
            Texture2D particleTex = new Texture2D(1, 1);
            particleTex.SetPixel(0, 0, dashEndColor);
            particleTex.Apply();
            
            particleSR.sprite = Sprite.Create(particleTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 20f);
            particleSR.color = Color.white;
            particleSR.sortingOrder = spriteRenderer.sortingOrder - 1;
            
            particle.transform.position = endPos;
            
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            StartCoroutine(AnimateParticle(particle, randomDir));
        }
        
        // 4. 플레이어 스케일 크게하면서 나타남 (텔레포트 효과)
        float appearDuration = 0.1f;
        elapsed = 0f;
        
        while (elapsed < appearDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / appearDuration;
            
            // 0에서 원래 크기로 커지면서 나타남
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
            
            yield return null;
        }
        
        transform.localScale = originalScale; // 원래 크기로 복원
        isDashing = false; // 대쉬 종료
    }
    
    IEnumerator AnimateParticle(GameObject particle, Vector2 direction)
    {
        if (particle == null) yield break;
        
        SpriteRenderer sr = particle.GetComponent<SpriteRenderer>();
        Vector3 startPos = particle.transform.position;
        float speed = 2f;
        float lifetime = 0.3f;
        float elapsed = 0f;
        Color startColor = sr.color;
        
        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;
            
            // 이동
            Vector3 newPos = startPos + (Vector3)(direction * speed * elapsed);
            
            // 경계 제한 (플레이어와 동일)
            newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
            newPos.y = Mathf.Clamp(newPos.y, minY, maxY);
            
            particle.transform.position = newPos;
            
            // 페이드아웃
            Color currentColor = startColor;
            currentColor.a = Mathf.Lerp(startColor.a, 0f, t);
            sr.color = currentColor;
            
            yield return null;
        }
        
        Destroy(particle);
    }
    
    IEnumerator FadeOutAfterimage(GameObject afterimage, float duration)
    {
        // 사용 안 함 (잔상 제거됨)
        yield break;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // 이미 죽었으면 무시
        if (isDead) return;
        
        // 무적 모드면 무시
        if (godMode)
        {
            Debug.Log("[Player] 무적 모드 - 충돌 무시");
            return;
        }
        
        // 위험한 태그 리스트 체크
        foreach (string dangerousTag in dangerousTags)
        {
            if (other.CompareTag(dangerousTag))
            {
                Die();
                return;
            }
        }
    }
    
    void Die()
    {
        isDead = true;
        
        Debug.Log("[Player] Die() 호출됨");
        
        // 즉시 멈추기
        moveInput = Vector2.zero;
        currentVelocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        
        // SoundManager에서 BGM AudioSource 가져오기
        AudioSource bgmAudioSource = GetBGMAudioSource();
        
        // 죽는 순간 바로 피치 내리기 시작
        if (bgmAudioSource != null)
        {
            Debug.Log($"[Player] BGM AudioSource 발견. 현재 피치: {bgmAudioSource.pitch}");
            StartCoroutine(LowerPitch());
        }
        else
        {
            Debug.LogError("[Player] BGM AudioSource를 찾을 수 없습니다!");
        }
        
        // 죽음 연출 시작
        StartCoroutine(DeathSequence());
    }
    
    AudioSource GetBGMAudioSource()
    {
        // SoundManager에서 BGM AudioSource 가져오기
        if (SoundManager.Instance != null)
        {
            return SoundManager.Instance.BGMSource;
        }
        return null;
    }
    
    IEnumerator LowerPitch()
    {
        AudioSource bgmAudioSource = GetBGMAudioSource();
        
        if (bgmAudioSource == null)
        {
            Debug.LogError("[Player] LowerPitch - bgmAudioSource를 찾을 수 없습니다!");
            yield break;
        }
        
        float originalPitch = bgmAudioSource.pitch;
        float elapsed = 0f;
        
        Debug.Log($"[Player] LowerPitch 시작. 원래 피치: {originalPitch} → 목표 피치: {targetPitch}, AudioSource: {bgmAudioSource.name}");
        
        while (elapsed < colorChangeDuration)
        {
            if (bgmAudioSource == null) yield break;
            
            elapsed += Time.unscaledDeltaTime; // 시간 정지 중에도 동작
            float t = elapsed / colorChangeDuration;
            
            // 피치 점점 낮아짐 (강제 적용)
            float newPitch = Mathf.Lerp(originalPitch, targetPitch, t * pitchChangeSpeed);
            bgmAudioSource.pitch = newPitch;
            
            // 매 프레임마다 피치 값 로그 (5프레임마다만)
            if (Time.frameCount % 5 == 0)
            {
                Debug.Log($"[Player] 피치 변화 중: {newPitch:F2} (t={t:F2})");
            }
            
            yield return null;
        }
        
        // 최종 피치 강제 설정
        bgmAudioSource.pitch = targetPitch;
        Debug.Log($"[Player] LowerPitch 완료. 최종 피치: {bgmAudioSource.pitch}");
    }
    
    IEnumerator DeathSequence()
    {
        Camera mainCam = Camera.main;
        Vector3 originalCamPos = mainCam != null ? mainCam.transform.position : Vector3.zero;
        
        // 1단계: 화면 플래시 (하얗게 번쩍)
        GameObject flashObj = new GameObject("DeathFlash");
        SpriteRenderer flashRenderer = flashObj.AddComponent<SpriteRenderer>();
        flashRenderer.sprite = Sprite.Create(
            Texture2D.whiteTexture, 
            new Rect(0, 0, 1, 1), 
            new Vector2(0.5f, 0.5f)
        );
        flashRenderer.color = Color.white;
        flashRenderer.sortingOrder = 9999;
        flashObj.transform.position = transform.position;
        flashObj.transform.localScale = new Vector3(100f, 100f, 1f);
        
        float flashElapsed = 0f;
        while (flashElapsed < flashDuration)
        {
            flashElapsed += Time.unscaledDeltaTime;
            float alpha = 1f - (flashElapsed / flashDuration);
            flashRenderer.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }
        Destroy(flashObj);
        
        // 2단계: 시간 천천히 느려지기 + 화면 흔들림
        float slowElapsed = 0f;
        float shakeElapsed = 0f;
        
        while (slowElapsed < slowMotionDuration)
        {
            slowElapsed += Time.unscaledDeltaTime;
            shakeElapsed += Time.unscaledDeltaTime;
            
            float t = slowElapsed / slowMotionDuration;
            
            // 시간이 점점 느려짐 (1.0 → minTimeScale)
            Time.timeScale = Mathf.Lerp(1f, minTimeScale, t);
            
            // 화면 흔들림 (처음에만)
            if (mainCam != null && shakeElapsed < screenShakeDuration)
            {
                float shakeAmount = screenShakeIntensity * (1f - shakeElapsed / screenShakeDuration);
                mainCam.transform.position = originalCamPos + new Vector3(
                    Random.Range(-shakeAmount, shakeAmount),
                    Random.Range(-shakeAmount, shakeAmount),
                    0f
                );
            }
            else if (mainCam != null)
            {
                // 흔들림 끝나면 위치 복원
                mainCam.transform.position = originalCamPos;
            }
            
            yield return null;
        }
        
        // 완전 정지
        Time.timeScale = 0f;
        
        // 카메라 위치 복원
        if (mainCam != null)
        {
            mainCam.transform.position = originalCamPos;
        }
        
        // 3단계: 플레이어 빨간색 펄스 (시간 정지 상태에서)
        for (int i = 0; i < deathPulseCount; i++)
        {
            // 빨간색으로 빠르게
            float pulseIn = 0f;
            while (pulseIn < pulseSpeed)
            {
                pulseIn += Time.unscaledDeltaTime;
                float t = pulseIn / pulseSpeed;
                
                // 원래 색 → 강렬한 빨간색
                Color targetColor = Color.Lerp(deathColor, Color.white, 0.3f); // 약간 밝은 빨강
                spriteRenderer.color = Color.Lerp(originalColor, targetColor, t);
                
                yield return null;
            }
            
            // 조금 어두운 빨간색으로
            float pulseOut = 0f;
            while (pulseOut < pulseSpeed)
            {
                pulseOut += Time.unscaledDeltaTime;
                float t = pulseOut / pulseSpeed;
                
                // 밝은 빨강 → 어두운 빨강
                Color brightRed = Color.Lerp(deathColor, Color.white, 0.3f);
                Color darkRed = deathColor * 0.7f;
                spriteRenderer.color = Color.Lerp(brightRed, darkRed, t);
                
                yield return null;
            }
        }
        
        // 4단계: 최종 빨간색 + 글리치 효과 적용
        spriteRenderer.color = deathColor;
        
        if (glitchMaterial != null)
        {
            spriteRenderer.material = glitchMaterial;
        }
        
        // 소리 완전히 끄기
        AudioSource bgmAudioSource = GetBGMAudioSource();
        if (bgmAudioSource != null)
        {
            bgmAudioSource.Stop();
        }
        
        // 글리치 효과 유지 (시간은 계속 정지 상태)
        float glitchElapsed = 0f;
        while (glitchElapsed < glitchDuration)
        {
            glitchElapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        
        // 5단계: 시간 복원 후 GameOver 씬으로 이동
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameOverSceneName);
    }
}
