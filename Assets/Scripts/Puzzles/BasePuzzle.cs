using UnityEngine;
using System;
using MemoryFracture.Core;

namespace MemoryFracture.Puzzles
{
    /// <summary>
    /// 모든 퍼즐의 기본 클래스
    /// </summary>
    public abstract class BasePuzzle : MonoBehaviour
    {
        [Header("Puzzle Settings")]
        public string puzzleId;
        public string puzzleName;
        public int difficulty = 1;
        public bool isRequired = true;
        public bool isHidden = false;
        public float timeLimit = 0f; // 0이면 무제한
        
        [Header("Flag Rewards")]
        public int truthReward = 0;
        public int oblivionPenalty = 0;
        public int sacrificeReward = 0;
        public int corruptionPenalty = 0;
        public int trackerPenalty = 0;
        public int cooperationReward = 0;
        
        [Header("Puzzle State")]
        public bool isCompleted = false;
        public bool isFailed = false;
        public float currentTime = 0f;
        public bool isActive = false;
        
        // Events
        public static event Action<string> OnPuzzleCompleted;
        public static event Action<string> OnPuzzleFailed;
        public static event Action<string> OnPuzzleStarted;
        
        protected GameManager gameManager;
        protected bool isInitialized = false;
        
        protected virtual void Awake()
        {
            gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogError("GameManager not found!");
                return;
            }
            
            InitializePuzzle();
        }
        
        protected virtual void Start()
        {
            if (!isInitialized)
            {
                InitializePuzzle();
            }
        }
        
        protected virtual void Update()
        {
            if (isActive && timeLimit > 0)
            {
                UpdateTimer();
            }
        }
        
        /// <summary>
        /// 퍼즐 초기화
        /// </summary>
        protected virtual void InitializePuzzle()
        {
            if (string.IsNullOrEmpty(puzzleId))
            {
                puzzleId = gameObject.name;
            }
            
            // 이미 완료된 퍼즐인지 확인
            if (gameManager.completedPuzzles.Contains(puzzleId))
            {
                isCompleted = true;
                OnPuzzleAlreadyCompleted();
            }
            
            isInitialized = true;
        }
        
        /// <summary>
        /// 퍼즐 시작
        /// </summary>
        public virtual void StartPuzzle()
        {
            if (isCompleted || isFailed)
                return;
                
            isActive = true;
            currentTime = 0f;
            OnPuzzleStarted?.Invoke(puzzleId);
            OnPuzzleStart();
        }
        
        /// <summary>
        /// 퍼즐 완료
        /// </summary>
        protected virtual void CompletePuzzle()
        {
            if (isCompleted)
                return;
                
            isCompleted = true;
            isActive = false;
            
            // 플래그 보상 지급
            if (truthReward > 0)
                gameManager.ModifyFlag(FlagType.Truth, truthReward);
            if (cooperationReward > 0)
                gameManager.ModifyFlag(FlagType.CooperationTrust, cooperationReward);
            if (sacrificeReward > 0)
                gameManager.ModifyFlag(FlagType.Sacrifice, sacrificeReward);
                
            // 퍼즐 완료 기록
            gameManager.CompletePuzzle(puzzleId, isHidden);
            
            OnPuzzleCompleted?.Invoke(puzzleId);
            OnPuzzleComplete();
        }
        
        /// <summary>
        /// 퍼즐 실패
        /// </summary>
        protected virtual void FailPuzzle()
        {
            if (isFailed)
                return;
                
            isFailed = true;
            isActive = false;
            
            // 플래그 페널티 적용
            if (oblivionPenalty > 0)
                gameManager.ModifyFlag(FlagType.Oblivion, oblivionPenalty);
            if (corruptionPenalty > 0)
                gameManager.ModifyFlag(FlagType.Corruption, corruptionPenalty);
            if (trackerPenalty > 0)
                gameManager.ModifyFlag(FlagType.Tracker, trackerPenalty);
                
            // 퍼즐 실패 기록
            gameManager.FailPuzzle(puzzleId);
            
            OnPuzzleFailed?.Invoke(puzzleId);
            OnPuzzleFail();
        }
        
        /// <summary>
        /// 타이머 업데이트
        /// </summary>
        protected virtual void UpdateTimer()
        {
            currentTime += Time.deltaTime;
            
            if (timeLimit > 0 && currentTime >= timeLimit)
            {
                OnTimeLimitReached();
            }
        }
        
        /// <summary>
        /// 시간 제한 도달
        /// </summary>
        protected virtual void OnTimeLimitReached()
        {
            FailPuzzle();
        }
        
        /// <summary>
        /// 퍼즐 재시작
        /// </summary>
        public virtual void RestartPuzzle()
        {
            isCompleted = false;
            isFailed = false;
            isActive = false;
            currentTime = 0f;
            
            OnPuzzleRestart();
        }
        
        /// <summary>
        /// 퍼즐 리셋
        /// </summary>
        public virtual void ResetPuzzle()
        {
            isCompleted = false;
            isFailed = false;
            isActive = false;
            currentTime = 0f;
            
            OnPuzzleReset();
        }
        
        /// <summary>
        /// 힌트 제공
        /// </summary>
        public virtual void ProvideHint()
        {
            OnHintProvided();
        }
        
        /// <summary>
        /// 퍼즐 스킵 (개발용)
        /// </summary>
        [ContextMenu("Skip Puzzle")]
        public virtual void SkipPuzzle()
        {
            if (!isCompleted && !isFailed)
            {
                CompletePuzzle();
            }
        }
        
        // 추상 메서드들 - 하위 클래스에서 구현
        protected abstract void OnPuzzleStart();
        protected abstract void OnPuzzleComplete();
        protected abstract void OnPuzzleFail();
        protected abstract void OnPuzzleRestart();
        protected abstract void OnPuzzleReset();
        protected abstract void OnHintProvided();
        protected abstract void OnPuzzleAlreadyCompleted();
        
        // 가상 메서드들 - 필요시 오버라이드
        protected virtual void OnTriggerEnter(Collider other)
        {
            // 플레이어가 퍼즐 영역에 들어왔을 때 자동 시작
            if (other.CompareTag("Player") && !isCompleted && !isFailed && !isActive)
            {
                StartPuzzle();
            }
        }
        
        protected virtual void OnTriggerExit(Collider other)
        {
            // 플레이어가 퍼즐 영역을 벗어났을 때
            if (other.CompareTag("Player"))
            {
                OnPlayerExitPuzzleArea();
            }
        }
        
        protected virtual void OnPlayerExitPuzzleArea()
        {
            // 기본 구현은 비어있음
        }
        
        // 디버그 정보
        protected virtual void OnGUI()
        {
            if (isActive && timeLimit > 0)
            {
                GUI.Label(new Rect(10, 10, 200, 20), $"Time: {timeLimit - currentTime:F1}");
            }
        }
        
        // Gizmos for debugging
        protected virtual void OnDrawGizmos()
        {
            if (isActive)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(transform.position, Vector3.one);
            }
            else if (isCompleted)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(transform.position, Vector3.one);
            }
            else if (isFailed)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(transform.position, Vector3.one);
            }
        }
    }
    
    /// <summary>
    /// 협동 퍼즐 기본 클래스
    /// </summary>
    public abstract class CooperativePuzzle : BasePuzzle
    {
        [Header("Cooperation Settings")]
        public bool requiresBothPlayers = true;
        public bool requiresSimultaneousAction = false;
        public float cooperationTimeWindow = 2f; // 동시 행동 허용 시간
        
        protected bool playerAReady = false;
        protected bool playerBReady = false;
        protected float lastPlayerActionTime = 0f;
        
        /// <summary>
        /// 플레이어 준비 상태 설정
        /// </summary>
        public virtual void SetPlayerReady(string playerId, bool ready)
        {
            if (playerId == "PlayerA")
            {
                playerAReady = ready;
            }
            else if (playerId == "PlayerB")
            {
                playerBReady = ready;
            }
            
            CheckCooperationCondition();
        }
        
        /// <summary>
        /// 협동 조건 확인
        /// </summary>
        protected virtual void CheckCooperationCondition()
        {
            if (requiresBothPlayers)
            {
                if (playerAReady && playerBReady)
                {
                    if (requiresSimultaneousAction)
                    {
                        // 동시 행동이 필요한 경우
                        if (Time.time - lastPlayerActionTime <= cooperationTimeWindow)
                        {
                            OnCooperationSuccess();
                        }
                        else
                        {
                            lastPlayerActionTime = Time.time;
                        }
                    }
                    else
                    {
                        // 순차적 행동이 가능한 경우
                        OnCooperationSuccess();
                    }
                }
            }
            else
            {
                // 한 명만 필요한 경우
                if (playerAReady || playerBReady)
                {
                    OnCooperationSuccess();
                }
            }
        }
        
        /// <summary>
        /// 협동 성공
        /// </summary>
        protected virtual void OnCooperationSuccess()
        {
            // 협동 보상 추가
            if (cooperationReward > 0)
            {
                gameManager.ModifyFlag(FlagType.CooperationTrust, cooperationReward);
            }
            
            CompletePuzzle();
        }
        
        /// <summary>
        /// 협동 실패
        /// </summary>
        protected virtual void OnCooperationFail()
        {
            FailPuzzle();
        }
        
        protected override void OnPuzzleRestart()
        {
            playerAReady = false;
            playerBReady = false;
            lastPlayerActionTime = 0f;
        }
        
        protected override void OnPuzzleReset()
        {
            playerAReady = false;
            playerBReady = false;
            lastPlayerActionTime = 0f;
        }
    }
}