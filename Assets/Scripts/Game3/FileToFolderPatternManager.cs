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
    
    [Tooltip("창 밖 보스 파일 프리팹 (5방향 고정 배치용)")]
    public GameObject bossFilePrefab;
    
    [Tooltip("진한 색 보스 프리팹 (폴더 변환 직전용)")]
    public GameObject darkBossFilePrefab;
    
    [Header("Canvas 설정")]
    [Tooltip("오브젝트가 생성될 Canvas")]
    public Canvas targetCanvas;
    
    [Header("위치 설정")]
    [Tooltip("중앙 위치")]
    public Vector2 centerPosition = Vector2.zero;
    
    [Tooltip("파일 시작 거리 (중앙에서)")]
    public float startDistance = 400f;
    
    [Tooltip("보스 파일 거리 (창 밖 고정 위치, 중앙에서)")]
    public float bossFileDistance = 600f;
    
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
    [Tooltip("32박자 패턴 반복 횟수")]
    public int patternRepeatMax = 7; // 총 7번 반복 (224박자)
    
    // 5방향 각도 (위, 오른쪽 위, 오른쪽 아래, 왼쪽 아래, 왼쪽 위)
    private float[] fileAngles = new float[] { 90f, 45f, -45f, -135f, 135f };
    
    private float currentPercent = 0.05f; // 현재 퍼센트
    private int scannedCount = 1024; // 스캔된 개수
    
    private bool hasStarted = false;
    private bool hasEnded = false;
    private int currentBeatIndex = 0;
    private int patternBeatIndex = 0; // 32박자 패턴 내 인덱스
    private int patternRepeatCount = 0; // 패턴 반복 횟수
    private int lastProcessedBeat = -1;
    private List<GameObject> currentFiles = new List<GameObject>(); // 현재 활성 파일들
    private GameObject currentFolder = null; // 현재 활성 폴더
    private List<GameObject> bossFiles = new List<GameObject>(); // 창 밖 보스 파일들 (5개 고정)
    
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
                
                // 박자 1: 파일 5개 소환 및 모으기 시작 + 창 밖 보스 파일 5개 소환 + 보스 깜빡임
                if (patternBeatIndex == 1)
                {
                    SpawnAndGatherFiles();
                    SpawnBossFiles(); // 창 밖 보스 파일 5개 소환
                    FlashBossFilesToDark(); // 보스 파일 깜빡임 (진해지기)
                }
                // 박자 13: 파일들을 폴더로 변환
                else if (patternBeatIndex == 13)
                {
                    TransformToFolder();
                }
                // 박자 17: 폴더에서 파일들이 다시 퍼지기
                else if (patternBeatIndex == 17)
                {
                    StartCoroutine(ScatterFilesFromFolder());
                }
                
                // 32박자 패턴 완료 체크 (박자 32 이후)
                if (patternBeatIndex >= 32)
                {
                    patternBeatIndex = 0; // 0으로 리셋 (다음 박자에서 1로 증가)
                    patternRepeatCount++;
                    
                    Debug.Log($"[FileToFolderPattern] ========== 32박자 패턴 {patternRepeatCount}회 완료! ==========");
                    
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
    /// 창 밖에 보스 파일 5개 소환 (5방향 고정 위치)
    /// </summary>
    void SpawnBossFiles()
    {
        if (bossFilePrefab == null)
        {
            Debug.LogError("[FileToFolderPattern] bossFilePrefab이 없습니다!");
            return;
        }
        
        if (targetCanvas == null)
        {
            Debug.LogError("[FileToFolderPattern] targetCanvas가 없습니다!");
            return;
        }
        
        // 기존 보스 파일들 정리
        foreach (GameObject bossFile in bossFiles)
        {
            if (bossFile != null)
            {
                Destroy(bossFile);
            }
        }
        bossFiles.Clear();
        
        Debug.Log($"[FileToFolderPattern] 창 밖 보스 파일 5개 소환 시작");
        
        // 5방향에 보스 파일 소환
        for (int i = 0; i < fileAngles.Length; i++)
        {
            float angle = fileAngles[i];
            
            // 창 밖 위치 계산 (중앙에서 bossFileDistance만큼 떨어진 위치)
            float rad = angle * Mathf.Deg2Rad;
            Vector2 bossPos = centerPosition + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * bossFileDistance;
            
            GameObject bossFile = Instantiate(bossFilePrefab, targetCanvas.transform);
            RectTransform bossRect = bossFile.GetComponent<RectTransform>();
            
            if (bossRect != null)
            {
                bossRect.anchoredPosition = bossPos;
                bossFile.name = $"BossFile_{patternRepeatCount}_{i}_{angle}deg";
            }
            else
            {
                bossFile.transform.localPosition = bossPos;
                bossFile.name = $"BossFile_{patternRepeatCount}_{i}_{angle}deg";
            }
            
            bossFiles.Add(bossFile);
            Debug.Log($"[FileToFolderPattern] 보스 파일 {i} 생성: {angle}도 방향, 위치: {bossPos}");
        }
    }
    
    /// <summary>
    /// 보스 파일들을 진한 색으로 깜빡임 (박자 1에 발사 타이밍)
    /// </summary>
    void FlashBossFilesToDark()
    {
        if (bossFiles.Count == 0) return;
        
        StartCoroutine(FlashBossFilesCoroutine());
    }
    
    /// <summary>
    /// 보스 파일 깜빡임 코루틴 (진한 색으로 0.1초간 변환)
    /// </summary>
    IEnumerator FlashBossFilesCoroutine()
    {
        if (darkBossFilePrefab == null || targetCanvas == null)
        {
            yield break;
        }
        
        Debug.Log($"[FileToFolderPattern] 보스 파일들 깜빡임 시작");
        
        List<Vector2> positions = new List<Vector2>();
        
        // 기존 보스 파일들 위치 저장하고 진한 색으로 교체
        for (int i = 0; i < bossFiles.Count; i++)
        {
            GameObject oldBoss = bossFiles[i];
            if (oldBoss == null) continue;
            
            // 위치 저장
            RectTransform oldRect = oldBoss.GetComponent<RectTransform>();
            Vector2 position = oldRect != null ? oldRect.anchoredPosition : (Vector2)oldBoss.transform.localPosition;
            positions.Add(position);
            
            // 임시로 비활성화
            oldBoss.SetActive(false);
            
            // 진한 색 보스 생성
            GameObject darkBoss = Instantiate(darkBossFilePrefab, targetCanvas.transform);
            RectTransform darkRect = darkBoss.GetComponent<RectTransform>();
            
            if (darkRect != null)
            {
                darkRect.anchoredPosition = position;
            }
            else
            {
                darkBoss.transform.localPosition = position;
            }
            
            darkBoss.name = $"TempDarkBoss_{i}";
        }
        
        // 0.1초 대기
        yield return new WaitForSeconds(0.1f);
        
        // 진한 색 보스들 삭제하고 원래 보스 다시 활성화
        GameObject[] tempDarkBosses = GameObject.FindGameObjectsWithTag("Untagged");
        foreach (GameObject obj in tempDarkBosses)
        {
            if (obj.name.StartsWith("TempDarkBoss_"))
            {
                Destroy(obj);
            }
        }
        
        // 원래 보스 다시 활성화
        foreach (GameObject boss in bossFiles)
        {
            if (boss != null)
            {
                boss.SetActive(true);
            }
        }
        
        Debug.Log($"[FileToFolderPattern] 보스 파일들 깜빡임 완료");
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
    /// 폴더에서 파일들이 다시 퍼지는 애니메이션 (박자 16에서 다음 박자 1로 넘어갈 때)
    /// </summary>
    IEnumerator ScatterFilesFromFolder()
    {
        if (currentFolder == null) yield break;
        if (filePrefab == null || vaccinePrefab == null) yield break;
        if (targetCanvas == null) yield break;
        
        Debug.Log($"[FileToFolderPattern] 폴더에서 파일들이 퍼지기 시작");
        
        // 폴더를 서서히 사라지게 하면서 파일 5개 생성
        float scatterDuration = gatherDuration; // 모이는 시간과 동일하게 퍼지기
        
        // 랜덤으로 하나를 백신으로 선택
        int vaccineIndex = Random.Range(0, fileAngles.Length);
        
        List<GameObject> scatteringFiles = new List<GameObject>();
        
        // 5개의 파일을 중앙에서 생성 (초기 크기 작게)
        for (int i = 0; i < fileAngles.Length; i++)
        {
            float angle = fileAngles[i];
            bool isVaccine = (i == vaccineIndex);
            
            GameObject file = Instantiate(isVaccine ? vaccinePrefab : filePrefab, targetCanvas.transform);
            if (isVaccine)
            {
                file.tag = "Vaccine";
            }
            
            RectTransform fileRect = file.GetComponent<RectTransform>();
            
            if (fileRect != null)
            {
                fileRect.anchoredPosition = centerPosition;
                fileRect.localScale = Vector3.zero; // 크기 0에서 시작
            }
            else
            {
                file.transform.localPosition = centerPosition;
                file.transform.localScale = Vector3.zero;
            }
            
            file.name = $"ScatterFile_{i}_{angle}deg";
            scatteringFiles.Add(file);
            
            // 목표 위치 계산
            float rad = angle * Mathf.Deg2Rad;
            Vector2 targetPos = centerPosition + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * startDistance;
            
            // 퍼지는 애니메이션 시작
            StartCoroutine(ScatterFileAnimation(file, fileRect, centerPosition, targetPos, scatterDuration));
        }
        
        // 폴더 사라지는 애니메이션
        RectTransform folderRect = currentFolder.GetComponent<RectTransform>();
        Vector3 folderOriginalScale = folderRect != null ? folderRect.localScale : currentFolder.transform.localScale;
        float elapsed = 0f;
        float folderDisappearDuration = 0.3f;
        
        while (elapsed < folderDisappearDuration)
        {
            if (currentFolder == null) break;
            
            elapsed += Time.deltaTime;
            float t = elapsed / folderDisappearDuration;
            
            Vector3 scale = Vector3.Lerp(folderOriginalScale, Vector3.zero, t);
            
            if (folderRect != null)
            {
                folderRect.localScale = scale;
            }
            else
            {
                currentFolder.transform.localScale = scale;
            }
            
            yield return null;
        }
        
        // 폴더 제거
        if (currentFolder != null)
        {
            Destroy(currentFolder);
            currentFolder = null;
        }
        
        Debug.Log($"[FileToFolderPattern] 폴더에서 파일들이 퍼지기 완료");
    }
    
    /// <summary>
    /// 파일 퍼지기 애니메이션 (중앙에서 바깥으로)
    /// </summary>
    IEnumerator ScatterFileAnimation(GameObject file, RectTransform fileRect, Vector2 startPos, Vector2 endPos, float duration)
    {
        if (file == null) yield break;
        
        // 프리팹의 원래 크기 저장 (생성 직후 크기가 0이므로, 프리팹 스케일을 가져와야 함)
        Vector3 targetScale;
        if (fileRect != null)
        {
            // RectTransform의 경우 프리팹의 원래 localScale 사용
            targetScale = filePrefab.GetComponent<RectTransform>() != null 
                ? filePrefab.GetComponent<RectTransform>().localScale 
                : Vector3.one;
        }
        else
        {
            targetScale = filePrefab.transform.localScale;
        }
        
        // 1단계: 먼저 크기만 빠르게 커지기 (0.2초)
        float scaleUpDuration = 0.2f;
        float elapsed = 0f;
        
        while (elapsed < scaleUpDuration)
        {
            if (file == null) yield break;
            
            elapsed += Time.deltaTime;
            float t = elapsed / scaleUpDuration;
            
            // EaseOutBack (튀는 효과)
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            t = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
            
            Vector3 currentScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            
            if (fileRect != null)
            {
                fileRect.localScale = currentScale;
            }
            else
            {
                file.transform.localScale = currentScale;
            }
            
            yield return null;
        }
        
        // 크기 최종 확정
        if (file != null)
        {
            if (fileRect != null)
            {
                fileRect.localScale = targetScale;
            }
            else
            {
                file.transform.localScale = targetScale;
            }
        }
        
        // 2단계: 크기는 유지하고 위치만 퍼지기
        elapsed = 0f;
        
        while (elapsed < duration)
        {
            if (file == null) yield break;
            
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // EaseOutQuad
            t = 1f - (1f - t) * (1f - t);
            
            Vector2 currentPos = Vector2.Lerp(startPos, endPos, t);
            
            if (fileRect != null)
            {
                fileRect.anchoredPosition = currentPos;
            }
            else
            {
                file.transform.localPosition = currentPos;
            }
            
            yield return null;
        }
        
        // 최종 위치 확정
        if (file != null)
        {
            if (fileRect != null)
            {
                fileRect.anchoredPosition = endPos;
            }
            else
            {
                file.transform.localPosition = endPos;
            }
            
            // 애니메이션 완료 후 파일 제거 (다음 반복에서 새로 생성될 것이므로)
            Destroy(file);
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
        
        // 보스 파일들 제거
        foreach (GameObject bossFile in bossFiles)
        {
            if (bossFile != null)
            {
                Destroy(bossFile);
            }
        }
        bossFiles.Clear();
    }
    
    /// <summary>
    /// 패턴 종료 시 정리
    /// </summary>
    void CleanupPattern()
    {
        CleanupCurrentObjects();
        Debug.Log("[FileToFolderPattern] 패턴 정리 완료");
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
