using UnityEditor;
using UnityEngine;
using System.Linq;

namespace MemoryFracture.Editor
{
    /// <summary>
    /// Unity 빌드 자동화 도구
    /// </summary>
    public class BuildAutomation : EditorWindow
    {
        private string buildPath = "Builds/";
        private string version = "0.1.0";
        private bool includeDebug = false;

        [MenuItem("Memory Fracture/Build Automation")]
        public static void ShowWindow()
        {
            GetWindow<BuildAutomation>("Build Automation");
        }

        void OnGUI()
        {
            GUILayout.Label("Memory Fracture Build Automation", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // 빌드 설정
            GUILayout.Label("빌드 설정:", EditorStyles.boldLabel);
            buildPath = EditorGUILayout.TextField("빌드 경로:", buildPath);
            version = EditorGUILayout.TextField("버전:", version);
            includeDebug = EditorGUILayout.Toggle("디버그 포함:", includeDebug);

            GUILayout.Space(10);

            // 플랫폼별 빌드 버튼
            GUILayout.Label("빌드 플랫폼:", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Windows 빌드"))
            {
                BuildWindows();
            }

            if (GUILayout.Button("Android 빌드"))
            {
                BuildAndroid();
            }

            if (GUILayout.Button("iOS 빌드"))
            {
                BuildiOS();
            }

            GUILayout.Space(10);

            // 개발용 빌드
            GUILayout.Label("개발용 빌드:", EditorStyles.boldLabel);
            
            if (GUILayout.Button("개발용 Windows 빌드"))
            {
                BuildWindowsDevelopment();
            }

            if (GUILayout.Button("테스트 빌드"))
            {
                BuildTest();
            }

            GUILayout.Space(10);

            // 빌드 폴더 열기
            if (GUILayout.Button("빌드 폴더 열기"))
            {
                OpenBuildFolder();
            }
        }

        private void BuildWindows()
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = GetEnabledScenes();
            buildPlayerOptions.locationPathName = $"{buildPath}MemoryFracture_Windows_v{version}.exe";
            buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
            buildPlayerOptions.options = BuildOptions.None;
            
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildResult result = report.summary.result;
            
            if (result == BuildResult.Succeeded)
            {
                Debug.Log($"Windows 빌드 성공: {buildPlayerOptions.locationPathName}");
                EditorUtility.DisplayDialog("빌드 완료", "Windows 빌드가 완료되었습니다.", "확인");
            }
            else
            {
                Debug.LogError("Windows 빌드 실패");
                EditorUtility.DisplayDialog("빌드 실패", "Windows 빌드에 실패했습니다.", "확인");
            }
        }

        private void BuildAndroid()
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = GetEnabledScenes();
            buildPlayerOptions.locationPathName = $"{buildPath}MemoryFracture_Android_v{version}.apk";
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.options = BuildOptions.None;
            
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildResult result = report.summary.result;
            
            if (result == BuildResult.Succeeded)
            {
                Debug.Log($"Android 빌드 성공: {buildPlayerOptions.locationPathName}");
                EditorUtility.DisplayDialog("빌드 완료", "Android 빌드가 완료되었습니다.", "확인");
            }
            else
            {
                Debug.LogError("Android 빌드 실패");
                EditorUtility.DisplayDialog("빌드 실패", "Android 빌드에 실패했습니다.", "확인");
            }
        }

        private void BuildiOS()
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = GetEnabledScenes();
            buildPlayerOptions.locationPathName = $"{buildPath}MemoryFracture_iOS_v{version}";
            buildPlayerOptions.target = BuildTarget.iOS;
            buildPlayerOptions.options = BuildOptions.None;
            
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildResult result = report.summary.result;
            
            if (result == BuildResult.Succeeded)
            {
                Debug.Log($"iOS 빌드 성공: {buildPlayerOptions.locationPathName}");
                EditorUtility.DisplayDialog("빌드 완료", "iOS 빌드가 완료되었습니다.", "확인");
            }
            else
            {
                Debug.LogError("iOS 빌드 실패");
                EditorUtility.DisplayDialog("빌드 실패", "iOS 빌드에 실패했습니다.", "확인");
            }
        }

        private void BuildWindowsDevelopment()
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = GetEnabledScenes();
            buildPlayerOptions.locationPathName = $"{buildPath}MemoryFracture_Windows_Dev_v{version}.exe";
            buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
            buildPlayerOptions.options = BuildOptions.Development | BuildOptions.AllowDebugging;
            
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildResult result = report.summary.result;
            
            if (result == BuildResult.Succeeded)
            {
                Debug.Log($"개발용 Windows 빌드 성공: {buildPlayerOptions.locationPathName}");
                EditorUtility.DisplayDialog("빌드 완료", "개발용 Windows 빌드가 완료되었습니다.", "확인");
            }
            else
            {
                Debug.LogError("개발용 Windows 빌드 실패");
                EditorUtility.DisplayDialog("빌드 실패", "개발용 Windows 빌드에 실패했습니다.", "확인");
            }
        }

        private void BuildTest()
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = GetEnabledScenes();
            buildPlayerOptions.locationPathName = $"{buildPath}MemoryFracture_Test_v{version}.exe";
            buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
            buildPlayerOptions.options = BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler;
            
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildResult result = report.summary.result;
            
            if (result == BuildResult.Succeeded)
            {
                Debug.Log($"테스트 빌드 성공: {buildPlayerOptions.locationPathName}");
                EditorUtility.DisplayDialog("빌드 완료", "테스트 빌드가 완료되었습니다.", "확인");
            }
            else
            {
                Debug.LogError("테스트 빌드 실패");
                EditorUtility.DisplayDialog("빌드 실패", "테스트 빌드에 실패했습니다.", "확인");
            }
        }

        private void OpenBuildFolder()
        {
            string fullPath = System.IO.Path.GetFullPath(buildPath);
            if (System.IO.Directory.Exists(fullPath))
            {
                EditorUtility.RevealInFinder(fullPath);
            }
            else
            {
                EditorUtility.DisplayDialog("오류", "빌드 폴더가 존재하지 않습니다.", "확인");
            }
        }

        private static string[] GetEnabledScenes()
        {
            return EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
        }
    }
}