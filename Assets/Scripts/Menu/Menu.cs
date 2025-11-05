using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
  
    public Text textComponent;

    [Header("Animation Settings")]
    public float scaleUpFactor = 1.1f;   // 커질 정도
    public float speed = 8f;             // 애니메이션 속도
    public Color hoverColor = Color.gray;

    private Vector3 originalScale;
    private Color originalColor;
    private bool isHovered = false;

    void Start()
    {
        originalScale = transform.localScale;
        originalColor = textComponent.color;
    }

    void Update()
    {
        // 현재 상태에 따라 크기와 색을 부드럽게 보간
        Vector3 targetScale = isHovered ? originalScale * scaleUpFactor : originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * speed);

        Color targetColor = isHovered ? hoverColor : originalColor;
        textComponent.color = Color.Lerp(textComponent.color, targetColor, Time.deltaTime * speed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }


    public void OnPlayButton()
    {
        SceneManager.LoadScene("Intro");  
    }

    public void OnQuitButton()
    {
       // Application.Quit(); 
    }
}


