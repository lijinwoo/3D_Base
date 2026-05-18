using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemyPreloader : MonoBehaviour
{
    private AsyncOperationHandle<IList<GameObject>> handle;
    private readonly List<GameObject> enemyPrefabs = new();

    private IEnumerator Start()
    {
        handle = Addressables.LoadAssetsAsync<GameObject>(
            "enemy", prefab => enemyPrefabs.Add(prefab));

        yield return handle;

        Debug.Log($"Loaded enemy prefabs: {enemyPrefabs.Count}");
    }

    private void OnDestroy()
    {
        if (handle.IsValid())
            Addressables.Release(handle);
    }
}