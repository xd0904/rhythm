using UnityEngine;
using System.Collections;

public class BossBulletPattern : MonoBehaviour
{
    [Header("타이밍 설정")]
    public float bossActivateTime = 70f; // 1분 10초 (70초)
    public float pattern1StartTime = 89f; // 1분 29초 (89초)
    public float pattern1EndTime = 95f; // 1분 35초 (95초)
    
    [Header("대상 오브젝트")]
    public GameObject bossObject; // Boss 오브젝트 (활성화할 대상)
    public Transform mouseCursor; // 마우스/보스 커서
    public Camera mainCamera;
    public Transform gameWindow; // 게임 내부 창 오브젝트
    public Transform bossHead; // BossHead
    public Transform bossMouse; // BossMouse (발사할 마우스)
    public Transform player; // 플레이어 오브젝트
    
    [Header("마우스 이동")]
    public float mouseMoveToBottomRightDuration = 1.5f; // 이동 시간
    
    [Header("게임 창 이동")]
    public float windowMoveSpeed = 2f; // 창 이동 속도 (초당 유닛)
    public float windowChangeDirectionInterval = 1f; // 방향 변경 간격 (초)
    
    [Header("보스 마우스 공격")]
    public float bossMouseScaleUpDuration = 2f; // 마우스 커지는 시간
    public float bossMouseMaxScale = 3f; // 마우스 최대 크기 배율
    public float bossMouseLaunchSpeed = 10f; // 마우스 발사 속도
    public float bossMouseAttackInterval = 2f; // 공격 간격 (초)
    public GameObject backgroundMousePrefab; // 배경 마우스 프리팹
    public int backgroundMouseCount = 5; // 배경 마우스 개수
    
    private bool bossActivated = false;
    private bool pattern1Started = false;
    private Vector3 windowTargetPosition;
    private float windowNextDirectionChangeTime;
    private Vector3 bossMouseOriginalScale;
    private Vector3 bossMouseOriginalLocalPosition;
    private bool bossMouseInitialized = false;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        // 타이밍 강제 설정
        bossActivateTime = 70f;
        pattern1StartTime = 89f;
        pattern1EndTime = 95f;
        mouseMoveToBottomRightDuration = 1.5f;
        windowMoveSpeed = 2f;
        windowChangeDirectionInterval = 1f;
        bossMouseScaleUpDuration = 2f;
        bossMouseMaxScale = 3f;
        bossMouseLaunchSpeed = 10f;
        bossMouseAttackInterval = 2f;
        backgroundMouseCount = 5;
        
        Debug.Log($"[BossBulletPattern] 타이밍 설정: 패턴1={pattern1StartTime}초~{pattern1EndTime}초");
        
        // 마우스 자동 찾기
        if (mouseCursor == null)
        {
            GameObject mouseObj = GameObject.Find("Mouse");
            if (mouseObj != null)
            {
                mouseCursor = mouseObj.transform;
            }
        }
        
        // 보스 자동 찾기
        if (mouseCursor != null)
        {
            Transform bossParent = mouseCursor.Find("Boss");
            if (bossParent != null)
            {
                if (bossHead == null) bossHead = bossParent.Find("BossHead");
                if (bossMouse == null) bossMouse = bossParent.Find("BossMouse");
                
                if (bossMouse != null)
                {
                    bossMouseOriginalScale = bossMouse.localScale;
                    bossMouseOriginalLocalPosition = bossMouse.localPosition;
                    Debug.Log($"[BossBulletPattern] BossMouse 발견! 원래 크기: {bossMouseOriginalScale}, 위치: {bossMouseOriginalLocalPosition}");
                    
                    // 크기가 0이면 기본값 설정
                    if (bossMouseOriginalScale == Vector3.zero)
                    {
                        bossMouseOriginalScale = Vector3.one;
                        bossMouse.localScale = bossMouseOriginalScale;
                        Debug.LogWarning("[BossBulletPattern] BossMouse 크기가 0이어서 1로 설정했습니다!");
                    }
                }
            }
        }
        
        // 플레이어 자동 찾기
        if (player == null)
        {
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("[BossBulletPattern] 플레이어 발견!");
            }
        }
        
        // 게임 창 자동 찾기 (Canvas 또는 GameWindow 등)
        if (gameWindow == null)
        {
            // 일반적인 이름들로 검색
            GameObject windowObj = GameObject.Find("GameWindow");
            if (windowObj == null) windowObj = GameObject.Find("Canvas");
            if (windowObj == null) windowObj = GameObject.Find("Game Window");
            if (windowObj == null) windowObj = GameObject.Find("Window");
            
            if (windowObj != null)
            {
                gameWindow = windowObj.transform;
                Debug.Log($"[BossBulletPattern] 게임 창 발견: {windowObj.name}");
            }
            else
            {
                Debug.LogWarning("[BossBulletPattern] 게임 창을 찾을 수 없습니다. Inspector에서 수동 할당이 필요합니다.");
            }
        }
    }

    void Update()
    {
        if (BeatBounce.Instance == null) return;
        
        double musicTime = BeatBounce.Instance.GetMusicTime();
        
        // 음악이 시작 안 했으면 대기
        if (musicTime <= 0) return;
        
        // 1분 10초 (70초)에 Boss 오브젝트 활성화
        if (!bossActivated && musicTime >= bossActivateTime)
        {
            bossActivated = true;
            if (bossObject != null)
            {
                bossObject.SetActive(true);
                Debug.Log($"[BossBulletPattern] 1분 10초에 Boss 오브젝트 활성화! musicTime: {musicTime}");
            }
            else
            {
                Debug.LogWarning("[BossBulletPattern] bossObject가 할당되지 않았습니다!");
            }
        }
        
        // 1분 29초 (89초)에 패턴 1 시작
        if (!pattern1Started && musicTime >= pattern1StartTime)
        {
            Debug.Log($"[BossBulletPattern] 패턴 1 시작! musicTime: {musicTime}");
            pattern1Started = true;
            StartCoroutine(Pattern1Sequence());
        }
    }

    IEnumerator Pattern1Sequence()
    {
        Debug.Log("[BossBulletPattern] 마우스를 오른쪽 아래로 이동 시작");
        
        // 1. 마우스를 오른쪽 아래로 이동 (89초)
        yield return StartCoroutine(MoveMouseToBottomRight());
        
        // 2. 게임 창 무작위 이동 시작 (89초부터 95초까지)
        StartCoroutine(MoveGameWindowRandomly());
        
        // 3. 보스 마우스 공격 시작 (반복)
        StartCoroutine(BossMouseAttackLoop());
        
        // 4. 95초까지 대기
        while (BeatBounce.Instance.GetMusicTime() < pattern1EndTime)
        {
            yield return null;
        }
        
        Debug.Log("[BossBulletPattern] 패턴 1 종료");
    }

    IEnumerator MoveMouseToBottomRight()
    {
        if (mouseCursor == null)
        {
            Debug.LogWarning("[BossBulletPattern] 마우스 커서가 할당되지 않았습니다!");
            yield break;
        }
        
        Vector3 startPos = mouseCursor.position;
        
        // 화면 오른쪽 아래 위치 계산
        Vector3 screenBottomRight = mainCamera.ScreenToWorldPoint(
            new Vector3(Screen.width * 0.85f, Screen.height * 0.15f, 10f)
        );
        screenBottomRight.z = startPos.z; // Z 위치는 유지
        
        float elapsed = 0f;
        
        Debug.Log($"[BossBulletPattern] 마우스 이동: {startPos} → {screenBottomRight}");
        
        while (elapsed < mouseMoveToBottomRightDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / mouseMoveToBottomRightDuration;
            
            // Ease-in-out 곡선
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            mouseCursor.position = Vector3.Lerp(startPos, screenBottomRight, smoothT);
            
            yield return null;
        }
        
        // 최종 위치 확정
        mouseCursor.position = screenBottomRight;
        
        Debug.Log($"[BossBulletPattern] 마우스 이동 완료! 위치: {mouseCursor.position}");
    }

    IEnumerator MoveGameWindowRandomly()
    {
        if (gameWindow == null)
        {
            Debug.LogWarning("[BossBulletPattern] 게임 창이 할당되지 않아서 움직일 수 없습니다!");
            yield break;
        }
        
        Debug.Log("[BossBulletPattern] 게임 창 무작위 이동 시작");
        
        // 화면 경계 계산 (게임 창이 화면 밖으로 나가지 않도록)
        float screenLeft = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, 10f)).x;
        float screenRight = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, 0, 10f)).x;
        float screenBottom = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, 10f)).y;
        float screenTop = mainCamera.ScreenToWorldPoint(new Vector3(0, Screen.height, 10f)).y;
        
        // 게임 창의 크기 (RectTransform 또는 Renderer 기준)
        float windowHalfWidth = 5f; // 기본값, 나중에 실제 크기로 조정
        float windowHalfHeight = 3f;
        
        // RectTransform이 있으면 실제 크기 사용
        RectTransform rectTransform = gameWindow.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            windowHalfWidth = rectTransform.rect.width / 2f;
            windowHalfHeight = rectTransform.rect.height / 2f;
        }
        
        // 첫 목표 위치 설정
        windowTargetPosition = GetRandomPositionInScreen(
            screenLeft + windowHalfWidth, 
            screenRight - windowHalfWidth,
            screenBottom + windowHalfHeight, 
            screenTop - windowHalfHeight
        );
        windowNextDirectionChangeTime = Time.time + windowChangeDirectionInterval;
        
        // 95초까지 계속 이동
        while (BeatBounce.Instance.GetMusicTime() < pattern1EndTime)
        {
            // 현재 위치에서 목표 위치로 이동
            Vector3 currentPos = gameWindow.position;
            Vector3 direction = (windowTargetPosition - currentPos).normalized;
            
            gameWindow.position += direction * windowMoveSpeed * Time.deltaTime;
            
            // 목표 위치에 도달하거나 시간이 지나면 새 목표 설정
            float distanceToTarget = Vector3.Distance(gameWindow.position, windowTargetPosition);
            if (distanceToTarget < 0.5f || Time.time >= windowNextDirectionChangeTime)
            {
                windowTargetPosition = GetRandomPositionInScreen(
                    screenLeft + windowHalfWidth, 
                    screenRight - windowHalfWidth,
                    screenBottom + windowHalfHeight, 
                    screenTop - windowHalfHeight
                );
                windowNextDirectionChangeTime = Time.time + windowChangeDirectionInterval;
            }
            
            yield return null;
        }
        
        Debug.Log("[BossBulletPattern] 게임 창 이동 종료");
    }
    
    Vector3 GetRandomPositionInScreen(float minX, float maxX, float minY, float maxY)
    {
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);
        return new Vector3(randomX, randomY, gameWindow.position.z);
    }
    
    void InitializeBossMouse()
    {
        Debug.Log("[BossBulletPattern] BossMouse 초기화 시도...");
        
        if (bossMouse == null)
        {
            // 마우스 커서에서 다시 찾기
            if (mouseCursor != null)
            {
                Transform bossParent = mouseCursor.Find("Boss");
                if (bossParent != null)
                {
                    bossMouse = bossParent.Find("BossMouse");
                }
            }
        }
        
        if (bossMouse != null)
        {
            bossMouseOriginalScale = bossMouse.localScale;
            bossMouseOriginalLocalPosition = bossMouse.localPosition;
            
            Debug.Log($"[BossBulletPattern] BossMouse 초기화 성공! 크기: {bossMouseOriginalScale}, 위치: {bossMouseOriginalLocalPosition}");
            
            // 크기가 0이면 기본값 설정
            if (bossMouseOriginalScale == Vector3.zero || bossMouseOriginalScale.magnitude < 0.01f)
            {
                bossMouseOriginalScale = Vector3.one * 0.5f; // 기본 크기
                bossMouse.localScale = bossMouseOriginalScale;
                Debug.LogWarning($"[BossBulletPattern] BossMouse 크기가 0이어서 {bossMouseOriginalScale}로 설정했습니다!");
            }
            
            bossMouseInitialized = true;
        }
        else
        {
            Debug.LogError("[BossBulletPattern] BossMouse를 찾을 수 없습니다! Boss/BossMouse 경로를 확인하세요.");
        }
    }
    
    IEnumerator BossMouseAttackLoop()
    {
        if (bossMouse == null)
        {
            Debug.LogWarning("[BossBulletPattern] BossMouse가 없어서 공격할 수 없습니다!");
            yield break;
        }
        
        if (player == null)
        {
            Debug.LogWarning("[BossBulletPattern] 플레이어가 없어서 공격할 수 없습니다!");
            yield break;
        }
        
        Debug.Log("[BossBulletPattern] 보스 마우스 공격 루프 시작");
        
        // 95초까지 반복 공격
        while (BeatBounce.Instance.GetMusicTime() < pattern1EndTime)
        {
            // 한 번의 공격 사이클
            yield return StartCoroutine(BossMouseSingleAttack());
            
            // 다음 공격까지 대기
            yield return new WaitForSeconds(bossMouseAttackInterval);
        }
        
        Debug.Log("[BossBulletPattern] 보스 마우스 공격 루프 종료");
    }
    
    IEnumerator BossMouseSingleAttack()
    {
        Debug.Log("[BossBulletPattern] 보스 마우스 공격 시작!");
        
        // 첫 공격 시 BossMouse 초기화 (Boss가 활성화된 후)
        if (!bossMouseInitialized)
        {
            InitializeBossMouse();
        }
        
        if (bossMouse == null || bossMouseOriginalScale == Vector3.zero)
        {
            Debug.LogError("[BossBulletPattern] BossMouse 초기화 실패! 공격을 중단합니다.");
            yield break;
        }
        
        // 플레이어 위치에서 -45도 각도로 보스 위치 계산 (오른쪽 아래)
        float angleMinus45 = -45f * Mathf.Deg2Rad;
        float distance = 8f; // 플레이어로부터의 거리
        Vector3 bossAttackPosition = player.position + new Vector3(
            Mathf.Cos(angleMinus45) * distance,  // x (오른쪽)
            Mathf.Sin(angleMinus45) * distance,  // y (아래)
            0f
        );
        
        // 화면 경계 확인 (화면 안에 있도록)
        float screenLeft = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, 10f)).x;
        float screenRight = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, 0, 10f)).x;
        float screenBottom = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, 10f)).y;
        float screenTop = mainCamera.ScreenToWorldPoint(new Vector3(0, Screen.height, 10f)).y;
        
        // 보스 위치를 화면 안으로 제한 (약간의 여유 공간)
        float margin = 2f;
        bossAttackPosition.x = Mathf.Clamp(bossAttackPosition.x, screenLeft + margin, screenRight - margin);
        bossAttackPosition.y = Mathf.Clamp(bossAttackPosition.y, screenBottom + margin, screenTop - margin);
        bossAttackPosition.z = mouseCursor.position.z; // Z값은 원래 마우스 커서의 Z값 유지!
        
        // 마우스 커서를 공격 위치로 이동
        mouseCursor.position = bossAttackPosition;
        
        // 원래 위치와 크기로 리셋
        bossMouse.localPosition = bossMouseOriginalLocalPosition;
        bossMouse.localScale = bossMouseOriginalScale;
        
        Debug.Log($"[BossBulletPattern] 보스 위치: {bossAttackPosition}, 플레이어: {player.position}");
        
        // 1단계: 마우스 커지기 (2초)
        float elapsed = 0f;
        Vector3 startScale = bossMouseOriginalScale;
        Vector3 targetScale = bossMouseOriginalScale * bossMouseMaxScale;
        
        Debug.Log($"[BossBulletPattern] 크기 변화: {startScale} → {targetScale}");
        
        while (elapsed < bossMouseScaleUpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / bossMouseScaleUpDuration;
            
            // Ease-in-out으로 커지기
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            bossMouse.localScale = Vector3.Lerp(startScale, targetScale, smoothT);
            
            yield return null;
        }
        
        bossMouse.localScale = targetScale;
        Debug.Log($"[BossBulletPattern] 마우스 커짐 완료! 크기: {bossMouse.localScale}");
        
        // 2단계: -45도 각도로 발사 (왼쪽 위로)
        Vector3 startPos = bossMouse.position;
        Vector3 direction = new Vector3(-Mathf.Cos(angleMinus45), -Mathf.Sin(angleMinus45), 0f).normalized; // 왼쪽 위로
        
        Debug.Log($"[BossBulletPattern] 마우스 45도 발사! 시작: {startPos}, 방향: {direction}");
        
        // 부모에서 분리 (독립적으로 날아가도록)
        Transform originalParent = bossMouse.parent;
        bossMouse.SetParent(null);
        
        // 배경 마우스 생성 (이펙트)
        StartCoroutine(CreateBackgroundMice(direction, startPos));
        
        // 발사 애니메이션 (3초 또는 화면 밖으로 나갈 때까지)
        float launchDuration = 3f;
        elapsed = 0f;
        
        while (elapsed < launchDuration)
        {
            elapsed += Time.deltaTime;
            
            bossMouse.position += direction * bossMouseLaunchSpeed * Time.deltaTime;
            
            // 화면 밖으로 나갔는지 체크 (간단히 거리로 체크)
            float distanceTraveled = Vector3.Distance(startPos, bossMouse.position);
            if (distanceTraveled > 20f) break;
            
            yield return null;
        }
        
        Debug.Log("[BossBulletPattern] 마우스 발사 완료!");
        
        // 3단계: 원래 부모로 복귀 및 리셋
        bossMouse.SetParent(originalParent);
        bossMouse.localPosition = bossMouseOriginalLocalPosition;
        bossMouse.localScale = bossMouseOriginalScale;
        bossMouse.localRotation = Quaternion.identity;
    }
    
    IEnumerator CreateBackgroundMice(Vector3 direction, Vector3 startPos)
    {
        if (backgroundMousePrefab == null)
        {
            Debug.LogWarning("[BossBulletPattern] backgroundMousePrefab이 할당되지 않았습니다!");
            yield break;
        }
        
        Debug.Log($"[BossBulletPattern] 배경 마우스 {backgroundMouseCount}개 생성 시작");
        
        for (int i = 0; i < backgroundMouseCount; i++)
        {
            // 넓게 퍼지도록 큰 오프셋 (메인 방향의 수직 방향으로 퍼지기)
            Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f);
            float spreadDistance = Random.Range(-5f, 5f); // 좌우로 넓게 퍼지기
            float depthOffset = Random.Range(-2f, 2f); // 앞뒤로도 약간
            
            Vector3 offset = perpendicular * spreadDistance + direction * depthOffset;
            Vector3 spawnPos = startPos + offset;
            
            // 프리팹 생성
            GameObject bgMouse = Instantiate(backgroundMousePrefab, spawnPos, Quaternion.identity);
            
            // 약간의 랜덤 크기 (배경이므로 작게)
            float randomScale = Random.Range(0.5f, 0.8f);
            bgMouse.transform.localScale = Vector3.one * randomScale;
            
            // 같은 방향으로 날리기
            StartCoroutine(LaunchBackgroundMouse(bgMouse, direction));
            
            // 약간의 지연 (동시에 생성되지 않게)
            yield return new WaitForSeconds(0.05f);
        }
        
        Debug.Log("[BossBulletPattern] 배경 마우스 생성 완료");
    }
    
    IEnumerator LaunchBackgroundMouse(GameObject bgMouse, Vector3 direction)
    {
        // 배경 마우스는 메인 마우스와 비슷한 속도로
        float speed = bossMouseLaunchSpeed * Random.Range(0.9f, 1.1f);
        float lifetime = 3f;
        float elapsed = 0f;
        
        // 원래 투명도 저장 (22/255 = 약 0.086)
        float originalAlpha = 0f;
        if (bgMouse.TryGetComponent<SpriteRenderer>(out SpriteRenderer sr))
        {
            originalAlpha = sr.color.a;
        }
        
        while (elapsed < lifetime && bgMouse != null)
        {
            elapsed += Time.deltaTime;
            
            // 같은 방향으로 날아가기
            bgMouse.transform.position += direction * speed * Time.deltaTime;
            
            // 페이드 아웃 (원래 투명도에서 0으로)
            if (sr != null)
            {
                float t = 1f - (elapsed / lifetime);
                Color color = sr.color;
                color.a = originalAlpha * t; // 원래 투명도 * 페이드 비율
                sr.color = color;
            }
            
            yield return null;
        }
        
        // 수명이 다하면 삭제
        if (bgMouse != null)
        {
            Destroy(bgMouse);
        }
    }
}
