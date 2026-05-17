using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;//Addressable 기본 API
using UnityEngine.ResourceManagement.AsyncOperations; 
//핸들러 ->AsyncOperationHandle

public class AddressLoader : MonoBehaviour
{
    //문자열로 주소값을 
    [SerializeField] private string address;

    // 비동기 작동 손잡이
    private AsyncOperationHandle<GameObject> handle;

    // IEnumerator -> 코루틴으로 -> Start 
    // void -> 반환이 없는 -> 단편적인 호출 함수 
    // 코루틴은 순차적으로 ' yield return' 기준으로 호출
    // IEnumerator Start
    // Unity 잡기술
    
    private IEnumerator Start()
    {
        // 핸든 -> C# 문버 -> unity 툴 
        // 비동기로 메모리에 올렸다 ( Load ) 
        handle = Addressables.LoadAssetAsync<GameObject>(address);
        yield return handle;

      
        // 로딩되고 있는 데이터의 상태 --> 
        // 로딩이 성공했는가 ? 로드 완료
       if (handle.Status == AsyncOperationStatus.Succeeded)
       {
           Instantiate(handle.Result, transform.position, Quaternion.identity);
       }
    }


    private void OnDestroy()
    {
        if (handle.IsValid())
        {
           Addressables.Release(handle); 
           
          
        }
    }
}