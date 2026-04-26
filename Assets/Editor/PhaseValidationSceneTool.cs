using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using SystemicOverload.Phase1;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SystemicOverload.EditorTools
{
    /// <summary>
    /// Phase별 Validation Scene을 생성/정렬하는 에디터 전용 Tool입니다.
    /// </summary>
    public static class PhaseValidationSceneTool
    {
        private const string PhaseValidationFolder = "Assets/01.Scenes/PhaseValidation";
        private const string Phase1ScenePath = "Assets/01.Scenes/PhaseValidation/Phase_01_MovementValidation.unity";

        [MenuItem("Tools/Systemic Overload/Phase Validation/Build Phase 1 Movement Scene")]
        public static void BuildPhase1MovementScene()
        {
            EnsureFolderPath(PhaseValidationFolder);

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SetupPhase1MovementScene();

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), Phase1ScenePath);
            AddSceneToBuildSettings(Phase1ScenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[PhaseValidationSceneTool] Phase 1 Validation Scene 생성 및 Build Settings 등록이 완료되었습니다.");
        }

        private static void SetupPhase1MovementScene()
        {
            GameObject lightRoot = new GameObject("Directional Light");
            Light directionalLight = lightRoot.AddComponent<Light>();
            directionalLight.type = LightType.Directional;
            directionalLight.intensity = 1.0f;
            lightRoot.transform.rotation = Quaternion.Euler(50.0f, -30.0f, 0.0f);

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(4.0f, 1.0f, 4.0f);

            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = new Vector3(0.0f, 1.0f, 0.0f);
            RemoveComponent<CapsuleCollider>(player);

            CharacterController characterController = player.AddComponent<CharacterController>();
            characterController.center = new Vector3(0.0f, 1.0f, 0.0f);
            characterController.height = 2.0f;
            characterController.radius = 0.45f;

            InputProvider inputProvider = player.AddComponent<InputProvider>();
            MovementComponent movementComponent = player.AddComponent<MovementComponent>();

            GameObject cameraRoot = new GameObject("Main Camera");
            Camera mainCamera = cameraRoot.AddComponent<Camera>();
            cameraRoot.tag = "MainCamera";
            mainCamera.nearClipPlane = 0.1f;
            mainCamera.farClipPlane = 300.0f;
            mainCamera.fieldOfView = 60.0f;

            Phase1OrbitCameraController cameraController = cameraRoot.AddComponent<Phase1OrbitCameraController>();
            SetPrivateField(cameraController, "followTarget", player.transform);
            SetPrivateField(cameraController, "inputProvider", inputProvider);
            SetPrivateField(cameraController, "movementComponent", movementComponent);
            SetPrivateField(cameraController, "pivotOffset", new Vector3(0.0f, 1.6f, 0.0f));
            SetPrivateField(cameraController, "defaultZoomDistance", 7.0f);
            SetPrivateField(cameraController, "maxZoomDistance", 14.0f);
            SetPrivateField(cameraController, "autoFollowMode", Phase1OrbitCameraController.AutoFollowMode.MovingOnly);
            SetPrivateField(cameraController, "collisionMask", -1);
            SetPrivateField(cameraController, "waterSurfaceHeight", -1000.0f);

            // Mouse Raycast 회전이 즉시 동작하도록 MovementComponent에 카메라를 명시적으로 연결합니다.
            SetPrivateField(movementComponent, "aimCamera", mainCamera);
            SetPrivateField(movementComponent, "groundLayerMask", -1);
            SetPrivateField(movementComponent, "aimRayMaxDistance", 500.0f);
            SetPrivateField(movementComponent, "useMouseRaycastRotation", false);
            SetPrivateField(movementComponent, "orbitCameraController", cameraController);

            SetPrivateField(inputProvider, "normalizeDiagonalInput", true);
            SetPrivateField(inputProvider, "enableDualMouseForwardMove", true);
            SetPrivateField(inputProvider, "dualMouseForwardAmount", 1.0f);
        }

        [MenuItem("Tools/Systemic Overload/Phase Validation/Generate Scene Policy Template")]
        public static void GenerateScenePolicyTemplate()
        {
            EnsureFolderPath(PhaseValidationFolder);

            string policyFilePath = Path.Combine(PhaseValidationFolder, "PhaseValidationSceneTemplate.txt");
            string policyTemplate = BuildPolicyTemplate();
            File.WriteAllText(policyFilePath, policyTemplate, Encoding.UTF8);

            AssetDatabase.Refresh();
            Debug.Log("[PhaseValidationSceneTool] Phase Validation Scene 템플릿 파일이 생성되었습니다.");
        }

        private static string BuildPolicyTemplate()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Phase Validation Scene Template");
            builder.AppendLine("- Scene Path Rule: Assets/01.Scenes/PhaseValidation/Phase_0N_<Feature>Validation.unity");
            builder.AppendLine("- Build Settings: Phase Scene를 enabled 상태로 등록");
            builder.AppendLine("- Minimum Objects:");
            builder.AppendLine("  - Ground (validation floor)");
            builder.AppendLine("  - Player/Target actor with current Phase components");
            builder.AppendLine("  - Main Camera with validation camera behavior");
            builder.AppendLine("- Validation Checklist:");
            builder.AppendLine("  - Smoke: 핵심 입력/동작 1회 이상 성공");
            builder.AppendLine("  - Input Mapping: LMB FreeLook, RMB SyncRotate, LMB+RMB Forward, Wheel Zoom");
            builder.AppendLine("  - Regression: 직전 Phase 핵심 기능이 유지됨");
            builder.AppendLine("  - Performance: 프레임 드랍/GC spike 여부 확인");
            return builder.ToString();
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            List<EditorBuildSettingsScene> buildScenes = EditorBuildSettings.scenes.ToList();
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

        private static void RemoveComponent<T>(GameObject targetGameObject) where T : Component
        {
            T targetComponent = targetGameObject.GetComponent<T>();
            if (targetComponent == null)
            {
                return;
            }

            Object.DestroyImmediate(targetComponent);
        }

        private static void SetPrivateField<TTarget>(TTarget targetObject, string fieldName, object value)
        {
            FieldInfo targetField = typeof(TTarget).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (targetField == null)
            {
                Debug.LogWarning($"[PhaseValidationSceneTool] 필드 연결 실패: {typeof(TTarget).Name}.{fieldName}");
                return;
            }

            targetField.SetValue(targetObject, value);
        }
    }
}
