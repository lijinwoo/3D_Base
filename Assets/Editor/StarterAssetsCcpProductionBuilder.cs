using System.IO;
using System.Linq;
using Lightbug.CharacterControllerPro.Core;
using Lightbug.CharacterControllerPro.Demo;
using Lightbug.CharacterControllerPro.Implementation;
using StarterAssets;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SystemicOverload.EditorTools
{
    /// <summary>
    /// 한국어 주석: StarterAssets + CCP 통합용 Production 결과물을 자동 생성합니다.
    /// </summary>
    public static class StarterAssetsCcpProductionBuilder
    {
        private const string OutputRootFolder = "Assets/StarterAssets/CCPProduction";
        private const string OutputPrefabFolder = "Assets/StarterAssets/CCPProduction/Prefabs";
        private const string OutputSceneFolder = "Assets/StarterAssets/CCPProduction/Scenes";
        private const string OutputPrefabPath = "Assets/StarterAssets/CCPProduction/Prefabs/CCP_StarterAssets_Player.prefab";
        private const string OutputScenePath = "Assets/StarterAssets/CCPProduction/Scenes/CCP_StarterAssets_Validation.unity";

        private const string SourceCcpCharacterPrefabPath = "Assets/Character Controller Pro/Demo/Prefabs/Characters/Demo Character 3D.prefab";
        private const string SourceFollowCameraPrefabPath = "Assets/StarterAssets/ThirdPersonController/Prefabs/PlayerFollowCamera.prefab";
        private const string SourceMainCameraPrefabPath = "Assets/StarterAssets/ThirdPersonController/Prefabs/MainCamera.prefab";
        private const string SourceMobileCanvasPrefabPath = "Assets/StarterAssets/Mobile/Prefabs/CanvasInputs/UI_Canvas_StarterAssetsInputs_Joysticks.prefab";

        [MenuItem("Tools/Systemic Overload/CCP Integration/Build Production Player + Validation Scene")]
        public static void BuildProductionPackage()
        {
            EnsureFolderPath(OutputRootFolder);
            EnsureFolderPath(OutputPrefabFolder);
            EnsureFolderPath(OutputSceneFolder);

            GameObject integratedPlayerPrefab = CreateOrUpdateIntegratedPlayerPrefab();
            CreateOrUpdateValidationScene(integratedPlayerPrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[StarterAssetsCcpProductionBuilder] CCP + StarterAssets Production 결과물이 생성되었습니다.");
        }

        private static GameObject CreateOrUpdateIntegratedPlayerPrefab()
        {
            GameObject sourceCharacterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SourceCcpCharacterPrefabPath);
            GameObject sourceFollowCameraPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SourceFollowCameraPrefabPath);
            GameObject sourceMobileCanvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SourceMobileCanvasPrefabPath);

            if (sourceCharacterPrefab == null)
            {
                throw new FileNotFoundException($"CCP 캐릭터 프리팹을 찾을 수 없습니다: {SourceCcpCharacterPrefabPath}");
            }

            GameObject root = new GameObject("CCP_StarterAssets_Player_Root");
            GameObject characterInstance = (GameObject)PrefabUtility.InstantiatePrefab(sourceCharacterPrefab);
            characterInstance.name = "CCP_Character";
            characterInstance.transform.SetParent(root.transform, false);

            CharacterActor characterActor = characterInstance.GetComponentInChildren<CharacterActor>(true);
            CharacterBrain characterBrain = characterInstance.GetComponentInChildren<CharacterBrain>(true);
            CharacterStateController stateController = characterInstance.GetComponentInChildren<CharacterStateController>(true);
            NormalMovement normalMovement = characterInstance.GetComponentInChildren<NormalMovement>(true);
            Animator characterAnimator = characterInstance.GetComponentInChildren<Animator>(true);

            if (characterActor == null || characterBrain == null || stateController == null || normalMovement == null)
            {
                UnityEngine.Object.DestroyImmediate(root);
                throw new MissingComponentException("CCP 기본 프리팹에 필수 컴포넌트(CharacterActor/Brain/StateController/NormalMovement)가 없습니다.");
            }

            GameObject playerGameObject = characterActor.gameObject;

            StarterAssetsInputs starterAssetsInputs = EnsureComponent<StarterAssetsInputs>(playerGameObject);
            StarterAssetsCcpInputHandler inputHandler = EnsureComponent<StarterAssetsCcpInputHandler>(playerGameObject);
            inputHandler.StarterAssetsInputs = starterAssetsInputs;

            StarterAssetsCcpCameraSync cameraSync = EnsureComponent<StarterAssetsCcpCameraSync>(playerGameObject);
            Transform cameraTarget = EnsureCameraTarget(playerGameObject.transform);
            cameraSync.Configure(starterAssetsInputs, cameraTarget);

            ConfigureBrainInputHandler(characterBrain, inputHandler);
            ConfigureMovementReference(stateController, cameraTarget);
            ConfigureNormalMovementLookingReference(normalMovement);

            StarterAssetsCcpAnimatorBridge animatorBridge = EnsureComponent<StarterAssetsCcpAnimatorBridge>(playerGameObject);
            animatorBridge.Configure(characterActor, characterBrain, characterAnimator);

            if (sourceFollowCameraPrefab != null)
            {
                GameObject followCameraInstance = (GameObject)PrefabUtility.InstantiatePrefab(sourceFollowCameraPrefab);
                followCameraInstance.name = "PlayerFollowCamera";
                followCameraInstance.transform.SetParent(root.transform, false);
                ConfigureCinemachineReferences(followCameraInstance, cameraTarget);
            }

            if (sourceMobileCanvasPrefab != null)
            {
                GameObject mobileCanvasInstance = (GameObject)PrefabUtility.InstantiatePrefab(sourceMobileCanvasPrefab);
                mobileCanvasInstance.name = "MobileInputCanvas";
                mobileCanvasInstance.transform.SetParent(root.transform, false);
                ConfigureMobileCanvasInput(mobileCanvasInstance, starterAssetsInputs);
            }

            GameObject createdPrefab = PrefabUtility.SaveAsPrefabAsset(root, OutputPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return createdPrefab;
        }

        private static void CreateOrUpdateValidationScene(GameObject integratedPlayerPrefab)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject lightRoot = new GameObject("Directional Light");
            Light directionalLight = lightRoot.AddComponent<Light>();
            directionalLight.type = LightType.Directional;
            directionalLight.intensity = 1.0f;
            lightRoot.transform.rotation = Quaternion.Euler(50.0f, -30.0f, 0.0f);

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(6.0f, 1.0f, 6.0f);

            if (integratedPlayerPrefab != null)
            {
                GameObject playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(integratedPlayerPrefab);
                playerInstance.transform.position = Vector3.zero;
            }

            GameObject sourceMainCameraPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SourceMainCameraPrefabPath);
            if (sourceMainCameraPrefab != null)
            {
                GameObject mainCameraInstance = (GameObject)PrefabUtility.InstantiatePrefab(sourceMainCameraPrefab);
                mainCameraInstance.name = "MainCamera";
            }

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), OutputScenePath);
            AddSceneToBuildSettings(OutputScenePath);
        }

        private static Transform EnsureCameraTarget(Transform playerTransform)
        {
            Transform cameraTarget = playerTransform.Find("PlayerCameraRoot");
            if (cameraTarget == null)
            {
                GameObject cameraTargetObject = new GameObject("PlayerCameraRoot");
                cameraTarget = cameraTargetObject.transform;
                cameraTarget.SetParent(playerTransform, false);
                cameraTarget.localPosition = new Vector3(0.0f, 1.4f, 0.0f);
            }

            return cameraTarget;
        }

        private static void ConfigureBrainInputHandler(CharacterBrain characterBrain, InputHandler inputHandler)
        {
            SerializedObject serializedBrain = new SerializedObject(characterBrain);
            SerializedProperty inputTypeProperty = serializedBrain.FindProperty("inputHandlerSettings.humanInputType");
            SerializedProperty inputHandlerProperty = serializedBrain.FindProperty("inputHandlerSettings.inputHandler");

            if (inputTypeProperty != null)
            {
                inputTypeProperty.intValue = (int)HumanInputType.Custom;
            }

            if (inputHandlerProperty != null)
            {
                inputHandlerProperty.objectReferenceValue = inputHandler;
            }

            serializedBrain.ApplyModifiedPropertiesWithoutUndo();
            characterBrain.SetInputHandler(inputHandler);
        }

        private static void ConfigureMovementReference(CharacterStateController stateController, Transform cameraTarget)
        {
            stateController.MovementReferenceMode = MovementReferenceParameters.MovementReferenceMode.External;
            stateController.ExternalReference = cameraTarget;
        }

        private static void ConfigureNormalMovementLookingReference(NormalMovement normalMovement)
        {
            SerializedObject serializedMovement = new SerializedObject(normalMovement);
            SerializedProperty lookingDirectionMode = serializedMovement.FindProperty("lookingDirectionParameters.lookingDirectionMode");
            SerializedProperty stableGroundedMode = serializedMovement.FindProperty("lookingDirectionParameters.stableGroundedLookingDirectionMode");
            SerializedProperty notGroundedMode = serializedMovement.FindProperty("lookingDirectionParameters.notGroundedLookingDirectionMode");

            if (lookingDirectionMode != null)
            {
                lookingDirectionMode.intValue = 1;
            }

            if (stableGroundedMode != null)
            {
                stableGroundedMode.intValue = 1;
            }

            if (notGroundedMode != null)
            {
                notGroundedMode.intValue = 1;
            }

            serializedMovement.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureCinemachineReferences(GameObject followCameraObject, Transform cameraTarget)
        {
            MonoBehaviour[] behaviours = followCameraObject.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (MonoBehaviour behaviour in behaviours.Where(candidate => candidate != null))
            {
                SerializedObject serializedBehaviour = new SerializedObject(behaviour);
                SerializedProperty followProperty = serializedBehaviour.FindProperty("m_Follow");
                SerializedProperty lookAtProperty = serializedBehaviour.FindProperty("m_LookAt");

                bool hasChanges = false;
                if (followProperty != null)
                {
                    followProperty.objectReferenceValue = cameraTarget;
                    hasChanges = true;
                }

                if (lookAtProperty != null)
                {
                    lookAtProperty.objectReferenceValue = cameraTarget;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    serializedBehaviour.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        private static void ConfigureMobileCanvasInput(GameObject mobileCanvasObject, StarterAssetsInputs starterAssetsInputs)
        {
            UICanvasControllerInput[] canvasInputs = mobileCanvasObject.GetComponentsInChildren<UICanvasControllerInput>(true);
            foreach (UICanvasControllerInput canvasInput in canvasInputs)
            {
                canvasInput.starterAssetsInputs = starterAssetsInputs;
                EditorUtility.SetDirty(canvasInput);
            }
        }

        private static T EnsureComponent<T>(GameObject targetObject) where T : Component
        {
            T component = targetObject.GetComponent<T>();
            if (component == null)
            {
                component = targetObject.AddComponent<T>();
            }

            return component;
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            var buildScenes = EditorBuildSettings.scenes.ToList();
            bool alreadyExists = buildScenes.Any(scene => scene.path == scenePath);
            if (alreadyExists)
            {
                return;
            }

            buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = buildScenes.ToArray();
        }

        private static void EnsureFolderPath(string assetPath)
        {
            string[] segments = assetPath.Split('/');
            if (segments.Length == 0 || segments[0] != "Assets")
            {
                throw new IOException("Asset path는 Assets 루트로 시작해야 합니다.");
            }

            string currentPath = "Assets";
            for (int segmentIndex = 1; segmentIndex < segments.Length; segmentIndex++)
            {
                string nextSegment = segments[segmentIndex];
                string nextPath = currentPath + "/" + nextSegment;
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, nextSegment);
                }

                currentPath = nextPath;
            }
        }
    }
}
