using UnityEngine;
using MemoryFracture.Core;

namespace MemoryFracture.Puzzles
{
    /// <summary>
    /// 거울 반사 퍼즐 - 챕터 1의 핵심 퍼즐
    /// </summary>
    public class MirrorReflectionPuzzle : BasePuzzle
    {
        [Header("Mirror Settings")]
        public Transform[] mirrors;
        public Transform lightSource;
        public Transform target;
        public float reflectionAccuracy = 0.1f;
        
        [Header("Visual Feedback")]
        public LineRenderer lightBeam;
        public Material mirrorMaterial;
        public Color activeColor = Color.yellow;
        public Color inactiveColor = Color.gray;
        
        private Vector3[] reflectionPoints;
        private bool isLightAligned = false;
        
        protected override void OnPuzzleStart()
        {
            puzzleId = "MirrorReflection_Chapter1";
            puzzleName = "거울 반사 퍼즐";
            difficulty = 2;
            isRequired = true;
            
            // 플래그 보상 설정
            truthReward = 1;
            cooperationReward = 2;
            
            // 거울 초기화
            InitializeMirrors();
            
            // 빛줄기 초기화
            if (lightBeam != null)
            {
                lightBeam.positionCount = mirrors.Length + 2;
                lightBeam.startWidth = 0.1f;
                lightBeam.endWidth = 0.1f;
            }
            
            Debug.Log("거울 반사 퍼즐 시작!");
        }
        
        protected override void OnPuzzleComplete()
        {
            // 거울들을 활성화 상태로 변경
            foreach (Transform mirror in mirrors)
            {
                if (mirrorMaterial != null)
                {
                    mirrorMaterial.color = activeColor;
                }
            }
            
            // 빛줄기 효과
            if (lightBeam != null)
            {
                lightBeam.enabled = true;
                UpdateLightBeam();
            }
            
            Debug.Log("거울 반사 퍼즐 완료!");
        }
        
        protected override void OnPuzzleFail()
        {
            // 거울들을 비활성화 상태로 변경
            foreach (Transform mirror in mirrors)
            {
                if (mirrorMaterial != null)
                {
                    mirrorMaterial.color = inactiveColor;
                }
            }
            
            if (lightBeam != null)
            {
                lightBeam.enabled = false;
            }
            
            Debug.Log("거울 반사 퍼즐 실패!");
        }
        
        protected override void OnPuzzleRestart()
        {
            isLightAligned = false;
            if (lightBeam != null)
            {
                lightBeam.enabled = false;
            }
        }
        
        protected override void OnPuzzleReset()
        {
            isLightAligned = false;
            if (lightBeam != null)
            {
                lightBeam.enabled = false;
            }
        }
        
        protected override void OnHintProvided()
        {
            Debug.Log("힌트: 거울의 각도를 조정하여 빛이 목표물에 도달하도록 하세요.");
        }
        
        protected override void OnPuzzleAlreadyCompleted()
        {
            // 이미 완료된 퍼즐의 상태 복원
            if (lightBeam != null)
            {
                lightBeam.enabled = true;
                UpdateLightBeam();
            }
        }
        
        private void InitializeMirrors()
        {
            reflectionPoints = new Vector3[mirrors.Length];
            
            // 거울들을 비활성화 상태로 초기화
            foreach (Transform mirror in mirrors)
            {
                if (mirrorMaterial != null)
                {
                    mirrorMaterial.color = inactiveColor;
                }
            }
        }
        
        private void Update()
        {
            if (isActive && !isCompleted)
            {
                CheckLightAlignment();
                UpdateLightBeam();
            }
        }
        
        private void CheckLightAlignment()
        {
            if (lightSource == null || target == null || mirrors.Length == 0)
                return;
            
            Vector3 currentLight = lightSource.position;
            Vector3 currentDirection = lightSource.forward;
            
            // 각 거울을 통한 반사 계산
            for (int i = 0; i < mirrors.Length; i++)
            {
                Vector3 mirrorNormal = mirrors[i].up;
                Vector3 incident = currentDirection;
                
                // 반사 벡터 계산
                Vector3 reflected = Vector3.Reflect(incident, mirrorNormal);
                
                // 반사점 저장
                reflectionPoints[i] = mirrors[i].position;
                
                // 다음 거울로 빛 전달
                currentLight = mirrors[i].position;
                currentDirection = reflected;
            }
            
            // 마지막 반사가 목표물에 도달하는지 확인
            Vector3 finalDirection = currentDirection;
            Vector3 toTarget = target.position - currentLight;
            
            float angle = Vector3.Angle(finalDirection, toTarget.normalized);
            
            if (angle < reflectionAccuracy)
            {
                if (!isLightAligned)
                {
                    isLightAligned = true;
                    CompletePuzzle();
                }
            }
            else
            {
                isLightAligned = false;
            }
        }
        
        private void UpdateLightBeam()
        {
            if (lightBeam == null || !isCompleted)
                return;
            
            Vector3[] positions = new Vector3[mirrors.Length + 2];
            positions[0] = lightSource.position;
            
            Vector3 currentLight = lightSource.position;
            Vector3 currentDirection = lightSource.forward;
            
            for (int i = 0; i < mirrors.Length; i++)
            {
                Vector3 mirrorNormal = mirrors[i].up;
                Vector3 incident = currentDirection;
                Vector3 reflected = Vector3.Reflect(incident, mirrorNormal);
                
                positions[i + 1] = mirrors[i].position;
                currentLight = mirrors[i].position;
                currentDirection = reflected;
            }
            
            positions[positions.Length - 1] = target.position;
            lightBeam.SetPositions(positions);
        }
        
        // 거울 회전을 위한 공개 메서드
        public void RotateMirror(int mirrorIndex, float angle)
        {
            if (mirrorIndex >= 0 && mirrorIndex < mirrors.Length)
            {
                mirrors[mirrorIndex].Rotate(0, angle, 0);
            }
        }
        
        // 거울 회전을 위한 공개 메서드 (협동용)
        public void RotateMirrorCooperatively(int mirrorIndex, float angle, string playerId)
        {
            if (mirrorIndex >= 0 && mirrorIndex < mirrors.Length)
            {
                mirrors[mirrorIndex].Rotate(0, angle, 0);
                
                // 협동 보상
                if (gameManager != null)
                {
                    gameManager.ModifyFlag(FlagType.CooperationTrust, 1);
                }
            }
        }
    }
}