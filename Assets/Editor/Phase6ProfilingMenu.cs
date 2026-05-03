using UnityEditor;
using UnityEngine;

namespace SystemicOverload.EditorTools
{
    /// <summary>
    /// 한국어 주석: Phase 6 프로파일링 워크플로를 돕는 최소 에디터 메뉴입니다.
    /// </summary>
    public static class Phase6ProfilingMenu
    {
        [MenuItem("Tools/Systemic Overload/Phase 6/Open Unity Profiler Window")]
        public static void OpenProfilerWindow()
        {
            EditorApplication.ExecuteMenuItem("Window/Analysis/Profiler");
            Debug.Log("[Phase6ProfilingMenu] Profiler 창을 열었습니다. Phase6_Profiling_Checklist.md를 함께 사용하세요.");
        }
    }
}
