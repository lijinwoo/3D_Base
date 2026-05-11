using System;
using System.IO;
using System.Linq;
using System.Reflection;
using SystemicOverload.AddressablesSupport;
using SystemicOverload.AI;
using SystemicOverload.Combat;
using SystemicOverload.Data;
using SystemicOverload.Encounter;
using SystemicOverload.Phase1;
using SystemicOverload.Pooling;
using SystemicOverload.Rpg;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace SystemicOverload.EditorTools
{
    /// <summary>
    /// 한국어 주석: Phase 3~6 RPG 검증 씬 생성 메뉴를 분리해 유지보수합니다(인카운터·세이브·퀘스트 포함).
    /// </summary>
    public static class PhaseValidationScenePhases3Plus
    {
        private const string PhaseValidationFolder = "Assets/01.Scenes/PhaseValidation";
        private const string PrefabFolder = "Assets/01.Scenes/PhaseValidation/Prefabs";
        private const string DataFolder = "Assets/Data/PhaseValidation";
        private const string Phase3ScenePath = "Assets/01.Scenes/PhaseValidation/Phase_03_CombatDataValidation.unity";
        private const string Phase4ScenePath = "Assets/01.Scenes/PhaseValidation/Phase_04_AINavigationValidation.unity";
        private const string Phase5ScenePath = "Assets/01.Scenes/PhaseValidation/Phase_05_AddressablesVFXValidation.unity";
        private const string Phase6ScenePath = "Assets/01.Scenes/PhaseValidation/Phase_06_FinalProfilingValidation.unity";
        private const string PoolEnemyPrefabPath = "Assets/01.Scenes/PhaseValidation/Prefabs/PoolEnemy.prefab";

        [MenuItem("Tools/Systemic Overload/RPG Vertical Slice/Build Phase 3 Combat Data Scene")]
        public static void BuildPhase3CombatDataScene()
        {
            EnsureFolderPath(PrefabFolder);
            EnsureFolderPath(DataFolder);

            GameObject poolEnemyPrefabAsset = EnsurePoolEnemyPrefabAsset();
            StatData statData = EnsureStatDataAsset();
            WeaponData weaponData = EnsureWeaponDataAsset();
            EncounterSpawnData encounterSpawnData = EnsureEncounterSpawnDataAsset(poolEnemyPrefabAsset);
            QuestDefinition questDefinition = EnsureQuestDefinitionAsset();

            EnsureFolderPath(PhaseValidationFolder);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            PhaseValidationSceneTool.InstantiatePhase1MovementCore(true, out GameObject player, out MovementComponent movementComponent);
            PhaseValidationSceneTool.AttachLocomotionAnimatorPublic(player, movementComponent);
            EnsureRpgServicesInScene(questDefinition);

            HealthComponent playerHealth = player.AddComponent<HealthComponent>();
            SetPrivateField(playerHealth, "statData", statData);
            SetPrivateField(playerHealth, "maxHealth", statData.MaxHealth);
            SetPrivateField(playerHealth, "currentHealth", statData.MaxHealth);

            CombatComponent combatComponent = player.AddComponent<CombatComponent>();
            Animator playerAnimator = player.GetComponent<Animator>();
            SetPrivateField(combatComponent, "movementComponent", movementComponent);
            SetPrivateField(combatComponent, "animator", playerAnimator);
            SetPrivateField(combatComponent, "weaponData", weaponData);

            GameObject dummy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            dummy.name = "TrainingDummy";
            dummy.transform.position = new Vector3(0.0f, 1.0f, 10.0f);
            HealthComponent dummyHealth = dummy.AddComponent<HealthComponent>();
            SetPrivateField(dummyHealth, "maxHealth", 500.0f);
            SetPrivateField(dummyHealth, "currentHealth", 500.0f);

            GameObject poolRoot = new GameObject("EnemyPool");
            GameObjectPool pool = poolRoot.AddComponent<GameObjectPool>();
            SetPrivateField(pool, "prefab", poolEnemyPrefabAsset);
            SetPrivateField(pool, "prewarmCount", 4);
            SetPrivateField(pool, "resetAnimatorOnSpawn", true);

            GameObject encounterRoot = new GameObject("EncounterDirector");
            encounterRoot.transform.position = Vector3.zero;
            EncounterDirector encounterDirector = encounterRoot.AddComponent<EncounterDirector>();
            SetPrivateField(encounterDirector, "encounterSpawnData", encounterSpawnData);
            SetPrivateField(encounterDirector, "enemyPool", pool);
            SetPrivateField(encounterDirector, "spawnAnchor", encounterRoot.transform);

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), Phase3ScenePath);
            AddSceneToBuildSettings(Phase3ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[PhaseValidationScenePhases3Plus] Phase 3 RPG 검증 씬이 생성되었습니다. EncounterSpawnData·Quest·WeaponData·StatData를 Assets/Data/PhaseValidation에서 확인하세요.");
        }

        [MenuItem("Tools/Systemic Overload/RPG Vertical Slice/Build Phase 4 AI Navigation Scene")]
        public static void BuildPhase4AiNavigationScene()
        {
            EnsureFolderPath(PhaseValidationFolder);
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
            NavMeshSurface navMeshSurface = ground.AddComponent<NavMeshSurface>();
            navMeshSurface.collectObjects = CollectObjects.All;
            navMeshSurface.BuildNavMesh();

            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = new Vector3(0.0f, 1.0f, 0.0f);
            RemoveComponent<CapsuleCollider>(player);
            player.AddComponent<PlayerTargetProvider>();

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

            SetPrivateField(movementComponent, "aimCamera", mainCamera);
            SetPrivateField(movementComponent, "groundLayerMask", -1);
            SetPrivateField(movementComponent, "aimRayMaxDistance", 500.0f);
            SetPrivateField(movementComponent, "useMouseRaycastRotation", true);
            SetPrivateField(movementComponent, "orbitCameraController", cameraController);

            SetPrivateField(inputProvider, "normalizeDiagonalInput", true);
            SetPrivateField(inputProvider, "enableDualMouseForwardMove", true);
            SetPrivateField(inputProvider, "dualMouseForwardAmount", 1.0f);

            PhaseValidationSceneTool.AttachLocomotionAnimatorPublic(player, movementComponent);

            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "NavMeshEnemy";
            enemy.transform.position = new Vector3(8.0f, 1.0f, 4.0f);
            NavMeshAgent agent = enemy.AddComponent<NavMeshAgent>();
            agent.speed = 3.5f;
            agent.acceleration = 12.0f;
            agent.stoppingDistance = 1.2f;
            enemy.AddComponent<EnemyAiBlackboard>();
            enemy.AddComponent<EnemyNavMeshChaser>();

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), Phase4ScenePath);
            AddSceneToBuildSettings(Phase4ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[PhaseValidationScenePhases3Plus] Phase 4 Validation Scene이 생성되었습니다. NavMesh Surface가 베이크되었는지 확인하세요.");
        }

        [MenuItem("Tools/Systemic Overload/RPG Vertical Slice/Build Phase 5 Addressables VFX Scene")]
        public static void BuildPhase5AddressablesVfxScene()
        {
            EnsureFolderPath(PhaseValidationFolder);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject lightRoot = new GameObject("Directional Light");
            Light directionalLight = lightRoot.AddComponent<Light>();
            directionalLight.type = LightType.Directional;
            directionalLight.intensity = 1.0f;
            lightRoot.transform.rotation = Quaternion.Euler(50.0f, -30.0f, 0.0f);

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(4.0f, 1.0f, 4.0f);

            GameObject anchor = new GameObject("AddressablesVfxAnchor");
            anchor.transform.position = new Vector3(0.0f, 1.0f, 2.0f);
            AddressablesOneShotVfx oneShot = anchor.AddComponent<AddressablesOneShotVfx>();
            AddressablesWarmupUtility warmup = anchor.AddComponent<AddressablesWarmupUtility>();

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), Phase5ScenePath);
            AddSceneToBuildSettings(Phase5ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[PhaseValidationScenePhases3Plus] Phase 5 Validation Scene이 생성되었습니다. Addressables 그룹에 VFX를 등록하고 AssetReference를 연결하세요.");
        }

        [MenuItem("Tools/Systemic Overload/RPG Vertical Slice/Build Phase 6 Final Profiling Scene")]
        public static void BuildPhase6FinalProfilingScene()
        {
            EnsureFolderPath(PrefabFolder);
            EnsureFolderPath(DataFolder);
            EnsureFolderPath(PhaseValidationFolder);

            GameObject poolEnemyPrefabAsset = EnsurePoolEnemyPrefabAsset();
            StatData statData = EnsureStatDataAsset();
            WeaponData weaponData = EnsureWeaponDataAsset();
            EncounterSpawnData encounterSpawnData = EnsureEncounterSpawnDataAsset(poolEnemyPrefabAsset);
            QuestDefinition questDefinition = EnsureQuestDefinitionAsset();

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject lightRoot = new GameObject("Directional Light");
            Light directionalLight = lightRoot.AddComponent<Light>();
            directionalLight.type = LightType.Directional;
            directionalLight.intensity = 1.0f;
            lightRoot.transform.rotation = Quaternion.Euler(50.0f, -30.0f, 0.0f);

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(8.0f, 1.0f, 8.0f);
            NavMeshSurface navMeshSurface = ground.AddComponent<NavMeshSurface>();
            navMeshSurface.collectObjects = CollectObjects.All;

            // 한국어 주석: Phase 1 코어(입력/이동/카메라) + Animator 연결.
            PhaseValidationSceneTool.InstantiatePhase1MovementCore(true, out GameObject player, out MovementComponent movementComponent);
            PhaseValidationSceneTool.AttachLocomotionAnimatorPublic(player, movementComponent);
            EnsureRpgServicesInScene(questDefinition);

            // 한국어 주석: Phase 2/3 전투·데이터 계층 연결.
            HealthComponent playerHealth = player.AddComponent<HealthComponent>();
            SetPrivateField(playerHealth, "statData", statData);
            SetPrivateField(playerHealth, "maxHealth", statData.MaxHealth);
            SetPrivateField(playerHealth, "currentHealth", statData.MaxHealth);

            CombatComponent combatComponent = player.AddComponent<CombatComponent>();
            Animator playerAnimator = player.GetComponent<Animator>();
            SetPrivateField(combatComponent, "movementComponent", movementComponent);
            SetPrivateField(combatComponent, "animator", playerAnimator);
            SetPrivateField(combatComponent, "weaponData", weaponData);

            GameObject trainingDummy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            trainingDummy.name = "TrainingDummy";
            trainingDummy.transform.position = new Vector3(0.0f, 1.0f, 12.0f);
            HealthComponent dummyHealth = trainingDummy.AddComponent<HealthComponent>();
            SetPrivateField(dummyHealth, "maxHealth", 500.0f);
            SetPrivateField(dummyHealth, "currentHealth", 500.0f);

            // 한국어 주석: Phase 3 Pool + 인카운터 세팅.
            GameObject poolRoot = new GameObject("EnemyPool");
            GameObjectPool pool = poolRoot.AddComponent<GameObjectPool>();
            SetPrivateField(pool, "prefab", poolEnemyPrefabAsset);
            SetPrivateField(pool, "prewarmCount", 8);
            SetPrivateField(pool, "resetAnimatorOnSpawn", true);

            GameObject encounterRoot = new GameObject("EncounterDirector");
            encounterRoot.transform.position = Vector3.zero;
            EncounterDirector encounterDirector = encounterRoot.AddComponent<EncounterDirector>();
            SetPrivateField(encounterDirector, "encounterSpawnData", encounterSpawnData);
            SetPrivateField(encounterDirector, "enemyPool", pool);
            SetPrivateField(encounterDirector, "spawnAnchor", encounterRoot.transform);

            // 한국어 주석: Phase 4 NavMesh 추격 AI 스모크 구성.
            GameObject navMeshEnemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            navMeshEnemy.name = "NavMeshEnemy";
            navMeshEnemy.transform.position = new Vector3(10.0f, 1.0f, 6.0f);
            NavMeshAgent navMeshAgent = navMeshEnemy.AddComponent<NavMeshAgent>();
            navMeshAgent.speed = 3.5f;
            navMeshAgent.acceleration = 12.0f;
            navMeshAgent.stoppingDistance = 1.2f;
            navMeshEnemy.AddComponent<EnemyAiBlackboard>();
            navMeshEnemy.AddComponent<EnemyNavMeshChaser>();

            // 한국어 주석: Phase 5 Addressables VFX 훅 배치.
            GameObject addressablesAnchor = new GameObject("AddressablesVfxAnchor");
            addressablesAnchor.transform.position = new Vector3(2.0f, 1.0f, 3.0f);
            addressablesAnchor.AddComponent<AddressablesOneShotVfx>();
            addressablesAnchor.AddComponent<AddressablesWarmupUtility>();

            GameObject note = new GameObject("ProfilingNote");
            note.transform.position = new Vector3(0.0f, 1.0f, 0.0f);
            note.AddComponent<Phase6ProfilingMarker>();

            // 한국어 주석: 플레이어/적/환경 배치 후 NavMesh를 마지막에 베이크합니다.
            navMeshSurface.BuildNavMesh();

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), Phase6ScenePath);
            AddSceneToBuildSettings(Phase6ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[PhaseValidationScenePhases3Plus] Phase 1~6 통합 RPG 검증 씬(Phase 6)이 생성되었습니다. 이동/전투/Pool/인카운터/AI/Addressables/VFX 및 프로파일링을 한 scene에서 검증하세요.");
        }

        /// <summary>
        /// 한국어 주석: 통합 Phase 6 scene에서 프로파일링 기준점을 찾기 위한 마커 컴포넌트입니다.
        /// </summary>
        private sealed class Phase6ProfilingMarker : MonoBehaviour
        {
        }

        private static GameObject EnsurePoolEnemyPrefabAsset()
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(PoolEnemyPrefabPath);
            if (existing != null)
            {
                return existing;
            }

            GameObject runtimeEnemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            runtimeEnemy.name = "PoolEnemy";
            // 한국어 주석: 히트 스캔을 위해 CapsuleCollider를 유지합니다.

            HealthComponent health = runtimeEnemy.AddComponent<HealthComponent>();
            SetPrivateField(health, "maxHealth", 40.0f);
            SetPrivateField(health, "currentHealth", 40.0f);
            runtimeEnemy.AddComponent<PooledHealthReset>();
            runtimeEnemy.AddComponent<PooledInstanceLink>();
            runtimeEnemy.AddComponent<PooledEnemyReleaseOnDeath>();

            EnsureFolderPath(PrefabFolder);
            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(runtimeEnemy, PoolEnemyPrefabPath);
            UnityEngine.Object.DestroyImmediate(runtimeEnemy);
            return prefabAsset;
        }

        private static StatData EnsureStatDataAsset()
        {
            string assetPath = $"{DataFolder}/Stat_PhaseValidation.asset";
            StatData asset = AssetDatabase.LoadAssetAtPath<StatData>(assetPath);
            if (asset != null)
            {
                return asset;
            }

            StatData created = ScriptableObject.CreateInstance<StatData>();
            AssetDatabase.CreateAsset(created, assetPath);
            SerializedObject serializedObject = new SerializedObject(created);
            serializedObject.FindProperty("maxHealth").floatValue = 120.0f;
            serializedObject.FindProperty("damageTakenMultiplier").floatValue = 1.0f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return created;
        }

        private static WeaponData EnsureWeaponDataAsset()
        {
            string assetPath = $"{DataFolder}/Weapon_PhaseValidation.asset";
            WeaponData asset = AssetDatabase.LoadAssetAtPath<WeaponData>(assetPath);
            if (asset != null)
            {
                return asset;
            }

            WeaponData created = ScriptableObject.CreateInstance<WeaponData>();
            AssetDatabase.CreateAsset(created, assetPath);
            SerializedObject serializedObject = new SerializedObject(created);
            serializedObject.FindProperty("hitScanDamage").floatValue = 14.0f;
            serializedObject.FindProperty("shotsPerSecond").floatValue = 5.0f;
            serializedObject.FindProperty("maxRange").floatValue = 55.0f;
            serializedObject.FindProperty("rayOriginHeight").floatValue = 1.0f;
            serializedObject.FindProperty("rayStartForwardOffset").floatValue = 0.35f;
            serializedObject.FindProperty("hitLayerMask").intValue = ~0;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return created;
        }

        private static EncounterSpawnData EnsureEncounterSpawnDataAsset(GameObject enemyPrefab)
        {
            string assetPath = $"{DataFolder}/Encounter_PhaseValidation.asset";
            EncounterSpawnData asset = AssetDatabase.LoadAssetAtPath<EncounterSpawnData>(assetPath);
            if (asset != null)
            {
                SerializedObject fixup = new SerializedObject(asset);
                fixup.FindProperty("pooledEnemyPrefab").objectReferenceValue = enemyPrefab;
                fixup.ApplyModifiedPropertiesWithoutUndo();
                return asset;
            }

            EncounterSpawnData created = ScriptableObject.CreateInstance<EncounterSpawnData>();
            AssetDatabase.CreateAsset(created, assetPath);
            SerializedObject serializedObject = new SerializedObject(created);
            serializedObject.FindProperty("pooledEnemyPrefab").objectReferenceValue = enemyPrefab;
            serializedObject.FindProperty("enemyCount").intValue = 4;
            serializedObject.FindProperty("spawnIntervalSeconds").floatValue = 1.1f;
            serializedObject.FindProperty("spawnRadius").floatValue = 6.0f;
            serializedObject.FindProperty("spawnCenterWorldOffset").vector3Value = new Vector3(0.0f, 0.0f, 12.0f);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return created;
        }

        private static QuestDefinition EnsureQuestDefinitionAsset()
        {
            string assetPath = $"{DataFolder}/Quest_PhaseValidation.asset";
            QuestDefinition asset = AssetDatabase.LoadAssetAtPath<QuestDefinition>(assetPath);
            if (asset != null)
            {
                return asset;
            }

            QuestDefinition created = ScriptableObject.CreateInstance<QuestDefinition>();
            AssetDatabase.CreateAsset(created, assetPath);
            SerializedObject serializedObject = new SerializedObject(created);
            serializedObject.FindProperty("questId").stringValue = "demo_clear_pooled_enemies";
            serializedObject.FindProperty("targetPooledEnemyKills").intValue = 4;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return created;
        }

        private static void EnsureRpgServicesInScene(QuestDefinition questDefinition)
        {
            GameObject rpgRoot = new GameObject("RpgSystems");
            rpgRoot.AddComponent<WorldStateService>();
            QuestService questService = rpgRoot.AddComponent<QuestService>();
            SetPrivateField(questService, "demoQuest", questDefinition);
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            System.Collections.Generic.List<EditorBuildSettingsScene> buildScenes = EditorBuildSettings.scenes.ToList();
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

            UnityEngine.Object.DestroyImmediate(targetComponent);
        }

        /// <summary>
        /// 한국어 주석: reflection 의존을 줄이기 위해 UnityEngine.Object 파생 타입에는 SerializedFieldUtility를 우선 사용합니다.
        /// </summary>
        private static void SetPrivateField<TTarget>(TTarget targetObject, string fieldName, object value)
        {
            if (targetObject is UnityEngine.Object unityObject)
            {
                SerializedFieldUtility.SetField(unityObject, fieldName, value);
                return;
            }

            FieldInfo targetField = typeof(TTarget).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (targetField == null)
            {
                Debug.LogWarning($"[PhaseValidationScenePhases3Plus] 필드 연결 실패: {typeof(TTarget).Name}.{fieldName}");
                return;
            }

            targetField.SetValue(targetObject, value);
        }
    }
}
