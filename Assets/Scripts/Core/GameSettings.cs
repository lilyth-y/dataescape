using UnityEngine;
using System;

namespace MemoryFracture.Core
{
    /// <summary>
    /// 게임 설정 관리 시스템
    /// </summary>
    public class GameSettings : MonoBehaviour
    {
        [Header("Audio Settings")]
        public float masterVolume = 1f;
        public float musicVolume = 0.8f;
        public float sfxVolume = 0.8f;
        
        [Header("Graphics Settings")]
        public int qualityLevel = 2;
        public bool fullscreen = true;
        public int targetFrameRate = 60;
        
        [Header("Gameplay Settings")]
        public float mouseSensitivity = 2f;
        public bool invertMouseY = false;
        public float interactionRange = 3f;
        
        [Header("Network Settings")]
        public string serverAddress = "localhost";
        public int serverPort = 7777;
        public bool autoConnect = true;
        
        // Events
        public static event Action<float> OnMasterVolumeChanged;
        public static event Action<float> OnMusicVolumeChanged;
        public static event Action<float> OnSFXVolumeChanged;
        public static event Action<int> OnQualityLevelChanged;
        public static event Action<bool> OnFullscreenChanged;
        
        // Singleton
        public static GameSettings Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadSettings();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            ApplySettings();
        }
        
        /// <summary>
        /// 설정 로드
        /// </summary>
        public void LoadSettings()
        {
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
            
            qualityLevel = PlayerPrefs.GetInt("QualityLevel", 2);
            fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            targetFrameRate = PlayerPrefs.GetInt("TargetFrameRate", 60);
            
            mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 2f);
            invertMouseY = PlayerPrefs.GetInt("InvertMouseY", 0) == 1;
            interactionRange = PlayerPrefs.GetFloat("InteractionRange", 3f);
            
            serverAddress = PlayerPrefs.GetString("ServerAddress", "localhost");
            serverPort = PlayerPrefs.GetInt("ServerPort", 7777);
            autoConnect = PlayerPrefs.GetInt("AutoConnect", 1) == 1;
        }
        
        /// <summary>
        /// 설정 저장
        /// </summary>
        public void SaveSettings()
        {
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            
            PlayerPrefs.SetInt("QualityLevel", qualityLevel);
            PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);
            PlayerPrefs.SetInt("TargetFrameRate", targetFrameRate);
            
            PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivity);
            PlayerPrefs.SetInt("InvertMouseY", invertMouseY ? 1 : 0);
            PlayerPrefs.SetFloat("InteractionRange", interactionRange);
            
            PlayerPrefs.SetString("ServerAddress", serverAddress);
            PlayerPrefs.SetInt("ServerPort", serverPort);
            PlayerPrefs.SetInt("AutoConnect", autoConnect ? 1 : 0);
            
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// 설정 적용
        /// </summary>
        public void ApplySettings()
        {
            // 품질 설정 적용
            QualitySettings.SetQualityLevel(qualityLevel);
            
            // 전체화면 설정 적용
            Screen.fullScreen = fullscreen;
            
            // 프레임레이트 설정 적용
            Application.targetFrameRate = targetFrameRate;
            
            // 이벤트 발생
            OnMasterVolumeChanged?.Invoke(masterVolume);
            OnMusicVolumeChanged?.Invoke(musicVolume);
            OnSFXVolumeChanged?.Invoke(sfxVolume);
            OnQualityLevelChanged?.Invoke(qualityLevel);
            OnFullscreenChanged?.Invoke(fullscreen);
        }
        
        /// <summary>
        /// 마스터 볼륨 설정
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            OnMasterVolumeChanged?.Invoke(masterVolume);
            SaveSettings();
        }
        
        /// <summary>
        /// 음악 볼륨 설정
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            OnMusicVolumeChanged?.Invoke(musicVolume);
            SaveSettings();
        }
        
        /// <summary>
        /// 효과음 볼륨 설정
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            OnSFXVolumeChanged?.Invoke(sfxVolume);
            SaveSettings();
        }
        
        /// <summary>
        /// 품질 레벨 설정
        /// </summary>
        public void SetQualityLevel(int level)
        {
            qualityLevel = Mathf.Clamp(level, 0, QualitySettings.names.Length - 1);
            QualitySettings.SetQualityLevel(qualityLevel);
            OnQualityLevelChanged?.Invoke(qualityLevel);
            SaveSettings();
        }
        
        /// <summary>
        /// 전체화면 설정
        /// </summary>
        public void SetFullscreen(bool fullscreen)
        {
            this.fullscreen = fullscreen;
            Screen.fullScreen = fullscreen;
            OnFullscreenChanged?.Invoke(fullscreen);
            SaveSettings();
        }
        
        /// <summary>
        /// 마우스 감도 설정
        /// </summary>
        public void SetMouseSensitivity(float sensitivity)
        {
            mouseSensitivity = Mathf.Clamp(sensitivity, 0.1f, 10f);
            SaveSettings();
        }
        
        /// <summary>
        /// 마우스 Y축 반전 설정
        /// </summary>
        public void SetInvertMouseY(bool invert)
        {
            invertMouseY = invert;
            SaveSettings();
        }
        
        /// <summary>
        /// 상호작용 범위 설정
        /// </summary>
        public void SetInteractionRange(float range)
        {
            interactionRange = Mathf.Clamp(range, 1f, 10f);
            SaveSettings();
        }
        
        /// <summary>
        /// 서버 주소 설정
        /// </summary>
        public void SetServerAddress(string address)
        {
            serverAddress = address;
            SaveSettings();
        }
        
        /// <summary>
        /// 서버 포트 설정
        /// </summary>
        public void SetServerPort(int port)
        {
            serverPort = Mathf.Clamp(port, 1024, 65535);
            SaveSettings();
        }
        
        /// <summary>
        /// 자동 연결 설정
        /// </summary>
        public void SetAutoConnect(bool auto)
        {
            autoConnect = auto;
            SaveSettings();
        }
        
        /// <summary>
        /// 설정 초기화
        /// </summary>
        public void ResetToDefaults()
        {
            masterVolume = 1f;
            musicVolume = 0.8f;
            sfxVolume = 0.8f;
            
            qualityLevel = 2;
            fullscreen = true;
            targetFrameRate = 60;
            
            mouseSensitivity = 2f;
            invertMouseY = false;
            interactionRange = 3f;
            
            serverAddress = "localhost";
            serverPort = 7777;
            autoConnect = true;
            
            ApplySettings();
            SaveSettings();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveSettings();
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                SaveSettings();
            }
        }
    }
}