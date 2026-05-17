using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private ItemData itemData;

    private GameObject spawned;

    private IEnumerator Start()
    {
        var handle = itemData.WorldPrefab.InstantiateAsync(
            transform.position, Quaternion.identity);

        yield return handle;
       
        if (handle.Status == AsyncOperationStatus.Succeeded)
            spawned = handle.Result;
    }

    private void OnDestroy()
    {
        if (spawned != null)
            Addressables.ReleaseInstance(spawned);
    }
}