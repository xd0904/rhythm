// MissileLauncher.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic; // List를 사용하기 위해 추가

public class MissileLauncher : MonoBehaviour
{
    public BeatBounce beatBounce;

    // 제공된 힌트 변수들
    [Header("미사일 설정")]
    public GameObject missilePrefab;        // 미사일 프리팹 (GuidedMissile 스크립트 포함)
    public GameObject outerObjectPrefab;    // 오브젝트 프리팹
    public GameObject smallCirclePrefab;    // 팡! 터지는 프리팹
    [Header("이펙트 프리팹")]
    public GameObject beamPrefab; // 빔 쏘는 연출용
    public GameObject beamHitParticlePrefab; // 빔이 벽에 닿을 때 파티클
    public int hitParticleCount = 5; // 벽에 닿을 때 파티클 개수
    
    [Header("보스 오브젝트")]
    public GameObject bossObject; // 비활성화할 보스 오브젝트

    public int missileCount = 4;            // 한 번에 생성할 개수
    public float spawnDelay = 0.2f;         // 각 미사일 생성 간격 
    public float missileSpeed = 7f;         // 발사 속도 (GuidedMissile로 전달)

    [Header("발사 패턴 설정")]
    public float spreadAngle = 90f;         // 미사일이 펼쳐질 부채꼴 각도 (예: 90도)
    public float spawnRadius = 0.5f;        // 스포너를 중심으로 생성될 반경 (곡선 생성 느낌을 위해)

    private Transform playerTransform;      // 플레이어의 위치를 저장할 변수
    private bool bossDeactivated = false;   // 보스 비활성화 여부

    void Start()
    {
        // "Player" 태그를 가진 오브젝트를 찾습니다. (씬에 플레이어 오브젝트가 "Player" 태그를 가졌다고 가정)
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player 오브젝트를 찾을 수 없습니다. 'Player' 태그를 확인하세요.");
        }

        // 테스트를 위해 게임 시작 후 바로 발사 루틴을 시작합니다.
        if (playerTransform != null)
        {
            StartCoroutine(SpawnAndFireRoutine());
        }
    }
    
    void Update()
    {
        if (beatBounce == null) return;
        
        double musicTime = beatBounce.GetMusicTime();
        
        // 57초에 보스 오브젝트 비활성화
        if (!bossDeactivated && musicTime >= 57f)
        {
            bossDeactivated = true;
            if (bossObject != null)
            {
                bossObject.SetActive(false);
                Debug.Log("[MissileLauncher] 57초에 Boss 오브젝트 비활성화됨");
            }
        }
    }

    IEnumerator SpawnAndFireRoutine()
    {
        while (beatBounce.GetMusicTime() < 57f)
            yield return null;

        while (beatBounce.GetMusicTime() >= 57f && beatBounce.GetMusicTime() < 89f)
        {
            Vector3 fixedPlayerPos = playerTransform.position;
            List<GuidedMissile> spawnedMissiles = new List<GuidedMissile>();
            List<GameObject> spawnedOuters = new List<GameObject>(); // 🔸 큰 오브젝트 저장 리스트

            yield return new WaitForSeconds(0.6f);

            for (int i = 0; i < missileCount; i++)
            {
                float angleStep = missileCount > 1 ? spreadAngle / (missileCount - 1f) : 0;
                float angle = -spreadAngle / 2f + angleStep * i;

                Vector3 directionToTarget = (fixedPlayerPos - transform.position).normalized;
                Quaternion baseRotation = Quaternion.LookRotation(Vector3.forward, directionToTarget);
                Quaternion finalRotation = baseRotation * Quaternion.Euler(0, 0, angle);

                // 기존 탄막 위치 (작은 원)
                float innerRadius = 1.0f;
                Vector3 missilePos = fixedPlayerPos + (finalRotation * Vector3.up) * innerRadius;

                // 새 오브젝트 위치 (더 큰 원)
                float outerRadius = 2.5f;
                Vector3 outerPos = fixedPlayerPos + (finalRotation * Vector3.up) * outerRadius;

                // 탄막 생성
                GameObject missileObj = Instantiate(missilePrefab, missilePos, finalRotation);
                GuidedMissile missile = missileObj.GetComponent<GuidedMissile>();
                if (missile != null)
                {
                    missile.IsReadyToFire = false;
                    missile.missileSpeed = this.missileSpeed;
                    spawnedMissiles.Add(missile);
                }

                // 🔸 탄막 주위에 작은 원 3개 팡! 튀는 효과
                if (smallCirclePrefab != null)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        // 360도를 3등분해서 각 방향으로 튀게
                        float burstAngle = j * 120f;
                        Vector3 dir = Quaternion.Euler(0, 0, burstAngle) * Vector3.up;

                        // 살짝 랜덤 위치로 퍼지게
                        Vector3 spawnPos = missilePos + dir * Random.Range(0.2f, 0.5f);

                        GameObject circle = Instantiate(smallCirclePrefab, spawnPos, Quaternion.identity);

                        // 크기·속도 랜덤화
                        float moveDistance = Random.Range(0.8f, 1.3f);
                        float fadeTime = Random.Range(0.4f, 0.6f);
                        StartCoroutine(MoveAndFade(circle, dir, moveDistance, fadeTime));
                    }
                }

                // 🔸 더 큰 원 오브젝트 생성 + 페이드아웃
                if (outerObjectPrefab != null)
                {
                    GameObject outer = Instantiate(outerObjectPrefab, outerPos, finalRotation);
                    spawnedOuters.Add(outer);

                    // 🔹 SpriteRenderer 색상 조정 (빨간색 + 투명도 조절)
                    SpriteRenderer sr = outer.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        // 생성 순서에 따라 투명도 설정 (i=0은 불투명, i가 커질수록 투명)
                        float alpha = Mathf.Lerp(1f, 0.4f, (float)i / (missileCount - 1f));
                        sr.color = new Color(1f, 0f, 0f, alpha); // 빨간색 + 투명도
                    }
                }

                yield return new WaitForSeconds(spawnDelay);
            }

            yield return new WaitForSeconds(0.2f); // 마지막 미사일 생성 후 발사까지의 지연 걍 수동으로 해놓음

            // 🔥 빔 일제히 발사!
            foreach (var outer in spawnedOuters)
            {
                if (outer != null && beamPrefab != null)
                {
                    // 🔹 outer 즉시 흰색으로 변함
                    SpriteRenderer outerSr = outer.GetComponent<SpriteRenderer>();
                    float alpha = 1f; // 기본값

                    var data = outer.GetComponent<OuterData>();
                    if (data != null)
                        alpha = data.alpha;

                    if (outerSr != null)
                        outerSr.color = new Color(1f, 1f, 1f, alpha); // 흰색 + 같은 투명도

                    // 🔹 빔 생성 시 같은 투명도 적용
                    Vector3 beamOffset = -outer.transform.up * 8f;
                    Vector3 beamPos = outer.transform.position + beamOffset;
                    Quaternion beamRot = outer.transform.rotation;
                    GameObject beam = Instantiate(beamPrefab, beamPos, beamRot);

                    SpriteRenderer beamSr = beam.GetComponent<SpriteRenderer>();
                    if (beamSr != null)
                    {
                        Color c = beamSr.color;
                        beamSr.color = new Color(c.r, c.g, c.b, alpha);
                    }

                    // 빔이 벽에 닿는 위치 계산 및 파티클 생성
                    Vector3 beamDirection = -outer.transform.up;
                    float beamLength = 10f; // 빔이 뻗어나가는 실제 거리
                    StartCoroutine(BeamShootAndFade(beam, 1f, 0.4f, beamDirection, beamLength));
                }
            }

            foreach (var missile in spawnedMissiles)
                if (missile != null)
                    missile.IsReadyToFire = true;

            // ✅ 모든 탄막 생성 완료 후 → 큰 오브젝트 한꺼번에 사라지기 시작
            foreach (var outer in spawnedOuters)
            {
                if (outer != null)
                    StartCoroutine(FadeAndDestroy(outer, 0.8f));
            }
        }
    }

    private IEnumerator FadeAndDestroy(GameObject obj, float duration)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            yield break;
        }

        float elapsed = 0f;
        Color startColor = sr.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(obj);
    }

    private IEnumerator MoveAndFade(GameObject obj, Vector3 dir, float distance, float duration)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Vector3 startPos = obj.transform.position;
        Vector3 targetPos = startPos + dir * distance;

        float elapsed = 0f;
        Color startColor = sr.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 위치 이동
            obj.transform.position = Vector3.Lerp(startPos, targetPos, t);

            // 서서히 사라지기
            sr.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);

            // 크기 살짝 줄이기
            obj.transform.localScale = Vector3.one * (1f - 0.3f * t);

            yield return null;
        }

        Destroy(obj);
    }

    // 빔이 쏘아지는 듯한 번쩍 효과
    private IEnumerator BeamFlashEffect(GameObject outer)
    {
        SpriteRenderer sr = outer.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        float flashTime = 0.25f;
        float elapsed = 0f;

        Color originalColor = sr.color;
        Vector3 originalScale = outer.transform.localScale;

        while (elapsed < flashTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flashTime;
            // 빨강 → 흰색으로 부드럽게 전환
            sr.color = Color.Lerp(originalColor, Color.white, t);
            outer.transform.localScale = originalScale * (1f + 0.1f * Mathf.Sin(t * Mathf.PI));
            yield return null;
        }

        sr.color = Color.white; // 완전히 흰색으로 마무리
    }

    private IEnumerator BeamShootAndFade(GameObject beam, float targetLength, float duration, Vector3 beamDirection, float beamLength)
    {
        if (beam == null) yield break;

        SpriteRenderer sr = beam.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Vector3 startScale = beam.transform.localScale;
        Color startColor = sr.color;
        float elapsed = 0f;

        // 1️⃣ 빔이 빠르게 뻗어나감
        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.1f;
            beam.transform.localScale = new Vector3(startScale.x, Mathf.Lerp(0f, targetLength, t), startScale.z);
            yield return null;
        }

        // 빔이 화면 경계로 들어오는 시작 지점 계산
        Vector3 beamStartPos = beam.transform.position;
        Vector3 beamDir = beam.transform.up;
        
        // 카메라 경계
        Camera cam = Camera.main;
        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;
        Vector3 camPos = cam.transform.position;
        
        float left = camPos.x - camWidth / 2f;
        float right = camPos.x + camWidth / 2f;
        float bottom = camPos.y - camHeight / 2f;
        float top = camPos.y + camHeight / 2f;
        
        // 빔 반대 방향으로 화면 경계와 만나는 지점 찾기 (빔이 들어오는 지점)
        Vector3 reverseDir = -beamDir; // 반대 방향
        Vector3 edgePos = beamStartPos;
        float minT = float.MaxValue;
        
        // 4개 경계 체크 (반대 방향)
        if (Mathf.Abs(reverseDir.x) > 0.001f)
        {
            float t1 = (left - beamStartPos.x) / reverseDir.x;
            if (t1 > 0 && t1 < minT)
            {
                Vector3 p = beamStartPos + reverseDir * t1;
                if (p.y >= bottom && p.y <= top) { minT = t1; edgePos = p; }
            }
            
            float t2 = (right - beamStartPos.x) / reverseDir.x;
            if (t2 > 0 && t2 < minT)
            {
                Vector3 p = beamStartPos + reverseDir * t2;
                if (p.y >= bottom && p.y <= top) { minT = t2; edgePos = p; }
            }
        }
        
        if (Mathf.Abs(reverseDir.y) > 0.001f)
        {
            float t3 = (bottom - beamStartPos.y) / reverseDir.y;
            if (t3 > 0 && t3 < minT)
            {
                Vector3 p = beamStartPos + reverseDir * t3;
                if (p.x >= left && p.x <= right) { minT = t3; edgePos = p; }
            }
            
            float t4 = (top - beamStartPos.y) / reverseDir.y;
            if (t4 > 0 && t4 < minT)
            {
                Vector3 p = beamStartPos + reverseDir * t4;
                if (p.x >= left && p.x <= right) { minT = t4; edgePos = p; }
            }
        }
        
        Debug.Log($"[Beam] 중간: {beamStartPos}, 시작 모서리: {edgePos}");
        SpawnBeamHitParticles(edgePos);

        // 2️⃣ 잠깐 유지
        yield return new WaitForSeconds(duration);

        // 3️⃣ 서서히 사라짐
        elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / 0.3f);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(beam);
    }

    // 빔 선분과 카메라 뷰포트 경계의 교차점 계산
    private Vector3 CalculateLineSegmentViewportIntersection(Vector3 lineStart, Vector3 lineEnd, Vector3 bottomLeft, Vector3 topRight)
    {
        float left = bottomLeft.x;
        float right = topRight.x;
        float bottom = bottomLeft.y;
        float top = topRight.y;

        Debug.Log($"[LineSegmentIntersection] Line: {lineStart} -> {lineEnd}");
        Debug.Log($"[LineSegmentIntersection] Viewport: L:{left}, R:{right}, B:{bottom}, T:{top}");

        Vector3 direction = (lineEnd - lineStart).normalized;
        float maxLength = Vector3.Distance(lineStart, lineEnd);

        float minT = maxLength; // 빔 길이를 초과하지 않도록
        Vector3 hitPos = lineEnd; // 기본값은 빔 끝점

        // 왼쪽 경계와의 교차
        if (Mathf.Abs(direction.x) > 0.001f)
        {
            float t = (left - lineStart.x) / direction.x;
            if (t > 0 && t <= maxLength && t < minT)
            {
                Vector3 point = lineStart + direction * t;
                if (point.y >= bottom && point.y <= top)
                {
                    minT = t;
                    hitPos = point;
                    Debug.Log($"[Intersection] Left boundary at {point}");
                }
            }
        }

        // 오른쪽 경계와의 교차
        if (Mathf.Abs(direction.x) > 0.001f)
        {
            float t = (right - lineStart.x) / direction.x;
            if (t > 0 && t <= maxLength && t < minT)
            {
                Vector3 point = lineStart + direction * t;
                if (point.y >= bottom && point.y <= top)
                {
                    minT = t;
                    hitPos = point;
                    Debug.Log($"[Intersection] Right boundary at {point}");
                }
            }
        }

        // 아래 경계와의 교차
        if (Mathf.Abs(direction.y) > 0.001f)
        {
            float t = (bottom - lineStart.y) / direction.y;
            if (t > 0 && t <= maxLength && t < minT)
            {
                Vector3 point = lineStart + direction * t;
                if (point.x >= left && point.x <= right)
                {
                    minT = t;
                    hitPos = point;
                    Debug.Log($"[Intersection] Bottom boundary at {point}");
                }
            }
        }

        // 위 경계와의 교차
        if (Mathf.Abs(direction.y) > 0.001f)
        {
            float t = (top - lineStart.y) / direction.y;
            if (t > 0 && t <= maxLength && t < minT)
            {
                Vector3 point = lineStart + direction * t;
                if (point.x >= left && point.x <= right)
                {
                    minT = t;
                    hitPos = point;
                    Debug.Log($"[Intersection] Top boundary at {point}");
                }
            }
        }

        Debug.Log($"[LineSegmentIntersection] Final intersection: {hitPos}");
        return hitPos;
    }

    // Main Camera 뷰포트 경계와의 충돌 지점 계산
    private Vector3 CalculateCameraViewportHit(Vector3 startPos, Vector3 direction)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("[MissileLauncher] Main Camera를 찾을 수 없습니다!");
            return startPos + direction * 10f;
        }

        // 카메라 월드 좌표에서의 경계 계산
        float distance = Mathf.Abs(cam.transform.position.z - startPos.z);
        
        // 카메라 뷰포트의 4개 코너를 월드 좌표로 변환
        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, distance));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, distance));

        float left = bottomLeft.x;
        float right = topRight.x;
        float bottom = bottomLeft.y;
        float top = topRight.y;

        Debug.Log($"[CalculateCameraViewportHit] 카메라 경계 - L:{left}, R:{right}, B:{bottom}, T:{top}");
        Debug.Log($"[CalculateCameraViewportHit] Start: {startPos}, Dir: {direction}");

        // 광선과 4개 경계선의 교차점 계산
        float minT = float.MaxValue;
        Vector3 hitPos = startPos + direction * 100f; // 기본값

        // 왼쪽 경계 (x = left)
        if (Mathf.Abs(direction.x) > 0.001f)
        {
            float t = (left - startPos.x) / direction.x;
            if (t > 0 && t < minT)
            {
                Vector3 point = startPos + direction * t;
                if (point.y >= bottom && point.y <= top)
                {
                    minT = t;
                    hitPos = point;
                    Debug.Log($"[Hit] Left wall at {point}");
                }
            }
        }

        // 오른쪽 경계 (x = right)
        if (Mathf.Abs(direction.x) > 0.001f)
        {
            float t = (right - startPos.x) / direction.x;
            if (t > 0 && t < minT)
            {
                Vector3 point = startPos + direction * t;
                if (point.y >= bottom && point.y <= top)
                {
                    minT = t;
                    hitPos = point;
                    Debug.Log($"[Hit] Right wall at {point}");
                }
            }
        }

        // 아래쪽 경계 (y = bottom)
        if (Mathf.Abs(direction.y) > 0.001f)
        {
            float t = (bottom - startPos.y) / direction.y;
            if (t > 0 && t < minT)
            {
                Vector3 point = startPos + direction * t;
                if (point.x >= left && point.x <= right)
                {
                    minT = t;
                    hitPos = point;
                    Debug.Log($"[Hit] Bottom wall at {point}");
                }
            }
        }

        // 위쪽 경계 (y = top)
        if (Mathf.Abs(direction.y) > 0.001f)
        {
            float t = (top - startPos.y) / direction.y;
            if (t > 0 && t < minT)
            {
                Vector3 point = startPos + direction * t;
                if (point.x >= left && point.x <= right)
                {
                    minT = t;
                    hitPos = point;
                    Debug.Log($"[Hit] Top wall at {point}");
                }
            }
        }

        Debug.Log($"[CalculateCameraViewportHit] Final hit: {hitPos}");
        return hitPos;
    }

    // Main Camera 화면 경계와의 충돌 지점 계산
    private Vector3 CalculateScreenBoundaryHit(Vector3 startPos, Vector3 direction)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("[MissileLauncher] Main Camera를 찾을 수 없습니다!");
            return startPos + direction * 10f;
        }

        // 화면 경계 (월드 좌표)
        Vector3 screenBottomLeft = cam.ScreenToWorldPoint(new Vector3(0, 0, Mathf.Abs(startPos.z - cam.transform.position.z)));
        Vector3 screenTopRight = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Mathf.Abs(startPos.z - cam.transform.position.z)));

        float left = screenBottomLeft.x;
        float right = screenTopRight.x;
        float bottom = screenBottomLeft.y;
        float top = screenTopRight.y;

        Debug.Log($"[CalculateScreenBoundaryHit] Start: {startPos}, Dir: {direction}");
        Debug.Log($"[CalculateScreenBoundaryHit] Screen bounds - L:{left}, R:{right}, B:{bottom}, T:{top}");

        // 광선과 4개 경계선의 교차점 계산
        float minT = float.MaxValue;
        Vector3 hitPos = startPos + direction * 100f; // 기본값

        // 왼쪽 경계 (x = left) - 방향이 왼쪽일 때만
        if (direction.x < -0.001f)
        {
            float t = (left - startPos.x) / direction.x;
            if (t > 0 && t < minT)
            {
                Vector3 point = startPos + direction * t;
                if (point.y >= bottom && point.y <= top)
                {
                    minT = t;
                    hitPos = point;
                    Debug.Log($"[Hit] Left wall at {point}");
                }
            }
        }

        // 오른쪽 경계 (x = right) - 방향이 오른쪽일 때만
        if (direction.x > 0.001f)
        {
            float t = (right - startPos.x) / direction.x;
            if (t > 0 && t < minT)
            {
                Vector3 point = startPos + direction * t;
                if (point.y >= bottom && point.y <= top)
                {
                    minT = t;
                    hitPos = point;
                    Debug.Log($"[Hit] Right wall at {point}");
                }
            }
        }

        // 아래쪽 경계 (y = bottom) - 방향이 아래일 때만
        if (direction.y < -0.001f)
        {
            float t = (bottom - startPos.y) / direction.y;
            if (t > 0 && t < minT)
            {
                Vector3 point = startPos + direction * t;
                if (point.x >= left && point.x <= right)
                {
                    minT = t;
                    hitPos = point;
                    Debug.Log($"[Hit] Bottom wall at {point}");
                }
            }
        }

        // 위쪽 경계 (y = top) - 방향이 위일 때만
        if (direction.y > 0.001f)
        {
            float t = (top - startPos.y) / direction.y;
            if (t > 0 && t < minT)
            {
                Vector3 point = startPos + direction * t;
                if (point.x >= left && point.x <= right)
                {
                    minT = t;
                    hitPos = point;
                    Debug.Log($"[Hit] Top wall at {point}");
                }
            }
        }

        Debug.Log($"[CalculateScreenBoundaryHit] Final hit: {hitPos}");
        return hitPos;
    }

    // 빔이 벽에 닿는 위치 계산
    private Vector3 CalculateBeamHitPosition(Vector3 beamStart, Vector3 beamDirection, float beamLength)
    {
        // 빔의 끝 위치 (충분히 긴 거리)
        Vector3 beamEnd = beamStart + beamDirection * beamLength;

        // 화면 경계 계산
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[MissileLauncher] Camera.main이 없습니다!");
            return beamEnd;
        }

        // 화면 경계 (월드 좌표)
        Vector3 screenBottomLeft = cam.ScreenToWorldPoint(new Vector3(0, 0, 10f));
        Vector3 screenTopRight = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 10f));
        
        float screenLeft = screenBottomLeft.x;
        float screenRight = screenTopRight.x;
        float screenBottom = screenBottomLeft.y;
        float screenTop = screenTopRight.y;

        Vector3 hitPosition = beamEnd;
        float minDistance = float.MaxValue;

        // X축 경계 체크 (오른쪽 벽)
        if (beamDirection.x > 0.01f)
        {
            float t = (screenRight - beamStart.x) / beamDirection.x;
            if (t > 0)
            {
                Vector3 hit = beamStart + beamDirection * t;
                float dist = Vector3.Distance(beamStart, hit);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    hitPosition = hit;
                }
            }
        }
        // X축 경계 체크 (왼쪽 벽)
        else if (beamDirection.x < -0.01f)
        {
            float t = (screenLeft - beamStart.x) / beamDirection.x;
            if (t > 0)
            {
                Vector3 hit = beamStart + beamDirection * t;
                float dist = Vector3.Distance(beamStart, hit);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    hitPosition = hit;
                }
            }
        }

        // Y축 경계 체크 (위쪽 벽)
        if (beamDirection.y > 0.01f)
        {
            float t = (screenTop - beamStart.y) / beamDirection.y;
            if (t > 0)
            {
                Vector3 hit = beamStart + beamDirection * t;
                float dist = Vector3.Distance(beamStart, hit);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    hitPosition = hit;
                }
            }
        }
        // Y축 경계 체크 (아래쪽 벽)
        else if (beamDirection.y < -0.01f)
        {
            float t = (screenBottom - beamStart.y) / beamDirection.y;
            if (t > 0)
            {
                Vector3 hit = beamStart + beamDirection * t;
                float dist = Vector3.Distance(beamStart, hit);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    hitPosition = hit;
                }
            }
        }

        Debug.Log($"[MissileLauncher] 빔 시작: {beamStart}, 방향: {beamDirection}, 벽 충돌: {hitPosition}, 거리: {minDistance}");
        
        return hitPosition;
    }

    // 벽에 닿을 때 파티클 생성
    private void SpawnBeamHitParticles(Vector3 position)
    {
        if (beamHitParticlePrefab != null)
        {
            // 프리팹 사용
            for (int i = 0; i < hitParticleCount; i++)
            {
                GameObject particle = Instantiate(beamHitParticlePrefab, position, Quaternion.identity);
                Vector2 randomDir = Random.insideUnitCircle.normalized;
                StartCoroutine(AnimateHitParticle(particle, randomDir));
            }
        }
        else
        {
            // 기본 파티클 생성 (작은 원)
            for (int i = 0; i < hitParticleCount; i++)
            {
                GameObject particle = new GameObject("BeamHitParticle");
                SpriteRenderer sr = particle.AddComponent<SpriteRenderer>();

                // 작은 원 생성
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();

                sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 20f);
                sr.color = new Color(1f, 1f, 1f, 1f); // 완전한 하얀색 (불투명)
                sr.sortingOrder = 100;

                particle.transform.position = position;
                particle.transform.localScale = Vector3.one * 4f; // 크기 증가 (2f → 4f)

                // 랜덤 방향으로 튕기기
                Vector2 randomDir = Random.insideUnitCircle.normalized;
                StartCoroutine(AnimateHitParticle(particle, randomDir));
            }
        }
        
        Debug.Log($"[MissileLauncher] 파티클 생성 위치: {position}");
    }

    // 파티클 애니메이션 (약하게 팡!)
    private IEnumerator AnimateHitParticle(GameObject particle, Vector2 direction)
    {
        if (particle == null) yield break;

        SpriteRenderer sr = particle.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Vector3 startPos = particle.transform.position;
        float speed = Random.Range(2f, 4f); // 속도 증가 (0.5~1.5 → 2~4)
        float lifetime = Random.Range(0.3f, 0.6f); // 수명 증가 (0.2~0.4 → 0.3~0.6)
        float elapsed = 0f;
        Color startColor = sr.color;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;

            // 이동
            particle.transform.position = startPos + (Vector3)direction * speed * elapsed;

            // 페이드아웃
            sr.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(startColor.a, 0f, t));

            // 크기 살짝 줄이기
            particle.transform.localScale = Vector3.one * 4f * (1f - 0.5f * t);

            yield return null;
        }

        Destroy(particle);
    }

}
