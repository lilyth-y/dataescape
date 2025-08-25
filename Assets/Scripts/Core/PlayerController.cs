using UnityEngine;
using MemoryFracture.Networking;

namespace MemoryFracture.Core
{
    /// <summary>
    /// 플레이어 컨트롤러 - 이동, 상호작용, 네트워크 동기화
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 5f;
        public float rotationSpeed = 100f;
        public float jumpForce = 5f;
        
        [Header("Interaction Settings")]
        public float interactionRange = 3f;
        public LayerMask interactableLayer = 1;
        
        [Header("Player Info")]
        public string playerId;
        public bool isLocalPlayer = false;
        
        // Components
        private CharacterController characterController;
        private NetworkManager networkManager;
        private Camera playerCamera;
        
        // Movement
        private Vector3 moveDirection = Vector3.zero;
        private float verticalVelocity = 0f;
        private bool isGrounded = false;
        
        // Interaction
        private GameObject currentInteractable;
        private bool canInteract = false;
        
        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            networkManager = NetworkManager.Instance;
            
            // 플레이어 카메라 설정
            if (isLocalPlayer)
            {
                playerCamera = Camera.main;
                if (playerCamera != null)
                {
                    playerCamera.transform.SetParent(transform);
                    playerCamera.transform.localPosition = new Vector3(0, 1.6f, 0);
                    playerCamera.transform.localRotation = Quaternion.identity;
                }
            }
        }
        
        private void Start()
        {
            if (isLocalPlayer)
            {
                // 로컬 플레이어 초기화
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        
        private void Update()
        {
            if (isLocalPlayer)
            {
                HandleMovement();
                HandleInteraction();
                HandleNetworkSync();
            }
        }
        
        /// <summary>
        /// 이동 처리
        /// </summary>
        private void HandleMovement()
        {
            // 지면 체크
            isGrounded = characterController.isGrounded;
            
            if (isGrounded && verticalVelocity < 0)
            {
                verticalVelocity = -2f;
            }
            
            // 입력 받기
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            
            // 이동 방향 계산
            Vector3 forward = transform.forward * vertical;
            Vector3 right = transform.right * horizontal;
            moveDirection = (forward + right).normalized;
            
            // 이동 적용
            if (moveDirection.magnitude >= 0.1f)
            {
                characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
            }
            
            // 점프
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                verticalVelocity = jumpForce;
            }
            
            // 중력 적용
            verticalVelocity += Physics.gravity.y * Time.deltaTime;
            characterController.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
            
            // 마우스 회전
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up * mouseX);
        }
        
        /// <summary>
        /// 상호작용 처리
        /// </summary>
        private void HandleInteraction()
        {
            // 상호작용 가능한 오브젝트 찾기
            Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRange, interactableLayer);
            
            GameObject nearestInteractable = null;
            float nearestDistance = float.MaxValue;
            
            foreach (Collider collider in colliders)
            {
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestInteractable = collider.gameObject;
                }
            }
            
            // 상호작용 가능한 오브젝트 변경
            if (nearestInteractable != currentInteractable)
            {
                if (currentInteractable != null)
                {
                    // 이전 오브젝트 하이라이트 제거
                    RemoveHighlight(currentInteractable);
                }
                
                currentInteractable = nearestInteractable;
                
                if (currentInteractable != null)
                {
                    // 새 오브젝트 하이라이트 추가
                    AddHighlight(currentInteractable);
                }
            }
            
            // 상호작용 키 입력
            if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
            {
                InteractWithObject(currentInteractable);
            }
        }
        
        /// <summary>
        /// 네트워크 동기화
        /// </summary>
        private void HandleNetworkSync()
        {
            if (networkManager != null && networkManager.isConnected)
            {
                // 위치 동기화
                networkManager.UpdatePlayerPosition(transform.position);
                
                // 액션 동기화
                if (Input.GetKeyDown(KeyCode.E))
                {
                    networkManager.SendPlayerAction("Interact", true);
                }
            }
        }
        
        /// <summary>
        /// 오브젝트와 상호작용
        /// </summary>
        private void InteractWithObject(GameObject interactable)
        {
            // 퍼즐 컴포넌트 확인
            BasePuzzle puzzle = interactable.GetComponent<BasePuzzle>();
            if (puzzle != null)
            {
                puzzle.StartPuzzle();
                return;
            }
            
            // 기타 상호작용 가능한 컴포넌트들
            IInteractable interactableComponent = interactable.GetComponent<IInteractable>();
            if (interactableComponent != null)
            {
                interactableComponent.Interact(this);
            }
        }
        
        /// <summary>
        /// 오브젝트 하이라이트 추가
        /// </summary>
        private void AddHighlight(GameObject obj)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                // 하이라이트 효과 추가 (간단한 색상 변경)
                renderer.material.color = Color.yellow;
            }
        }
        
        /// <summary>
        /// 오브젝트 하이라이트 제거
        /// </summary>
        private void RemoveHighlight(GameObject obj)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                // 원래 색상으로 복원
                renderer.material.color = Color.white;
            }
        }
        
        /// <summary>
        /// 플레이어 ID 설정
        /// </summary>
        public void SetPlayerId(string id)
        {
            playerId = id;
        }
        
        /// <summary>
        /// 로컬 플레이어 설정
        /// </summary>
        public void SetLocalPlayer(bool local)
        {
            isLocalPlayer = local;
        }
        
        private void OnDrawGizmosSelected()
        {
            // 상호작용 범위 표시
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
    
    /// <summary>
    /// 상호작용 가능한 인터페이스
    /// </summary>
    public interface IInteractable
    {
        void Interact(PlayerController player);
    }
}