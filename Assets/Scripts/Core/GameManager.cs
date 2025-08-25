using System.Collections.Generic;
using UnityEngine;
using System;

namespace MemoryFracture.Core
{
    /// <summary>
    /// 게임의 핵심 관리 시스템
    /// 플래그, 엔딩, 챕터 관리를 담당
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Game Settings")]
        public bool isMultiplayer = true;
        public float gameTime = 0f;
        public int currentChapter = 1;
        
        [Header("Flag System")]
        public int truthFlag = 0;
        public int oblivionFlag = 0;
        public int sacrificeFlag = 0;
        public int corruptionFlag = 0;
        public int trackerFlag = 0;
        public int cooperationTrustFlag = 0;
        
        [Header("Puzzle Progress")]
        public List<string> completedPuzzles = new List<string>();
        public List<string> failedPuzzles = new List<string>();
        public List<string> hiddenPuzzles = new List<string>();
        
        [Header("Player Data")]
        public PlayerData playerA;
        public PlayerData playerB;
        
        // Events
        public static event Action<int> OnFlagChanged;
        public static event Action<int> OnChapterChanged;
        public static event Action<EndingType> OnEndingDetermined;
        
        // Singleton
        public static GameManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGame();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            // 게임 시작 시 초기화
            LoadGameData();
        }
        
        private void Update()
        {
            // 게임 시간 업데이트
            gameTime += Time.deltaTime;
            
            // 플래그 기반 엔딩 체크
            CheckEndingConditions();
        }
        
        /// <summary>
        /// 게임 초기화
        /// </summary>
        private void InitializeGame()
        {
            playerA = new PlayerData("PlayerA");
            playerB = new PlayerData("PlayerB");
            
            ResetFlags();
            ResetPuzzleProgress();
        }
        
        /// <summary>
        /// 플래그 초기화
        /// </summary>
        public void ResetFlags()
        {
            truthFlag = 0;
            oblivionFlag = 0;
            sacrificeFlag = 0;
            corruptionFlag = 0;
            trackerFlag = 0;
            cooperationTrustFlag = 0;
        }
        
        /// <summary>
        /// 퍼즐 진행도 초기화
        /// </summary>
        public void ResetPuzzleProgress()
        {
            completedPuzzles.Clear();
            failedPuzzles.Clear();
            hiddenPuzzles.Clear();
        }
        
        /// <summary>
        /// 플래그 변경
        /// </summary>
        public void ModifyFlag(FlagType flagType, int amount)
        {
            switch (flagType)
            {
                case FlagType.Truth:
                    truthFlag = Mathf.Clamp(truthFlag + amount, 0, 10);
                    break;
                case FlagType.Oblivion:
                    oblivionFlag = Mathf.Clamp(oblivionFlag + amount, 0, 10);
                    break;
                case FlagType.Sacrifice:
                    sacrificeFlag = Mathf.Clamp(sacrificeFlag + amount, 0, 10);
                    break;
                case FlagType.Corruption:
                    corruptionFlag = Mathf.Clamp(corruptionFlag + amount, 0, 10);
                    break;
                case FlagType.Tracker:
                    trackerFlag = Mathf.Clamp(trackerFlag + amount, 0, 10);
                    break;
                case FlagType.CooperationTrust:
                    cooperationTrustFlag = Mathf.Clamp(cooperationTrustFlag + amount, 0, 10);
                    break;
            }
            
            OnFlagChanged?.Invoke((int)flagType);
        }
        
        /// <summary>
        /// 퍼즐 완료 처리
        /// </summary>
        public void CompletePuzzle(string puzzleId, bool isHidden = false)
        {
            if (!completedPuzzles.Contains(puzzleId))
            {
                completedPuzzles.Add(puzzleId);
                
                if (isHidden)
                {
                    hiddenPuzzles.Add(puzzleId);
                }
            }
        }
        
        /// <summary>
        /// 퍼즐 실패 처리
        /// </summary>
        public void FailPuzzle(string puzzleId)
        {
            if (!failedPuzzles.Contains(puzzleId))
            {
                failedPuzzles.Add(puzzleId);
            }
        }
        
        /// <summary>
        /// 챕터 변경
        /// </summary>
        public void ChangeChapter(int newChapter)
        {
            if (newChapter >= 1 && newChapter <= 5)
            {
                currentChapter = newChapter;
                OnChapterChanged?.Invoke(currentChapter);
            }
        }
        
        /// <summary>
        /// 엔딩 조건 체크
        /// </summary>
        private void CheckEndingConditions()
        {
            EndingType ending = DetermineEnding();
            if (ending != EndingType.None)
            {
                OnEndingDetermined?.Invoke(ending);
            }
        }
        
        /// <summary>
        /// 엔딩 결정
        /// </summary>
        public EndingType DetermineEnding()
        {
            // 현실 귀환 (진엔딩)
            if (truthFlag >= 3 && corruptionFlag < 5)
            {
                return EndingType.RealityReturn;
            }
            
            // 거짓된 기억
            if (oblivionFlag >= 3)
            {
                return EndingType.FalseMemory;
            }
            
            // 희생 엔딩
            if (sacrificeFlag >= 2 && truthFlag == 0)
            {
                return EndingType.Sacrifice;
            }
            
            // 공존 엔딩 (히든)
            if (completedPuzzles.Count >= 15 && hiddenPuzzles.Count >= 3)
            {
                return EndingType.Coexistence;
            }
            
            // 루프 엔딩
            if (corruptionFlag >= 5)
            {
                return EndingType.Loop;
            }
            
            // 추적자 승리
            if (trackerFlag >= 2)
            {
                return EndingType.TrackerVictory;
            }
            
            return EndingType.None;
        }
        
        /// <summary>
        /// 게임 데이터 저장
        /// </summary>
        public void SaveGameData()
        {
            GameData data = new GameData
            {
                gameTime = gameTime,
                currentChapter = currentChapter,
                truthFlag = truthFlag,
                oblivionFlag = oblivionFlag,
                sacrificeFlag = sacrificeFlag,
                corruptionFlag = corruptionFlag,
                trackerFlag = trackerFlag,
                cooperationTrustFlag = cooperationTrustFlag,
                completedPuzzles = completedPuzzles.ToArray(),
                failedPuzzles = failedPuzzles.ToArray(),
                hiddenPuzzles = hiddenPuzzles.ToArray()
            };
            
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("GameData", json);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// 게임 데이터 로드
        /// </summary>
        public void LoadGameData()
        {
            if (PlayerPrefs.HasKey("GameData"))
            {
                string json = PlayerPrefs.GetString("GameData");
                GameData data = JsonUtility.FromJson<GameData>(json);
                
                gameTime = data.gameTime;
                currentChapter = data.currentChapter;
                truthFlag = data.truthFlag;
                oblivionFlag = data.oblivionFlag;
                sacrificeFlag = data.sacrificeFlag;
                corruptionFlag = data.corruptionFlag;
                trackerFlag = data.trackerFlag;
                cooperationTrustFlag = data.cooperationTrustFlag;
                
                completedPuzzles = new List<string>(data.completedPuzzles);
                failedPuzzles = new List<string>(data.failedPuzzles);
                hiddenPuzzles = new List<string>(data.hiddenPuzzles);
            }
        }
        
        /// <summary>
        /// 게임 데이터 삭제
        /// </summary>
        public void DeleteGameData()
        {
            PlayerPrefs.DeleteKey("GameData");
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// 게임 재시작
        /// </summary>
        public void RestartGame()
        {
            ResetFlags();
            ResetPuzzleProgress();
            currentChapter = 1;
            gameTime = 0f;
            SaveGameData();
        }
    }
    
    /// <summary>
    /// 플래그 타입
    /// </summary>
    public enum FlagType
    {
        Truth,
        Oblivion,
        Sacrifice,
        Corruption,
        Tracker,
        CooperationTrust
    }
    
    /// <summary>
    /// 엔딩 타입
    /// </summary>
    public enum EndingType
    {
        None,
        RealityReturn,
        FalseMemory,
        Sacrifice,
        Coexistence,
        Loop,
        TrackerVictory
    }
    
    /// <summary>
    /// 플레이어 데이터
    /// </summary>
    [System.Serializable]
    public class PlayerData
    {
        public string playerId;
        public Vector3 position;
        public bool isHuman; // 정체성 (게임 끝까지 비공개)
        public List<string> discoveredClues = new List<string>();
        
        public PlayerData(string id)
        {
            playerId = id;
            position = Vector3.zero;
            isHuman = UnityEngine.Random.Range(0, 2) == 0; // 랜덤 정체성
        }
    }
    
    /// <summary>
    /// 게임 데이터 (저장용)
    /// </summary>
    [System.Serializable]
    public class GameData
    {
        public float gameTime;
        public int currentChapter;
        public int truthFlag;
        public int oblivionFlag;
        public int sacrificeFlag;
        public int corruptionFlag;
        public int trackerFlag;
        public int cooperationTrustFlag;
        public string[] completedPuzzles;
        public string[] failedPuzzles;
        public string[] hiddenPuzzles;
    }
}