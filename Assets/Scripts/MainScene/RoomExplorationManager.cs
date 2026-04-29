using UnityEngine;
using UnityEngine.SceneManagement;
using MainScene;

namespace MainScene
{
    public class RoomExplorationManager : MonoBehaviour
    {
        public static RoomExplorationManager Instance { get; private set; }

        [Header("State")]
        public int currentLoopCount = 0;        // 현재까지 클리어한 방의 수 (최대 3)
        public int currentRoomInteractions = 0; // 현재 방에서 오브젝트와 상호작용한 횟수 (최대 5)
        public int currentRoomIndex = -1;
        
        private Vector3 savedHallwayPosition;   // 복도로 돌아갈 때를 대비해 저장해둘 좌표
        private Transform playerTransform;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            GameObject playerGo = GameObject.FindGameObjectWithTag("Player") ?? GameObject.Find("Player");
            if (playerGo != null)
            {
                playerTransform = playerGo.transform;
            }
        }

        [Header("Room Settings")]
        [Tooltip("방의 인덱스(0, 1, 2...)에 맞는 스폰 오브젝트(Transform)를 설정하세요. spawn1~6 등을 드래그 앤 드롭하시면 됩니다.")]
        public Transform[] roomSpawns = new Transform[6];

        // 방에 입장할 때 호출
        public void EnterRoom(int roomIndex)
        {
            if (playerTransform == null)
            {
                GameObject playerGo = GameObject.FindGameObjectWithTag("Player") ?? GameObject.Find("Player");
                if (playerGo != null) playerTransform = playerGo.transform;
            }

            if (playerTransform != null)
            {
                savedHallwayPosition = playerTransform.position;
                
                // CharacterController가 있으면 비활성화 후 이동시켜야 충돌체 문제로 텔레포트가 씹히지 않습니다.
                CharacterController cc = playerTransform.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                
                // roomIndex에 맞는 Transform으로 순간이동
                if (roomIndex >= 0 && roomIndex < roomSpawns.Length && roomSpawns[roomIndex] != null)
                {
                    playerTransform.position = roomSpawns[roomIndex].position;
                    playerTransform.rotation = roomSpawns[roomIndex].rotation; // 방향도 맞춰주면 좋습니다
                }
                else
                {
                    Debug.LogWarning($"[RoomExplorationManager] 방 {roomIndex} 에 대한 스폰 포인트가 설정되지 않았습니다!");
                    // 스폰이 없을 경우의 예비용 하드코딩 좌표 (원하시면 지우셔도 됩니다)
                    playerTransform.position = new Vector3(-330f, 27f, 464f);
                }
                
                if (cc != null) cc.enabled = true;
            }
            
            currentRoomIndex = roomIndex;
            currentRoomInteractions = 0; 

            Debug.Log($"[RoomExplorationManager] 방 {roomIndex} 에 입장했습니다.");
        }

        // 오브젝트 클릭 시 호출 (임시 딕셔너리로 스탯 보너스 넘김)
        public void OnObjectInteracted(string statName, int value)
        {
            currentRoomInteractions++;
            
            // 능력치 임시 저장 (배틀씬으로 넘기기 위함)
            int currentStat = PlayerPrefs.GetInt("Bonus_" + statName, 0);
            PlayerPrefs.SetInt("Bonus_" + statName, currentStat + value);

            Debug.Log($"[RoomExplorationManager] 오브젝트 상호작용 ({currentRoomInteractions}/5). 얻은 스탯: {statName} +{value}");

            if (currentRoomInteractions >= 5)
            {
                Debug.Log("[RoomExplorationManager] 상호작용 5회 달성! 보상 창을 호출합니다.");
                // 상호작용 5회 달성 -> 카드 보상 트리거
                if (RoomAttributeManager.Instance != null)
                {
                    RoomAttributeManager.Instance.TriggerReward(currentRoomIndex);
                }
            }
        }

        // 카드 보상을 3번 모두 마쳤을 때 호출
        public void ExitRoomOrBattle()
        {
            currentLoopCount++;
            Debug.Log($"[RoomExplorationManager] 방 클리어 완료. 현재 루프: {currentLoopCount}/3");

            if (currentLoopCount >= 3)
            {
                // 3회 루프 달성 -> 전투 씬으로 전환
                Debug.Log("[RoomExplorationManager] 3회 클리어 달성! 전투 진입.");
                SceneManager.LoadScene("Battle");
            }
            else
            {
                // 복도로 복귀
                if (playerTransform != null)
                {
                    CharacterController cc = playerTransform.GetComponent<CharacterController>();
                    if (cc != null) cc.enabled = false;
                    
                    playerTransform.position = savedHallwayPosition;
                    
                    if (cc != null) cc.enabled = true;
                }
                
                currentRoomIndex = -1; // 방 밖으로 나왔음을 표시
                // 다시 초기화
                ResetAllDoors();
            }
        }

        private void ResetAllDoors()
        {
            InteractableDoor[] doors = FindObjectsByType<InteractableDoor>(FindObjectsSortMode.None);
            foreach(var door in doors)
            {
                door.ResetHint();
            }
            Debug.Log("[RoomExplorationManager] 모든 문의 힌트를 리셋했습니다.");
        }
    }
}
