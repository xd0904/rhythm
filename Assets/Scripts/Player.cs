using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Player : MonoBehaviour
{
    public float speed = 5f; // 이동 속도
    public float acceleration = 20f; // 가속도 (빠르게 반응)
    public float deceleration = 15f; // 감속도 (약간 미끄러짐)
    
    [Header("대쉬 설정")]
    public float dashSpeed = 15f; // 대쉬 속도
    public float dashDuration = 0.2f; // 대쉬 지속 시간
    public float dashCooldown = 0.5f; // 대쉬 쿨타임
    
    [Header("경계 설정")]
    public float minX = -8f;  // 왼쪽 경계
    public float maxX = 8f;   // 오른쪽 경계
    public float minY = -4.5f; // 아래쪽 경계
    public float maxY = 4.5f;  // 위쪽 경계
    
    [Header("죽음 설정")]
    public Color deathColor = Color.red; // 죽을 때 색상
    public float colorChangeDuration = 0.5f; // 색 변화 시간
    public float glitchDuration = 0.3f; // 글리치 효과 시간
    public Material glitchMaterial; // 글리치 셰이더 머티리얼
    public string gameOverSceneName = "GameOver"; // 게임오버 씬 이름
    public float pitchChangeSpeed = 1.5f; // 피치 변화 속도
    public float targetPitch = 0.3f; // 목표 피치 (낮고 기분 나쁜 소리)

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 currentVelocity;
    private SpriteRenderer spriteRenderer;
    private Material originalMaterial;
    private Color originalColor;
    private bool isDead = false;
    private bool isDashing = false;
    private float dashTimeLeft = 0f;
    private float dashCooldownLeft = 0f;
    private Vector2 dashDirection;

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
    }

    void Update()
    {
        // 죽었으면 입력 무시
        if (isDead) return;
        
        // 쿨타임 감소
        if (dashCooldownLeft > 0)
        {
            dashCooldownLeft -= Time.deltaTime;
        }
        
        // 입력 받기
        float moveX = Input.GetAxisRaw("Horizontal"); // A, D
        float moveY = Input.GetAxisRaw("Vertical");   // W, S

        moveInput = new Vector2(moveX, moveY).normalized; // 대각선 이동 시 속도 보정
        
        // 스페이스바로 대쉬
        if (Input.GetKeyDown(KeyCode.Space) && !isDashing && dashCooldownLeft <= 0 && moveInput.magnitude > 0.1f)
        {
            StartDash();
        }
    }

    void FixedUpdate()
    {
        Vector3 pos = transform.position;
        
        // 대쉬 중일 때
        if (isDashing)
        {
            dashTimeLeft -= Time.fixedDeltaTime;
            
            if (dashTimeLeft <= 0)
            {
                // 대쉬 종료
                isDashing = false;
                dashCooldownLeft = dashCooldown;
            }
            else
            {
                // 대쉬 속도로 이동
                rb.linearVelocity = dashDirection * dashSpeed;
                
                // 위치 제한
                pos.x = Mathf.Clamp(pos.x, minX, maxX);
                pos.y = Mathf.Clamp(pos.y, minY, maxY);
                transform.position = pos;
                
                return; // 대쉬 중에는 일반 이동 처리 안 함
            }
        }
        
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
        
        // 위치 제한 (창 밖으로 못 나가게)
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
    
    void StartDash()
    {
        isDashing = true;
        dashTimeLeft = dashDuration;
        dashDirection = moveInput.normalized;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // 이미 죽었으면 무시
        if (isDead) return;
        
        // 탄막 태그 체크
        if (other.CompareTag("Triangle") || 
            other.CompareTag("Circle") || 
            other.CompareTag("Wave") || 
            other.CompareTag("WaveAlt"))
        {
            Die();
        }
    }
    
    void Die()
    {
        isDead = true;
        
        Debug.Log("[Player] Die() 호출됨");
        
        // 입력 막기
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
            
            elapsed += Time.deltaTime;
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
        // 1단계: 색상이 빨간색으로 변함 (피치는 이미 LowerPitch에서 처리 중)
        float elapsed = 0f;
        
        while (elapsed < colorChangeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / colorChangeDuration;
            
            // 색상 변화만 처리
            spriteRenderer.color = Color.Lerp(originalColor, deathColor, t);
            
            yield return null;
        }
        
        // 2단계: 글리치 셰이더 적용하고 음악 끄기
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
        
        yield return new WaitForSeconds(glitchDuration);
        
        // 3단계: GameOver 씬으로 이동
        SceneManager.LoadScene(gameOverSceneName);
    }
}
