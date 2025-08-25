using UnityEngine;
using System;
using System.Collections.Generic;
using MemoryFracture.Core;

namespace MemoryFracture.Networking
{
    /// <summary>
    /// 멀티플레이어 네트워킹 관리 시스템
    /// </summary>
    public class NetworkManager : MonoBehaviour
    {
        [Header("Network Settings")]
        public string gameVersion = "1.0";
        public int maxPlayers = 2;
        public bool autoConnect = true;
        
        [Header("Connection Status")]
        public bool isConnected = false;
        public bool isHost = false;
        public string roomName = "";
        public string playerId = "";
        
        // Events
        public static event Action OnConnectedToServer;
        public static event Action OnDisconnectedFromServer;
        public static event Action<string> OnPlayerJoined;
        public static event Action<string> OnPlayerLeft;
        public static event Action<string> OnMessageReceived;
        public static event Action<Vector3> OnPlayerPositionUpdated;
        public static event Action<string, bool> OnPlayerActionPerformed;
        
        // Player data
        private Dictionary<string, PlayerNetworkData> connectedPlayers = new Dictionary<string, PlayerNetworkData>();
        private GameManager gameManager;
        
        // Singleton
        public static NetworkManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeNetwork();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            gameManager = GameManager.Instance;
            if (autoConnect)
            {
                ConnectToServer();
            }
        }
        
        /// <summary>
        /// 네트워크 초기화
        /// </summary>
        private void InitializeNetwork()
        {
            // 네트워크 설정 초기화
            playerId = GeneratePlayerId();
        }
        
        /// <summary>
        /// 서버 연결
        /// </summary>
        public void ConnectToServer()
        {
            try
            {
                // 실제 네트워크 라이브러리 연결 로직
                Debug.Log("Connecting to server...");
                isConnected = true;
                OnConnectedToServer?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to connect to server: {e.Message}");
            }
        }
        
        /// <summary>
        /// 서버 연결 해제
        /// </summary>
        public void DisconnectFromServer()
        {
            try
            {
                Debug.Log("Disconnecting from server...");
                isConnected = false;
                connectedPlayers.Clear();
                OnDisconnectedFromServer?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to disconnect from server: {e.Message}");
            }
        }
        
        /// <summary>
        /// 방 생성
        /// </summary>
        public void CreateRoom(string roomName)
        {
            if (!isConnected)
            {
                Debug.LogWarning("Not connected to server");
                return;
            }
            
            try
            {
                this.roomName = roomName;
                isHost = true;
                Debug.Log($"Created room: {roomName}");
                
                // 자신을 플레이어로 추가
                AddPlayer(playerId, "PlayerA");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create room: {e.Message}");
            }
        }
        
        /// <summary>
        /// 방 참가
        /// </summary>
        public void JoinRoom(string roomName)
        {
            if (!isConnected)
            {
                Debug.LogWarning("Not connected to server");
                return;
            }
            
            try
            {
                this.roomName = roomName;
                isHost = false;
                Debug.Log($"Joined room: {roomName}");
                
                // 자신을 플레이어로 추가
                AddPlayer(playerId, "PlayerB");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to join room: {e.Message}");
            }
        }
        
        /// <summary>
        /// 방 나가기
        /// </summary>
        public void LeaveRoom()
        {
            if (string.IsNullOrEmpty(roomName))
            {
                Debug.LogWarning("Not in a room");
                return;
            }
            
            try
            {
                Debug.Log($"Left room: {roomName}");
                roomName = "";
                isHost = false;
                connectedPlayers.Clear();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to leave room: {e.Message}");
            }
        }
        
        /// <summary>
        /// 플레이어 추가
        /// </summary>
        private void AddPlayer(string playerId, string playerName)
        {
            if (!connectedPlayers.ContainsKey(playerId))
            {
                PlayerNetworkData playerData = new PlayerNetworkData
                {
                    playerId = playerId,
                    playerName = playerName,
                    position = Vector3.zero,
                    isReady = false
                };
                
                connectedPlayers.Add(playerId, playerData);
                OnPlayerJoined?.Invoke(playerId);
            }
        }
        
        /// <summary>
        /// 플레이어 제거
        /// </summary>
        private void RemovePlayer(string playerId)
        {
            if (connectedPlayers.ContainsKey(playerId))
            {
                connectedPlayers.Remove(playerId);
                OnPlayerLeft?.Invoke(playerId);
            }
        }
        
        /// <summary>
        /// 메시지 전송
        /// </summary>
        public void SendMessage(string message)
        {
            if (!isConnected || string.IsNullOrEmpty(roomName))
            {
                Debug.LogWarning("Cannot send message: not connected or not in room");
                return;
            }
            
            try
            {
                NetworkMessage networkMessage = new NetworkMessage
                {
                    senderId = playerId,
                    messageType = MessageType.Chat,
                    content = message,
                    timestamp = Time.time
                };
                
                // 실제 네트워크 전송 로직
                Debug.Log($"Sent message: {message}");
                OnMessageReceived?.Invoke(message);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to send message: {e.Message}");
            }
        }
        
        /// <summary>
        /// 플레이어 위치 업데이트
        /// </summary>
        public void UpdatePlayerPosition(Vector3 position)
        {
            if (!isConnected)
                return;
                
            try
            {
                NetworkMessage positionMessage = new NetworkMessage
                {
                    senderId = playerId,
                    messageType = MessageType.Position,
                    position = position,
                    timestamp = Time.time
                };
                
                // 실제 네트워크 전송 로직
                OnPlayerPositionUpdated?.Invoke(position);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to update position: {e.Message}");
            }
        }
        
        /// <summary>
        /// 플레이어 액션 전송
        /// </summary>
        public void SendPlayerAction(string action, bool value)
        {
            if (!isConnected)
                return;
                
            try
            {
                NetworkMessage actionMessage = new NetworkMessage
                {
                    senderId = playerId,
                    messageType = MessageType.Action,
                    content = action,
                    boolValue = value,
                    timestamp = Time.time
                };
                
                // 실제 네트워크 전송 로직
                OnPlayerActionPerformed?.Invoke(action, value);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to send action: {e.Message}");
            }
        }
        
        /// <summary>
        /// 퍼즐 상태 동기화
        /// </summary>
        public void SyncPuzzleState(string puzzleId, bool isCompleted)
        {
            if (!isConnected)
                return;
                
            try
            {
                NetworkMessage puzzleMessage = new NetworkMessage
                {
                    senderId = playerId,
                    messageType = MessageType.PuzzleState,
                    content = puzzleId,
                    boolValue = isCompleted,
                    timestamp = Time.time
                };
                
                // 실제 네트워크 전송 로직
                Debug.Log($"Synced puzzle state: {puzzleId} = {isCompleted}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to sync puzzle state: {e.Message}");
            }
        }
        
        /// <summary>
        /// 플래그 상태 동기화
        /// </summary>
        public void SyncFlagState(FlagType flagType, int value)
        {
            if (!isConnected)
                return;
                
            try
            {
                NetworkMessage flagMessage = new NetworkMessage
                {
                    senderId = playerId,
                    messageType = MessageType.FlagState,
                    content = flagType.ToString(),
                    intValue = value,
                    timestamp = Time.time
                };
                
                // 실제 네트워크 전송 로직
                Debug.Log($"Synced flag state: {flagType} = {value}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to sync flag state: {e.Message}");
            }
        }
        
        /// <summary>
        /// 플레이어 ID 생성
        /// </summary>
        private string GeneratePlayerId()
        {
            return System.Guid.NewGuid().ToString().Substring(0, 8);
        }
        
        /// <summary>
        /// 연결된 플레이어 목록 가져오기
        /// </summary>
        public List<PlayerNetworkData> GetConnectedPlayers()
        {
            return new List<PlayerNetworkData>(connectedPlayers.Values);
        }
        
        /// <summary>
        /// 특정 플레이어 데이터 가져오기
        /// </summary>
        public PlayerNetworkData GetPlayerData(string playerId)
        {
            if (connectedPlayers.ContainsKey(playerId))
            {
                return connectedPlayers[playerId];
            }
            return null;
        }
        
        /// <summary>
        /// 플레이어 준비 상태 설정
        /// </summary>
        public void SetPlayerReady(bool ready)
        {
            if (connectedPlayers.ContainsKey(playerId))
            {
                connectedPlayers[playerId].isReady = ready;
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // 앱이 백그라운드로 갈 때 연결 유지
                Debug.Log("Application paused - maintaining connection");
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                // 앱이 포커스를 잃었을 때
                Debug.Log("Application lost focus");
            }
        }
        
        private void OnDestroy()
        {
            if (isConnected)
            {
                DisconnectFromServer();
            }
        }
    }
    
    /// <summary>
    /// 플레이어 네트워크 데이터
    /// </summary>
    [System.Serializable]
    public class PlayerNetworkData
    {
        public string playerId;
        public string playerName;
        public Vector3 position;
        public bool isReady;
        public float lastUpdateTime;
    }
    
    /// <summary>
    /// 네트워크 메시지
    /// </summary>
    [System.Serializable]
    public class NetworkMessage
    {
        public string senderId;
        public MessageType messageType;
        public string content;
        public Vector3 position;
        public bool boolValue;
        public int intValue;
        public float timestamp;
    }
    
    /// <summary>
    /// 메시지 타입
    /// </summary>
    public enum MessageType
    {
        Chat,
        Position,
        Action,
        PuzzleState,
        FlagState,
        PlayerJoin,
        PlayerLeave,
        GameState
    }
}