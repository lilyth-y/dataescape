using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;

namespace MemoryFracture.Editor
{
    /// <summary>
    /// Unity 에디터에서 Git 통합을 위한 도구
    /// </summary>
    public class GitIntegration : EditorWindow
    {
        private string commitMessage = "";
        private bool showGitStatus = false;
        private Vector2 scrollPosition;

        [MenuItem("Memory Fracture/Git Integration")]
        public static void ShowWindow()
        {
            GetWindow<GitIntegration>("Git Integration");
        }

        void OnGUI()
        {
            GUILayout.Label("Git Integration Tool", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // Git 상태 확인
            if (GUILayout.Button("Git Status 확인"))
            {
                showGitStatus = true;
                CheckGitStatus();
            }

            if (showGitStatus)
            {
                GUILayout.Label("Git Status:", EditorStyles.boldLabel);
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
                // Git 상태 정보 표시
                EditorGUILayout.EndScrollView();
            }

            GUILayout.Space(10);

            // 커밋 메시지 입력
            GUILayout.Label("커밋 메시지:");
            commitMessage = EditorGUILayout.TextField(commitMessage);

            // 커밋 및 푸시
            if (GUILayout.Button("커밋 및 푸시"))
            {
                CommitAndPush();
            }

            GUILayout.Space(10);

            // GitHub 저장소 열기
            if (GUILayout.Button("GitHub 저장소 열기"))
            {
                OpenGitHubRepository();
            }

            // Unity에서 Git 설정 열기
            if (GUILayout.Button("Unity Git 설정 열기"))
            {
                OpenUnityGitSettings();
            }
        }

        private void CheckGitStatus()
        {
            string projectPath = Application.dataPath.Replace("/Assets", "");
            string gitStatus = ExecuteGitCommand(projectPath, "status");
            UnityEngine.Debug.Log("Git Status:\n" + gitStatus);
        }

        private void CommitAndPush()
        {
            if (string.IsNullOrEmpty(commitMessage))
            {
                EditorUtility.DisplayDialog("오류", "커밋 메시지를 입력해주세요.", "확인");
                return;
            }

            string projectPath = Application.dataPath.Replace("/Assets", "");
            
            // Git add
            ExecuteGitCommand(projectPath, "add .");
            
            // Git commit
            ExecuteGitCommand(projectPath, $"commit -m \"{commitMessage}\"");
            
            // Git push
            ExecuteGitCommand(projectPath, "push origin main");

            EditorUtility.DisplayDialog("완료", "커밋 및 푸시가 완료되었습니다.", "확인");
            commitMessage = "";
        }

        private void OpenGitHubRepository()
        {
            Application.OpenURL("https://github.com/lilyth-y/dataescape");
        }

        private void OpenUnityGitSettings()
        {
            EditorWindow.GetWindow<UnityEditor.VersionControl.GitSettingsWindow>();
        }

        private string ExecuteGitCommand(string workingDirectory, string arguments)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "git";
                startInfo.Arguments = arguments;
                startInfo.WorkingDirectory = workingDirectory;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.CreateNoWindow = true;

                using (Process process = Process.Start(startInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(error))
                    {
                        UnityEngine.Debug.LogError("Git Error: " + error);
                    }

                    return output;
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError("Git command failed: " + e.Message);
                return "";
            }
        }
    }
}