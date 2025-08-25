using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using MemoryFracture.Core;
using MemoryFracture.Networking;

namespace MemoryFracture.UI
{
    /// <summary>
    /// 게임 UI 관리 시스템
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Main UI Panels")]
        public GameObject mainMenuPanel;
        public GameObject gamePanel;
        public GameObject pausePanel;
        public GameObject settingsPanel;
        public GameObject chatPanel;
        public GameObject puzzlePanel;
        public GameObject endingPanel;
        
        [Header("HUD Elements")]
        public TextMeshProUGUI gameTimeText;
        public TextMeshProUGUI chapterText;
        public TextMeshProUGUI playerStatusText;
        public Slider[] flagSliders;
        public TextMeshProUGUI[] flagTexts;
        
        [Header("Chat System")]
        public TMP_InputField chatInput;
        public ScrollRect chatScrollRect;
        public GameObject chatMessagePrefab;
        public Transform chatContent;
        public Button sendButton;
        
        [Header("Puzzle UI")]
        public GameObject puzzleHintPanel;
        public TextMeshProUGUI puzzleHintText;
        public Button hintButton;
        public Button skipButton;
        
        [Header("Network UI")]
        public GameObject connectionPanel;
        public TMP_InputField roomNameInput;
        public Button createRoomButton;
        public Button joinRoomButton;
        public TextMeshProUGUI connectionStatusText;
        public TextMeshProUGUI playerListText;
        
        [Header("Settings")]
        public Slider masterVolumeSlider;
        public Slider musicVolumeSlider;
        public Slider sfxVolumeSlider;
        public Toggle fullscreenToggle;
        public TMP_Dropdown qualityDropdown;
        public TMP_Dropdown languageDropdown;
        
        // Private variables
        private GameManager gameManager;
        private NetworkManager networkManager;
        private List<GameObject> chatMessages = new List<GameObject>();
        private bool isPaused = false;
        
        // Singleton
        public static UIManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeUI();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            gameManager = GameManager.Instance;
            networkManager = NetworkManager.Instance;
            
            if (gameManager != null)
            {
                // 이벤트 구독
                GameManager.OnFlagChanged += OnFlagChanged;
                GameManager.OnChapterChanged += OnChapterChanged;
                GameManager.OnEndingDetermined += OnEndingDetermined;
            }
            
            if (networkManager != null)
            {
                // 네트워크 이벤트 구독
                NetworkManager.OnConnectedToServer += OnConnectedToServer;
                NetworkManager.OnDisconnectedFromServer += OnDisconnectedFromServer;
                NetworkManager.OnPlayerJoined += OnPlayerJoined;
                NetworkManager.OnPlayerLeft += OnPlayerLeft;
                NetworkManager.OnMessageReceived += OnMessageReceived;
            }
            
            ShowMainMenu();
        }
        
        private void Update()
        {
            UpdateHUD();
            
            // ESC 키로 일시정지
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }
        
        /// <summary>
        /// UI 초기화
        /// </summary>
        private void InitializeUI()
        {
            // 버튼 이벤트 연결
            if (sendButton != null)
                sendButton.onClick.AddListener(SendChatMessage);
            
            if (hintButton != null)
                hintButton.onClick.AddListener(ShowHint);
            
            if (skipButton != null)
                skipButton.onClick.AddListener(SkipPuzzle);
            
            if (createRoomButton != null)
                createRoomButton.onClick.AddListener(CreateRoom);
            
            if (joinRoomButton != null)
                joinRoomButton.onClick.AddListener(JoinRoom);
            
            // 설정 초기화
            InitializeSettings();
        }
        
        /// <summary>
        /// 설정 초기화
        /// </summary>
        private void InitializeSettings()
        {
            // 볼륨 설정
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }
            
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }
            
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
            
            // 품질 설정
            if (qualityDropdown != null)
            {
                qualityDropdown.value = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
                qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            }
            
            // 언어 설정
            if (languageDropdown != null)
            {
                languageDropdown.value = PlayerPrefs.GetInt("Language", 0);
                languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
            }
        }
        
        /// <summary>
        /// HUD 업데이트
        /// </summary>
        private void UpdateHUD()
        {
            if (gameManager != null)
            {
                // 게임 시간 업데이트
                if (gameTimeText != null)
                {
                    int hours = Mathf.FloorToInt(gameManager.gameTime / 3600f);
                    int minutes = Mathf.FloorToInt((gameManager.gameTime % 3600f) / 60f);
                    int seconds = Mathf.FloorToInt(gameManager.gameTime % 60f);
                    gameTimeText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
                }
                
                // 챕터 정보 업데이트
                if (chapterText != null)
                {
                    chapterText.text = $"Chapter {gameManager.currentChapter}/5";
                }
                
                // 플래그 슬라이더 업데이트
                UpdateFlagSliders();
            }
        }
        
        /// <summary>
        /// 플래그 슬라이더 업데이트
        /// </summary>
        private void UpdateFlagSliders()
        {
            if (gameManager == null || flagSliders == null)
                return;
            
            if (flagSliders.Length >= 6)
            {
                flagSliders[0].value = gameManager.truthFlag / 10f;
                flagSliders[1].value = gameManager.oblivionFlag / 10f;
                flagSliders[2].value = gameManager.sacrificeFlag / 10f;
                flagSliders[3].value = gameManager.corruptionFlag / 10f;
                flagSliders[4].value = gameManager.trackerFlag / 10f;
                flagSliders[5].value = gameManager.cooperationTrustFlag / 10f;
            }
            
            if (flagTexts != null && flagTexts.Length >= 6)
            {
                flagTexts[0].text = $"진실: {gameManager.truthFlag}";
                flagTexts[1].text = $"망각: {gameManager.oblivionFlag}";
                flagTexts[2].text = $"희생: {gameManager.sacrificeFlag}";
                flagTexts[3].text = $"손상: {gameManager.corruptionFlag}";
                flagTexts[4].text = $"추적자: {gameManager.trackerFlag}";
                flagTexts[5].text = $"협동: {gameManager.cooperationTrustFlag}";
            }
        }
        
        /// <summary>
        /// 메인 메뉴 표시
        /// </summary>
        public void ShowMainMenu()
        {
            SetPanelActive(mainMenuPanel, true);
            SetPanelActive(gamePanel, false);
            SetPanelActive(pausePanel, false);
            SetPanelActive(settingsPanel, false);
            SetPanelActive(connectionPanel, false);
        }
        
        /// <summary>
        /// 게임 UI 표시
        /// </summary>
        public void ShowGameUI()
        {
            SetPanelActive(mainMenuPanel, false);
            SetPanelActive(gamePanel, true);
            SetPanelActive(pausePanel, false);
            SetPanelActive(settingsPanel, false);
            SetPanelActive(connectionPanel, false);
        }
        
        /// <summary>
        /// 일시정지 토글
        /// </summary>
        public void TogglePause()
        {
            if (gamePanel.activeSelf)
            {
                isPaused = !isPaused;
                SetPanelActive(pausePanel, isPaused);
                Time.timeScale = isPaused ? 0f : 1f;
            }
        }
        
        /// <summary>
        /// 설정 패널 표시
        /// </summary>
        public void ShowSettings()
        {
            SetPanelActive(settingsPanel, true);
        }
        
        /// <summary>
        /// 연결 패널 표시
        /// </summary>
        public void ShowConnectionPanel()
        {
            SetPanelActive(connectionPanel, true);
            UpdateConnectionStatus();
        }
        
        /// <summary>
        /// 채팅 패널 토글
        /// </summary>
        public void ToggleChat()
        {
            if (chatPanel != null)
            {
                chatPanel.SetActive(!chatPanel.activeSelf);
                if (chatPanel.activeSelf && chatInput != null)
                {
                    chatInput.Select();
                }
            }
        }
        
        /// <summary>
        /// 채팅 메시지 전송
        /// </summary>
        public void SendChatMessage()
        {
            if (chatInput != null && !string.IsNullOrEmpty(chatInput.text))
            {
                string message = chatInput.text;
                if (networkManager != null)
                {
                    networkManager.SendMessage(message);
                }
                chatInput.text = "";
            }
        }
        
        /// <summary>
        /// 힌트 표시
        /// </summary>
        public void ShowHint()
        {
            if (puzzleHintPanel != null)
            {
                puzzleHintPanel.SetActive(true);
                // 현재 퍼즐에 대한 힌트 표시
                if (puzzleHintText != null)
                {
                    puzzleHintText.text = "현재 퍼즐에 대한 힌트를 여기에 표시합니다.";
                }
            }
        }
        
        /// <summary>
        /// 퍼즐 스킵
        /// </summary>
        public void SkipPuzzle()
        {
            // 현재 활성화된 퍼즐 스킵
            Debug.Log("Puzzle skipped");
        }
        
        /// <summary>
        /// 방 생성
        /// </summary>
        public void CreateRoom()
        {
            if (roomNameInput != null && networkManager != null)
            {
                string roomName = roomNameInput.text;
                if (!string.IsNullOrEmpty(roomName))
                {
                    networkManager.CreateRoom(roomName);
                    ShowGameUI();
                }
            }
        }
        
        /// <summary>
        /// 방 참가
        /// </summary>
        public void JoinRoom()
        {
            if (roomNameInput != null && networkManager != null)
            {
                string roomName = roomNameInput.text;
                if (!string.IsNullOrEmpty(roomName))
                {
                    networkManager.JoinRoom(roomName);
                    ShowGameUI();
                }
            }
        }
        
        /// <summary>
        /// 연결 상태 업데이트
        /// </summary>
        private void UpdateConnectionStatus()
        {
            if (connectionStatusText != null && networkManager != null)
            {
                if (networkManager.isConnected)
                {
                    connectionStatusText.text = "연결됨";
                    connectionStatusText.color = Color.green;
                }
                else
                {
                    connectionStatusText.text = "연결 안됨";
                    connectionStatusText.color = Color.red;
                }
            }
            
            UpdatePlayerList();
        }
        
        /// <summary>
        /// 플레이어 목록 업데이트
        /// </summary>
        private void UpdatePlayerList()
        {
            if (playerListText != null && networkManager != null)
            {
                var players = networkManager.GetConnectedPlayers();
                string playerList = "플레이어 목록:\n";
                foreach (var player in players)
                {
                    playerList += $"- {player.playerName} ({player.playerId})\n";
                }
                playerListText.text = playerList;
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
        
        // 이벤트 핸들러들
        private void OnFlagChanged(int flagType)
        {
            UpdateFlagSliders();
        }
        
        private void OnChapterChanged(int chapter)
        {
            if (chapterText != null)
            {
                chapterText.text = $"Chapter {chapter}/5";
            }
        }
        
        private void OnEndingDetermined(EndingType ending)
        {
            ShowEnding(ending);
        }
        
        private void OnConnectedToServer()
        {
            UpdateConnectionStatus();
        }
        
        private void OnDisconnectedFromServer()
        {
            UpdateConnectionStatus();
        }
        
        private void OnPlayerJoined(string playerId)
        {
            UpdatePlayerList();
        }
        
        private void OnPlayerLeft(string playerId)
        {
            UpdatePlayerList();
        }
        
        private void OnMessageReceived(string message)
        {
            AddChatMessage(message);
        }
        
        /// <summary>
        /// 채팅 메시지 추가
        /// </summary>
        private void AddChatMessage(string message)
        {
            if (chatMessagePrefab != null && chatContent != null)
            {
                GameObject messageObj = Instantiate(chatMessagePrefab, chatContent);
                TextMeshProUGUI messageText = messageObj.GetComponent<TextMeshProUGUI>();
                if (messageText != null)
                {
                    messageText.text = message;
                }
                
                chatMessages.Add(messageObj);
                
                // 메시지가 너무 많으면 오래된 것 삭제
                if (chatMessages.Count > 50)
                {
                    Destroy(chatMessages[0]);
                    chatMessages.RemoveAt(0);
                }
                
                // 스크롤을 맨 아래로
                if (chatScrollRect != null)
                {
                    Canvas.ForceUpdateCanvases();
                    chatScrollRect.verticalNormalizedPosition = 0f;
                }
            }
        }
        
        /// <summary>
        /// 엔딩 표시
        /// </summary>
        private void ShowEnding(EndingType ending)
        {
            SetPanelActive(endingPanel, true);
            // 엔딩에 따른 UI 표시
            Debug.Log($"Ending: {ending}");
        }
        
        // 설정 변경 이벤트 핸들러들
        private void OnMasterVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("MasterVolume", value);
            // 오디오 매니저에 적용
        }
        
        private void OnMusicVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("MusicVolume", value);
            // 오디오 매니저에 적용
        }
        
        private void OnSFXVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("SFXVolume", value);
            // 오디오 매니저에 적용
        }
        
        private void OnQualityChanged(int index)
        {
            PlayerPrefs.SetInt("QualityLevel", index);
            QualitySettings.SetQualityLevel(index);
        }
        
        private void OnLanguageChanged(int index)
        {
            PlayerPrefs.SetInt("Language", index);
            // 언어 매니저에 적용
        }
        
        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (gameManager != null)
            {
                GameManager.OnFlagChanged -= OnFlagChanged;
                GameManager.OnChapterChanged -= OnChapterChanged;
                GameManager.OnEndingDetermined -= OnEndingDetermined;
            }
            
            if (networkManager != null)
            {
                NetworkManager.OnConnectedToServer -= OnConnectedToServer;
                NetworkManager.OnDisconnectedFromServer -= OnDisconnectedFromServer;
                NetworkManager.OnPlayerJoined -= OnPlayerJoined;
                NetworkManager.OnPlayerLeft -= OnPlayerLeft;
                NetworkManager.OnMessageReceived -= OnMessageReceived;
            }
        }
    }
}