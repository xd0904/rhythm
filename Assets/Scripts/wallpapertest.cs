using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

public class wallpapertest : MonoBehaviour
{
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int SystemParametersInfo(int uAction, int uParam, StringBuilder lpvParam, int fuWinIni);

    private const int SPI_GETDESKWALLPAPER = 0x0073;
    private const int MAX_PATH = 260;

    void Start()
    {
        LoadWindowsWallpaper();
    }

    /// <summary>
    /// 윈도우 배경화면을 자동으로 불러와서 이 오브젝트의 컴포넌트에 적용
    /// </summary>
    public void LoadWindowsWallpaper()
    {
        // 윈도우 API를 통해 현재 배경화면 경로 가져오기
        string wallpaperPath = GetWindowsWallpaperPath();

        if (string.IsNullOrEmpty(wallpaperPath) || !File.Exists(wallpaperPath))
        {
            Debug.LogError($"윈도우 배경화면을 찾을 수 없습니다: {wallpaperPath}");
            return;
        }

        Debug.Log($"윈도우 배경화면 경로: {wallpaperPath}");

        // 이미지 파일 읽기
        byte[] imageData = File.ReadAllBytes(wallpaperPath);
        
        // Texture2D 생성
        Texture2D texture = new Texture2D(2, 2);
        if (!texture.LoadImage(imageData))
        {
            Debug.LogError("배경화면 이미지 로드 실패!");
            return;
        }

        // 원본 이미지 정보 디버그
        float originalAspect = (float)texture.width / texture.height;
        Debug.Log($"[원본 이미지] 크기: {texture.width}x{texture.height}, 비율: {originalAspect:F3} ({texture.width}:{texture.height})");

        // 이 GameObject에 있는 컴포넌트 자동 탐지 및 적용
        ApplyToComponent(texture);

        Debug.Log("윈도우 배경화면 적용 완료!");
    }

    /// <summary>
    /// 윈도우 API를 통해 현재 배경화면 경로 가져오기
    /// </summary>
    private string GetWindowsWallpaperPath()
    {
        try
        {
            StringBuilder wallpaperPath = new StringBuilder(MAX_PATH);
            SystemParametersInfo(SPI_GETDESKWALLPAPER, MAX_PATH, wallpaperPath, 0);
            return wallpaperPath.ToString();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"배경화면 경로 가져오기 실패: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// GameObject에 있는 컴포넌트를 자동으로 찾아서 이미지 적용
    /// </summary>
    private void ApplyToComponent(Texture2D texture)
    {
        // 1. RawImage 컴포넌트 찾기 (추천)
        RawImage rawImage = GetComponent<RawImage>();
        if (rawImage != null)
        {
            rawImage.texture = texture;
            AdjustRectTransformTo16by9(rawImage.rectTransform);
            rawImage.uvRect = new Rect(0, 0, 1, 1);
            Debug.Log("RawImage 컴포넌트에 배경화면 적용 (16:9 비율)");
            return;
        }

        // 2. Image 컴포넌트 찾기
        Image image = GetComponent<Image>();
        if (image != null)
        {
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
            image.sprite = sprite;
            image.preserveAspect = false;
            AdjustRectTransformTo16by9(image.rectTransform);
            Debug.Log("Image 컴포넌트에 배경화면 적용 (16:9 비율)");
            return;
        }

        // 3. SpriteRenderer 컴포넌트 찾기
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100.0f
            );
            spriteRenderer.sprite = sprite;
            AdjustSpriteRendererTo16by9(spriteRenderer, texture);
            Debug.Log("SpriteRenderer 컴포넌트에 배경화면 적용 (16:9 비율)");
            return;
        }

        Debug.LogWarning("이미지를 적용할 수 있는 컴포넌트(RawImage, Image, SpriteRenderer)를 찾을 수 없습니다!");
    }

    /// <summary>
    /// RectTransform을 16:9 비율로 조정
    /// </summary>
    private void AdjustRectTransformTo16by9(RectTransform rectTransform)
    {
        if (rectTransform == null) return;

        // 현재 사각형의 크기
        Vector2 currentSize = rectTransform.rect.size;
        float currentWidth = currentSize.x;
        float currentHeight = currentSize.y;
        float currentAspect = currentWidth / currentHeight;

        Debug.Log($"[현재 사각형] 크기: {currentWidth}x{currentHeight}, 비율: {currentAspect:F3}");

        // 16:9 비율
        float targetAspect = 16f / 9f;
        Debug.Log($"[목표 비율] 16:9 = {targetAspect:F3}");

        // 새로운 크기 계산
        float newWidth, newHeight;

        if (currentAspect > targetAspect)
        {
            // 현재가 16:9보다 더 넓음 -> 높이를 기준으로 너비 조정
            newHeight = currentHeight;
            newWidth = newHeight * targetAspect;
            Debug.Log($"[계산] 현재가 더 넓음 -> 높이 유지({newHeight}), 너비 조정({newWidth})");
        }
        else
        {
            // 현재가 16:9보다 더 좁음 -> 너비를 기준으로 높이 조정
            newWidth = currentWidth;
            newHeight = newWidth / targetAspect;
            Debug.Log($"[계산] 현재가 더 좁음 -> 너비 유지({newWidth}), 높이 조정({newHeight})");
        }

        // RectTransform 크기 변경
        rectTransform.sizeDelta = new Vector2(newWidth, newHeight);

        float newAspect = newWidth / newHeight;
        Debug.Log($"[최종 사각형] 크기: {newWidth}x{newHeight}, 비율: {newAspect:F3}");
        Debug.Log($"[SizeDelta 설정] {rectTransform.sizeDelta}");
    }

    /// <summary>
    /// SpriteRenderer를 16:9 비율로 조정
    /// </summary>
    private void AdjustSpriteRendererTo16by9(SpriteRenderer spriteRenderer, Texture2D texture)
    {
        if (spriteRenderer == null) return;

        // 원본 이미지 비율
        float originalAspect = (float)texture.width / texture.height;
        Debug.Log($"[원본 비율] {originalAspect:F3}");

        // 목표 비율 16:9
        float targetAspect = 16f / 9f;
        Debug.Log($"[목표 비율] 16:9 = {targetAspect:F3}");

        // 16:9 비율로 스케일 조정
        // 높이를 1로 고정하고 너비를 16:9에 맞춤
        float scaleX = targetAspect / originalAspect;
        float scaleY = 1f;

        // 원하는 크기 (Unity 단위) - 기본적으로 화면에 맞게 설정
        float desiredHeight = 10f; // 기본 높이
        Camera mainCamera = Camera.main;
        if (mainCamera != null && mainCamera.orthographic)
        {
            desiredHeight = mainCamera.orthographicSize * 2f;
        }

        float desiredWidth = desiredHeight * targetAspect;

        // 텍스처 픽셀을 Unity 단위로 변환 (PixelsPerUnit = 100)
        float pixelsPerUnit = 100f;
        float baseScale = (desiredHeight * pixelsPerUnit) / texture.height;

        // 최종 스케일 적용
        Vector3 finalScale = new Vector3(baseScale * scaleX, baseScale * scaleY, 1f);
        transform.localScale = finalScale;

        Debug.Log($"[비율 조정] scaleX: {scaleX:F3}, scaleY: {scaleY:F3}");
        Debug.Log($"[최종 스케일] {finalScale}");
        Debug.Log($"[목표 크기] {desiredWidth:F2} x {desiredHeight:F2} Unity 단위");
    }
}
