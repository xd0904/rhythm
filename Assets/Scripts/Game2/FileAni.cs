using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.PackageManager;
#endif

public class FileAni : MonoBehaviour
{
    [Header("창 설정")]
    public GameObject windowPrefab;        // 검은 창 프리팹


    void Start()
    {
        StartCoroutine(Appear());
    }

    IEnumerator Appear()
    {
        yield return new WaitForSeconds(1);

        windowPrefab.SetActive(true);


    }

}
