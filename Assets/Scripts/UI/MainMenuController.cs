using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MemoryFracture.Core;

namespace MemoryFracture.UI
{
    /// <summary>
    /// 메인 메뉴 컨트롤러
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Menu Panels")]
        public GameObject mainPanel;
        public GameObject settingsPanel;
        public GameObject creditsPanel;
        public GameObject multiplayerPanel;
        
        [Header("Main Menu Buttons")]
        public Button startGameButton;
        public Button multiplayerButton;
        public Button settingsButton;
        public Button creditsButton;
        public Button quitButton;
        
        [Header("Settings UI")]
        public Slider masterVolumeSlider;
        public Slider musicVolumeSlider;
        public Slider sfxVolumeSlider;
        public TMP_Dropdown qualityDropdown;
        public Toggle fullscreenToggle;
        public Button settingsBackButton;
        
        [Header("Multiplayer UI")]
        public TMP_InputField roomNameInput;
        public Button createRoomButton;
        public Button joinRoomButton;
        public Button multiplayerBackButton;
        
        [Header("Credits UI")]
        public Button creditsBackButton;
        
        private GameInitializer gameInitializer;
        
        private void Start()
        {
            gameInitializer = FindObjectOfType<GameInitializer>();
            if (gameInitializer == null)
            {
                gameInitializer = new GameObject("GameInitializer").AddComponent<GameInitializer>();
            }
            
            InitializeUI();
            ShowMainPanel();
        }
        
        /// <summary>
        /// UI 초기화
        /// </summary>
        private void InitializeUI()
        {
            // 메인 메뉴 버튼 이벤트 연결
            if (startGameButton != null)
                startGameButton.onClick.AddListener(StartGame);
            
            if (multiplayerButton != null)
                multiplayerButton.onClick.AddListener(ShowMultiplayerPanel);
            
            if (settingsButton != null)
                settingsButton.onClick.AddListener(ShowSettingsPanel);
            
            if (creditsButton != null)
                creditsButton.onClick.AddListener(ShowCreditsPanel);
            
            if (quitButton != null)
                quitButton.onClick.AddListener(QuitGame);
            
            // 설정 UI 이벤트 연결
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            
            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            
            if (qualityDropdown != null)
                qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            
            if (fullscreenToggle != null)
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            
            if (settingsBackButton != null)
                settingsBackButton.onClick.AddListener(ShowMainPanel);
            
            // 멀티플레이어 UI 이벤트 연결
            if (createRoomButton != null)
                createRoomButton.onClick.AddListener(CreateRoom);
            
            if (joinRoomButton != null)
                joinRoomButton.onClick.AddListener(JoinRoom);
            
            if (multiplayerBackButton != null)
                multiplayerBackButton.onClick.AddListener(ShowMainPanel);
            
            // 크레딧 UI 이벤트 연결
            if (creditsBackButton != null)
                creditsBackButton.onClick.AddListener(ShowMainPanel);
            
            // 설정값 로드
            LoadSettings();
        }
        
        /// <summary>
        /// 메인 패널 표시
        /// </summary>
        public void ShowMainPanel()
        {
            SetPanelActive(mainPanel, true);
            SetPanelActive(settingsPanel, false);
            SetPanelActive(creditsPanel, false);
            SetPanelActive(multiplayerPanel, false);
        }
        
        /// <summary>
        /// 설정 패널 표시
        /// </summary>
        public void ShowSettingsPanel()
        {
            SetPanelActive(mainPanel, false);
            SetPanelActive(settingsPanel, true);
            SetPanelActive(creditsPanel, false);
            SetPanelActive(multiplayerPanel, false);
        }
        
        /// <summary>
        /// 크레딧 패널 표시
        /// </summary>
        public void ShowCreditsPanel()
        {
            SetPanelActive(mainPanel, false);
            SetPanelActive(settingsPanel, false);
            SetPanelActive(creditsPanel, true);
            SetPanelActive(multiplayerPanel, false);
        }
        
        /// <summary>
        /// 멀티플레이어 패널 표시
        /// </summary>
        public void ShowMultiplayerPanel()
        {
            SetPanelActive(mainPanel, false);
            SetPanelActive(settingsPanel, false);
            SetPanelActive(creditsPanel, false);
            SetPanelActive(multiplayerPanel, true);
        }
        
        /// <summary>
        /// 게임 시작
        /// </summary>
        public void StartGame()
        {
            if (gameInitializer != null)
            {
                gameInitializer.StartGame();
            }
        }
        
        /// <summary>
        /// 방 생성
        /// </summary>
        public void CreateRoom()
        {
            if (roomNameInput != null && !string.IsNullOrEmpty(roomNameInput.text))
            {
                string roomName = roomNameInput.text;
                // NetworkManager를 통해 방 생성
                NetworkManager networkManager = NetworkManager.Instance;
                if (networkManager != null)
                {
                    networkManager.CreateRoom(roomName);
                    StartGame();
                }
            }
        }
        
        /// <summary>
        /// 방 참가
        /// </summary>
        public void JoinRoom()
        {
            if (roomNameInput != null && !string.IsNullOrEmpty(roomNameInput.text))
            {
                string roomName = roomNameInput.text;
                // NetworkManager를 통해 방 참가
                NetworkManager networkManager = NetworkManager.Instance;
                if (networkManager != null)
                {
                    networkManager.JoinRoom(roomName);
                    StartGame();
                }
            }
        }
        
        /// <summary>
        /// 게임 종료
        /// </summary>
        public void QuitGame()
        {
            if (gameInitializer != null)
            {
                gameInitializer.QuitGame();
            }
        }
        
        /// <summary>
        /// 설정 로드
        /// </summary>
        private void LoadSettings()
        {
            GameSettings settings = GameSettings.Instance;
            if (settings != null)
            {
                if (masterVolumeSlider != null)
                    masterVolumeSlider.value = settings.masterVolume;
                
                if (musicVolumeSlider != null)
                    musicVolumeSlider.value = settings.musicVolume;
                
                if (sfxVolumeSlider != null)
                    sfxVolumeSlider.value = settings.sfxVolume;
                
                if (qualityDropdown != null)
                    qualityDropdown.value = settings.qualityLevel;
                
                if (fullscreenToggle != null)
                    fullscreenToggle.isOn = settings.fullscreen;
            }
        }
        
        // 설정 변경 이벤트 핸들러들
        private void OnMasterVolumeChanged(float value)
        {
            GameSettings settings = GameSettings.Instance;
            if (settings != null)
            {
                settings.SetMasterVolume(value);
            }
        }
        
        private void OnMusicVolumeChanged(float value)
        {
            GameSettings settings = GameSettings.Instance;
            if (settings != null)
            {
                settings.SetMusicVolume(value);
            }
        }
        
        private void OnSFXVolumeChanged(float value)
        {
            GameSettings settings = GameSettings.Instance;
            if (settings != null)
            {
                settings.SetSFXVolume(value);
            }
        }
        
        private void OnQualityChanged(int index)
        {
            GameSettings settings = GameSettings.Instance;
            if (settings != null)
            {
                settings.SetQualityLevel(index);
            }
        }
        
        private void OnFullscreenChanged(bool fullscreen)
        {
            GameSettings settings = GameSettings.Instance;
            if (settings != null)
            {
                settings.SetFullscreen(fullscreen);
            }
        }
        
        /// <summary>
        /// 패널 활성화 설정
        /// </summary>
        private void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
            {
                panel.SetActive(active);
            }
        }
    }
}