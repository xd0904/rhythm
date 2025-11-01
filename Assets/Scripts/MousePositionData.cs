using UnityEngine;

/// <summary>
/// 씬 전환 시 마우스 위치를 저장하고 유지하는 싱글톤 클래스
/// static 변수로 씬 전환 시에도 데이터 유지
/// </summary>
public class MousePositionData : MonoBehaviour
{
    public static MousePositionData Instance { get; private set; }
    
    // static 변수로 선언하여 인스턴스가 파괴되어도 값 유지
    private static Vector3 _savedMousePosition = Vector3.zero;
    private static bool _hasRedMouse = false;
    
    [Header("저장된 마우스 정보 (읽기 전용)")]
    public Vector3 savedMousePosition = Vector3.zero;
    public bool hasRedMouse = false;
    
    private void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // static 값을 public 변수에 동기화 (Inspector 확인용)
            savedMousePosition = _savedMousePosition;
            hasRedMouse = _hasRedMouse;
            
            Debug.Log("[MousePositionData] 싱글톤 초기화 완료");
            Debug.Log($"[MousePositionData] 복원된 static 값 - 위치: {_savedMousePosition}, 빨간마우스: {_hasRedMouse}");
        }
        else
        {
            Destroy(gameObject);
            Debug.Log("[MousePositionData] 중복 인스턴스 제거");
        }
    }
    
    /// <summary>
    /// 마우스 위치 저장
    /// </summary>
    public void SaveMousePosition(Vector3 position, bool isRed = false)
    {
        // static 변수에 저장 (인스턴스 파괴되어도 유지)
        _savedMousePosition = position;
        _hasRedMouse = isRed;
        
        // public 변수도 동기화 (Inspector 확인용)
        savedMousePosition = position;
        hasRedMouse = isRed;
        
        Debug.Log($"[MousePositionData] 마우스 위치 저장: {position}, 빨간마우스: {isRed}");
        Debug.Log($"[MousePositionData] 저장 후 확인 - static 값: {_savedMousePosition}, {_hasRedMouse}");
    }
    
    /// <summary>
    /// 저장된 마우스 위치 가져오기
    /// </summary>
    public Vector3 GetSavedMousePosition()
    {
        Debug.Log($"[MousePositionData] 위치 가져오기 호출 - static 값: {_savedMousePosition}");
        return _savedMousePosition;
    }
    
    /// <summary>
    /// 빨간 마우스 상태 확인
    /// </summary>
    public bool IsRedMouse()
    {
        Debug.Log($"[MousePositionData] 빨간마우스 상태 확인 - static 값: {_hasRedMouse}");
        return _hasRedMouse;
    }
    
    private void OnDestroy()
    {
        Debug.LogWarning($"[MousePositionData] OnDestroy 호출됨! static 값 (유지됨): {_savedMousePosition}, {_hasRedMouse}");
    }
}
