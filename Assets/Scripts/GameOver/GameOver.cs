using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOver : MonoBehaviour
{
    [Header("공포 효과 설정")]
    [Tooltip("흔들릴 이미지 (SpriteRenderer 또는 RectTransform)")]
    public Transform targetImage;
    
    [Tooltip("초기 확대 시간")]
    public float zoomDuration = 1.5f;
    
    [Tooltip("목표 크기 (배율)")]
    public float targetScale = 1.5f;
    
    [Tooltip("점프스케어 흔들림 시간")]
    public float shakeDuration = 2f;
    
    [Tooltip("흔들림 강도 (작을수록 화면 안에 머묾)")]
    public float shakeIntensity = 15f;
    
    [Tooltip("회전 강도")]
    public float rotationIntensity = 10f;
    
    [Tooltip("점프스케어 사운드")]
    public AudioClip scareSound;
    
    [Header("씬 전환 설정")]
    [Tooltip("GameOver 후 Intro로 돌아가는 대기 시간")]
    public float returnDelay = 1.5f;
    
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    
    void Start()
    {
        // targetImage가 없으면 자기 자신 사용
        if (targetImage == null)
        {
            targetImage = transform;
        }
        
        // 원본 저장
        originalPosition = targetImage.localPosition;
        originalScale = targetImage.localScale;
        originalRotation = targetImage.localRotation;
        
        // 점프스케어 시작
        StartCoroutine(JumpScareSequence());
    }
    
    IEnumerator JumpScareSequence()
    {
        // 1단계: 서서히 확대 (공포감 조성)
        float elapsed = 0f;
        
        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomDuration;
            
            // Ease-in으로 점점 빠르게 확대
            float smoothT = t * t;
            
            Vector3 currentScale = Vector3.Lerp(originalScale, originalScale * targetScale, smoothT);
            targetImage.localScale = currentScale;
            
            yield return null;
        }
        
        // 최종 크기 설정
        targetImage.localScale = originalScale * targetScale;
        
        // 2단계: 갑작스러운 점프스케어 (사운드와 함께)
        if (scareSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(scareSound);
        }
        
        // 3단계: 격렬한 흔들림 (화면 안에서)
        elapsed = 0f;
        Vector3 scaredPosition = targetImage.localPosition;
        Vector3 scaredScale = targetImage.localScale;
        
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            
            // 시간이 지날수록 흔들림 감소
            float intensity = 1f - (elapsed / shakeDuration) * 0.3f;
            
            // 화면 안에서만 흔들림 (작은 범위)
            float shakeX = Random.Range(-shakeIntensity, shakeIntensity) * intensity;
            float shakeY = Random.Range(-shakeIntensity, shakeIntensity) * intensity;
            targetImage.localPosition = scaredPosition + new Vector3(shakeX, shakeY, 0);
            
            // 좌우 회전 (Z축만)
            float rotZ = Random.Range(-rotationIntensity, rotationIntensity) * intensity;
            targetImage.localRotation = originalRotation * Quaternion.Euler(0, 0, rotZ);
            
            // 크기 미세 변동 (펄싱)
            float scaleVariation = 1f + Random.Range(-0.05f, 0.05f) * intensity;
            targetImage.localScale = scaredScale * scaleVariation;
            
            yield return null;
        }
        
        // 마지막: 고정 (흔들림 멈춤)
        targetImage.localPosition = scaredPosition;
        targetImage.localScale = scaredScale;
        targetImage.localRotation = originalRotation;
        
        // 일정 시간 후 Game1 씬으로 돌아가기 (44초부터 시작)
        yield return new WaitForSeconds(returnDelay);
        
        // Game1의 44초(마우스 커지는 부분)부터 시작하도록 플래그 설정
        PlayerPrefs.SetFloat("StartTime", 44f);
        PlayerPrefs.Save();
        
        SceneManager.LoadScene("Game1");
    }
}
