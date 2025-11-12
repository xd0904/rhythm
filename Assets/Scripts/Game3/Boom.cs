using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boom : MonoBehaviour
{

    [Header("프리팹")]
    public GameObject miniWindowPrefab; // 작은 창 프리팹
    public GameObject eyeBulletPrefab; // 보스 눈 모양 탄막 프리팹
    public GameObject cannonObject1; // 대포 오브젝트 1
    public GameObject cannonObject2; // 대포 오브젝트 2 (깜빡임)

    [Header("설정")]
    public Transform spawnPoint; // 창 생성 위치
    public Transform player; // 플레이어 Transform
    public float windowSpawnRadius = 3f; // 창이 생성될 반경
    public int windowPositionCount = 8; // 창 위치 개수 (원형으로 배치)
    public float bulletSpeed = 5f; // 탄막 속도
    public float bulletRotationSpeed = 180f; // 탄막 회전 속도 (도/초)

    private List<double> beatTimings = new List<double>(); // 짝 타이밍들
    private List<GameObject> activeWindows = new List<GameObject>(); // 활성 창 리스트
    private List<Vector3> windowPositions = new List<Vector3>(); // 미리 계산된 창 위치들
    private int currentWindowPositionIndex = 0; // 현재 사용할 창 위치 인덱스
    private int cannonRotationCount = 0; // 대포 회전 횟수

    private int spawnedWindowCount = 0; // 생성된 창 개수 추적

    private bool patternActive = false;
    private int currentBeatIndex = 0;

    void Start()
    {
        InitializeBeatTimings();
        InitializeWindowPositions();
        spawnedWindowCount = 0; // 초기화

        // 플레이어가 할당되지 않았다면 자동으로 찾기
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }
    void InitializeWindowPositions()
    {
        // 원형으로 균등하게 배치된 위치들 계산
        windowPositions.Clear();
        float angleStep = 360f / windowPositionCount;

        for (int i = 0; i < windowPositionCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * windowSpawnRadius,
                Mathf.Sin(angle) * windowSpawnRadius,
                0
            );
            windowPositions.Add(spawnPoint.position + offset);
        }
    }
    void InitializeBeatTimings()
    {
        // 첫 번째 사이클 (21.7 ~ 26.5)
        beatTimings.Add(21.7);
        beatTimings.Add(22.3);
        beatTimings.Add(23.0);
        beatTimings.Add(23.7);
        beatTimings.Add(24.3);
        beatTimings.Add(25.0);
        beatTimings.Add(25.5);
        //beatTimings.Add(26.0);
        //beatTimings.Add(26.2);
        //beatTimings.Add(26.3);
        //beatTimings.Add(26.5);

        // 두 번째 사이클 (+5.3초)
        beatTimings.Add(27.0);
        beatTimings.Add(27.7);
        beatTimings.Add(28.3);
        beatTimings.Add(29.0);
        beatTimings.Add(29.7);
        beatTimings.Add(30.3);
        beatTimings.Add(30.8);
        //beatTimings.Add(31.3);
        //beatTimings.Add(31.5);
        //beatTimings.Add(31.7);
        //beatTimings.Add(31.8);
    }

    void Update()
    {
        if (Game3SequenceManager.Instance == null) return;

        double musicTime = Game3SequenceManager.Instance.GetMusicTime();

        // 패턴 시작 체크 (21.3초)
        if (!patternActive && musicTime >= 21.3)
        {
            patternActive = true;

            if (cannonObject1 != null)
            {
                cannonObject1.SetActive(true);
            }
            if (cannonObject2 != null)
            {
                cannonObject2.SetActive(true);
            }
        }

        // 패턴 종료 체크 (32초)
        if (patternActive && musicTime >= 32.0)
        {
            EndPattern();
            return;
        }

        // 비트 타이밍 체크
        if (patternActive && currentBeatIndex < beatTimings.Count)
        {
            if (musicTime >= beatTimings[currentBeatIndex])
            {
                OnBeat(currentBeatIndex);
                currentBeatIndex++;
            }
        }
    }

    void OnBeat(int beatIndex)
    {
        // 창 소환
        SpawnMiniWindow();

        // cannonObject2 깜빡임 효과
        StartCoroutine(BlinkCannon2());

        // 대포 회전 (1번째, 3번째, 5번째 비트 = 인덱스 0, 2, 4)
        if (beatIndex == 0 || beatIndex == 2 || beatIndex == 4 ||
            beatIndex == 11 || beatIndex == 13 || beatIndex == 15) // 두 번째 사이클도 포함
        {
            RotateCannon();
        }

        // 다음 비트에 탄막 발사 (마지막 비트가 아닐 경우)
        if (beatIndex + 1 < beatTimings.Count)
        {
            double nextBeatTime = beatTimings[beatIndex + 1];
            double currentTime = beatTimings[beatIndex];
            float delay = (float)(nextBeatTime - currentTime);

            StartCoroutine(FireBulletAfterDelay(delay));
        }
    }

    IEnumerator BlinkCannon2()
    {
        if (cannonObject2 != null)
        {
            cannonObject2.SetActive(false);
            yield return new WaitForSeconds(0.2f);
            cannonObject2.SetActive(true);
        }
    }

    void SpawnMiniWindow()
    {
        if (miniWindowPrefab == null) return;

        // 스폰 가능한 화면 범위 설정
        float minX = -8f, maxX = 8f;
        float minY = -4f, maxY = 4f;

        Vector3 spawnPosition;

        // 창이 있는 중앙 구역은 제외 (-4~4, -3.6~3.6)
        do
        {
            float randomX = Random.Range(minX, maxX);
            float randomY = Random.Range(minY, maxY);
            spawnPosition = new Vector3(randomX, randomY, 0);
        }
        while (spawnPosition.x >= -4f && spawnPosition.x <= 4f &&
               spawnPosition.y >= -3.6f && spawnPosition.y <= 3.6f);

        GameObject window = Instantiate(miniWindowPrefab, spawnPosition, Quaternion.identity);
        activeWindows.Add(window);
        spawnedWindowCount++;
    }

    IEnumerator FireBulletAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 모든 활성 창에서 탄막 발사
        foreach (GameObject window in activeWindows)
        {
            if (window != null)
            {
                FireEyeBullet(window.transform.position);
            }

            yield return new WaitForSeconds(0.6f);
        }
    }

    void FireEyeBullet(Vector3 startPosition)
    {
        if (eyeBulletPrefab == null) return;

        GameObject bullet = Instantiate(eyeBulletPrefab, startPosition, Quaternion.identity);

        // 탄막 스크립트 설정 (플레이어 위치 전달)
        EyeBullet bulletScript = bullet.GetComponent<EyeBullet>();
        if (bulletScript != null)
        {
            Vector3 targetPosition = player != null ? player.position : Vector3.zero;
            bulletScript.Initialize(bulletSpeed, bulletRotationSpeed, targetPosition);
        }
    }

    void RotateCannon()
    {
        cannonRotationCount++;
        float targetRotation = cannonRotationCount * 90f;

        // 두 대포 모두 부드럽게 회전
        if (cannonObject1 != null)
        {
            StartCoroutine(RotateCannonSmooth(cannonObject1, targetRotation));
        }
        if (cannonObject2 != null)
        {
            StartCoroutine(RotateCannonSmooth(cannonObject2, targetRotation));
        }
    }

    IEnumerator RotateCannonSmooth(GameObject cannon, float targetAngle)
    {
        float startAngle = cannon.transform.eulerAngles.z;
        float elapsed = 0f;
        float duration = 0.3f; // 회전 시간

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float angle = Mathf.LerpAngle(startAngle, targetAngle, t);
            cannon.transform.rotation = Quaternion.Euler(0, 0, angle);
            yield return null;
        }

        cannon.transform.rotation = Quaternion.Euler(0, 0, targetAngle);
    }

    // MiniWindow에서 호출할 public 메서드
    public void OnWindowClosed(GameObject window)
    {
        if (activeWindows.Contains(window))
        {
            activeWindows.Remove(window);
            Debug.Log($"[Pattern] 창 닫힘. 남은 창: {activeWindows.Count}개");
        }
    }

    void EndPattern()
    {
        patternActive = false;

        // 두 대포 모두 비활성화
        if (cannonObject1 != null)
        {
            cannonObject1.SetActive(false);
        }
        if (cannonObject2 != null)
        {
            cannonObject2.SetActive(false);
        }

        // 모든 창 제거
        foreach (GameObject window in activeWindows)
        {
            if (window != null)
            {
                Destroy(window);
            }
        }
        activeWindows.Clear();

        // 패턴 매니저 비활성화 또는 제거
        this.enabled = false;
    }

    void OnDestroy()
    {
        // 남아있는 모든 오브젝트 정리
        foreach (GameObject window in activeWindows)
        {
            if (window != null)
            {
                Destroy(window);
            }
        }
    }
}