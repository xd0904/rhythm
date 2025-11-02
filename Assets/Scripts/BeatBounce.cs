using UnityEngine;

public class BeatBounce : MonoBehaviour
{
    public Transform circle;      // 원 오브젝트
    public float bpm = 120f;      // 비트 속도 (설정 가능)
    public float maxScale = 1.3f; // 얼마나 커질지
    public float smooth = 5f;     // 커지고 줄어드는 부드러움

    private float timer = 0f;
    private float beatInterval;   // 한 비트 주기 (초 단위)
    private Vector3 baseScale;    // 원래 크기

    void Start()
    {
        baseScale = circle.localScale;
        beatInterval = 60f / bpm;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 비트마다 "쿵" 트리거
        if (timer >= beatInterval)
        {
            timer -= beatInterval;
            // 살짝 커졌다 줄어드는 효과 트리거
            StopAllCoroutines();
            StartCoroutine(BounceOnce());
        }
    }

    System.Collections.IEnumerator BounceOnce()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * smooth;
            float scaleFactor = 1f + Mathf.Sin(t * Mathf.PI) * (maxScale - 1f);
            circle.localScale = baseScale * scaleFactor;
            yield return null;
        }
        circle.localScale = baseScale;
    }

    public void SetBPM(float newBpm)
    {
        bpm = newBpm;
        beatInterval = 60f / bpm;
    }
}

