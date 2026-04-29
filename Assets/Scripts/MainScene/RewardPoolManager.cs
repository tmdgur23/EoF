using System.Collections.Generic;
using UnityEngine;

namespace MainScene
{
    public class RewardOption
    {
        public string RewardName;
        public string RewardId; 

        public RewardOption(string name, string id)
        {
            RewardName = name;
            RewardId = id;
        }
    }

    public class RewardPoolManager : MonoBehaviour
    {
        public static RewardPoolManager Instance { get; private set; }

        private List<RewardOption> round1Pool;
        private List<RewardOption> round2Pool;
        private List<RewardOption> round3Pool;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            InitializePools();
        }

        private void InitializePools()
        {
            // 라운드 1
            round1Pool = new List<RewardOption>()
            {
                new RewardOption("근력 +1", "STR_1"),
                new RewardOption("지식 +1", "INT_1"),
                new RewardOption("정신력 +1", "MEN_1"),
                new RewardOption("카드 보상", "CARD_1"),
                new RewardOption("체력회복 +4", "HP_4"),
                new RewardOption("최대체력 +2", "MAXHP_2"),
                new RewardOption("Soul +2", "SOUL_3"),
                new RewardOption("다음 전투 방어도 +3", "NEXT_DEF_3"),
                new RewardOption("다음 드로우 +1", "NEXT_DRAW_1"),
                new RewardOption("다음 주사위 +1", "NEXT_DICE_1"),
                new RewardOption("체력 -2, 카드 보상", "HP_-2_CARD_1"),
                new RewardOption("체력 -2, 아무 능력치 +1", "HP_-1_STAT_1"),
                new RewardOption("Soul -1, 카드 보상", "SOUL_-1_CARD_1"),
                new RewardOption("변화없음", "NOTHING")
            };

            // 라운드 2
            round2Pool = new List<RewardOption>()
            {
                new RewardOption("근력 +2", "STR_2"),
                new RewardOption("지식 +2", "INT_2"),
                new RewardOption("정신력 +2", "MEN_2"),
                new RewardOption("카드 보상", "CARD_1"),
                new RewardOption("카드 제거", "CARD_REMOVE"),
                new RewardOption("카드 강화", "CARD_UPGRADE"),
                new RewardOption("체력회복 +6", "HP_6"),
                new RewardOption("최대체력 +4", "MAXHP_4"),
                new RewardOption("Soul +5", "SOUL_5"),
                new RewardOption("다음 전투 드로우 +1", "NEXT_DRAW_1"),
                new RewardOption("다음 전투 방어도 +5", "NEXT_DEF_5"),
                new RewardOption("다음 전투 주사위 +1", "NEXT_DICE_1"),
                new RewardOption("체력 -3", "HP_-3"),
                new RewardOption("체력 -4, Soul +3", "HP_-4_SOUL_3"),
                new RewardOption("체력 -5, 카드 보상", "HP_-5_CARD_1"),
                new RewardOption("카드 제거, 카드 보상", "REMOVE_1_CARD_1")
            };

            // 라운드 3
            round3Pool = new List<RewardOption>()
            {
                new RewardOption("근력 +3", "STR_3"),
                new RewardOption("지식 +3", "INT_3"),
                new RewardOption("정신력 +3", "MEN_3"),
                new RewardOption("모든 능력치 +1", "ALL_STAT_1"),
                new RewardOption("카드 2장 보상", "CARD_2"),
                new RewardOption("카드 보상", "CARD_1"),
                new RewardOption("카드 제거 2회", "CARD_REMOVE_2"),
                new RewardOption("카드 강화 2회", "CARD_UPGRADE_2"),
                new RewardOption("체력회복 +8", "HP_8"),
                new RewardOption("최대체력 +6", "HP_6"),
                new RewardOption("Soul +6", "SOUL_6"),
                new RewardOption("다음 전투 드로우 +2", "NEXT_DRAW_2"),
                new RewardOption("다음 전투 방어도 +8", "NEXT_DEF_8"),
                new RewardOption("다음 전투 주사위 +2", "NEXT_DICE_2"),
                new RewardOption("체력 -5", "HP_-5"),
                new RewardOption("체력 -7, 카드 보상", "HP_-7_CARD"),
                new RewardOption("Soul -3, 카드 2장 보상", "SOUL_-3_CARD_2"),
                new RewardOption("체력 절반 감소, 카드 강화 2회", "HP_HALF_UPGRADE_2"),
                new RewardOption("카드 제거 2회, 카드 보상", "REMOVE_2_CARD_1")
            };
        }

        public List<RewardOption> GetRandomRewards(int round, int count = 3)
        {
            List<RewardOption> sourcePool;
            
            // round는 0, 1, 2로 들어온다고 가정합니다. (currentLoopCount)
            if (round <= 0) sourcePool = round1Pool;
            else if (round == 1) sourcePool = round2Pool;
            else sourcePool = round3Pool;

            // 원본 풀을 복사해서 섞습니다 (Fisher-Yates Shuffle)
            List<RewardOption> temp = new List<RewardOption>(sourcePool);
            for (int i = 0; i < temp.Count; i++)
            {
                int rnd = Random.Range(0, temp.Count);
                RewardOption t = temp[i];
                temp[i] = temp[rnd];
                temp[rnd] = t;
            }

            // 앞의 count 개수만큼 잘라서 반환
            return temp.GetRange(0, Mathf.Min(count, temp.Count));
        }
    }
}
