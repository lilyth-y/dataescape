using UnityEngine;
using MemoryFracture.UI;

namespace MemoryFracture.Core
{
    /// <summary>
    /// 테스트용 게임 설정 - Unity에서 바로 테스트할 수 있도록 도와줌
    /// </summary>
    public class TestGameSetup : MonoBehaviour
    {
        [Header("Test Settings")]
        public bool autoSetupOnStart = true;
        public bool createTestPuzzle = true;
        public bool createTestPlayer = true;
        
        [Header("Test Objects")]
        public GameObject testPlayerPrefab;
        public GameObject testPuzzlePrefab;
        public GameObject testUI;
        
        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupTestEnvironment();
            }
        }
        
        /// <summary>
        /// 테스트 환경 설정
        /// </summary>
        public void SetupTestEnvironment()
        {
            Debug.Log("테스트 환경 설정 시작...");
            
            // 1. 게임 매니저 생성
            CreateGameManager();
            
            // 2. 네트워크 매니저 생성
            CreateNetworkManager();
            
            // 3. UI 매니저 생성
            CreateUIManager();
            
            // 4. 게임 설정 생성
            CreateGameSettings();
            
            // 5. 테스트 플레이어 생성
            if (createTestPlayer)
            {
                CreateTestPlayer();
            }
            
            // 6. 테스트 퍼즐 생성
            if (createTestPuzzle)
            {
                CreateTestPuzzle();
            }
            
            Debug.Log("테스트 환경 설정 완료!");
        }
        
        /// <summary>
        /// 게임 매니저 생성
        /// </summary>
        private void CreateGameManager()
        {
            if (GameManager.Instance == null)
            {
                GameObject gameManagerObj = new GameObject("GameManager");
                gameManagerObj.AddComponent<GameManager>();
                Debug.Log("GameManager 생성됨");
            }
        }
        
        /// <summary>
        /// 네트워크 매니저 생성
        /// </summary>
        private void CreateNetworkManager()
        {
            if (NetworkManager.Instance == null)
            {
                GameObject networkManagerObj = new GameObject("NetworkManager");
                networkManagerObj.AddComponent<NetworkManager>();
                Debug.Log("NetworkManager 생성됨");
            }
        }
        
        /// <summary>
        /// UI 매니저 생성
        /// </summary>
        private void CreateUIManager()
        {
            if (UIManager.Instance == null)
            {
                GameObject uiManagerObj = new GameObject("UIManager");
                uiManagerObj.AddComponent<UIManager>();
                Debug.Log("UIManager 생성됨");
            }
        }
        
        /// <summary>
        /// 게임 설정 생성
        /// </summary>
        private void CreateGameSettings()
        {
            if (GameSettings.Instance == null)
            {
                GameObject gameSettingsObj = new GameObject("GameSettings");
                gameSettingsObj.AddComponent<GameSettings>();
                Debug.Log("GameSettings 생성됨");
            }
        }
        
        /// <summary>
        /// 테스트 플레이어 생성
        /// </summary>
        private void CreateTestPlayer()
        {
            if (testPlayerPrefab != null)
            {
                Vector3 spawnPosition = new Vector3(0, 1, 0);
                GameObject player = Instantiate(testPlayerPrefab, spawnPosition, Quaternion.identity);
                
                PlayerController controller = player.GetComponent<PlayerController>();
                if (controller != null)
                {
                    controller.SetPlayerId("TestPlayer");
                    controller.SetLocalPlayer(true);
                }
                
                Debug.Log("테스트 플레이어 생성됨");
            }
            else
            {
                // 기본 플레이어 생성
                GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                player.name = "TestPlayer";
                player.transform.position = new Vector3(0, 1, 0);
                
                // 플레이어 컨트롤러 추가
                player.AddComponent<CharacterController>();
                PlayerController controller = player.AddComponent<PlayerController>();
                controller.SetPlayerId("TestPlayer");
                controller.SetLocalPlayer(true);
                
                Debug.Log("기본 테스트 플레이어 생성됨");
            }
        }
        
        /// <summary>
        /// 테스트 퍼즐 생성
        /// </summary>
        private void CreateTestPuzzle()
        {
            if (testPuzzlePrefab != null)
            {
                Vector3 puzzlePosition = new Vector3(5, 0, 0);
                GameObject puzzle = Instantiate(testPuzzlePrefab, puzzlePosition, Quaternion.identity);
                Debug.Log("테스트 퍼즐 생성됨");
            }
            else
            {
                // 기본 거울 반사 퍼즐 생성
                GameObject puzzle = new GameObject("TestMirrorPuzzle");
                puzzle.transform.position = new Vector3(5, 0, 0);
                
                MirrorReflectionPuzzle mirrorPuzzle = puzzle.AddComponent<MirrorReflectionPuzzle>();
                
                // 거울 오브젝트 생성
                GameObject mirror = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mirror.name = "Mirror";
                mirror.transform.SetParent(puzzle.transform);
                mirror.transform.localPosition = Vector3.zero;
                mirror.transform.localScale = new Vector3(2, 1, 0.1f);
                
                // 빛 소스 생성
                GameObject lightSource = new GameObject("LightSource");
                lightSource.transform.SetParent(puzzle.transform);
                lightSource.transform.localPosition = new Vector3(-3, 0, 0);
                
                Light light = lightSource.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1f;
                
                // 목표물 생성
                GameObject target = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                target.name = "Target";
                target.transform.SetParent(puzzle.transform);
                target.transform.localPosition = new Vector3(3, 0, 0);
                target.transform.localScale = Vector3.one * 0.5f;
                
                // 퍼즐 설정
                mirrorPuzzle.mirrors = new Transform[] { mirror.transform };
                mirrorPuzzle.lightSource = lightSource.transform;
                mirrorPuzzle.target = target.transform;
                
                Debug.Log("기본 거울 반사 퍼즐 생성됨");
            }
        }
        
        /// <summary>
        /// 테스트 UI 생성
        /// </summary>
        public void CreateTestUI()
        {
            if (testUI != null)
            {
                Instantiate(testUI);
                Debug.Log("테스트 UI 생성됨");
            }
        }
        
        /// <summary>
        /// 모든 테스트 오브젝트 제거
        /// </summary>
        public void CleanupTestEnvironment()
        {
            // 테스트 오브젝트들 찾아서 제거
            GameObject[] testObjects = GameObject.FindGameObjectsWithTag("TestObject");
            foreach (GameObject obj in testObjects)
            {
                DestroyImmediate(obj);
            }
            
            Debug.Log("테스트 환경 정리 완료");
        }
        
        /// <summary>
        /// 게임 테스트 실행
        /// </summary>
        public void RunGameTest()
        {
            Debug.Log("게임 테스트 시작...");
            
            // 게임 매니저 테스트
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ModifyFlag(FlagType.Truth, 1);
                Debug.Log($"진실 플래그: {GameManager.Instance.truthFlag}");
            }
            
            // 퍼즐 테스트
            MirrorReflectionPuzzle[] puzzles = FindObjectsOfType<MirrorReflectionPuzzle>();
            foreach (MirrorReflectionPuzzle puzzle in puzzles)
            {
                puzzle.StartPuzzle();
                Debug.Log($"퍼즐 시작: {puzzle.puzzleName}");
            }
            
            Debug.Log("게임 테스트 완료!");
        }
        
        private void OnGUI()
        {
            // 테스트용 GUI
            GUILayout.BeginArea(new Rect(10, 10, 200, 300));
            GUILayout.Label("Memory Fracture - 테스트 도구");
            
            if (GUILayout.Button("테스트 환경 설정"))
            {
                SetupTestEnvironment();
            }
            
            if (GUILayout.Button("게임 테스트 실행"))
            {
                RunGameTest();
            }
            
            if (GUILayout.Button("테스트 UI 생성"))
            {
                CreateTestUI();
            }
            
            if (GUILayout.Button("테스트 환경 정리"))
            {
                CleanupTestEnvironment();
            }
            
            GUILayout.EndArea();
        }
    }
}