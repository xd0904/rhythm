using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShockWave : MonoBehaviour
{
    [Header("충격파 프리팹")]
    public GameObject shockwavePrefab;    // '빠'
    public GameObject shockwavePrefab2;   // '따'

    [Header("음악 시간 정보")]
    public BeatBounce beatBounce;

    [Header("활성화 시간 구간 (초 단위)")]
    public float startTime = 70f;  // 1:10
    public float endTime = 83.2f;  // 1:23.2

    [Header("보스 관련")]
    public GameObject Bossobj;

    private List<double> beatTimings = new List<double>();
    private int nextBeatIndex = 0;
    private List<GameObject> activeShockwaves = new List<GameObject>();
    private bool bossSpawned = false;

    void Start()
    {
        InitializeBeatTimings();
        Bossobj.SetActive(false);
    }

    void Update()
    {
        double currentTime = beatBounce.GetMusicTime();

        // 활성 구간 밖이면 리턴
        if (currentTime < startTime || currentTime > endTime) return;

        // 보스 활성화
        if (!bossSpawned && currentTime >= startTime)
        {
            bossSpawned = true;
            Bossobj.SetActive(true);
        }

        // 다음 비트 타이밍 체크
        if (nextBeatIndex < beatTimings.Count && currentTime >= beatTimings[nextBeatIndex])
        {
            SpawnShockwaveForBeat(nextBeatIndex);
            nextBeatIndex++;
        }
    }

    void InitializeBeatTimings()
    {
        // '빠'와 '따'만 충격파, 초 단위로 변환
        double[] baseTimes = new double[]
        {
            70.8, 72.4,    // 빠
            73.6, 74.8, 75.2 // 따
        };

        // +6.4초씩 2회 반복
        for (int cycle = 0; cycle < 2; cycle++)
        {
            double offset = 6.4 * cycle;
            foreach (var t in baseTimes)
                beatTimings.Add(t + offset);
        }
        // 최종 beatTimings: 70.8,72.4,73.6,74.8,75.2, 77.2,78.8,80.0,81.2,81.6
    }

    void SpawnShockwaveForBeat(int index)
    {
        Vector2 bossPos = Bossobj.transform.position;

        GameObject prefab;
        float expandSpeed, lifetime, maxScale;

        // 인덱스 기준: 0,1,5,6 → 빠 → 소형
        //             2,3,4,7,8,9 → 따 → 대형
        if (index == 0 || index == 1 || index == 5 || index == 6)
        {
            prefab = shockwavePrefab;
            expandSpeed = 1f;
            lifetime = 1.2f;
            maxScale = 5f;
        }
        else
        {
            prefab = shockwavePrefab2;
            expandSpeed = 1.5f;
            lifetime = 1.6f;
            maxScale = 5f;
        }

        StartCoroutine(SpawnAndExpandShockwave(bossPos, prefab, expandSpeed, lifetime, maxScale));
    }

    IEnumerator SpawnAndExpandShockwave(Vector2 pos, GameObject prefab, float expandSpeed, float lifetime, float maxScale)
    {
        GameObject obj = Instantiate(prefab, pos, Quaternion.identity);
        activeShockwaves.Add(obj);

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        Vector3 startScale = obj.transform.localScale;
        float elapsed = 0f;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;
            obj.transform.localScale = startScale * Mathf.Lerp(1f, maxScale, t * expandSpeed);

            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                sr.color = c;
            }

            yield return null;
        }

        activeShockwaves.Remove(obj);
        Destroy(obj);
    }
}
