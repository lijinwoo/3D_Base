using System.Collections.Generic;
using System.Text;
using SystemicOverload.Comparison;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SystemicOverload.EditorTools.Comparison
{
    /// <summary>
    /// MonoBehaviour vs ScriptableObject 비교 scene을 사전 배치 형태로 생성/갱신하는 에디터 도구입니다.
    /// </summary>
    public static class ComparisonSceneBakeTool
    {
        private const string ScenePath = "Assets/01.Scenes/Comparison/MonoVsScriptableObject_Comparison.unity";
        private const string ConfigAssetPath = "Assets/04.Data/Comparison/GoblinConfig.asset";
        private const int EnemyCount = 5;

        [MenuItem("SystemicOverload/Tools/Comparison/Bake Mono vs SO Scene")]
        public static void BakeSceneFromMenu()
        {
            BakeMonoVsSoScene();
        }

        /// <summary>
        /// batchmode에서 -executeMethod로 호출하기 위한 진입점입니다.
        /// </summary>
        public static void BakeMonoVsSoScene()
        {
            EnsureFolder("Assets/01.Scenes/Comparison");
            EnsureFolder("Assets/04.Data/Comparison");

            Scene targetScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ClearSceneRoots(targetScene);

            ComparisonEnemyConfigSO configAsset = EnsureConfigAsset();
            GameObject cameraObject = CreateCamera();
            GameObject lightObject = CreateDirectionalLight();
            GameObject groundObject = CreateGround();
            GameObject monoRoot = new GameObject("Mono Group Root");
            GameObject soRoot = new GameObject("SO Group Root");
            GameObject controllerObject = new GameObject("Comparison Demo Controller");

            List<MonoEnemyStatsComponent> monoComponents = new List<MonoEnemyStatsComponent>(EnemyCount);
            List<SoEnemyStatsComponent> soComponents = new List<SoEnemyStatsComponent>(EnemyCount);

            for (int index = 0; index < EnemyCount; index++)
            {
                float yPosition = 0.5f + (index * 1.4f);

                GameObject monoEnemyObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                monoEnemyObject.name = $"Mono Enemy {index + 1}";
                monoEnemyObject.transform.SetParent(monoRoot.transform);
                monoEnemyObject.transform.position = new Vector3(-4.0f, yPosition, 0.0f);
                ApplyColor(monoEnemyObject, new Color(0.2f, 0.55f, 1.0f, 1.0f));

                MonoEnemyStatsComponent monoComponent = monoEnemyObject.AddComponent<MonoEnemyStatsComponent>();
                monoComponent.ConfigureLocalBaseStats(monoEnemyObject.name, 100.0f, 10.0f);
                monoComponents.Add(monoComponent);

                GameObject soEnemyObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                soEnemyObject.name = $"SO Enemy {index + 1}";
                soEnemyObject.transform.SetParent(soRoot.transform);
                soEnemyObject.transform.position = new Vector3(4.0f, yPosition, 0.0f);
                ApplyColor(soEnemyObject, new Color(1.0f, 0.45f, 0.2f, 1.0f));

                SoEnemyStatsComponent soComponent = soEnemyObject.AddComponent<SoEnemyStatsComponent>();
                soComponent.Configure(configAsset, soEnemyObject.name);
                soComponents.Add(soComponent);
            }

            ComparisonDemoController controller = controllerObject.AddComponent<ComparisonDemoController>();
            AssignPrivateReferenceArray(controller, "monoEnemies", monoComponents.ToArray());
            AssignPrivateReferenceArray(controller, "soEnemies", soComponents.ToArray());
            AssignPrivateObject(controller, "sharedGoblinConfig", configAsset);

            EditorUtility.SetDirty(cameraObject);
            EditorUtility.SetDirty(lightObject);
            EditorUtility.SetDirty(groundObject);
            EditorUtility.SetDirty(monoRoot);
            EditorUtility.SetDirty(soRoot);
            EditorUtility.SetDirty(controllerObject);
            EditorUtility.SetDirty(configAsset);

            EditorSceneManager.MarkSceneDirty(targetScene);
            EditorSceneManager.SaveScene(targetScene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void AssignPrivateReferenceArray<T>(Object targetObject, string fieldName, T[] values) where T : Object
        {
            SerializedObject serializedObject = new SerializedObject(targetObject);
            SerializedProperty targetProperty = serializedObject.FindProperty(fieldName);
            if (targetProperty == null || !targetProperty.isArray)
            {
                return;
            }

            targetProperty.arraySize = values.Length;
            for (int index = 0; index < values.Length; index++)
            {
                targetProperty.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignPrivateObject(Object targetObject, string fieldName, Object value)
        {
            SerializedObject serializedObject = new SerializedObject(targetObject);
            SerializedProperty targetProperty = serializedObject.FindProperty(fieldName);
            if (targetProperty == null)
            {
                return;
            }

            targetProperty.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ClearSceneRoots(Scene scene)
        {
            GameObject[] rootObjects = scene.GetRootGameObjects();
            for (int index = 0; index < rootObjects.Length; index++)
            {
                Object.DestroyImmediate(rootObjects[index]);
            }
        }

        private static ComparisonEnemyConfigSO EnsureConfigAsset()
        {
            ComparisonEnemyConfigSO configAsset = AssetDatabase.LoadAssetAtPath<ComparisonEnemyConfigSO>(ConfigAssetPath);
            if (configAsset != null)
            {
                return configAsset;
            }

            configAsset = ScriptableObject.CreateInstance<ComparisonEnemyConfigSO>();
            AssetDatabase.CreateAsset(configAsset, ConfigAssetPath);
            return configAsset;
        }

        private static GameObject CreateCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";

            Camera cameraComponent = cameraObject.AddComponent<Camera>();
            cameraComponent.transform.position = new Vector3(0.0f, 5.5f, -14.0f);
            cameraComponent.transform.rotation = Quaternion.Euler(18.0f, 0.0f, 0.0f);

            cameraObject.AddComponent<AudioListener>();
            return cameraObject;
        }

        private static GameObject CreateDirectionalLight()
        {
            GameObject lightObject = new GameObject("Directional Light");
            Light directionalLight = lightObject.AddComponent<Light>();
            directionalLight.type = LightType.Directional;
            directionalLight.intensity = 1.0f;
            lightObject.transform.rotation = Quaternion.Euler(50.0f, -30.0f, 0.0f);
            return lightObject;
        }

        private static GameObject CreateGround()
        {
            GameObject groundObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            groundObject.name = "Comparison Ground";
            groundObject.transform.position = new Vector3(0.0f, -0.05f, 0.0f);
            groundObject.transform.localScale = new Vector3(22.0f, 0.1f, 10.0f);
            ApplyColor(groundObject, new Color(0.2f, 0.2f, 0.2f, 1.0f));
            return groundObject;
        }

        private static void ApplyColor(GameObject targetObject, Color color)
        {
            Renderer targetRenderer = targetObject.GetComponent<Renderer>();
            if (targetRenderer == null)
            {
                return;
            }

            Shader targetShader = Shader.Find("Universal Render Pipeline/Lit");
            if (targetShader == null)
            {
                targetShader = Shader.Find("Standard");
            }

            Material newMaterial = targetShader != null ? new Material(targetShader) : new Material(targetRenderer.sharedMaterial);
            if (newMaterial.HasProperty("_BaseColor"))
            {
                newMaterial.SetColor("_BaseColor", color);
            }

            if (newMaterial.HasProperty("_Color"))
            {
                newMaterial.SetColor("_Color", color);
            }

            targetRenderer.sharedMaterial = newMaterial;
        }

        private static void EnsureFolder(string targetPath)
        {
            if (AssetDatabase.IsValidFolder(targetPath))
            {
                return;
            }

            string normalizedPath = targetPath.Replace("\\", "/");
            string[] parts = normalizedPath.Split('/');
            if (parts.Length < 2)
            {
                return;
            }

            StringBuilder currentPathBuilder = new StringBuilder(parts[0]);
            for (int index = 1; index < parts.Length; index++)
            {
                string folderName = parts[index];
                string parentPath = currentPathBuilder.ToString();
                string combinedPath = parentPath + "/" + folderName;
                if (!AssetDatabase.IsValidFolder(combinedPath))
                {
                    AssetDatabase.CreateFolder(parentPath, folderName);
                }

                currentPathBuilder.Append('/').Append(folderName);
            }
        }
    }
}
