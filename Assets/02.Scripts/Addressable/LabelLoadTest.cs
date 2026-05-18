using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LabelLoadTest : MonoBehaviour
{
   //로드할 라벨 문자열 
   [SerializeField]
   private string targetLabel = "Consumable";
   
   //핸들
   private AsyncOperationHandle<IList<GameObject>> loadHandle;

   private void Start()
   {
      //라벨로 아이템 로드하기
      LoadItemsByLabel();
   }

   private void LoadItemsByLabel()
   {
      //1.라벨로 복수의 프리팹을 로드 
      loadHandle = Addressables.LoadAssetsAsync<GameObject>(targetLabel, OnSingleItemLoaded);
      //2. 두 번째 파라메터 --> 아이템이 하나씩 로드 될때마다 실행되는 함수
      
      //3. 모든 에셋이 로드 완료되면 실행되는 함수 
      loadHandle.Completed += OnAllItemsLoaded;
   }

   private void OnSingleItemLoaded(GameObject loadedObj)
   {
      Debug.Log($"개별 로드 완료 {loadedObj.name}이 메모리에 로드 되었습니다.");
      
      // 로드된 에셋을 씬에 무작위 위치로 생성 (Instantiate) 
      Vector3 randomPos = new Vector3(
         UnityEngine.Random.Range(-10f, 10f),
         0,
         UnityEngine.Random.Range(-10f, 10f)
      );
      
      Instantiate(loadedObj, randomPos, Quaternion.identity);
   }

   
   // 이해 X -> 그냥 구문 복붙 
   //event 등록에 필요한 parameter 는 ~ 
   private void OnAllItemsLoaded(AsyncOperationHandle<IList<GameObject>> handle)
   {
      //핸들에서 제공하는, 로드성공하는 함수
      if (handle.Status == AsyncOperationStatus.Succeeded)
      {
         Debug.Log($"[전체 로드 완료!] {handle.Result.Count}");
      }
      else
      {
         Debug.LogError("에셋 로드에 실패했습니다.");
      }
   }

   private void OnDestroy()
   {
      //핸들유효하다면 ~ == 로드를 했음 
      if (loadHandle.IsValid())
      {
         Addressables.Release(loadHandle);
         Debug.Log("라벨 로드 핸들 해제");
      }
   }
}
