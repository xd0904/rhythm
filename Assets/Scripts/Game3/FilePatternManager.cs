using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Game3에서 10.7초~21.3초 동안 파일 패턴을 관리
/// 디디 디디디딩 패턴을 4회 반복
/// </summary>
public class FilePatternManager : MonoBehaviour
{
    [Header("타이밍 설정")]
    [Tooltip("패턴 시작 시간 (초)")]
    public float patternStartTime = 10.7f;
    
    [Tooltip("패턴 종료 시간 (초)")]
    public float patternEndTime = 21.3f;
    
    [Header("오브젝트 설정")]
    [Tooltip("파일 폴더 오브젝트 (씬에 미리 배치된 것)")]
    public GameObject folderObject;
    
    [Tooltip("일반 문서 탄막 프리팹")]
    public GameObject documentBulletPrefab;
    
    [Tooltip("큰 문서 탄막 프리팹 (딩 박자용)")]
    public GameObject bigDocumentBulletPrefab;
    
    [Header("Canvas 설정")]
    [Tooltip("탄막이 생성될 Canvas")]
    public Canvas targetCanvas;
    
    [Header("폴더 설정")]
    [Tooltip("폴더 시작 위치 오프셋 (현재 위치 기준)")]
    public Vector3 folderStartOffset = new Vector3(0, -300f, 0);
    
    [Tooltip("폴더 상승 시작 시간 (패턴 시작 전)")]
    public float folderRiseStartTime = 8.7f; // 10.7초 2초 전에 시작
    
    [Tooltip("폴더 상승 시간 (초)")]
    public float folderRiseDuration = 2f;
    
    [Header("탄막 설정")]
    [Tooltip("일반 문서 탄막 속도")]
    public float documentSpeed = 5f;
    
    [Tooltip("큰 문서 탄막 속도 (느림)")]
    public float bigDocumentSpeed = 3f;
    
    [Tooltip("탄막 발사 각도들 (8개 고정)")]
    public float[] bulletAngles = new float[] { 40f, 30f, 20f, 10f, -10f, -20f, -30f, -40f };
    
    [Tooltip("탄막 수명 (초)")]
    public float bulletLifetime = 10f;
    
    [Header("BPM 설정")]
    [Tooltip("BPM (비트 속도) - Game3SequenceManager와 동일해야 함")]
    public float bpm = 180f;
    
    // 비트 타이밍 데이터 (초 단위)
    // [타입] - 타입: 0=작은파일탄막(8개), 1=큰파일탄막(8개)
    // 180 BPM = 1박자 0.333초
    private float beatInterval; // 1박자 간격
    
    private float[][] beatTimings;
    
    private bool hasStarted = false;
    private bool folderRising = false;
    private bool folderRiseStarted = false;
    private int currentBeatIndex = 0;
    private int patternRepeatCount = 0; // 0~3 (총 4회)
    private List<GameObject> activeBullets = new List<GameObject>();
    private int fireCounter = 0; // 짝수 번째 "디" 발사 카운터
    private HashSet<int> processedBeats = new HashSet<int>(); // 처리된 비트 인덱스
    private Vector3 folderOriginalPosition;
    private Vector3 folderTargetPosition;
    
    void Start()
    {
        // BPM 기반 박자 간격 계산
        beatInterval = 60f / bpm; // 180 BPM = 0.333초
        
        // 비트 타이밍 데이터 초기화 (박자 기반)
        // 16박자 패턴을 2번 반복 (총 32박자)
        beatTimings = new float[][]
        {
            // ========== 1회차 (박자 2~15) ==========
            // 박자 2: 작은 파일 탄막
            new float[] { patternStartTime + beatInterval * 2, 0 },
            
            // 박자 3.5: 작은 파일 탄막
            new float[] { patternStartTime + beatInterval * 3.5f, 0 },
            
            // 박자 4.5: 큰 파일 탄막
            new float[] { patternStartTime + beatInterval * 4.5f, 1 },
            
            // 박자 6: 작은 파일 탄막
            new float[] { patternStartTime + beatInterval * 6, 0 },
            
            // 박자 7.5: 작은 파일 탄막
            new float[] { patternStartTime + beatInterval * 7.5f, 0 },
            
            // 박자 8.5: 큰 파일 탄막
            new float[] { patternStartTime + beatInterval * 8.5f, 1 },
            
            // 박자 10: 작은 파일 탄막
            new float[] { patternStartTime + beatInterval * 10, 0 },
            
            // 박자 11.5: 작은 파일 탄막
            new float[] { patternStartTime + beatInterval * 11.5f, 0 },
            
            // 박자 12.5: 큰 파일 탄막
            new float[] { patternStartTime + beatInterval * 12.5f, 1 },
            
            // 박자 13: 작은 파일 탄막
            new float[] { patternStartTime + beatInterval * 13, 0 },
            
            // 박자 15: 작은 파일 탄막
            new float[] { patternStartTime + beatInterval * 15, 0 },
            
            // ========== 2회차 (박자 18~31) - 16박자 후부터 시작 ==========
            // 박자 18: 작은 파일 탄막
            new float[] { patternStartTime + beatInterval * 18, 0 },
            
            // 박자 19.5: 작은 파일 탄막
            new float[] { patternStartTime + beatInterval * 19.5f, 0 },
            
            // 박자 20.5: 큰 파일 탄막
            new float[] { patternStartTime + beatInterval * 20.5f, 1 },
            
            // 박자 22: 작은 파일 탄막
            new float[] { patternStartTime + beatInterval * 22, 0 },
            
            // 박자 23.5: 작은 파일 탄막
            new float[] { patternStartTime + beatInterval * 23.5f, 0 },
            
            // 박자 24.5: 큰 파일 탄막
            new float[] { patternStartTime + beatInterval * 24.5f, 1 },
            
            // 박자 26: 작은 파일 탄막
            new float[] { patternStartTime + beatInterval * 26, 0 },
            
            // 박자 27.5: 작은 파일 탄막
            new float[] { patternStartTime + beatInterval * 27.5f, 0 },
            
            // 박자 28.5: 큰 파일 탄막
            new float[] { patternStartTime + beatInterval * 28.5f, 1 },
            
            // 박자 29: 작은 파일 탄막
            new float[] { patternStartTime + beatInterval * 29, 0 },
            
            // 박자 31: 작은 파일 탄막
            new float[] { patternStartTime + beatInterval * 31, 0 }
        };
        
        // 폴더 초기 위치 저장
        if (folderObject != null)
        {
            folderTargetPosition = folderObject.transform.localPosition;
            folderOriginalPosition = folderTargetPosition + folderStartOffset;
            folderObject.transform.localPosition = folderOriginalPosition;
            
            Debug.Log($"[FilePatternManager] 폴더 초기화: {folderOriginalPosition} → {folderTargetPosition}");
        }
        else
        {
            Debug.LogWarning("[FilePatternManager] folderObject가 설정되지 않았습니다!");
        }
        
        Debug.Log($"[FilePatternManager] 초기화 완료 - 박자 간격: {beatInterval:F3}초, 비트 수: {beatTimings.Length}");
    }
    
    void Update()
    {
        if (Game3SequenceManager.Instance == null) return;
        
        double musicTime = Game3SequenceManager.Instance.GetMusicTime();
        
        // 음악이 시작 안 했으면 대기
        if (musicTime <= 0) return;
        
        // 폴더 상승 시작 체크 (패턴 시작 2초 전)
        if (!folderRiseStarted && musicTime >= folderRiseStartTime)
        {
            folderRiseStarted = true;
            Debug.Log($"[FilePatternManager] 폴더 상승 시작! 시간: {musicTime:F2}초");
            StartCoroutine(RiseFolderAnimation());
        }
        
        // 패턴 시작 체크
        if (!hasStarted && musicTime >= patternStartTime)
        {
            hasStarted = true;
            currentBeatIndex = 0;
            patternRepeatCount = 0;
            fireCounter = 0;
            processedBeats.Clear();
            
            Debug.Log($"[FilePatternManager] ========== 패턴 시작! 시간: {musicTime:F2}초 ==========");
        }
        
        // 패턴 진행 중
        if (hasStarted && musicTime < patternEndTime)
        {
            // 모든 비트 타이밍 체크 (놓친 비트가 없도록)
            for (int i = 0; i < beatTimings.Length; i++)
            {
                // 이미 처리한 비트는 건너뛰기
                if (processedBeats.Contains(i)) continue;
                
                float beatTime = beatTimings[i][0];
                int beatType = (int)beatTimings[i][1];
                
                // 현재 음악 시간이 비트 시간을 지났는지 체크
                if (musicTime >= beatTime)
                {
                    ProcessBeat(beatType, i);
                    processedBeats.Add(i);
                    
                    Debug.Log($"[FilePatternManager] 비트 {i + 1}/{beatTimings.Length}, 시간: {musicTime:F2}초, 타입: {beatType}");
                }
            }
        }
        
        // 패턴 종료 체크
        if (hasStarted && musicTime >= patternEndTime)
        {
            hasStarted = false;
            CleanupPattern();
            Debug.Log("[FilePatternManager] ========== 패턴 종료 ==========");
        }
    }
    
    /// <summary>
    /// 폴더 상승 애니메이션
    /// </summary>
    IEnumerator RiseFolderAnimation()
    {
        if (folderObject == null) yield break;
        
        folderRising = true;
        Vector3 startPos = folderOriginalPosition;
        Vector3 endPos = folderTargetPosition;
        float elapsed = 0f;
        
        Debug.Log($"[FilePatternManager] 폴더 상승 시작: {startPos} → {endPos}");
        
        while (elapsed < folderRiseDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / folderRiseDuration;
            
            // EaseOutQuad 보간
            t = 1f - (1f - t) * (1f - t);
            
            if (folderObject != null)
            {
                folderObject.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            }
            
            yield return null;
        }
        
        // 최종 위치 확정
        if (folderObject != null)
        {
            folderObject.transform.localPosition = endPos;
        }
        
        folderRising = false;
        Debug.Log("[FilePatternManager] 폴더 상승 완료");
    }
    
    /// <summary>
    /// 비트 처리 (탄막 발사)
    /// </summary>
    void ProcessBeat(int beatType, int beatIndex)
    {
        if (folderObject == null) return;
        
        Vector3 spawnPosition = folderObject.transform.position;
        
        switch (beatType)
        {
            case 0: // 작은 파일 탄막 8개 발사
                Fire8Documents(spawnPosition, false);
                break;
                
            case 1: // 큰 파일 탄막 8개 발사
                Fire8Documents(spawnPosition, true);
                break;
        }
    }
    
    /// <summary>
    /// 8개의 문서 탄막을 고정된 각도로 발사
    /// </summary>
    void Fire8Documents(Vector3 spawnPosition, bool isBig)
    {
        GameObject prefab = isBig ? bigDocumentBulletPrefab : documentBulletPrefab;
        float speed = isBig ? bigDocumentSpeed : documentSpeed;
        
        if (prefab == null)
        {
            Debug.LogError($"[FilePatternManager] {(isBig ? "bigDocumentBulletPrefab" : "documentBulletPrefab")}이 설정되지 않았습니다!");
            return;
        }
        
        if (targetCanvas == null)
        {
            Debug.LogError("[FilePatternManager] targetCanvas가 설정되지 않았습니다!");
            return;
        }
        
        // 8개의 탄막을 고정된 각도로 발사
        for (int i = 0; i < bulletAngles.Length; i++)
        {
            float angle = bulletAngles[i];
            Vector3 direction = Quaternion.Euler(0, 0, angle) * Vector3.up;
            
            // 탄막 생성 (Canvas 안에)
            GameObject bullet = Instantiate(prefab, targetCanvas.transform);
            bullet.transform.position = spawnPosition;
            activeBullets.Add(bullet);
            
            // 탄막 이동 시작
            StartCoroutine(MoveBullet(bullet, direction, speed, bulletLifetime));
        }
        
        Debug.Log($"[FilePatternManager] {(isBig ? "큰" : "작은")} 문서 8개 발사 완료");
    }
    
    /// <summary>
    /// 탄막 이동 코루틴
    /// </summary>
    IEnumerator MoveBullet(GameObject bullet, Vector3 direction, float speed, float lifetime)
    {
        if (bullet == null) yield break;
        
        float elapsed = 0f;
        
        while (elapsed < lifetime && bullet != null)
        {
            elapsed += Time.deltaTime;
            
            // 탄막 이동
            bullet.transform.position += direction * speed * Time.deltaTime;
            
            // 방향에 맞춰 회전
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
            
            yield return null;
        }
        
        // 수명 다하면 삭제
        if (bullet != null)
        {
            activeBullets.Remove(bullet);
            Destroy(bullet);
        }
    }
    
    /// <summary>
    /// 패턴 종료 시 정리
    /// </summary>
    void CleanupPattern()
    {
        // 폴더는 씬에 남겨둠 (삭제하지 않음)
        // 필요하다면 원래 위치로 돌려놓기
        if (folderObject != null)
        {
            folderObject.transform.localPosition = folderOriginalPosition;
            Debug.Log("[FilePatternManager] 폴더 원래 위치로 복귀");
        }
        
        // 모든 활성 탄막 삭제
        foreach (GameObject bullet in activeBullets)
        {
            if (bullet != null)
            {
                Destroy(bullet);
            }
        }
        activeBullets.Clear();
        
        Debug.Log("[FilePatternManager] 패턴 정리 완료");
    }
    
    void OnDestroy()
    {
        // 매니저 삭제 시 정리
        CleanupPattern();
    }
}
