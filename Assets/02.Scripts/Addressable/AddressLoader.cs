using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Addressables를 활용한 비동기 에셋 로드 및 메모리 생명주기(Lifecycle) 관리 구조를 보여주는 클래스입니다.
/// </summary>
public class AddressLoader : MonoBehaviour
{
    // 로드할 에셋의 Addressables 주소(Address)입니다.
    [SerializeField] private string address = "items/health_kit";

    // 비동기 작업 상태를 추적하고, 이후 메모리 해제를 위해 유지해야 하는 Handle 객체입니다.
    private AsyncOperationHandle<GameObject> handle;
    
    // 씬(Scene)에 실제 생성된 GameObject 인스턴스를 추적하기 위한 참조입니다.
    private GameObject instance;

    // 비동기 처리의 흐름을 제어하기 위해 Start를 Coroutine 형태로 선언합니다.
    private IEnumerator Start()
    {
        // 1. Load: 지정된 Address의 에셋을 메모리에 비동기적으로 적재합니다.
        // 이 단계에서는 원본 데이터만 메모리에 올라가며, 씬에는 아직 아무것도 나타나지 않습니다.
        handle = Addressables.LoadAssetAsync<GameObject>(address);
        
        // 메인 스레드의 블로킹(Blocking) 없이 작업 완료를 대기합니다.
        yield return handle;

        // 2. Validation: 로드 실패에 대한 방어적 처리입니다.
        // 네트워크 상태 이상이나 잘못된 키 값 입력 등으로 인한 예외 상황을 통제합니다.
        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"Load failed: {address}");
            yield break; // 코루틴을 즉시 종료하여 비정상적인 Instantiate 실행을 차단합니다.
        }

        // 3. Instantiate: 메모리에 적재된 원본(handle.Result)을 복제하여 씬에 배치합니다.
        // 향후 객체 파괴를 직접 제어하기 위해 생성된 결과물을 instance 변수에 캐싱합니다.
        instance = Instantiate(handle.Result, transform.position, Quaternion.identity);
    }

    /// <summary>
    /// 오브젝트 파괴 시 호출되어 메모리 누수(Memory Leak)를 방지하는 역할을 수행합니다.
    /// 씬 객체 파괴(Destroy)와 메모리 해제(Release)가 명확히 분리되어 순차적으로 이루어져야 합니다.
    /// </summary>
    private void OnDestroy()
    {
        // 1. Destroy: 씬에 생성된 복제본(Instance)이 존재한다면 씬 계층(Hierarchy)에서 파괴합니다.
        // 이는 Addressables 메모리 자체를 지우는 것이 아니라, 화면에 보이는 객체만 제거하는 행위입니다.
        if (instance != null)
            Destroy(instance);

        // 2. Release: Handle이 유효한 상태라면 Addressables 내부의 참조 카운트(Reference Count)를 1 감소시킵니다.
        // 참조 카운트가 0이 되면 메모리에서 원본 에셋이 완전히 언로드(Unload)됩니다.
        if (handle.IsValid())
            Addressables.Release(handle);
    }
}