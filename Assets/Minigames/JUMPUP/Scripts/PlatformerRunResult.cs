namespace TreeClimb
{
    public readonly struct PlatformerRunResult
    {
        public int FinalScore { get; }
        public float MaxHeightReached { get; }
        public int XpReward { get; }
        public PlatformerRunResult(int score, float maxHeight, int xp)
        { FinalScore = score; MaxHeightReached = maxHeight; XpReward = xp; }
    }

    // Optional overrides; unspecified values preserve the existing Inspector configuration.
    public sealed class PlatformerGameplaySettings
    {
        public float? MoveSpeed, WindStrength, BounceStrength;
        public float? MinimumPlatformSpacing, MaximumPlatformSpacing;
        public float? EnemySpawnChance, PowerupSpawnChance;
        public PlatformerControlMode? ControlMode;
        public bool? ScreenWrapping;
    }
}
