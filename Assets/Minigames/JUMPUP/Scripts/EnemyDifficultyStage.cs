using System;
using System.Collections.Generic;
using UnityEngine;

namespace TreeClimb
{
    [Serializable]
    public sealed class EnemyDifficultyStage
    {
        [Min(0), InspectorName("Min Score"), Tooltip("The highest eligible minimum score wins. List order need not be sorted; the last entry wins a tied minimum.")]
        public int minimumScore;
        [Range(0f, 1f), Tooltip("Chance per eligible generated platform, before placement and lethal-spacing restrictions.")]
        public float enemyChance;
        [Min(0f)] public float staticWeight;
        [Min(0f)] public float movingWeight;
        [Min(0f)] public float shootingWeight;
        public EnemyDifficultyStage(int score, float chance, float stationary, float moving, float shooting)
        {
            minimumScore = score; enemyChance = chance;
            staticWeight = stationary; movingWeight = moving; shootingWeight = shooting;
        }
        public static List<EnemyDifficultyStage> Defaults() => new List<EnemyDifficultyStage>
        {
            new EnemyDifficultyStage(0, 0, 0, 0, 0),
            new EnemyDifficultyStage(500, 0, 0, 0, 0),
            new EnemyDifficultyStage(1000, 0.08f, 100, 0, 0),
            new EnemyDifficultyStage(2000, 0.12f, 70, 30, 0),
            new EnemyDifficultyStage(3000, 0.16f, 50, 35, 15)
        };
    }
}
