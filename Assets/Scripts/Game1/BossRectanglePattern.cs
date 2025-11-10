using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BossRectanglePattern : MonoBehaviour
{
    [Header("타이밍 설정")]
    public float patternStartTime = 108.8f; // 1분 48.8초
    public float patternEndTime = 121.6f; // 2분 1.6초
    public float patternStartTime2 = 134.4f; // 2분 14.4초 (두 번째 구간)
    public float patternEndTime2 = 147.2f; // 2분 27.2초 (두 번째 구간)
    public float beatInterval = 0.4f; // BPM 150 기준 (60/150)
    
    [Header("대상 오브젝트")]
    public Transform gameWindow; // 게임 창
    public Transform boss; // 보스 오브젝트
    
    [Header("보스 이동 설정")]
    public float bossMoveSpeed = 5f; // 보스 이동 속도
    public float bossRushSpeed = 15f; // 보스 돌진 속도
    public Vector3 bossWaitPosition = new Vector3(5f, 0f, 0f); // 오른쪽 대기 위치
    public Vector3 bossRushTargetPosition = new Vector3(-5f, 0f, 0f); // 왼쪽 돌진 목표
    
    [Header("백신 효과 설정")]
    public Color vaccineColor = new Color(0f, 1f, 0f, 1f); // 백신 초록색
    public Color originalBossColor = new Color(1f, 0f, 0f, 1f); // 원래 빨간색
    public float colorChangeDuration = 0.3f; // 색 변경 지속 시간
    public float noiseDuration = 0.2f; // 노이즈 효과 지속 시간
    public GameObject vaccineProgram; // 백신 프로그램 창 (Object2)
    public Image redGaugeImage; // 빨간색 게이지 이미지
    public Text gaugeText; // 퍼센트 텍스트
    public Text gaugeText2; // 스캔된 개수 텍스트
    public Text gaugeText4; // 진행 단계 텍스트
    public float percentIncrement = 0.08f; // 8% 증가
    private float currentPercent = 0.05f; // 현재 퍼센트 (0.05부터 시작 = 5%)
    private int scannedCount = 1024; // 스캔된 개수
    
    [Header("백신 창 표시 시간")]
    public float vaccineProgramDisplayDuration = 0.8f; // 백신 창 표시 시간 (초) - 직사각형 재소환 전에 꺼지도록 짧게
    
    [Header("직사각형 설정")]
    public GameObject[] topRectangles = new GameObject[5]; // 위쪽 직사각형 5개 (1, 2, 3, 4, 5)
    public GameObject[] bottomRectangles = new GameObject[5]; // 아래쪽 직사각형 5개 (1*, 2*, 3*, 4*, 5*)
    
    private bool patternStarted = false;
    private Bounds gameWindowBounds;
    private System.Collections.Generic.List<GameObject> spawnedRectangles = new System.Collections.Generic.List<GameObject>(); // 생성된 직사각형 추적
    
    private bool bossTagChanged1 = false; // 1분 48.8초 태그 변경 플래그
    private bool bossTagReverted1 = false; // 2분 1.6초 태그 복원 플래그
    private bool bossTagChanged2 = false; // 2분 14.4초 태그 변경 플래그
    private bool bossTagReverted2 = false; // 2분 27.2초 태그 복원 플래그
    
    void Start()
    {
        // 게임 창 정보 가져오기
        if (gameWindow != null)
        {
            gameWindowBounds = GetWindowBounds();
            Debug.Log($"[BossRectanglePattern] 게임 창 중심: {gameWindowBounds.center}, 크기: {gameWindowBounds.size}");
        }
        
        // Boss 자동 찾기
        if (boss == null)
        {
            GameObject bossObj = GameObject.Find("Boss");
            if (bossObj != null)
            {
                boss = bossObj.transform;
                Debug.Log("[BossRectanglePattern] Boss 오브젝트 자동 찾기 완료");
            }
        }
        
        // 백신 프로그램 자동 찾기 (Object2 같은 이름일 수도 있음)
        if (vaccineProgram == null)
        {
            // 여러 가능한 이름으로 찾기
            vaccineProgram = GameObject.Find("VaccineProgram");
            if (vaccineProgram == null) vaccineProgram = GameObject.Find("Object2");
            if (vaccineProgram == null) vaccineProgram = GameObject.Find("Vaccine");
            
            if (vaccineProgram != null)
            {
                Debug.Log($"[BossRectanglePattern] 백신 프로그램 자동 찾기 완료: {vaccineProgram.name}");
            }
        }
        
        // 백신 게이지 초기화
        if (redGaugeImage != null)
        {
            currentPercent = redGaugeImage.fillAmount;
            Debug.Log($"[BossRectanglePattern] 백신 게이지 초기값: {Mathf.RoundToInt(currentPercent * 100)}%");
        }
    }
    
    void Update()
    {
        if (BeatBounce.Instance == null) return;
        
        double musicTime = BeatBounce.Instance.GetMusicTime();
        
        // 1분 48.8초 (108.8초)에 Boss 태그를 RectangleBoss로 변경
        if (!bossTagChanged1 && musicTime >= 108.8f)
        {
            bossTagChanged1 = true;
            GameObject mouseCursorObj = GameObject.Find("Mouse");
            if (mouseCursorObj != null)
            {
                Transform bossTransform = mouseCursorObj.transform.Find("Boss");
                if (bossTransform != null)
                {
                    bossTransform.gameObject.tag = "RectangleBoss";
                    Debug.Log("[BossRectanglePattern] 108.8초에 Boss 태그를 'RectangleBoss'로 변경했습니다.");
                }
                else
                {
                    Debug.LogWarning("[BossRectanglePattern] Mouse > Boss를 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.LogWarning("[BossRectanglePattern] Mouse 오브젝트를 찾을 수 없습니다.");
            }
        }
        
        // 2분 1.6초 (121.6초)에 Boss 태그를 Untagged로 복원
        if (!bossTagReverted1 && musicTime >= 121.6f)
        {
            bossTagReverted1 = true;
            GameObject mouseCursorObj = GameObject.Find("Mouse");
            if (mouseCursorObj != null)
            {
                Transform bossTransform = mouseCursorObj.transform.Find("Boss");
                if (bossTransform != null)
                {
                    bossTransform.gameObject.tag = "Untagged";
                    Debug.Log("[BossRectanglePattern] 121.6초에 Boss 태그를 'Untagged'로 복원했습니다.");
                }
                else
                {
                    Debug.LogWarning("[BossRectanglePattern] Mouse > Boss를 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.LogWarning("[BossRectanglePattern] Mouse 오브젝트를 찾을 수 없습니다.");
            }
        }
        
        // 2분 14.4초 (134.4초)에 Boss 태그를 RectangleBoss로 변경
        if (!bossTagChanged2 && musicTime >= 134.4f)
        {
            bossTagChanged2 = true;
            GameObject mouseCursorObj = GameObject.Find("Mouse");
            if (mouseCursorObj != null)
            {
                Transform bossTransform = mouseCursorObj.transform.Find("Boss");
                if (bossTransform != null)
                {
                    bossTransform.gameObject.tag = "RectangleBoss";
                    Debug.Log("[BossRectanglePattern] 134.4초에 Boss 태그를 'RectangleBoss'로 변경했습니다.");
                }
                else
                {
                    Debug.LogWarning("[BossRectanglePattern] Mouse > Boss를 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.LogWarning("[BossRectanglePattern] Mouse 오브젝트를 찾을 수 없습니다.");
            }
        }
        
        // 2분 27.2초 (147.2초)에 Boss 태그를 Untagged로 복원
        if (!bossTagReverted2 && musicTime >= 147.2f)
        {
            bossTagReverted2 = true;
            GameObject mouseCursorObj = GameObject.Find("Mouse");
            if (mouseCursorObj != null)
            {
                Transform bossTransform = mouseCursorObj.transform.Find("Boss");
                if (bossTransform != null)
                {
                    bossTransform.gameObject.tag = "Untagged";
                    Debug.Log("[BossRectanglePattern] 147.2초에 Boss 태그를 'Untagged'로 복원했습니다.");
                }
                else
                {
                    Debug.LogWarning("[BossRectanglePattern] Mouse > Boss를 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.LogWarning("[BossRectanglePattern] Mouse 오브젝트를 찾을 수 없습니다.");
            }
        }
        
        // 1분 48초보다 1초 전에 보스 이동 시작 (107.8초)
        if (!patternStarted && musicTime >= patternStartTime - 1.0 && musicTime < patternStartTime)
        {
            Debug.Log($"[BossRectanglePattern] 보스 준비 이동 시작 (1차)! musicTime: {musicTime}");
            StartCoroutine(PrepareBossPosition(bossWaitPosition, patternStartTime));
        }
        
        // 1분 48.8초에 패턴 시작 (첫 번째 구간) - 정확히 108.8초에만 트리거
        if (!patternStarted && musicTime >= patternStartTime && musicTime < patternStartTime + 0.1)
        {
            patternStarted = true;
            Debug.Log($"[BossRectanglePattern] 패턴 시작 (1차)! musicTime: {musicTime}");
            StartCoroutine(RectanglePatternSequence());
        }
        
        // 2분 14.4초보다 1초 전에 보스 이동 시작 (133.4초)
        if (!patternStarted && musicTime >= patternStartTime2 - 1.0 && musicTime < patternStartTime2)
        {
            Debug.Log($"[BossRectanglePattern] 보스 준비 이동 시작 (2차)! musicTime: {musicTime}");
            StartCoroutine(PrepareBossPosition(bossWaitPosition, patternStartTime2));
        }
        
        // 2분 14.4초에 패턴 다시 시작 (두 번째 구간) - 정확히 134.4초에만 트리거
        if (!patternStarted && musicTime >= patternStartTime2 && musicTime < patternStartTime2 + 0.1)
        {
            patternStarted = true;
            Debug.Log($"[BossRectanglePattern] 패턴 시작 (2차)! musicTime: {musicTime}");
            StartCoroutine(RectanglePatternSequence2());
        }
        
        // 패턴 종료 (첫 번째 구간)
        if (patternStarted && musicTime >= patternEndTime && musicTime < patternStartTime2)
        {
            patternStarted = false;
            Debug.Log("[BossRectanglePattern] 패턴 종료 (1차)");
        }
        
        // 패턴 종료 (두 번째 구간)
        if (patternStarted && musicTime >= patternEndTime2)
        {
            patternStarted = false;
            Debug.Log("[BossRectanglePattern] 패턴 종료 (2차)");
        }
    }
    
    IEnumerator RectanglePatternSequence()
    {
        // 108.8초부터 121.6초까지 4번 반복 (총 32박자)
        // ⚠️ Boss 이동은 이미 Update()에서 1초 전에 시작했음!
        
        for (int repeat = 0; repeat < 4; repeat++)
        {
            if (BeatBounce.Instance == null || BeatBounce.Instance.GetMusicTime() >= patternEndTime)
            {
                break;
            }
            
            // === 1단계: 직사각형 생성 + 돌진 (총 8박자) ===
            // 0번 소환
            SpawnRectangle(0);
            yield return new WaitForSeconds(beatInterval / 2f); // 0.5박자
            
            // 1번 소환
            SpawnRectangle(1);
            yield return new WaitForSeconds(beatInterval / 2f); // 0.5박자
            
            // 2번 소환
            SpawnRectangle(2);
            yield return new WaitForSeconds(beatInterval / 4f); // 0.25박자
            
            // 3번 소환
            SpawnRectangle(3);
            yield return new WaitForSeconds(beatInterval / 2f); // 0.5박자
            
            // 4번 소환과 동시에 Boss 돌진!
            SpawnRectangle(4);
            if (repeat % 2 == 0)
            {
                StartCoroutine(BossRushLeft()); // 짝수 번째: 오른쪽 → 왼쪽
            }
            else
            {
                StartCoroutine(BossRushRight()); // 홀수 번째: 왼쪽 → 오른쪽
            }
            
            // 나머지 시간 대기 (총 8박자: 1.75박자 지남, 6.25박자 남음)
            yield return new WaitForSeconds(beatInterval * 6.25f);
            
            // === 2단계: 직사각형 제거 + Boss 반전 ===
            FlipBoss(); // Boss 반전
            ClearSpawnedRectangles(); // 직사각형 제거
        }
        
        Debug.Log("[BossRectanglePattern] 시퀀스 완료 (1차): 4번 반복, 32박자");
    }
    
    IEnumerator RectanglePatternSequence2()
    {
        // 134.4초부터 147.2초까지 4번 반복 (총 32박자)
        // ⚠️ Boss 이동은 이미 Update()에서 1초 전에 시작했음!
        
        for (int repeat = 0; repeat < 4; repeat++)
        {
            if (BeatBounce.Instance == null || BeatBounce.Instance.GetMusicTime() >= patternEndTime2)
            {
                break;
            }
            
            // === 1단계: 직사각형 생성 + 돌진 (총 8박자) ===
            // 0번 소환
            SpawnRectangle(0);
            yield return new WaitForSeconds(beatInterval / 2f); // 0.5박자
            
            // 1번 소환
            SpawnRectangle(1);
            yield return new WaitForSeconds(beatInterval / 2f); // 0.5박자
            
            // 2번 소환
            SpawnRectangle(2);
            yield return new WaitForSeconds(beatInterval / 4f); // 0.25박자
            
            // 3번 소환
            SpawnRectangle(3);
            yield return new WaitForSeconds(beatInterval / 2f); // 0.5박자
            
            // 4번 소환과 동시에 Boss 돌진!
            SpawnRectangle(4);
            if (repeat % 2 == 0)
            {
                StartCoroutine(BossRushLeft()); // 짝수 번째: 오른쪽 → 왼쪽
            }
            else
            {
                StartCoroutine(BossRushRight()); // 홀수 번째: 왼쪽 → 오른쪽
            }
            
            // 나머지 시간 대기 (총 8박자: 1.75박자 지남, 6.25박자 남음)
            yield return new WaitForSeconds(beatInterval * 6.25f);
            
            // === 2단계: 직사각형 제거 + Boss 반전 ===
            FlipBoss(); // Boss 반전
            ClearSpawnedRectangles(); // 직사각형 제거
        }
        
        Debug.Log("[BossRectanglePattern] 시퀀스 완료 (2차): 4번 반복, 32박자");
    }
    
    void SpawnRectangle(int index)
    {
        // 위쪽 직사각형 생성
        if (topRectangles != null && index < topRectangles.Length && topRectangles[index] != null)
        {
            GameObject topRect = Instantiate(topRectangles[index]);
            topRect.SetActive(true); // 명시적으로 활성화
            
            // 스케일 확인 및 강제 설정
            if (topRect.transform.localScale == Vector3.zero)
            {
                topRect.transform.localScale = Vector3.one;
                Debug.LogWarning($"[BossRectanglePattern] 위 직사각형 {index} 스케일이 0이어서 1로 설정!");
            }
            
            // SpriteRenderer 확인
            SpriteRenderer sr = topRect.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = true;
                Debug.Log($"[BossRectanglePattern] 위 직사각형 {index} SpriteRenderer 활성화, sortingOrder: {sr.sortingOrder}");
            }
            
            spawnedRectangles.Add(topRect); // 리스트에 추가
            Debug.Log($"[BossRectanglePattern] 위 직사각형 {index} 생성: 위치={topRect.transform.position}, 스케일={topRect.transform.localScale}, 활성={topRect.activeSelf}");
        }
        else
        {
            Debug.LogError($"[BossRectanglePattern] 위 직사각형 {index} 프리팹이 할당되지 않았습니다!");
        }
        
        // 아래쪽 직사각형 생성
        if (bottomRectangles != null && index < bottomRectangles.Length && bottomRectangles[index] != null)
        {
            GameObject bottomRect = Instantiate(bottomRectangles[index]);
            bottomRect.SetActive(true); // 명시적으로 활성화
            
            // 스케일 확인 및 강제 설정
            if (bottomRect.transform.localScale == Vector3.zero)
            {
                bottomRect.transform.localScale = Vector3.one;
                Debug.LogWarning($"[BossRectanglePattern] 아래 직사각형 {index} 스케일이 0이어서 1로 설정!");
            }
            
            // SpriteRenderer 확인
            SpriteRenderer sr = bottomRect.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = true;
                Debug.Log($"[BossRectanglePattern] 아래 직사각형 {index} SpriteRenderer 활성화, sortingOrder: {sr.sortingOrder}");
            }
            
            spawnedRectangles.Add(bottomRect); // 리스트에 추가
            Debug.Log($"[BossRectanglePattern] 아래 직사각형 {index} 생성: 위치={bottomRect.transform.position}, 스케일={bottomRect.transform.localScale}, 활성={bottomRect.activeSelf}");
        }
        else
        {
            Debug.LogError($"[BossRectanglePattern] 아래 직사각형 {index} 프리팹이 할당되지 않았습니다!");
        }
    }
    
    void ClearSpawnedRectangles()
    {
        // 생성된 모든 직사각형 제거
        foreach (GameObject rect in spawnedRectangles)
        {
            if (rect != null)
            {
                Destroy(rect);
            }
        }
        spawnedRectangles.Clear();
        Debug.Log("[BossRectanglePattern] 모든 직사각형 제거 완료");
    }
    
    IEnumerator PrepareBossPosition(Vector3 targetPosition, float patternStartTime)
    {
        if (boss == null) yield break;
        
        Vector3 startPosition = boss.position;
        float distance = Vector3.Distance(startPosition, targetPosition);
        
        // 패턴 시작까지 남은 시간 (1초 전에 호출되므로 약 1초)
        double currentTime = BeatBounce.Instance.GetMusicTime();
        float timeUntilPattern = (float)(patternStartTime - currentTime);
        
        // 남은 시간에 맞춰 속도 계산
        float requiredSpeed = distance / timeUntilPattern;
        
        // 최대 속도 제한 (너무 빠르지 않게)
        requiredSpeed = Mathf.Min(requiredSpeed, bossMoveSpeed * 5f);
        
        Debug.Log($"[BossRectanglePattern] 보스 준비 이동: {startPosition} → {targetPosition}, 거리: {distance:F2}, 남은 시간: {timeUntilPattern:F2}초, 속도: {requiredSpeed:F2}");
        
        float elapsed = 0f;
        while (elapsed < timeUntilPattern && Vector3.Distance(boss.position, targetPosition) > 0.1f)
        {
            boss.position = Vector3.MoveTowards(boss.position, targetPosition, requiredSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // 정확히 목표 위치로
        boss.position = targetPosition;
        Debug.Log($"[BossRectanglePattern] 보스 준비 완료: {boss.position}");
    }

    IEnumerator BossRushLeft()
    {
        if (boss == null) yield break;
        
        Debug.Log($"[BossRectanglePattern] Boss 돌진 시작: {boss.position} → {bossRushTargetPosition}");
        
        while (Vector3.Distance(boss.position, bossRushTargetPosition) > 0.1f)
        {
            boss.position = Vector3.MoveTowards(boss.position, bossRushTargetPosition, bossRushSpeed * Time.deltaTime);
            yield return null;
        }
        
        boss.position = bossRushTargetPosition;
        Debug.Log($"[BossRectanglePattern] Boss 돌진 완료: {boss.position}");
        
        // 벽에 부딪힘 - 색상 변경 및 백신 효과
        StartCoroutine(BossHitWallEffect());
    }
    
    IEnumerator BossRushRight()
    {
        if (boss == null) yield break;
        
        Debug.Log($"[BossRectanglePattern] Boss 역돌진 시작: {boss.position} → {bossWaitPosition}");
        
        while (Vector3.Distance(boss.position, bossWaitPosition) > 0.1f)
        {
            boss.position = Vector3.MoveTowards(boss.position, bossWaitPosition, bossRushSpeed * Time.deltaTime);
            yield return null;
        }
        
        boss.position = bossWaitPosition;
        Debug.Log($"[BossRectanglePattern] Boss 역돌진 완료: {boss.position}");
        
        // 벽에 부딪힘 - 색상 변경 및 백신 효과
        StartCoroutine(BossHitWallEffect());
    }
    
    void FlipBoss()
    {
        if (boss == null) return;
        
        // X축 스케일 반전
        Vector3 scale = boss.localScale;
        scale.x *= -1;
        boss.localScale = scale;
        
        Debug.Log($"[BossRectanglePattern] Boss 좌우 반전: scale.x = {scale.x}");
    }
    
    IEnumerator BossHitWallEffect()
    {
        if (boss == null) yield break;
        
        Debug.Log("[BossRectanglePattern] 벽 충돌 효과 시작!");
        
        // Boss의 SpriteRenderer 찾기
        SpriteRenderer bossRenderer = boss.GetComponentInChildren<SpriteRenderer>();
        if (bossRenderer == null)
        {
            // Boss 자체에 없으면 BossHead나 BossMouse에서 찾기
            Transform bossHead = boss.Find("BossHead");
            if (bossHead != null)
            {
                bossRenderer = bossHead.GetComponent<SpriteRenderer>();
            }
        }
        
        if (bossRenderer != null)
        {
            Color originalColor = bossRenderer.color;
            
            // 1. 백신 색깔로 변경
            bossRenderer.color = vaccineColor;
            Debug.Log("[BossRectanglePattern] Boss 색상 → 백신 초록색");
            
            yield return new WaitForSeconds(colorChangeDuration);
            
            // 2. 노이즈 효과 (색상 깜빡임)
            float noiseElapsed = 0f;
            while (noiseElapsed < noiseDuration)
            {
                bossRenderer.color = Random.value > 0.5f ? vaccineColor : originalBossColor;
                noiseElapsed += 0.05f;
                yield return new WaitForSeconds(0.05f);
            }
            
            // 3. 원래 빨간색으로 복구
            bossRenderer.color = originalColor;
            Debug.Log("[BossRectanglePattern] Boss 색상 → 원래 빨간색 복구");
        }
        
        // 백신 창 표시 및 퍼센트 증가
        yield return ShowVaccineAlarmAndIncreasePercent();
    }
    
    IEnumerator ShowVaccineAlarmAndIncreasePercent()
    {
        // 백신 프로그램 창 표시
        if (vaccineProgram != null)
        {
            bool wasActive = vaccineProgram.activeSelf;
            vaccineProgram.SetActive(true);
            Debug.Log($"[BossRectanglePattern] 백신 프로그램 표시 (이전 상태: {wasActive})");
        }
        
        // 퍼센트 증가 애니메이션 (Percent.cs 방식)
        if (redGaugeImage != null)
        {
            float targetPercent = Mathf.Min(currentPercent + percentIncrement, 1f);
            float startPercent = currentPercent;
            float elapsed = 0f;
            float duration = 0.5f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                currentPercent = Mathf.Lerp(startPercent, targetPercent, elapsed / duration);
                redGaugeImage.fillAmount = currentPercent;
                
                // 퍼센트 텍스트 업데이트
                if (gaugeText != null)
                {
                    gaugeText.text = Mathf.RoundToInt(currentPercent * 100) + "%";
                }
                
                // 스캔된 개수 증가 (예: 1024 -> 3000 -> 5000...)
                if (gaugeText2 != null)
                {
                    scannedCount += Random.Range(50, 150);
                    gaugeText2.text = scannedCount.ToString();
                }
                
                // 진행 단계 업데이트 (5% -> 13% -> 21%...)
                if (gaugeText4 != null)
                {
                    int stage = Mathf.FloorToInt(currentPercent * 100 / 8); // 8%마다 1단계
                    gaugeText4.text = stage + "단계";
                }
                
                yield return null;
            }
            
            currentPercent = targetPercent;
            redGaugeImage.fillAmount = currentPercent;
            
            // 최종 텍스트 업데이트
            if (gaugeText != null)
            {
                gaugeText.text = Mathf.RoundToInt(currentPercent * 100) + "%";
            }
            if (gaugeText2 != null)
            {
                gaugeText2.text = scannedCount.ToString();
            }
            if (gaugeText4 != null)
            {
                int stage = Mathf.FloorToInt(currentPercent * 100 / 8);
                gaugeText4.text = stage + "단계";
            }
            
            Debug.Log($"[BossRectanglePattern] 백신 퍼센트 증가: {Mathf.RoundToInt(currentPercent * 100)}% (스캔: {scannedCount})");
        }
        
        // 설정된 시간만큼 표시 (기본 1.5초)
        yield return new WaitForSeconds(vaccineProgramDisplayDuration);
        
        if (vaccineProgram != null)
        {
            vaccineProgram.SetActive(false);
            Debug.Log("[BossRectanglePattern] 백신 프로그램 숨기기");
        }
    }
    
    Bounds GetWindowBounds()
    {
        if (gameWindow == null) return new Bounds(Vector3.zero, new Vector3(6f, 4f, 0f));
        
        // Collider2D가 있으면 사용
        Collider2D col = gameWindow.GetComponent<Collider2D>();
        if (col != null)
        {
            return col.bounds;
        }
        
        // RectTransform인 경우 (UI)
        RectTransform rect = gameWindow.GetComponent<RectTransform>();
        if (rect != null)
        {
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Bounds bounds = new Bounds(corners[0], Vector3.zero);
            foreach (Vector3 corner in corners)
            {
                bounds.Encapsulate(corner);
            }
            return bounds;
        }
        
        // 기본값
        return new Bounds(Vector3.zero, new Vector3(6f, 4f, 0f));
    }
}
