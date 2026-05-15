using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressSpawnExample : MonoBehaviour
{
    [SerializeField] private string address = "Enemy_Goblin";
    private AsyncOperationHandle<GameObject> instanceHandle;

    private void Start()
    {
        Spawn();
    }


    public void Spawn()
    {
        instanceHandle = Addressables.InstantiateAsync(
            address, transform.position, Quaternion.identity);

        instanceHandle.Completed += h =>
        {
            if (h.Status == AsyncOperationStatus.Succeeded)
                Debug.Log($"Spawned: {h.Result.name}");
        };
    }

    private void OnDestroy()
    {
        if (instanceHandle.IsValid())
            Addressables.ReleaseInstance(instanceHandle);
    }
}