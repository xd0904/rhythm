using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class Wave : MonoBehaviour
{
    public BeatBounce beatBounce;

    [Header("Wave Prefabs")]
    public GameObject bounceWavePrefab; // 사용할 물결 오브젝트 프리팹
    public Transform waveParent;       // 생성된 물결을 정리할 부모 오브젝트

    [Header("Generation Settings")]
    public int seriesCount = 5;         // 한 번에 쏟아지는 줄(시리즈)의 총 개수
    public int burstCount = 3;          // 한 줄당 생성될 오브젝트의 개수 (물결 길이)
    public float waveSpacing = 0.4f;    // 한 줄 내 오브젝트 간 세로 간격
    public float burstInterval = 1.0f;  // 줄과 줄 사이의 대기 간격 (툭툭 떨어지는 템포)
    public float seriesCooldown = 1.0f; // 전체 시리즈가 끝난 후 다음 시리즈 시작까지의 대기 시간

    [Header("Movement & Appearance")]
    public float waveLifetime = 2.5f;   // ⭐물결의 길이 결정 (0.8f로 짧게 설정)
    public float moveSpeed = 0.01f;      // Y축 이동 속도 계수
    public float leftFlowSpeed = 1.0f;  // X축 이동 속도 계수 (왼쪽으로 흐름)
    public float amplitude = 1.5f;      // 좌우 흔들림 높이 계수
    public float destroyBoundaryY = 10f; // 오브젝트 파괴 Y축 경계 (화면 밖)

    private bool isSpawning = false; // 중복 생성을 막는 플래그

    public float startSyncTime = 25.7f; // ⭐ 웨이브 시작 시간 (25.7초)
    public float endSyncTime = 44.0f;   // ⭐ 웨이브 종료 시간 (44.0초)

    void Start()
    {
        if (beatBounce == null)
        {
            Debug.LogError("BeatBounce 스크립트가 할당되지 않았습니다.");
            return;
        }
       
        // 시작 동기화 코루틴 호출
        StartCoroutine(StartWaveSeriesSync());
    }


    // ====================================================================
    // 1. 시작 동기화: 25.7초까지 대기
    // ====================================================================
   
    private IEnumerator StartWaveSeriesSync()
    {
        Debug.Log($"Wave Manager: {startSyncTime}초까지 웨이브 생성을 대기합니다.");

        // 25.7초가 될 때까지 대기합니다.
        yield return new WaitUntil(() => beatBounce.GetMusicTime() >= startSyncTime);

        Debug.Log($"Wave Manager: {startSyncTime}초에 도달하여 웨이브 생성 루프를 시작합니다.");

        // 25.7초에 도달하면 웨이브 생성 루프 시작
        StartCoroutine(StartWaveSeriesLoop());
    }

    // ====================================================================
    // 2. 메인 루프: 44.0초가 되면 종료
    // ====================================================================

    private IEnumerator StartWaveSeriesLoop()
    {
        if (isSpawning) yield break;
        isSpawning = true;

        // ⭐ 44.0초를 초과하지 않을 때까지만 반복합니다.
        while (beatBounce.GetMusicTime() < endSyncTime)
        {
            // A. 위에서 아래로 쏟아지는 물결 시리즈
            Vector3 bounceTopPos = new Vector3(2f, 5f, 0f);

            // **중요:** 물결 시리즈 코루틴이 끝날 때, 이미 44초가 지났다면
            // 다음 시리즈를 실행하지 않도록 한 번 더 체크하는 것이 안정적입니다.
            yield return StartCoroutine(VerticalBurstSeriesDown(
                bounceTopPos, bounceWavePrefab, waveParent,
                seriesCount, burstCount, waveSpacing, burstInterval));

            if (beatBounce.GetMusicTime() >= endSyncTime) break; // 중간 종료 체크

            // B. 아래에서 위로 솟아오르는 물결 시리즈
            Vector3 bounceBottomPos = new Vector3(2f, -5f, 0f);
            yield return StartCoroutine(VerticalBurstSeriesUp(
                bounceBottomPos, bounceWavePrefab, waveParent,
                seriesCount, burstCount, waveSpacing, burstInterval));

            if (beatBounce.GetMusicTime() >= endSyncTime) break; // 중간 종료 체크

            // 두 시리즈가 완전히 끝난 후 다음 시리즈까지 대기
            yield return new WaitForSeconds(seriesCooldown);
        }

        isSpawning = false; // 생성 종료 플래그 OFF
        Debug.Log($"Wave Manager: {endSyncTime}초가 초과되어 웨이브 생성을 종료합니다.");
    }

    // ====================================================================
    // 2. 시리즈 생성: 한 방향으로 여러 줄(Series) 쏟아내기
    // ====================================================================

    private IEnumerator VerticalBurstSeriesDown(
        Vector3 spawnPos, GameObject wavePrefab, Transform waveParent,
        int seriesCount, int burstCount, float waveSpacing, float burstInterval)
    {
        for (int s = 0; s < seriesCount; s++)
        {
            for (int i = 0; i < burstCount; i++)
            {
                // 세로로 늘어선 줄 생성
                Vector3 offsetPos = new Vector3(0 , spawnPos.y - i * waveSpacing, spawnPos.z);
                GameObject wave = Instantiate(wavePrefab, offsetPos, Quaternion.identity, waveParent);

                // 개별 물결 이동 코루틴 시작
                StartCoroutine(VerticalDiagonalMove(wave, -1)); // 방향: 아래 (-1)

                yield return new WaitForSeconds(0.05f); // 같은 줄 내 간격
            }

            yield return new WaitForSeconds(burstInterval); // 줄 간 간격
        }
    }

    private IEnumerator VerticalBurstSeriesUp(
        Vector3 spawnPos, GameObject wavePrefab, Transform waveParent,
        int seriesCount, int burstCount, float waveSpacing, float burstInterval)
    {
        for (int s = 0; s < seriesCount; s++)
        {
            for (int i = 0; i < burstCount; i++)
            {
                // 세로로 늘어선 줄 생성
                Vector3 offsetPos = new Vector3(2, spawnPos.y + i * waveSpacing, spawnPos.z);
                GameObject wave = Instantiate(wavePrefab, offsetPos, Quaternion.identity, waveParent);

                // 개별 물결 이동 코루틴 시작
                StartCoroutine(VerticalDiagonalMove(wave, 1)); // 방향: 위 (1)

                yield return new WaitForSeconds(0.05f);
            }

            yield return new WaitForSeconds(burstInterval);
        }
    }


    // ====================================================================
    // 3. 개별 물결 이동 및 파괴 (하나의 함수로 통합)
    // ====================================================================

    private IEnumerator VerticalDiagonalMove(GameObject wave, int direction)
    {
        Vector3 startPos = wave.transform.position;
        float timer = 0f;
        float speed = 1.5f; // 타이머 속도 계수 (흔들림 속도와 연관)

        // ⭐핵심: waveLifetime보다 짧은 시간 동안만 물결을 유지하여 길이를 짧게 만듦
        while (wave != null && timer < waveLifetime)
        {
            timer += Time.deltaTime * speed;

            // --- ⭐ 수정 부분 시작 ⭐ ---

            // 1. 고유 위치(startPos.y)와 현재 시간(timer)을 결합하여 파동 계산
            //    - startPos.y: 세로 위치에 따라 위상이 달라져 연속적인 파동을 만듦.
            //    - timer: 시간이 지남에 따라 파동이 좌우로 움직이게 함.
            float wavePhase = startPos.y * 3f; // Y축 위치에 따른 위상 차이 계수 (3f는 파동의 밀도)
            float timeFactor = timer * 3f; // 시간에 따른 이동 속도 계수 (6f는 파동의 속도)

            // 좌우 흔들림 (Sin 함수를 이용해 파동 모양 생성)
            // Mathf.Sin(wavePhase + timeFactor)를 사용하여 파동이 흘러가는 듯한 효과를 만듭니다.
            float offsetX = Mathf.Sin(wavePhase + timeFactor) * amplitude * 0.5f; // 0.5f는 흔들림 크기 조정 계수

            // --- ⭐ 수정 부분 끝 ⭐ ---

            // Y축 이동 (direction: -1이면 아래, 1이면 위)
            float moveY = timer * moveSpeed * 6f * direction;

            // X축 이동 (왼쪽 흐름)
            float moveX = -timer * leftFlowSpeed * 1.8f;

            wave.transform.position = startPos + new Vector3(moveX + offsetX, moveY, 0f);

            // 화면 밖으로 완전히 나갔는지 체크 후 파괴
            if ((direction == -1 && wave.transform.position.y < -destroyBoundaryY) ||
                (direction == 1 && wave.transform.position.y > destroyBoundaryY))
            {
                Destroy(wave);
                yield break;
            }

            yield return null;
        }

        // 수명(Lifetime)이 다 되었을 때도 파괴
        if (wave != null)
            Destroy(wave);
    }
}
