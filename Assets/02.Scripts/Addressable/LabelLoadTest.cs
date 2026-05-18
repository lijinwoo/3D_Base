using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LabelLoadTest : MonoBehaviour
{
    // 로드할 라벨의 이름
    [SerializeField] private string targetLabel = "Consumable"; 
    
    // 메모리 해제를 위해 핸들을 저장해둘 변수
    private AsyncOperationHandle<IList<GameObject>> loadHandle;

    void Start()
    {
        LoadItemsByLabel();
    }

    private void LoadItemsByLabel()
    {
        // 1. 라벨을 이용해 다수의 에셋 로드 요청
        // 2. 두 번째 파라미터(OnSingleItemLoaded)는 에셋이 '하나씩' 로드될 때마다 실행됨
        loadHandle = Addressables.LoadAssetsAsync<GameObject>(targetLabel, OnSingleItemLoaded);

        // 3. '모든' 에셋의 로드가 완전히 끝났을 때 실행될 완료 이벤트
        loadHandle.Completed += OnAllItemsLoaded;
    }

    // 개별 에셋이 메모리에 올라올 때마다 호출 (예: 3개면 3번 호출)
    private void OnSingleItemLoaded(GameObject loadedObj)
    {
        Debug.Log($"[개별 로드 완료] {loadedObj.name}이(가) 메모리에 로드되었습니다!");
        
        // 로드된 에셋을 씬에 무작위 위치로 생성 (Instantiate)
        Vector3 randomPos = new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
        Instantiate(loadedObj, randomPos, Quaternion.identity);
    }

    // 전체 로드 작업이 끝났을 때 1번 호출
    private void OnAllItemsLoaded(AsyncOperationHandle<IList<GameObject>> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log($"[전체 로드 완료] 총 {handle.Result.Count}개의 에셋 로드 완료!");
        }
        else
        {
            Debug.LogError("에셋 로드에 실패했습니다.");
        }
    }

    private void OnDestroy()
    {
        // 씬이 넘어가거나 오브젝트가 파괴될 때 반드시 메모리 해제!
        if (loadHandle.IsValid())
        {
            Addressables.Release(loadHandle);
            Debug.Log("라벨로 불러온 모든 에셋의 메모리를 해제했습니다.");
        }
    }
}