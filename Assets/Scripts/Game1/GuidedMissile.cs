using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuidedMissile : MonoBehaviour
{

    // Launcher 스크립트에서 설정해줄 변수들
    [HideInInspector] public float missileSpeed;
    [HideInInspector] public bool IsReadyToFire = false;

    [Header("소멸 설정")]
    public float lifeTime = 3f; // 발사 후 3초 뒤에 사라짐

    private Vector3 initialDirection;

    void Awake()
    {
        // Awake에서 초기 방향을 저장. (Instantiate될 때의 방향)
        // 이 방향으로 계속 직진하게 됩니다.
        initialDirection = transform.up;

        // Start()가 아닌 Awake()에서 설정하므로, Instanstiate 직후 바로 설정 가능
        // 하지만 발사는 IsReadyToFire = true 가 된 후에 이루어집니다.
    }

    void Update()
    {
        // IsReadyToFire가 true일 때만 이동
        if (IsReadyToFire)
        {
            // 초기 방향으로 직진 이동
            transform.position += initialDirection * missileSpeed * Time.deltaTime;
        }
    }

    // 발사 명령이 내려지면(IsReadyToFire = true) 이 코루틴을 시작합니다.
    public void LaunchMissile()
    {
        // 미사일 발사 허용
        IsReadyToFire = true;
        // 수명 코루틴 시작
        StartCoroutine(DestroyAfterTime(lifeTime));
    }

    // 수명 관리를 위한 코루틴
    IEnumerator DestroyAfterTime(float delay)
    {
        // 설정된 시간만큼 대기
        yield return new WaitForSeconds(delay);

        // 오브젝트 소멸
        Destroy(gameObject);
    }


}
