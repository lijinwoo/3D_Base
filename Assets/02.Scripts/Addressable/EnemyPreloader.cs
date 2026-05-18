using System.Collections; // IEnumerator(코루틴)를 사용하기 위해 필수 추가
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemyPreloader : MonoBehaviour
{
    // 전체 로드 작업의 상태를 추적하고, 나중에 메모리를 해제(Release)하기 위해 보관하는 핸들
    private AsyncOperationHandle<IList<GameObject>> handle;

    // 로드된 적(Enemy) 프리팹 원본들을 보관할 리스트 
    // (보통 이렇게 캐싱해두고, 적 스폰 매니저나 오브젝트 풀링에서 꺼내 씁니다)
    private readonly List<GameObject> enemyPrefabs = new();

    // 비동기 대기(yield return)를 사용하기 위해 Start 함수를 코루틴 형태로 선언합니다.
    private IEnumerator Start()
    {
        // 1. "enemy" 라벨이 붙은 모든 프리팹(GameObject)의 로드를 요청합니다.
        // 2. 람다식(prefab => enemyPrefabs.Add(prefab))은 콜백 함수로,
        //    에셋이 '하나씩' 메모리에 로드될 때마다 실행되어 리스트에 차곡차곡 담습니다.
        handle = Addressables.LoadAssetsAsync<GameObject>(
            "enemy", prefab => enemyPrefabs.Add(prefab));

        // 3. 전체 에셋(예: 몬스터 10종)의 로드가 완전히 끝날 때까지 여기서 실행을 일시 정지(대기)합니다.
        // 이 동안 게임은 멈추지 않고(Freezing 방지) 백그라운드에서 로딩이 진행됩니다.
        yield return handle;

        // 4. 로딩이 100% 완료된 직후 실행됩니다. 
        // 총 몇 개의 프리팹이 성공적으로 로드되어 리스트에 담겼는지 확인합니다.
        Debug.Log($"Loaded enemy prefabs: {enemyPrefabs.Count}");
    }

    private void OnDestroy()
    {
        // 씬이 변경되거나 해당 스크립트가 파괴될 때, 메모리 누수(Leak)를 방지하는 필수 단계!
        // 핸들이 유효한 상태인지(정상적으로 로드 작업이 생성되었는지) 확인합니다.
        if (handle.IsValid())
        {
            // 메모리에서 로드했던 프리팹들을 전부 내려줍니다(Reference Count 감소).
            Addressables.Release(handle);
        }
    }
}