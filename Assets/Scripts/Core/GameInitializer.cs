using UnityEngine;
using UnityEngine.SceneManagement;
using MemoryFracture.UI;

namespace MemoryFracture.Core
{
    /// <summary>
    /// 게임 초기화 및 시작 관리
    /// </summary>
    public class GameInitializer : MonoBehaviour
    {
        [Header("Game Settings")]
        public string mainMenuScene = "MainMenu";
        public string firstChapterScene = "Chapter1_MirrorCorridor";
        public bool startInMainMenu = true;
        
        [Header("Managers")]
        public GameObject gameManagerPrefab;
        public GameObject networkManagerPrefab;
        public GameObject uiManagerPrefab;
        
        private void Awake()
        {
            // 싱글톤 매니저들 초기화
            InitializeManagers();
            
            // 씬 설정
            if (startInMainMenu)
            {
                LoadMainMenu();
            }
            else
            {
                StartGame();
            }
        }
        
        /// <summary>
        /// 매니저들 초기화
        /// </summary>
        private void InitializeManagers()
        {
            // GameManager 초기화
            if (GameManager.Instance == null && gameManagerPrefab != null)
            {
                Instantiate(gameManagerPrefab);
            }
            
            // NetworkManager 초기화
            if (NetworkManager.Instance == null && networkManagerPrefab != null)
            {
                Instantiate(networkManagerPrefab);
            }
            
            // UIManager 초기화
            if (UIManager.Instance == null && uiManagerPrefab != null)
            {
                Instantiate(uiManagerPrefab);
            }
        }
        
        /// <summary>
        /// 메인 메뉴 로드
        /// </summary>
        public void LoadMainMenu()
        {
            SceneManager.LoadScene(mainMenuScene);
        }
        
        /// <summary>
        /// 게임 시작
        /// </summary>
        public void StartGame()
        {
            // 게임 데이터 초기화
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }
            
            // 첫 번째 챕터 로드
            SceneManager.LoadScene(firstChapterScene);
        }
        
        /// <summary>
        /// 특정 챕터 로드
        /// </summary>
        public void LoadChapter(int chapterNumber)
        {
            string sceneName = $"Chapter{chapterNumber}_";
            
            switch (chapterNumber)
            {
                case 1:
                    sceneName += "MirrorCorridor";
                    break;
                case 2:
                    sceneName += "DataArchive";
                    break;
                case 3:
                    sceneName += "IllusionForest";
                    break;
                case 4:
                    sceneName += "TrackerJudgment";
                    break;
                case 5:
                    sceneName += "CoreMemory";
                    break;
                default:
                    Debug.LogError($"Unknown chapter: {chapterNumber}");
                    return;
            }
            
            SceneManager.LoadScene(sceneName);
        }
        
        /// <summary>
        /// 게임 종료
        /// </summary>
        public void QuitGame()
        {
            // 게임 데이터 저장
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SaveGameData();
            }
            
            // 네트워크 연결 해제
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.DisconnectFromServer();
            }
            
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        
        /// <summary>
        /// 설정 저장
        /// </summary>
        public void SaveSettings()
        {
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// 설정 로드
        /// </summary>
        public void LoadSettings()
        {
            // 기본 설정값들
            if (!PlayerPrefs.HasKey("MasterVolume"))
                PlayerPrefs.SetFloat("MasterVolume", 1f);
            
            if (!PlayerPrefs.HasKey("MusicVolume"))
                PlayerPrefs.SetFloat("MusicVolume", 0.8f);
            
            if (!PlayerPrefs.HasKey("SFXVolume"))
                PlayerPrefs.SetFloat("SFXVolume", 0.8f);
            
            if (!PlayerPrefs.HasKey("QualityLevel"))
                PlayerPrefs.SetInt("QualityLevel", QualitySettings.GetQualityLevel());
            
            if (!PlayerPrefs.HasKey("Language"))
                PlayerPrefs.SetInt("Language", 0);
        }
        
        private void Start()
        {
            // 설정 로드
            LoadSettings();
            
            // UI 매니저에게 초기화 완료 알림
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMainMenu();
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // 게임 일시정지 시 데이터 저장
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SaveGameData();
                }
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                // 게임 포커스 잃을 때 데이터 저장
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SaveGameData();
                }
            }
        }
    }
}