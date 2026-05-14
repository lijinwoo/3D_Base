using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SystemicOverload.Comparison
{
    /// <summary>
    /// MonoBehaviour vs ScriptableObject 비교 데모의 상태 갱신과 관찰 UI를 담당합니다.
    /// Scene에 미리 배치된 오브젝트를 참조하여 동작하며, 입력은 Input System(InputAction)으로 처리합니다.
    /// </summary>
    public sealed class ComparisonDemoController : MonoBehaviour
    {
        [Header("Shared ScriptableObject")]
        [SerializeField] private ComparisonEnemyConfigSO sharedGoblinConfig;

        [Header("Scene References")]
        [SerializeField] private MonoEnemyStatsComponent[] monoEnemies = new MonoEnemyStatsComponent[0];
        [SerializeField] private SoEnemyStatsComponent[] soEnemies = new SoEnemyStatsComponent[0];

        [Header("Input (Input System)")]
        [SerializeField] private float demoDamageAmount = 20.0f;
        [SerializeField] private float soMaxHealthDecreaseStep = 10.0f;
        [SerializeField] private float monoMaxHealthDecreaseStep = 10.0f;
        [SerializeField] private InputAction damageMonoAction = new InputAction("DamageMono", InputActionType.Button, "<Keyboard>/1");
        [SerializeField] private InputAction damageSoAction = new InputAction("DamageSo", InputActionType.Button, "<Keyboard>/2");
        [SerializeField] private InputAction decreaseSharedSoAction = new InputAction("DecreaseSharedSo", InputActionType.Button, "<Keyboard>/3");
        [SerializeField] private InputAction decreaseMonoOnlyAction = new InputAction("DecreaseMonoOnly", InputActionType.Button, "<Keyboard>/4");
        [SerializeField] private InputAction resetAction = new InputAction("ResetDemo", InputActionType.Button, "<Keyboard>/r");
        private readonly StringBuilder infoBuilder = new StringBuilder(512);

        private float initialSoMaxHealth;
        private bool hasCachedInitialSoValue;

        private void Awake()
        {
            NormalizeInspectorValues();
            TryLoadDefaultSharedConfig();
            CacheInitialSoData();
            ResolveSceneReferences();
        }

        private void OnEnable()
        {
            BindInputActions();
            EnableInputActions();
        }

        private void OnDisable()
        {
            DisableInputActions();
            UnbindInputActions();
        }

        private void OnGUI()
        {
            DrawInformationPanel();
        }

        private void OnValidate()
        {
            NormalizeInspectorValues();
        }

        /// <summary>
        /// 입력 액션 콜백을 연결합니다.
        /// </summary>
        private void BindInputActions()
        {
            damageMonoAction.performed += OnDamageMonoActionPerformed;
            damageSoAction.performed += OnDamageSoActionPerformed;
            decreaseSharedSoAction.performed += OnDecreaseSharedSoActionPerformed;
            decreaseMonoOnlyAction.performed += OnDecreaseMonoOnlyActionPerformed;
            resetAction.performed += OnResetActionPerformed;
        }

        /// <summary>
        /// 입력 액션 콜백 연결을 해제합니다.
        /// </summary>
        private void UnbindInputActions()
        {
            damageMonoAction.performed -= OnDamageMonoActionPerformed;
            damageSoAction.performed -= OnDamageSoActionPerformed;
            decreaseSharedSoAction.performed -= OnDecreaseSharedSoActionPerformed;
            decreaseMonoOnlyAction.performed -= OnDecreaseMonoOnlyActionPerformed;
            resetAction.performed -= OnResetActionPerformed;
        }

        /// <summary>
        /// 입력 액션을 활성화합니다.
        /// </summary>
        private void EnableInputActions()
        {
            damageMonoAction.Enable();
            damageSoAction.Enable();
            decreaseSharedSoAction.Enable();
            decreaseMonoOnlyAction.Enable();
            resetAction.Enable();
        }

        /// <summary>
        /// 입력 액션을 비활성화합니다.
        /// </summary>
        private void DisableInputActions()
        {
            resetAction.Disable();
            decreaseMonoOnlyAction.Disable();
            decreaseSharedSoAction.Disable();
            damageSoAction.Disable();
            damageMonoAction.Disable();
        }

        /// <summary>
        /// 사전 배치된 Scene 참조가 비어 있으면 자동으로 탐색합니다.
        /// </summary>
        private void ResolveSceneReferences()
        {
            if (monoEnemies == null || monoEnemies.Length == 0)
            {
                monoEnemies = FindObjectsByType<MonoEnemyStatsComponent>(FindObjectsSortMode.None);
            }

            if (soEnemies == null || soEnemies.Length == 0)
            {
                soEnemies = FindObjectsByType<SoEnemyStatsComponent>(FindObjectsSortMode.None);
            }
        }

        private void OnDamageMonoActionPerformed(InputAction.CallbackContext context)
        {
            DamageFirstMonoEnemy();
        }

        private void OnDamageSoActionPerformed(InputAction.CallbackContext context)
        {
            DamageFirstSoEnemy();
        }

        private void OnDecreaseSharedSoActionPerformed(InputAction.CallbackContext context)
        {
            DecreaseSharedSoMaxHealth();
        }

        private void OnDecreaseMonoOnlyActionPerformed(InputAction.CallbackContext context)
        {
            DecreaseFirstMonoMaxHealthOnly();
        }

        private void OnResetActionPerformed(InputAction.CallbackContext context)
        {
            ResetDemoState();
        }

        private void DamageFirstMonoEnemy()
        {
            if (monoEnemies == null || monoEnemies.Length <= 0 || monoEnemies[0] == null)
            {
                return;
            }

            monoEnemies[0].ApplyDamage(demoDamageAmount);
        }

        private void DamageFirstSoEnemy()
        {
            if (soEnemies == null || soEnemies.Length <= 0 || soEnemies[0] == null)
            {
                return;
            }

            soEnemies[0].ApplyDamage(demoDamageAmount);
        }

        /// <summary>
        /// 공유 ScriptableObject의 최대 체력을 직접 낮춰, 참조 객체 전체에 미치는 영향을 노출합니다.
        /// </summary>
        private void DecreaseSharedSoMaxHealth()
        {
            if (sharedGoblinConfig == null)
            {
                return;
            }

            float nextMaxHealth = sharedGoblinConfig.MaxHealth - soMaxHealthDecreaseStep;
            sharedGoblinConfig.SetMaxHealthForDemo(nextMaxHealth);

            if (soEnemies == null)
            {
                return;
            }

            for (int index = 0; index < soEnemies.Length; index++)
            {
                SoEnemyStatsComponent soEnemy = soEnemies[index];
                if (soEnemy == null)
                {
                    continue;
                }

                soEnemy.ClampCurrentHealthToSharedMax();
            }
        }

        /// <summary>
        /// Mono 그룹 첫 번째 객체의 로컬 최대 체력만 낮춰 개별 상태임을 보여줍니다.
        /// </summary>
        private void DecreaseFirstMonoMaxHealthOnly()
        {
            if (monoEnemies == null || monoEnemies.Length <= 0 || monoEnemies[0] == null)
            {
                return;
            }

            float nextMaxHealth = monoEnemies[0].MaxHealth - monoMaxHealthDecreaseStep;
            monoEnemies[0].SetLocalMaxHealth(nextMaxHealth);
        }

        /// <summary>
        /// 데모 상태를 초기값으로 복원합니다.
        /// </summary>
        private void ResetDemoState()
        {
            if (sharedGoblinConfig != null && hasCachedInitialSoValue)
            {
                sharedGoblinConfig.SetMaxHealthForDemo(initialSoMaxHealth);
            }

            if (monoEnemies != null)
            {
                for (int index = 0; index < monoEnemies.Length; index++)
                {
                    MonoEnemyStatsComponent monoEnemy = monoEnemies[index];
                    if (monoEnemy != null)
                    {
                        monoEnemy.ResetRuntimeState();
                    }
                }
            }

            if (soEnemies != null)
            {
                for (int index = 0; index < soEnemies.Length; index++)
                {
                    SoEnemyStatsComponent soEnemy = soEnemies[index];
                    if (soEnemy != null)
                    {
                        soEnemy.ResetRuntimeState();
                    }
                }
            }
        }

        private void CacheInitialSoData()
        {
            if (sharedGoblinConfig == null)
            {
                hasCachedInitialSoValue = false;
                initialSoMaxHealth = 0.0f;
                return;
            }

            hasCachedInitialSoValue = true;
            initialSoMaxHealth = sharedGoblinConfig.MaxHealth;
        }

        /// <summary>
        /// Inspector 참조가 비어 있을 때 기본 경로의 에셋을 자동 로딩합니다.
        /// </summary>
        private void TryLoadDefaultSharedConfig()
        {
            if (sharedGoblinConfig != null)
            {
                return;
            }

            sharedGoblinConfig = Resources.Load<ComparisonEnemyConfigSO>("Comparison/GoblinConfig");
        }

        private void DrawInformationPanel()
        {
            GUI.Box(new Rect(12.0f, 12.0f, 760.0f, 260.0f), string.Empty);

            infoBuilder.Clear();
            infoBuilder.AppendLine("MonoBehaviour vs ScriptableObject 비교 데모");
            infoBuilder.AppendLine("1: Mono 첫 오브젝트 데미지 / 2: SO 첫 오브젝트 데미지");
            infoBuilder.AppendLine("3: SO 원본 MaxHealth 감소(공유 위험) / 4: Mono 첫 오브젝트 MaxHealth 감소(개별)");
            infoBuilder.AppendLine("R: 전체 리셋");
            infoBuilder.AppendLine();
            infoBuilder.AppendLine(GetMonoSummaryText());
            infoBuilder.AppendLine(GetSoSummaryText());
            infoBuilder.AppendLine(GetSharedSoText());

            GUI.Label(new Rect(24.0f, 24.0f, 740.0f, 240.0f), infoBuilder.ToString());
        }

        private string GetMonoSummaryText()
        {
            if (monoEnemies == null || monoEnemies.Length <= 0 || monoEnemies[0] == null)
            {
                return "Mono Group: 데이터 없음";
            }

            MonoEnemyStatsComponent first = monoEnemies[0];
            return $"Mono Group(첫 객체) - MaxHealth: {first.MaxHealth:0.0}, CurrentHealth: {first.CurrentHealth:0.0}, AttackPower: {first.AttackPower:0.0}";
        }

        private string GetSoSummaryText()
        {
            if (soEnemies == null || soEnemies.Length <= 0 || soEnemies[0] == null)
            {
                return "SO Group: 데이터 없음";
            }

            SoEnemyStatsComponent first = soEnemies[0];
            return $"SO Group(첫 객체) - MaxHealth: {first.MaxHealth:0.0}, CurrentHealth: {first.CurrentHealth:0.0}, AttackPower: {first.AttackPower:0.0}";
        }

        private string GetSharedSoText()
        {
            if (sharedGoblinConfig == null)
            {
                return "Shared SO Asset: 미할당";
            }

            return $"현재 SO Asset 값 - EnemyId: {sharedGoblinConfig.EnemyId}, MaxHealth: {sharedGoblinConfig.MaxHealth:0.0}, AttackPower: {sharedGoblinConfig.AttackPower:0.0}";
        }

        private void NormalizeInspectorValues()
        {
            demoDamageAmount = Mathf.Max(0.0f, demoDamageAmount);
            soMaxHealthDecreaseStep = Mathf.Max(1.0f, soMaxHealthDecreaseStep);
            monoMaxHealthDecreaseStep = Mathf.Max(1.0f, monoMaxHealthDecreaseStep);
        }
    }
}
