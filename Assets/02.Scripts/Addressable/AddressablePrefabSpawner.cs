using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets; // Addressables 핵심 API
using UnityEngine.ResourceManagement.AsyncOperations; // 비동기 작업 핸들링(AsyncOperationHandle)을 위한 네임스페이스

/// <summary>
/// Addressables 시스템을 사용하여 런타임에 프리팹을 비동기적으로 로드하고 생성(Instantiate)하는 기초 스크립트입니다.
/// </summary>
public class AddressablePrefabSpawner : MonoBehaviour
{
    // 인스펙터에서 입력받을 Addressables 키(Key) 값입니다. 
    // 로드하고자 하는 에셋의 그룹/경로 이름과 일치해야 합니다.
    [SerializeField] private string address;

    // 비동기 작업의 상태(진행도, 성공 여부)와 결과물(Result)을 추적하고 메모리를 관리하기 위한 구조체입니다.
    // 추후 메모리 해제(Release)를 위해 참조를 캐싱해 두는 것이 권장됩니다.
    private AsyncOperationHandle<GameObject> handle;

    // 비동기 작업(Load)을 대기하기 위해 Start 메서드를 Coroutine 형태로 사용합니다.
    private IEnumerator Start()
    {
        // 1. LoadAssetAsync: 주어진 주소(address)에 해당하는 GameObject 에셋을 메모리에 비동기적으로 로드합니다.
        // (주의: 이 단계에서는 메모리에 올라오기만 할 뿐, 씬에 생성되지 않습니다.)
        handle = Addressables.LoadAssetAsync<GameObject>(address);

        // 2. yield return: 에셋 로드 작업이 완전히 끝날 때까지 프레임 실행을 대기합니다.
        // 이를 통해 로드 중 메인 스레드가 멈추는(Freezing) 현상을 방지합니다.
        yield return handle;

        // 3. Validation: 비동기 작업이 성공적으로 완료되었는지 상태(Status)를 검증합니다.
        // 잘못된 주소 입력이나 네트워크/디스크 읽기 실패로 인한 오류를 방지하는 필수적인 예외 처리입니다.
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            // 4. Instantiate: 성공적으로 로드된 원본 에셋(handle.Result)을 
            // 현재 스크립트가 붙은 게임 오브젝트의 위치에 생성합니다.
            Instantiate(handle.Result, transform.position, Quaternion.identity);
        }
        else
        {
            // 로드 실패 시 디버깅을 위한 로그 처리 (추가됨)
            Debug.LogError($"[Addressables] Failed to load asset at address: {address}");
        }
    }

    /// <summary>
    /// Addressables는 Resources 폴더와 달리 자동 메모리 관리가 되지 않으므로,
    /// 오브젝트가 파괴되거나 더 이상 필요 없을 때 반드시 명시적으로 메모리를 해제해야 합니다.
    /// </summary>
    private void OnDestroy()
    {
        // handle이 유효하고 작업이 수행된 적이 있다면 메모리에서 에셋을 해제(Release)합니다.
        if (handle.IsValid())
        {
            Addressables.Release(handle);
        }
    }
}
