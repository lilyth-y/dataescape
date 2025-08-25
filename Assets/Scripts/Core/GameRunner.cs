using UnityEngine;
using UnityEngine.SceneManagement;

namespace MemoryFracture.Core
{
    /// <summary>
    /// 게임 실행 및 테스트를 위한 간단한 러너
    /// </summary>
    public class GameRunner : MonoBehaviour
    {
        [Header("Game Settings")]
        public bool autoStart = true;
        public bool showDebugInfo = true;
        
        private GameBuilder gameBuilder;
        private bool gameStarted = false;
        
        private void Start()
        {
            if (autoStart)
            {
                StartGame();
            }
        }
        
        private void Update()
        {
            // ESC 키로 일시정지
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
            
            // F1 키로 게임 재시작
            if (Input.GetKeyDown(KeyCode.F1))
            {
                RestartGame();
            }
            
            // F2 키로 디버그 정보 토글
            if (Input.GetKeyDown(KeyCode.F2))
            {
                showDebugInfo = !showDebugInfo;
            }
        }
        
        /// <summary>
        /// 게임 시작
        /// </summary>
        public void StartGame()
        {
            if (gameStarted) return;
            
            Debug.Log("=== Memory Fracture 게임 시작 ===");
            
            // 게임 빌더 찾기 또는 생성
            gameBuilder = FindObjectOfType<GameBuilder>();
            if (gameBuilder == null)
            {
                GameObject builderObj = new GameObject("GameBuilder");
                gameBuilder = builderObj.AddComponent<GameBuilder>();
            }
            
            // 게임 환경 구축
            gameBuilder.BuildCompleteGame();
            
            gameStarted = true;
            
            Debug.Log("게임이 성공적으로 시작되었습니다!");
            Debug.Log("조작법:");
            Debug.Log("- WASD: 이동");
            Debug.Log("- 마우스: 시점 조작");
            Debug.Log("- E: 상호작용");
            Debug.Log("- ESC: 일시정지");
            Debug.Log("- F1: 게임 재시작");
            Debug.Log("- F2: 디버그 정보 토글");
        }
        
        /// <summary>
        /// 일시정지 토글
        /// </summary>
        public void TogglePause()
        {
            if (Time.timeScale > 0)
            {
                Time.timeScale = 0;
                Debug.Log("게임 일시정지");
            }
            else
            {
                Time.timeScale = 1;
                Debug.Log("게임 재개");
            }
        }
        
        /// <summary>
        /// 게임 재시작
        /// </summary>
        public void RestartGame()
        {
            Debug.Log("게임 재시작...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        /// <summary>
        /// 게임 종료
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("게임 종료");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        
        private void OnGUI()
        {
            if (!showDebugInfo) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.Label("=== Memory Fracture - 디버그 정보 ===");
            
            // 게임 상태 정보
            if (GameManager.Instance != null)
            {
                GUILayout.Label($"게임 시간: {GameManager.Instance.gameTime:F1}초");
                GUILayout.Label($"현재 챕터: {GameManager.Instance.currentChapter}/5");
                GUILayout.Label($"진실 플래그: {GameManager.Instance.truthFlag}");
                GUILayout.Label($"망각 플래그: {GameManager.Instance.oblivionFlag}");
                GUILayout.Label($"희생 플래그: {GameManager.Instance.sacrificeFlag}");
                GUILayout.Label($"손상 플래그: {GameManager.Instance.corruptionFlag}");
                GUILayout.Label($"추적자 플래그: {GameManager.Instance.trackerFlag}");
                GUILayout.Label($"협동 플래그: {GameManager.Instance.cooperationTrustFlag}");
            }
            
            GUILayout.Space(10);
            
            // 네트워크 상태
            if (NetworkManager.Instance != null)
            {
                GUILayout.Label($"네트워크 연결: {(NetworkManager.Instance.isConnected ? "연결됨" : "연결 안됨")}");
                GUILayout.Label($"방 이름: {NetworkManager.Instance.roomName}");
                GUILayout.Label($"호스트: {(NetworkManager.Instance.isHost ? "예" : "아니오")}");
            }
            
            GUILayout.Space(10);
            
            // 플레이어 정보
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                GUILayout.Label($"플레이어 위치: {player.transform.position}");
                GUILayout.Label($"플레이어 ID: {player.playerId}");
                GUILayout.Label($"로컬 플레이어: {(player.isLocalPlayer ? "예" : "아니오")}");
            }
            
            GUILayout.Space(10);
            
            // 퍼즐 정보
            MirrorReflectionPuzzle[] puzzles = FindObjectsOfType<MirrorReflectionPuzzle>();
            GUILayout.Label($"활성 퍼즐 수: {puzzles.Length}");
            foreach (var puzzle in puzzles)
            {
                GUILayout.Label($"- {puzzle.puzzleName}: {(puzzle.isCompleted ? "완료" : puzzle.isActive ? "진행중" : "대기중")}");
            }
            
            GUILayout.Space(10);
            
            // 버튼들
            if (GUILayout.Button("게임 재시작 (F1)"))
            {
                RestartGame();
            }
            
            if (GUILayout.Button("일시정지/재개 (ESC)"))
            {
                TogglePause();
            }
            
            if (GUILayout.Button("게임 종료"))
            {
                QuitGame();
            }
            
            GUILayout.EndArea();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Debug.Log("애플리케이션 일시정지");
            }
            else
            {
                Debug.Log("애플리케이션 재개");
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                Debug.Log("애플리케이션 포커스 해제");
            }
            else
            {
                Debug.Log("애플리케이션 포커스 획득");
            }
        }
    }
}