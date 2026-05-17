using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressSpawnExample : MonoBehaviour
{
    [SerializeField] private string address = "Enemy_Goblin";
    private AsyncOperationHandle<GameObject> instanceHandle;

    public void Spawn()
    {
        instanceHandle = Addressables.InstantiateAsync(
            address, transform.position, Quaternion.identity);

        instanceHandle.Completed += callBackContext =>
        {
            if (callBackContext.Status == AsyncOperationStatus.Succeeded)
                Debug.Log($"Spawned: {callBackContext.Result.name}");
        };
    }

    private void OnDestroy()
    {
        if (instanceHandle.IsValid())
            Addressables.ReleaseInstance(instanceHandle);
    }
}