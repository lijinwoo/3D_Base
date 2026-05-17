using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressLoadExample : MonoBehaviour
{
    [SerializeField] private string address = "Enemy_Goblin";
    private AsyncOperationHandle<GameObject> handle;
    private GameObject spawned;

    private void Start()
    {
        handle = Addressables.LoadAssetAsync<GameObject>(address);
        handle.Completed += OnLoaded;
    }

    private void OnLoaded(AsyncOperationHandle<GameObject> h)
    {
        if (h.Status != AsyncOperationStatus.Succeeded) return;
        spawned = Instantiate(h.Result, transform.position, Quaternion.identity);
    }

    private void OnDestroy()
    {
        if (spawned != null) Destroy(spawned);
        if (handle.IsValid()) Addressables.Release(handle);
    }
}