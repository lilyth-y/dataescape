using UnityEngine;
using MemoryFracture.Networking;

namespace MemoryFracture.Core
{
    /// <summary>
    /// 플레이어 생성 및 관리 시스템
    /// </summary>
    public class PlayerSpawner : MonoBehaviour
    {
        [Header("Player Prefabs")]
        public GameObject playerAPrefab;
        public GameObject playerBPrefab;
        
        [Header("Spawn Points")]
        public Transform[] spawnPoints;
        
        [Header("Player Settings")]
        public bool spawnOnStart = true;
        public bool isMultiplayer = true;
        
        private GameObject currentPlayer;
        private NetworkManager networkManager;
        
        private void Start()
        {
            networkManager = NetworkManager.Instance;
            
            if (spawnOnStart)
            {
                SpawnPlayer();
            }
        }
        
        /// <summary>
        /// 플레이어 생성
        /// </summary>
        public void SpawnPlayer()
        {
            if (isMultiplayer && networkManager != null)
            {
                SpawnMultiplayerPlayer();
            }
            else
            {
                SpawnSinglePlayer();
            }
        }
        
        /// <summary>
        /// 싱글플레이어 생성
        /// </summary>
        private void SpawnSinglePlayer()
        {
            // 기본 플레이어 A 생성
            if (playerAPrefab != null)
            {
                Transform spawnPoint = GetRandomSpawnPoint();
                currentPlayer = Instantiate(playerAPrefab, spawnPoint.position, spawnPoint.rotation);
                
                // 플레이어 컨트롤러 설정
                PlayerController controller = currentPlayer.GetComponent<PlayerController>();
                if (controller != null)
                {
                    controller.SetPlayerId("PlayerA");
                    controller.SetLocalPlayer(true);
                }
                
                Debug.Log("싱글플레이어 생성 완료");
            }
        }
        
        /// <summary>
        /// 멀티플레이어 생성
        /// </summary>
        private void SpawnMultiplayerPlayer()
        {
            if (!networkManager.isConnected)
            {
                Debug.LogWarning("네트워크에 연결되지 않음. 싱글플레이어로 생성합니다.");
                SpawnSinglePlayer();
                return;
            }
            
            // 호스트인지 클라이언트인지에 따라 다른 플레이어 생성
            GameObject playerPrefab = networkManager.isHost ? playerAPrefab : playerBPrefab;
            string playerId = networkManager.isHost ? "PlayerA" : "PlayerB";
            
            if (playerPrefab != null)
            {
                Transform spawnPoint = GetRandomSpawnPoint();
                currentPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
                
                // 플레이어 컨트롤러 설정
                PlayerController controller = currentPlayer.GetComponent<PlayerController>();
                if (controller != null)
                {
                    controller.SetPlayerId(playerId);
                    controller.SetLocalPlayer(true);
                }
                
                Debug.Log($"멀티플레이어 생성 완료: {playerId}");
            }
        }
        
        /// <summary>
        /// 랜덤 스폰 포인트 반환
        /// </summary>
        private Transform GetRandomSpawnPoint()
        {
            if (spawnPoints.Length == 0)
            {
                return transform;
            }
            
            int randomIndex = Random.Range(0, spawnPoints.Length);
            return spawnPoints[randomIndex];
        }
        
        /// <summary>
        /// 특정 스폰 포인트에서 플레이어 생성
        /// </summary>
        public void SpawnPlayerAtPoint(int spawnPointIndex)
        {
            if (spawnPointIndex >= 0 && spawnPointIndex < spawnPoints.Length)
            {
                Transform spawnPoint = spawnPoints[spawnPointIndex];
                
                if (currentPlayer != null)
                {
                    Destroy(currentPlayer);
                }
                
                GameObject playerPrefab = networkManager.isHost ? playerAPrefab : playerBPrefab;
                string playerId = networkManager.isHost ? "PlayerA" : "PlayerB";
                
                if (playerPrefab != null)
                {
                    currentPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
                    
                    PlayerController controller = currentPlayer.GetComponent<PlayerController>();
                    if (controller != null)
                    {
                        controller.SetPlayerId(playerId);
                        controller.SetLocalPlayer(true);
                    }
                }
            }
        }
        
        /// <summary>
        /// 플레이어 제거
        /// </summary>
        public void DespawnPlayer()
        {
            if (currentPlayer != null)
            {
                Destroy(currentPlayer);
                currentPlayer = null;
            }
        }
        
        /// <summary>
        /// 현재 플레이어 반환
        /// </summary>
        public GameObject GetCurrentPlayer()
        {
            return currentPlayer;
        }
        
        /// <summary>
        /// 플레이어 위치 설정
        /// </summary>
        public void SetPlayerPosition(Vector3 position)
        {
            if (currentPlayer != null)
            {
                currentPlayer.transform.position = position;
            }
        }
        
        /// <summary>
        /// 플레이어 회전 설정
        /// </summary>
        public void SetPlayerRotation(Quaternion rotation)
        {
            if (currentPlayer != null)
            {
                currentPlayer.transform.rotation = rotation;
            }
        }
        
        private void OnDrawGizmos()
        {
            // 스폰 포인트 표시
            Gizmos.color = Color.green;
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                    Gizmos.DrawRay(spawnPoint.position, spawnPoint.forward);
                }
            }
        }
    }
}