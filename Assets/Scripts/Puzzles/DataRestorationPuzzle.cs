using UnityEngine;
using MemoryFracture.Core;

namespace MemoryFracture.Puzzles
{
    /// <summary>
    /// 데이터 복원 퍼즐 - 챕터 2의 핵심 퍼즐
    /// </summary>
    public class DataRestorationPuzzle : CooperativePuzzle
    {
        [Header("Data Settings")]
        public GameObject[] dataFragments;
        public Transform[] restorationPoints;
        public Material corruptedMaterial;
        public Material restoredMaterial;
        
        [Header("Cooperation")]
        public bool requiresBothPlayers = true;
        public float cooperationTimeWindow = 3f;
        
        private bool[] fragmentsRestored;
        private int currentFragmentIndex = 0;
        
        protected override void OnPuzzleStart()
        {
            puzzleId = "DataRestoration_Chapter2";
            puzzleName = "데이터 복원 퍼즐";
            difficulty = 3;
            isRequired = true;
            
            // 플래그 보상 설정
            truthReward = 2;
            cooperationReward = 3;
            
            // 데이터 조각 초기화
            InitializeDataFragments();
            
            Debug.Log("데이터 복원 퍼즐 시작!");
        }
        
        protected override void OnPuzzleComplete()
        {
            // 모든 데이터 조각을 복원된 상태로 변경
            foreach (GameObject fragment in dataFragments)
            {
                if (fragment != null)
                {
                    Renderer renderer = fragment.GetComponent<Renderer>();
                    if (renderer != null && restoredMaterial != null)
                    {
                        renderer.material = restoredMaterial;
                    }
                }
            }
            
            Debug.Log("데이터 복원 퍼즐 완료!");
        }
        
        protected override void OnPuzzleFail()
        {
            // 데이터 조각들을 손상된 상태로 변경
            foreach (GameObject fragment in dataFragments)
            {
                if (fragment != null)
                {
                    Renderer renderer = fragment.GetComponent<Renderer>();
                    if (renderer != null && corruptedMaterial != null)
                    {
                        renderer.material = corruptedMaterial;
                    }
                }
            }
            
            Debug.Log("데이터 복원 퍼즐 실패!");
        }
        
        protected override void OnPuzzleRestart()
        {
            currentFragmentIndex = 0;
            InitializeDataFragments();
        }
        
        protected override void OnPuzzleReset()
        {
            currentFragmentIndex = 0;
            InitializeDataFragments();
        }
        
        protected override void OnHintProvided()
        {
            Debug.Log("힌트: 두 플레이어가 동시에 데이터 조각을 복원해야 합니다.");
        }
        
        protected override void OnPuzzleAlreadyCompleted()
        {
            // 이미 완료된 퍼즐의 상태 복원
            foreach (GameObject fragment in dataFragments)
            {
                if (fragment != null)
                {
                    Renderer renderer = fragment.GetComponent<Renderer>();
                    if (renderer != null && restoredMaterial != null)
                    {
                        renderer.material = restoredMaterial;
                    }
                }
            }
        }
        
        private void InitializeDataFragments()
        {
            fragmentsRestored = new bool[dataFragments.Length];
            
            // 모든 데이터 조각을 손상된 상태로 초기화
            foreach (GameObject fragment in dataFragments)
            {
                if (fragment != null)
                {
                    Renderer renderer = fragment.GetComponent<Renderer>();
                    if (renderer != null && corruptedMaterial != null)
                    {
                        renderer.material = corruptedMaterial;
                    }
                }
            }
        }
        
        /// <summary>
        /// 데이터 조각 복원 시도
        /// </summary>
        public void RestoreDataFragment(int fragmentIndex, string playerId)
        {
            if (fragmentIndex < 0 || fragmentIndex >= dataFragments.Length)
                return;
                
            if (fragmentsRestored[fragmentIndex])
                return;
                
            // 플레이어 준비 상태 설정
            SetPlayerReady(playerId, true);
            
            // 데이터 조각 복원
            fragmentsRestored[fragmentIndex] = true;
            
            if (dataFragments[fragmentIndex] != null)
            {
                Renderer renderer = dataFragments[fragmentIndex].GetComponent<Renderer>();
                if (renderer != null && restoredMaterial != null)
                {
                    renderer.material = restoredMaterial;
                }
            }
            
            // 모든 조각이 복원되었는지 확인
            CheckAllFragmentsRestored();
        }
        
        private void CheckAllFragmentsRestored()
        {
            bool allRestored = true;
            foreach (bool restored in fragmentsRestored)
            {
                if (!restored)
                {
                    allRestored = false;
                    break;
                }
            }
            
            if (allRestored)
            {
                CompletePuzzle();
            }
        }
        
        /// <summary>
        /// 데이터 조각 위치 반환
        /// </summary>
        public Vector3 GetFragmentPosition(int fragmentIndex)
        {
            if (fragmentIndex >= 0 && fragmentIndex < dataFragments.Length)
            {
                return dataFragments[fragmentIndex].transform.position;
            }
            return Vector3.zero;
        }
        
        /// <summary>
        /// 복원 포인트 위치 반환
        /// </summary>
        public Vector3 GetRestorationPointPosition(int pointIndex)
        {
            if (pointIndex >= 0 && pointIndex < restorationPoints.Length)
            {
                return restorationPoints[pointIndex].position;
            }
            return Vector3.zero;
        }
    }
}