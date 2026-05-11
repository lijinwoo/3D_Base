using System.IO;
using NUnit.Framework;
using UnityEditor;

namespace SystemicOverload.Tests.EditMode
{
    /// <summary>
    /// 한국어 주석: RPG 수직 슬라이스 검증 씬 경로(Phase_0N_<Feature>Validation.unity)를 검증해
    /// 에디터 빌드 메뉴와 EditorBuildSettings 간의 정합성을 회귀 테스트로 보장합니다.
    /// </summary>
    public sealed class PhaseValidationScenePathRuleTests
    {
        private const string PhaseValidationFolder = "Assets/01.Scenes/PhaseValidation";
        private static readonly System.Text.RegularExpressions.Regex PhaseScenePattern =
            new System.Text.RegularExpressions.Regex(@"^Phase_\d{2}_[A-Za-z0-9]+Validation\.unity$");

        [Test]
        public void PhaseValidationFolder_Exists()
        {
            Assert.IsTrue(AssetDatabase.IsValidFolder(PhaseValidationFolder),
                $"PhaseValidation 폴더가 존재해야 합니다: {PhaseValidationFolder}");
        }

        [Test]
        public void AllPhaseValidationScenes_FollowNamingRule()
        {
            string absoluteFolder = Path.Combine(System.IO.Directory.GetCurrentDirectory(), PhaseValidationFolder);
            if (!System.IO.Directory.Exists(absoluteFolder))
            {
                Assert.Inconclusive("PhaseValidation 폴더가 비어 있어 명명 규칙 검증을 생략합니다.");
                return;
            }

            string[] sceneFiles = System.IO.Directory.GetFiles(absoluteFolder, "*.unity", System.IO.SearchOption.TopDirectoryOnly);
            foreach (string scenePath in sceneFiles)
            {
                string fileName = Path.GetFileName(scenePath);
                Assert.IsTrue(PhaseScenePattern.IsMatch(fileName),
                    $"PhaseValidation scene 명명 규칙 위반: {fileName} (기대: Phase_0N_<Feature>Validation.unity)");
            }
        }

        [Test]
        public void EditorBuildSettings_ScenesExistOnDisk()
        {
            EditorBuildSettingsScene[] registeredScenes = EditorBuildSettings.scenes;
            foreach (EditorBuildSettingsScene scene in registeredScenes)
            {
                if (string.IsNullOrEmpty(scene.path))
                {
                    continue;
                }

                Assert.IsTrue(System.IO.File.Exists(scene.path),
                    $"BuildSettings에 등록된 scene이 디스크에 없습니다: {scene.path}");
            }
        }
    }
}
