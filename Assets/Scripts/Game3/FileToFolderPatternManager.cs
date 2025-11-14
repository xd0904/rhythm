using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Game3에서 53초~1:30.7초 동안 파일→폴더 패턴 관리
/// 16박자 패턴을 7번 반복 (총 112박자)
/// </summary>
public class FileToFolderPatternManager : MonoBehaviour
{
    [Header("타이밍 설정")]
    [Tooltip("패턴 시작 박자 (Game3SequenceManager 기준)")]
    public int patternStartBeat = 159; // 53초 = 159박자 (180 BPM 기준)
    
    [Tooltip("패턴 종료 박자")]
    public int patternEndBeat = 272; // 1:30.7초 ≈ 272박자
    
    [Header("프리팹 설정")]
    [Tooltip("일반 파일 프리팹 (4개 소환용)")]
    public GameObject filePrefab;
    
    [Tooltip("백신 프리팹 (1개 소환용)")]
    public GameObject vaccinePrefab;
    
    [Tooltip("집 파일 프리팹 (중앙 변신용)")]
    public GameObject folderPrefab;
    
    [Tooltip("보스 파일 프리팹 - 원래 색상 (5방향 고정 배치용)")]
    public GameObject bossNormalPrefab;
    
    [Tooltip("보스 파일 프리팹 - 빨간색 (깜빡일 때)")]
    public GameObject bossRedPrefab;
    
    [Header("Canvas 설정")]
    [Tooltip("오브젝트가 생성될 Canvas")]
    public Canvas targetCanvas;
    
    [Header("위치 설정")]
    [Tooltip("중앙 위치")]
    public Vector2 centerPosition = Vector2.zero;
    
    [Tooltip("파일 시작 거리 (중앙에서)")]
    public float startDistance = 400f;
    
    [Tooltip("보스 배치 거리 (화면 밖 배경에서)")]
    public float bossDistance = 800f;
    
    [Tooltip("파일 이동 시간 (박자 1에서 시작 → 박자 12에 도착, 총 11박자 이동)")]
    public float gatherDuration = 3.67f; // 11박자 = 3.67초 (180 BPM, 60/180 * 11 = 3.67)
    
    [Header("백신 효과 설정")]
    [Tooltip("백신 프로그램 창")]
    public GameObject vaccineProgram;
    
    [Tooltip("백신 게이지 이미지")]
    public Image redGaugeImage;
    
    [Tooltip("퍼센트 텍스트")]
    public Text gaugeText;
    
    [Tooltip("스캔된 개수 텍스트")]
    public Text gaugeText2;
    
    [Tooltip("진행 단계 텍스트")]
    public Text gaugeText4;
    
    [Tooltip("퍼센트 증가량 (8%)")]
    public float percentIncrement = 0.08f;
    
    [Tooltip("백신 창 표시 시간 (초)")]
    public float vaccineProgramDisplayDuration = 0.8f;
    
    [Header("반복 설정")]
    [Tooltip("16박자 패턴 반복 횟수")]
    public int patternRepeatMax = 7; // 총 7번 반복 (112박자)
    
    // 5방향 각도 (위, 오른쪽 위, 오른쪽 아래, 왼쪽 아래, 왼쪽 위)
    private float[] fileAngles = new float[] { 90f, 45f, -45f, -135f, 135f };
    
    private float currentPercent = 0.05f; // 현재 퍼센트
    private int scannedCount = 1024; // 스캔된 개수
    
    private bool hasStarted = false;
    private bool hasEnded = false;
    private int currentBeatIndex = 0;
    private int patternBeatIndex = 0; // 16박자 패턴 내 인덱스
    private int patternRepeatCount = 0; // 패턴 반복 횟수
    private int lastProcessedBeat = -1;
    private List<GameObject> currentFiles = new List<GameObject>(); // 현재 활성 파일들
    private GameObject currentFolder = null; // 현재 활성 폴더
    private List<GameObject> bossFiles = new List<GameObject>(); // 5방향 고정 보스들
    
    void Update()
    {
        if (Game3SequenceManager.Instance == null) return;
        
        double musicTime = Game3SequenceManager.Instance.GetMusicTime();
        
        // 현재 박자 계산 (180 BPM 기준)
        float beatInterval = 60f / 180f;
        int currentBeat = Mathf.FloorToInt((float)(musicTime / beatInterval));
        
        // 패턴 시작 체크
        if (!hasStarted && !hasEnded && currentBeat >= patternStartBeat)
        {
            hasStarted = true;
            currentBeatIndex = 1;
            patternBeatIndex = 0; // 0에서 시작, Update에서 1로 증가
            patternRepeatCount = 0;
            lastProcessedBeat = currentBeat - 1; // 현재 박자를 처리하기 위해
            
            // 패턴 시작 시 보스 5개 소환 (고정 위치)
            SpawnBossFiles();
            
            Debug.Log($"[FileToFolderPattern] ========== 패턴 시작! 박자: {currentBeat}, 음악시간: {musicTime:F2}초 ==========");
        }
        
        // 패턴 진행 중
        if (hasStarted && !hasEnded)
        {
            // 새로운 박자가 왔을 때만 처리
            if (currentBeat > lastProcessedBeat)
            {
                lastProcessedBeat = currentBeat;
                patternBeatIndex++;
                
                Debug.Log($"[FileToFolderPattern] 전체박자 {currentBeat}, 패턴박자 {patternBeatIndex}, 반복 {patternRepeatCount + 1}/{patternRepeatMax}");
                
                // 박자 1: 파일 5개 소환 및 모으기 시작
                if (patternBeatIndex == 1)
                {
                    SpawnAndGatherFiles();
                }
                // 박자 13: 파일들을 폴더로 변환
                else if (patternBeatIndex == 13)
                {
                    TransformToFolder();
                }
                
                // 16박자 패턴 완료 체크 (박자 16 이후)
                if (patternBeatIndex >= 16)
                {
                    patternBeatIndex = 0; // 0으로 리셋 (다음 박자에서 1로 증가)
                    patternRepeatCount++;
                    
                    Debug.Log($"[FileToFolderPattern] ========== 16박자 패턴 {patternRepeatCount}회 완료! ==========");
                    
                    // 설정된 반복 횟수에 도달하면 패턴 종료
                    if (patternRepeatCount >= patternRepeatMax)
                    {
                        hasStarted = false;
                        hasEnded = true;
                        CleanupPattern();
                        Debug.Log($"[FileToFolderPattern] ========== 패턴 {patternRepeatMax}회 반복 완료! 종료 ==========");
                        return;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 패턴 시작 시 보스 5개를 5방향에 고정 소환
    /// </summary>
    void SpawnBossFiles()
    {
        if (bossNormalPrefab == null)
        {
            Debug.LogError("[FileToFolderPattern] bossNormalPrefab이 없습니다!");
            return;
        }
        
        if (targetCanvas == null)
        {
            Debug.LogError("[FileToFolderPattern] targetCanvas가 없습니다!");
            return;
        }
        
        Debug.Log($"[FileToFolderPattern] 보스 5개 소환 (5방향 고정)");
        
        // 5방향에 보스 소환 (원래 색상으로)
        for (int i = 0; i < fileAngles.Length; i++)
        {
            float angle = fileAngles[i];
            
            // 보스 위치 계산 (중앙에서 bossDistance만큼 떨어진 위치 - 화면 밖)
            float rad = angle * Mathf.Deg2Rad;
            Vector2 bossPos = centerPosition + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * bossDistance;
            
            // 보스 생성 (원래 색상)
            GameObject boss = Instantiate(bossNormalPrefab, targetCanvas.transform);
            RectTransform bossRect = boss.GetComponent<RectTransform>();
            
            if (bossRect != null)
            {
                bossRect.anchoredPosition = bossPos;
                boss.name = $"BossFile_{i}_{angle}deg";
            }
            else
            {
                boss.transform.localPosition = bossPos;
                boss.name = $"BossFile_{i}_{angle}deg";
            }
            
            bossFiles.Add(boss);
            Debug.Log($"[FileToFolderPattern] 보스 {i} 생성: {angle}도 방향, 위치: {bossPos}");
        }
    }
    
    /// <summary>
    /// 파일 4개 + 백신 1개 소환 및 중앙으로 모으기 시작
    /// </summary>
    void SpawnAndGatherFiles()
    {
        if (filePrefab == null)
        {
            Debug.LogError("[FileToFolderPattern] filePrefab이 없습니다!");
            return;
        }
        
        if (vaccinePrefab == null)
        {
            Debug.LogError("[FileToFolderPattern] vaccinePrefab이 없습니다!");
            return;
        }
        
        if (targetCanvas == null)
        {
            Debug.LogError("[FileToFolderPattern] targetCanvas가 없습니다!");
            return;
        }
        
        // 기존 파일/폴더 정리
        CleanupCurrentObjects();
        
        // 보스들 깜빡이기 (파일이 날아갈 때)
        StartCoroutine(FlashBossFiles());
        
        Debug.Log($"[FileToFolderPattern] 파일 4개 + 백신 1개 소환 시작");
        
        // 랜덤으로 하나를 백신으로 선택 (0~4 중 하나)
        int vaccineIndex = Random.Range(0, fileAngles.Length);
        
        // 5개의 오브젝트를 5방향에 소환
        for (int i = 0; i < fileAngles.Length; i++)
        {
            float angle = fileAngles[i];
            
            // 시작 위치 계산 (중앙에서 startDistance만큼 떨어진 위치)
            float rad = angle * Mathf.Deg2Rad;
            Vector2 startPos = centerPosition + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * startDistance;
            
            GameObject spawnedObject;
            bool isVaccine = (i == vaccineIndex);
            
            // 백신 또는 파일 생성
            if (isVaccine)
            {
                spawnedObject = Instantiate(vaccinePrefab, targetCanvas.transform);
                spawnedObject.tag = "Vaccine"; // 백신 태그 설정
                Debug.Log($"[FileToFolderPattern] 백신 {i} 생성: {angle}도 방향, 위치: {startPos}");
            }
            else
            {
                spawnedObject = Instantiate(filePrefab, targetCanvas.transform);
                Debug.Log($"[FileToFolderPattern] 파일 {i} 생성: {angle}도 방향, 위치: {startPos}");
            }
            
            RectTransform objRect = spawnedObject.GetComponent<RectTransform>();
            
            if (objRect != null)
            {
                objRect.anchoredPosition = startPos;
                spawnedObject.name = isVaccine ? $"Vaccine_{patternRepeatCount}_{i}_{angle}deg" : $"File_{patternRepeatCount}_{i}_{angle}deg";
            }
            else
            {
                spawnedObject.transform.localPosition = startPos;
                spawnedObject.name = isVaccine ? $"Vaccine_{patternRepeatCount}_{i}_{angle}deg" : $"File_{patternRepeatCount}_{i}_{angle}deg";
            }
            
            currentFiles.Add(spawnedObject);
            
            // 중앙으로 모으는 애니메이션 시작
            StartCoroutine(GatherFileAnimation(spawnedObject, objRect, startPos, centerPosition, isVaccine));
        }
    }
    
    /// <summary>
    /// 파일/백신을 중앙으로 모으는 애니메이션
    /// </summary>
    IEnumerator GatherFileAnimation(GameObject obj, RectTransform objRect, Vector2 startPos, Vector2 endPos, bool isVaccine = false)
    {
        if (obj == null) yield break;
        
        float elapsed = 0f;
        
        while (elapsed < gatherDuration)
        {
            if (obj == null) yield break;
            
            elapsed += Time.deltaTime;
            float t = elapsed / gatherDuration;
            
            // EaseInOutQuad
            t = t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
            
            Vector2 currentPos = Vector2.Lerp(startPos, endPos, t);
            
            if (objRect != null)
            {
                objRect.anchoredPosition = currentPos;
            }
            else
            {
                obj.transform.localPosition = currentPos;
            }
            
            yield return null;
        }
        
        // 최종 위치 확정
        if (obj != null)
        {
            if (objRect != null)
            {
                objRect.anchoredPosition = endPos;
            }
            else
            {
                obj.transform.localPosition = endPos;
            }
        }
    }
    
    /// <summary>
    /// 보스 파일들을 잠깐 빨간 프리팹으로 교체
    /// </summary>
    IEnumerator FlashBossFiles()
    {
        if (bossRedPrefab == null)
        {
            Debug.LogWarning("[FileToFolderPattern] bossRedPrefab이 없어서 깜빡임 스킵!");
            yield break;
        }
        
        float flashDuration = 0.1f; // 0.1초 깜빡임
        
        // 각 보스의 위치와 정보 저장
        List<Vector2> bossPositions = new List<Vector2>();
        List<string> bossNames = new List<string>();
        
        foreach (GameObject boss in bossFiles)
        {
            if (boss != null)
            {
                RectTransform bossRect = boss.GetComponent<RectTransform>();
                if (bossRect != null)
                {
                    bossPositions.Add(bossRect.anchoredPosition);
                }
                else
                {
                    bossPositions.Add(boss.transform.localPosition);
                }
                bossNames.Add(boss.name);
            }
        }
        
        // 기존 보스들 제거
        foreach (GameObject boss in bossFiles)
        {
            if (boss != null)
            {
                Destroy(boss);
            }
        }
        bossFiles.Clear();
        
        // 빨간 보스로 교체
        for (int i = 0; i < bossPositions.Count; i++)
        {
            GameObject redBoss = Instantiate(bossRedPrefab, targetCanvas.transform);
            RectTransform redRect = redBoss.GetComponent<RectTransform>();
            
            if (redRect != null)
            {
                redRect.anchoredPosition = bossPositions[i];
            }
            else
            {
                redBoss.transform.localPosition = bossPositions[i];
            }
            
            redBoss.name = bossNames[i] + "_Red";
            bossFiles.Add(redBoss);
        }
        
        Debug.Log($"[FileToFolderPattern] 보스 {bossFiles.Count}개 빨간색으로 교체!");
        
        // 잠깐 대기
        yield return new WaitForSeconds(flashDuration);
        
        // 원래 보스로 복구
        bossPositions.Clear();
        bossNames.Clear();
        
        foreach (GameObject boss in bossFiles)
        {
            if (boss != null)
            {
                RectTransform bossRect = boss.GetComponent<RectTransform>();
                if (bossRect != null)
                {
                    bossPositions.Add(bossRect.anchoredPosition);
                }
                else
                {
                    bossPositions.Add(boss.transform.localPosition);
                }
                bossNames.Add(boss.name.Replace("_Red", ""));
            }
        }
        
        // 빨간 보스들 제거
        foreach (GameObject boss in bossFiles)
        {
            if (boss != null)
            {
                Destroy(boss);
            }
        }
        bossFiles.Clear();
        
        // 원래 보스로 복구
        for (int i = 0; i < bossPositions.Count; i++)
        {
            GameObject normalBoss = Instantiate(bossNormalPrefab, targetCanvas.transform);
            RectTransform normalRect = normalBoss.GetComponent<RectTransform>();
            
            if (normalRect != null)
            {
                normalRect.anchoredPosition = bossPositions[i];
            }
            else
            {
                normalBoss.transform.localPosition = bossPositions[i];
            }
            
            normalBoss.name = bossNames[i];
            bossFiles.Add(normalBoss);
        }
        
        Debug.Log($"[FileToFolderPattern] 보스 원래 색상으로 복구!");
    }
    
    /// <summary>
    /// 중앙에 모인 파일들을 폴더로 변환
    /// </summary>
    void TransformToFolder()
    {
        if (folderPrefab == null)
        {
            Debug.LogError("[FileToFolderPattern] folderPrefab이 없습니다!");
            return;
        }
        
        if (targetCanvas == null)
        {
            Debug.LogError("[FileToFolderPattern] targetCanvas가 없습니다!");
            return;
        }
        
        Debug.Log($"[FileToFolderPattern] 파일들을 폴더로 변환");
        
        // 모든 파일 제거
        foreach (GameObject file in currentFiles)
        {
            if (file != null)
            {
                Destroy(file);
            }
        }
        currentFiles.Clear();
        
        // 중앙에 폴더 생성
        currentFolder = Instantiate(folderPrefab, targetCanvas.transform);
        RectTransform folderRect = currentFolder.GetComponent<RectTransform>();
        
        if (folderRect != null)
        {
            folderRect.anchoredPosition = centerPosition;
            currentFolder.name = $"Folder_{patternRepeatCount}";
        }
        else
        {
            currentFolder.transform.localPosition = centerPosition;
            currentFolder.name = $"Folder_{patternRepeatCount}";
        }
        
        Debug.Log($"[FileToFolderPattern] 폴더 생성 완료: {currentFolder.name}");
        
        // 폴더 등장 애니메이션 (선택사항)
        StartCoroutine(FolderAppearAnimation(currentFolder, folderRect));
    }
    
    /// <summary>
    /// 폴더 등장 애니메이션
    /// </summary>
    IEnumerator FolderAppearAnimation(GameObject folder, RectTransform folderRect)
    {
        if (folder == null) yield break;
        
        float appearDuration = 0.2f;
        float elapsed = 0f;
        
        Vector3 originalScale = folderRect != null ? folderRect.localScale : folder.transform.localScale;
        
        // 크기 0에서 시작
        if (folderRect != null)
        {
            folderRect.localScale = Vector3.zero;
        }
        else
        {
            folder.transform.localScale = Vector3.zero;
        }
        
        while (elapsed < appearDuration)
        {
            if (folder == null) yield break;
            
            elapsed += Time.deltaTime;
            float t = elapsed / appearDuration;
            
            // EaseOutBack (약간 튀는 효과)
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            t = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
            
            Vector3 scale = Vector3.Lerp(Vector3.zero, originalScale, t);
            
            if (folderRect != null)
            {
                folderRect.localScale = scale;
            }
            else
            {
                folder.transform.localScale = scale;
            }
            
            yield return null;
        }
        
        // 최종 크기 확정
        if (folder != null)
        {
            if (folderRect != null)
            {
                folderRect.localScale = originalScale;
            }
            else
            {
                folder.transform.localScale = originalScale;
            }
        }
    }
    
    /// <summary>
    /// 현재 오브젝트들 정리
    /// </summary>
    void CleanupCurrentObjects()
    {
        // 파일들 제거
        foreach (GameObject file in currentFiles)
        {
            if (file != null)
            {
                Destroy(file);
            }
        }
        currentFiles.Clear();
        
        // 폴더 제거
        if (currentFolder != null)
        {
            Destroy(currentFolder);
            currentFolder = null;
        }
    }
    
    /// <summary>
    /// 패턴 종료 시 정리
    /// </summary>
    void CleanupPattern()
    {
        CleanupCurrentObjects();
        
        // 보스들 제거
        foreach (GameObject boss in bossFiles)
        {
            if (boss != null)
            {
                Destroy(boss);
            }
        }
        bossFiles.Clear();
        
        Debug.Log("[FileToFolderPattern] 패턴 정리 완료 (보스 포함)");
    }
    
    /// <summary>
    /// 백신 먹었을 때 효과 (BossRectanglePattern과 동일)
    /// </summary>
    public IEnumerator ShowVaccineAlarmAndIncreasePercent()
    {
        Debug.Log("[FileToFolderPattern] 백신 효과 시작!");
        
        // 백신 프로그램 창 표시
        if (vaccineProgram != null)
        {
            bool wasActive = vaccineProgram.activeSelf;
            vaccineProgram.SetActive(true);
            Debug.Log($"[FileToFolderPattern] 백신 프로그램 표시 (이전 상태: {wasActive})");
        }
        
        // 퍼센트 증가 애니메이션
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
                
                // 스캔된 개수 증가
                if (gaugeText2 != null)
                {
                    scannedCount += Random.Range(50, 150);
                    gaugeText2.text = scannedCount.ToString();
                }
                
                // 진행 단계 업데이트 (8%마다 1단계)
                if (gaugeText4 != null)
                {
                    int stage = Mathf.FloorToInt(currentPercent * 100 / 8);
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
            
            Debug.Log($"[FileToFolderPattern] 백신 퍼센트 증가: {Mathf.RoundToInt(currentPercent * 100)}% (스캔: {scannedCount})");
        }
        
        // 설정된 시간만큼 표시
        yield return new WaitForSeconds(vaccineProgramDisplayDuration);
        
        if (vaccineProgram != null)
        {
            vaccineProgram.SetActive(false);
            Debug.Log("[FileToFolderPattern] 백신 프로그램 숨기기");
        }
    }
    
    void OnDestroy()
    {
        CleanupPattern();
    }
}
