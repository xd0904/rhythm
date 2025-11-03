using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Player : MonoBehaviour
{
    public float speed = 5f; // 이동 속도
    public float acceleration = 20f; // 가속도 (빠르게 반응)
    public float deceleration = 15f; // 감속도 (약간 미끄러짐)
    
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
    public AudioSource bgmAudioSource; // BGM 오디오 소스
    public float pitchChangeSpeed = 1.5f; // 피치 변화 속도
    public float targetPitch = 0.3f; // 목표 피치 (낮고 기분 나쁜 소리)

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 currentVelocity;
    private SpriteRenderer spriteRenderer;
    private Material originalMaterial;
    private Color originalColor;
    private bool isDead = false;

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
        
        // 입력 받기
        float moveX = Input.GetAxisRaw("Horizontal"); // A, D
        float moveY = Input.GetAxisRaw("Vertical");   // W, S

        moveInput = new Vector2(moveX, moveY).normalized; // 대각선 이동 시 속도 보정
    }

    void FixedUpdate()
    {
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
        Vector3 pos = transform.position;
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
        
        // 입력 막기
        moveInput = Vector2.zero;
        currentVelocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        
        // 죽음 연출 시작
        StartCoroutine(DeathSequence());
    }
    
    IEnumerator DeathSequence()
    {
        // 1단계: 색상이 빨간색으로 변하면서 음악 피치가 낮아짐
        float elapsed = 0f;
        float originalPitch = bgmAudioSource != null ? bgmAudioSource.pitch : 1f;
        
        while (elapsed < colorChangeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / colorChangeDuration;
            
            // 색상 변화
            spriteRenderer.color = Color.Lerp(originalColor, deathColor, t);
            
            // 음악 피치 점점 낮아짐 (기분 나쁘게)
            if (bgmAudioSource != null)
            {
                bgmAudioSource.pitch = Mathf.Lerp(originalPitch, targetPitch, t * pitchChangeSpeed);
            }
            
            yield return null;
        }
        
        // 2단계: 글리치 셰이더 적용하고 음악 끄기
        if (glitchMaterial != null)
        {
            spriteRenderer.material = glitchMaterial;
        }
        
        // 소리 완전히 끄기
        if (bgmAudioSource != null)
        {
            bgmAudioSource.Stop();
        }
        
        yield return new WaitForSeconds(glitchDuration);
        
        // 3단계: GameOver 씬으로 이동
        SceneManager.LoadScene(gameOverSceneName);
    }
}
