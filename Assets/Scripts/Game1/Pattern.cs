using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pattern : MonoBehaviour
{
    // ⭐ 사용자님의 음악 시간 제어 컴포넌트
    public BeatBounce beatBounce;

    [Header("Bullet Prefabs & Guided Missile Settings")]
    public GameObject directionalBulletPrefab; // 탁! (동그라미) 시각 효과 탄막 프리팹
    public GameObject homingBulletPrefab;      // 파지직! (세모) 유도 탄막 프리팹 (GuidedMissile.cs가 붙어있어야 함)
    public float bulletSpeed = 5f;
    public float takInterval = 0.4f;   // '탁' 소리 간격 (세모탄막 1발 생성 주기)
    public float pazijikDelay = 0.1f;  // 세모 4발 모인 후 '파지직'까지의 딜레이

    // ⭐ 플레이어 오브젝트를 찾기 위한 변수
    private GameObject player;

    // ⭐ 세모 탄막 관리용 리스트와 카운터
    private List<GameObject> pendingGuidedMissiles = new List<GameObject>(); // 리스트 이름 변경
    private int currentHomingCount = 0;
    private const int TOTAL_HOMING_COUNT = 4; // 4개가 모였을 때 공격 시작

    // ⭐ 둥근 대형을 위한 4개의 오프셋 정의 (플레이어 중심 기준)
    private Vector3[] homingOffsets = new Vector3[]
    {
        new Vector3(-1.0f, 0.0f, 0f), // 왼쪽 
        new Vector3( 0.0f, 1.0f, 0f), // 위쪽 
        new Vector3( 1.0f, 1.0f, 0f), // 오른쪽 위
        new Vector3( 1.0f, 0.0f, 0f)  // 오른쪽
    };

    void Start()
    {
        // 플레이어 오브젝트를 미리 찾아둡니다.
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) Debug.LogError("플레이어 오브젝트에 'Player' 태그가 없습니다. 유도 기능 작동 불가.");

        // 게임 시작 시 코루틴 실행
        StartCoroutine(LaunchHomingPattern());
    }

    // ⭐ 57초부터 1분 29초(89초)까지 패턴을 실행하는 메인 코루틴
    public IEnumerator LaunchHomingPattern()
    {
        float startTime = 57f;
        float endTime = 89f;

        // 1. 패턴 시작 시간(57초)까지 대기
        while (beatBounce.GetMusicTime() < startTime)
        {
            yield return null;
        }

        float lastTakTime = startTime;

        // 2. 패턴 종료 시간까지 반복 (57초 ~ 89초)
        while (beatBounce.GetMusicTime() >= startTime && beatBounce.GetMusicTime() < endTime)
        {
            // '탁' 소리 타이밍 (세모 생성 및 시각 효과)
            if (beatBounce.GetMusicTime() >= lastTakTime + takInterval)
            {
                // 1. 시각 효과 탄막 (동그라미) 3개 팡! (고정 방향으로 즉시 발사)
                StartCoroutine(SpawnVisualEffectBullets(3, player.transform.position, 0f, 120f));

                // 2. 세모 유도탄 생성 및 대기 (리스트에 추가)
                SpawnGuidedMissileAndHold(); // 함수 이름도 GuidedMissile로 변경할 수 있으나, 기존 호출 유지

                // 3. 4개가 모였는지 확인하고 일제 공격 시작
                if (currentHomingCount >= TOTAL_HOMING_COUNT)
                {
                    // '파지직' 딜레이 후 일제히 공격 시작
                    StartCoroutine(LaunchHomingAttackAfterDelay(pazijikDelay));
                    currentHomingCount = 0; // 카운터 초기화
                }

                lastTakTime += takInterval;
            }

            yield return null;
        }
    }

    // --- 서브 함수들 ---

    // 세모탄막 생성 및 리스트에 추가 (GuidedMissile)
    private void SpawnGuidedMissileAndHold()
    {
        if (currentHomingCount >= TOTAL_HOMING_COUNT) return;

        // 1. 오프셋 계산
        Vector3 offset = homingOffsets[currentHomingCount];

        // 2. 탄막 생성: 탄막은 플레이어의 현재 위치 + 오프셋 위치에 '고정'되어 생성됩니다.
        GameObject guidedMissile = Instantiate(homingBulletPrefab, player.transform.position + offset, Quaternion.identity);

        // ⭐ 3. GuidedMissile 스크립트에 초기 오프셋 값 전달
        GuidedMissile missileScript = guidedMissile.GetComponent<GuidedMissile>();
        if (missileScript != null)
        {
            missileScript.initialOffset = offset;
        }

        pendingGuidedMissiles.Add(guidedMissile); // 리스트에 추가
        currentHomingCount++;
    }

    // 시각 효과 탄막 (동그라미) 생성
    private IEnumerator SpawnVisualEffectBullets(int count, Vector3 spawnPos, float startAngle, float angleIncrement)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + i * angleIncrement;
            Vector3 direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0).normalized;

            GameObject bullet = Instantiate(directionalBulletPrefab, spawnPos, Quaternion.identity);
            // 참고: directionalBulletPrefab의 이동 로직은 이 스크립트 외부에 있어야 합니다.
        }
        yield return null;
    }

    // '파지직' - 딜레이 후 일제히 공격 시작 코루틴
    private IEnumerator LaunchHomingAttackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 리스트에 있는 모든 탄막에 공격 명령 전달
        foreach (GameObject missile in pendingGuidedMissiles)
        {
            if (missile != null)
            {
                // ⭐ GuidedMissile 스크립트의 Launch() 함수를 호출하여 이동 시작
                GuidedMissile missileScript = missile.GetComponent<GuidedMissile>();
                if (missileScript != null)
                {
                    missileScript.Launch();
                }
            }
        }
        pendingGuidedMissiles.Clear(); // 리스트 비우기
    }
}
