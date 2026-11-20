using System.Collections.Generic;
using UnityEngine;

namespace TreeClimb
{
    public class PlatformGenerator : MonoBehaviour
    {
        public PlayerController player;
        public Camera viewCamera;
        public GameObject platformPrefab;
        public GameManager gameManager;
        [Tooltip("Requested world-space spacing, clamped to the normal jump and movement envelopes.")]
        [Min(0.1f)] public float minimumVerticalSpacing = 1.8f;
        [Min(0.1f)] public float maximumVerticalSpacing = 2.15f;
        [Min(0f)] public float horizontalSpawnRange = 3.8f;
        [Min(0f), Tooltip("Preferred minimum center offset; relaxed only when no safe landing interval exists.")]
        public float minimumHorizontalOffset = 1.45f;
        [InspectorName("Maximum Horizontal Offset"), Min(0f)] public float maximumHorizontalStep = 2.2f;
        [Range(0f, 1f), Tooltip("Chance to continue across the tree rather than zigzag straight back.")]
        public float horizontalDirectionPersistence = 0.75f;
        [Min(4)] public int platformsAhead = 4;
        [Min(0f), Tooltip("World units generated ahead; the visible camera top is always covered too.")]
        public float generationDistanceAhead = 6f;
        [Tooltip("Distance below the highest player Y. Keep this larger than Death Fall Distance to allow recovery.")]
        [Min(1f)] public float platformCleanupDistance = 13f;
        [Range(0f, 1f)] public float powerupSpawnChance = 0.18f;
        [Tooltip("Primary enemy generation configuration. Positive weights make a type eligible in that stage; zero excludes it.")]
        public List<EnemyDifficultyStage> enemyDifficultyStages = EnemyDifficultyStage.Defaults();
        [Min(0), Tooltip("Minimum generated platforms without a moving/shooting enemy between lethal encounters. Static enemies count as non-lethal. Rejects enemies, never adds platforms.")]
        public int minimumSafePlatformsBetweenLethalEnemies = 1;
#if UNITY_EDITOR
        public bool overrideEnemyDifficultyScore;
        [Min(0)] public int enemyDebugDifficultyScore;
        public bool showEnemyDifficultyDebug;
#endif
        int highestEnemyProgressionScore, lastLethalPlatformIndex = -1;
        public int ProceduralPlatformsGenerated { get; private set; }
        public int EnemyPlatformsEvaluated { get; private set; }
        public int EnemyChancePasses { get; private set; }
        public int StaticEnemiesGenerated { get; private set; }
        public int MovingEnemiesGenerated { get; private set; }
        public int ShootingEnemiesGenerated { get; private set; }
        public int EnemyPlacementSkips { get; private set; }
        public int EnemySafetySkips { get; private set; }
        public int EnemyWeightSkips { get; private set; }
        public int EnemyProgressionScore
        {
            get
            {
                highestEnemyProgressionScore = Mathf.Max(highestEnemyProgressionScore, CurrentRunScore);
#if UNITY_EDITOR
                if (overrideEnemyDifficultyScore) return Mathf.Max(0, enemyDebugDifficultyScore);
#endif
                return highestEnemyProgressionScore;
            }
        }
        public EnemyDifficultyStage CurrentEnemyStage => SelectEnemyStage(EnemyProgressionScore);
        public EnemyDifficultyStage SelectEnemyStage(int score)
        {
            EnemyDifficultyStage chosen = null;
            if (enemyDifficultyStages == null) return null;
            foreach (var stage in enemyDifficultyStages)
                if (stage != null && Mathf.Max(0, stage.minimumScore) <= score &&
                    (chosen == null || Mathf.Max(0, stage.minimumScore) >= Mathf.Max(0, chosen.minimumScore))) chosen = stage;
            return chosen;
        }
        // The existing integration setting edits the primary stage data. It is not a second override.
        public void SetEnemySpawnChance(float chance)
        {
            if (enemyDifficultyStages == null) return;
            foreach (var stage in enemyDifficultyStages)
                if (stage != null && (stage.staticWeight > 0f || stage.movingWeight > 0f || stage.shootingWeight > 0f))
                    stage.enemyChance = Mathf.Clamp01(chance);
        }
        [Tooltip("Trigger-only static obstacle prefab. Its world size must fit the supporting branch.")]
        public PlatformEnemy staticEnemyPrefab;
        public PlatformEnemy movingEnemyPrefab;
        public PlatformEnemy shootingEnemyPrefab;
        public EnemyProjectile enemyProjectilePrefab;
        [Min(0.1f)] public float enemyFireInterval = 5f;
        [Min(0f)] public float enemyShootingRange = 7f;
        [Min(0.1f)] public float enemyProjectileSpeed = 3f;
        [Min(0.1f)] public float enemyProjectileLifetime = 6f;
        [Min(0.05f)] public float enemyProjectileSize = 0.18f;
        [Min(0.1f)] public float shootingEnemyStompBounce = 12f;
        [Min(0)] public int shootingEnemyKillScore = 100;
        [Range(0f, 1f)] public float enemyHorizontalMovementChance = 0.5f;
        [Min(0f)] public float enemyMovementSpeed = 0.6f;
        [Min(0f)] public float enemyHorizontalTravelRange = 0.6f;
        [Min(0f)] public float enemyVerticalTravelRange = 0.6f;
        [Min(0.1f)] public float enemyStompBounceStrength = 12f;
        [Min(0)] public int movingEnemyKillScore = 50;
        [Min(0f)] public float horizontalKnockback = 6f;
        [Min(0f)] public float verticalKnockback = 4f;
        [Min(0f)] public float enemySteeringSuppression = 0.2f;
        [Min(0f)] public float enemyKnockbackCooldown = 0.8f;
        [Tooltip("0 gives a different layout each run; use another value to repeat a layout.")]
        public int seed;
        [Tooltip("Log each procedural spawn index, type, initial position and gap. Off by default.")]
        public bool showGenerationDebug;

        [Min(0), Tooltip("Minimum score for weighted selection. All-zero eligible weights fall back to static.")]
        public int staticPlatformMinimumScore;
        [Min(0)] public int breakablePlatformMinimumScore = 500;
        [Min(0)] public int movingPlatformMinimumScore = 2000;
        [Header("Platform weights") ]
        [Min(0f), Tooltip("Relative spawn weight. All-zero eligible weights use static. All types share the same spawn positions.")]
        public float staticPlatformWeight = 55f;
        [Min(0f), Tooltip("Relative moving-platform weight. Travel is fitted to the shared spawn positions.")]
        public float movingPlatformWeight = 20f;
        [Min(0f), Tooltip("Relative breakable-platform weight, normalized with other unlocked types.")]
        public float breakablePlatformWeight = 25f;
        [Range(0f, 1f), Tooltip("Moving platforms: 0 is all vertical, 1 is all horizontal.")]
        public float horizontalMovementChance = 0.5f;
        [Min(0f), Tooltip("Generated speed is capped at 20% of normal bounce velocity.")]
        public float platformMovementSpeed = 0.7f;
        [HideInInspector, Min(0f), Tooltip("Legacy half-range, retained unchanged for serialized scenes and existing callers. Inspector shows twice this value.")]
        public float platformMovementDistance = 0.35f;
        [HideInInspector, SerializeField] float verticalTravelRange = -1f;
        public float HorizontalTravelRange { get => Mathf.Max(0f, platformMovementDistance) * 2f; set => platformMovementDistance = Mathf.Max(0f, value) * 0.5f; }
        public float VerticalTravelRange { get => verticalTravelRange < 0f ? HorizontalTravelRange : verticalTravelRange; set => verticalTravelRange = Mathf.Max(0f, value); }
        float MinimumGenerationGap => platformPrefab == null ? 0f :
            platformPrefab.GetComponent<BoxCollider2D>().size.y * Mathf.Abs(platformPrefab.transform.lossyScale.y) + minimumPlatformClearance;
        float MaximumGenerationGap => Mathf.Max(0.05f, Mathf.Min(maximumVerticalSpacing, player.JumpHeight * jumpHeightSafety));
        float BaseGapLimit => Mathf.Max(MinimumGenerationGap, Mathf.Min(MaximumGenerationGap,
            GapForHorizontalOffset(Mathf.Min(minimumHorizontalOffset, maximumHorizontalStep))));
        float MinimumBaseGap => Mathf.Clamp(Mathf.Max(minimumVerticalSpacing, player.JumpHeight * minimumJumpHeightFraction),
            Mathf.Min(MinimumGenerationGap, BaseGapLimit), BaseGapLimit);
        // A landing can use the destination's safe top surface, with the player's
        // entire width supported and a small edge inset. No origin-edge credit.
        float LandingReachAllowance
        {
            get
            {
                if (player == null || platformPrefab == null) return 0f;
                var shape = platformPrefab.GetComponent<BoxCollider2D>();
                var playerShape = player.GetComponent<BoxCollider2D>();
                return Mathf.Max(0f, shape.size.x * Mathf.Abs(platformPrefab.transform.lossyScale.x) * 0.5f -
                    playerShape.size.x * Mathf.Abs(player.transform.lossyScale.x) * 0.5f - 0.05f);
            }
        }
        // Upper bounds only: each branch is further constrained by both neighbors.
        public float HorizontalTravelLimit
        {
            get
            {
                if (player == null || platformPrefab == null) return 0f;
                float halfWidth = platformPrefab.GetComponent<BoxCollider2D>().size.x * Mathf.Abs(platformPrefab.transform.lossyScale.x) * 0.5f;
                float range = Mathf.Max(0f, Mathf.Min(horizontalSpawnRange, player.horizontalLimit - halfWidth));
                float reach = HorizontalReach(MinimumBaseGap);
                return Mathf.Min(range * 2f, Mathf.Max(0f, reach + LandingReachAllowance - Mathf.Min(minimumHorizontalOffset, maximumHorizontalStep, reach)));
            }
        }
        public float VerticalTravelLimit => player == null || platformPrefab == null ? 0f : Mathf.Max(0f,
            Mathf.Min(MaximumGenerationGap - MinimumBaseGap, MinimumBaseGap - MinimumGenerationGap));

        void OnValidate()
        {
            // Preserve the old shared half-range when introducing a separate total Y range.
            if (verticalTravelRange < 0f) verticalTravelRange = Mathf.Max(0f, platformMovementDistance) * 2f;
        }
        [Min(0f)] public float platformBreakDelay = 0.12f;
        public Color movingPlatformColor = new Color(0.3f, 0.65f, 0.95f);
        public Color breakablePlatformColor = new Color(0.95f, 0.55f, 0.25f);
        [Range(0.2f, 0.9f)] public float jumpHeightSafety = 0.88f;
        [Range(0.2f, 0.9f), Tooltip("Preferred minimum spacing as a fraction of normal jump height; safety checks can reduce it.")]
        public float minimumJumpHeightFraction = 0.72f;
        [Range(0.2f, 0.9f)] public float horizontalReachSafety = 0.85f;
        [Min(0.05f), Tooltip("Minimum empty vertical space between platform travel envelopes.")]
        public float minimumPlatformClearance = 0.2f;

        public readonly List<Transform> Platforms = new List<Transform>();
        public int TotalSpawned { get; private set; }
        public int TotalRemoved { get; private set; }
        enum PlatformKind { Static, Moving, Breakable }
        Vector2 lastPosition;
        Vector2 pendingPosition;
        bool hasPendingPosition;
        float lastHorizontalTravel;
        float lastVerticalTravel;
        int horizontalDirection = 1;
        public int CurrentRunScore => gameManager != null ? gameManager.Score : 0;
        System.Random random, contentRandom, enemyRandom;

        public bool IsRunning { get; private set; }
        Transform templates;
        bool initialized;

        public void Initialize()
        {
            if (initialized) return;
            initialized = true;
            // Preserve the authored starting layout and its serialized prefab overrides.
            var originals = new List<Transform>();
            foreach (Transform child in transform) originals.Add(child);
            templates = new GameObject("Starting layout (runtime templates)").transform;
            templates.SetParent(transform, false);
            templates.gameObject.SetActive(false);
            foreach (var child in originals) child.SetParent(templates, true);
        }

        public void ResetRun()
        {
            Initialize();
            ClearRun();
            int runSeed = seed == 0 ? new System.Random().Next() : seed;
            random = new System.Random(runSeed);
            contentRandom = new System.Random(unchecked(runSeed ^ 0x5f3759df));
            enemyRandom = new System.Random(unchecked(runSeed ^ 0x3942a1b7));
            lastPosition = new Vector2(0f, float.NegativeInfinity);
            foreach (Transform template in templates)
            {
                var branch = Instantiate(template.gameObject, template.position, template.rotation, transform);
                branch.transform.localScale = template.localScale;
                branch.SetActive(true);
                Platforms.Add(branch.transform);
                if (branch.transform.position.y >= lastPosition.y) lastPosition = branch.transform.position;
            }
            if (Platforms.Count == 0) lastPosition = new Vector2(0f, player.transform.position.y - 0.8f);
            hasPendingPosition = false;
            lastHorizontalTravel = lastVerticalTravel = 0f;
            horizontalDirection = random.NextDouble() < 0.5 ? -1 : 1;
            TotalSpawned = Platforms.Count;
            IsRunning = true;
            GenerateAhead();
        }

        public void SetRunning(bool running) => IsRunning = running;
        public void ClearRun()
        {
            IsRunning = false;
            foreach (var branch in Platforms)
            {
                if (branch == null) continue;
                branch.gameObject.SetActive(false);
                Destroy(branch.gameObject);
            }
            Platforms.Clear();
            TotalSpawned = TotalRemoved = 0;
            highestEnemyProgressionScore = 0; lastLethalPlatformIndex = -1;
            ProceduralPlatformsGenerated = EnemyPlatformsEvaluated = EnemyChancePasses = StaticEnemiesGenerated = MovingEnemiesGenerated = ShootingEnemiesGenerated = 0;
            EnemyPlacementSkips = EnemySafetySkips = EnemyWeightSkips = 0;
        }

        void Update()
        {
            if (!IsRunning || !player.IsAlive) return;
            GenerateAhead();
            float cutoff = player.HighestY - platformCleanupDistance;
            for (int i = Platforms.Count - 1; i >= 0; i--)
            {
                var branch = Platforms[i];
                // Broken branches are removed from the list without reusing their slots.
                // A mover stays until even its highest possible top is below cutoff.
                if (branch != null)
                {
                    var mover = branch.GetComponent<PlatformerMovingPlatform>();
                    float upperY = mover != null ? mover.UpperY : branch.position.y;
                    if (upperY >= cutoff) continue;
                    branch.gameObject.SetActive(false);
                    Destroy(branch.gameObject);
                }
                Platforms.RemoveAt(i);
                TotalRemoved++;
            }
        }

        void GenerateAhead()
        {
            float maxGap = MaximumGenerationGap;
            float targetHeight = Mathf.Max(player.transform.position.y + Mathf.Max(generationDistanceAhead, platformsAhead * maxGap),
                viewCamera.transform.position.y + viewCamera.orthographicSize + maxGap);
            var prefabShape = platformPrefab.GetComponent<BoxCollider2D>();
            float halfWidth = prefabShape.size.x * Mathf.Abs(platformPrefab.transform.lossyScale.x) * 0.5f;
            float range = Mathf.Max(0f, Mathf.Min(horizontalSpawnRange, player.horizontalLimit - halfWidth));
            while (lastPosition.y < targetHeight)
            {
                // Positions are type-independent. One lookahead position reserves room
                // for the next branch's sweep without inserting or moving any rows.
                if (!hasPendingPosition) pendingPosition = CalculateNextPosition(lastPosition, range);
                var position = pendingPosition;
                var nextPosition = CalculateNextPosition(position, range);
                hasPendingPosition = true;
                pendingPosition = nextPosition;
                var kind = ChoosePlatformKind();
                var axis = contentRandom.NextDouble() < horizontalMovementChance ?
                    PlatformerMovementAxis.Horizontal : PlatformerMovementAxis.Vertical;
                float horizontalTravel = 0f, verticalTravel = 0f;
                if (kind == PlatformKind.Moving)
                {
                    float incomingGap = position.y - lastPosition.y;
                    float outgoingGap = nextPosition.y - position.y;
                    float incomingX = Mathf.Abs(GetHorizontalDelta(lastPosition.x, position.x));
                    float outgoingX = Mathf.Abs(GetHorizontalDelta(position.x, nextPosition.x));
                    if (axis == PlatformerMovementAxis.Horizontal)
                    {
                        float incomingBudget = HorizontalReach(incomingGap + lastVerticalTravel) + LandingReachAllowance - incomingX - lastHorizontalTravel;
                        float outgoingBudget = (HorizontalReach(outgoingGap) + LandingReachAllowance - outgoingX) * 0.5f;
                        horizontalTravel = Mathf.Max(0f, Mathf.Min(Mathf.Min(HorizontalTravelRange, HorizontalTravelLimit) * 0.5f, incomingBudget, outgoingBudget,
                            range - Mathf.Abs(position.x)));
                    }
                    else
                    {
                        float incomingBudget = Mathf.Min(incomingGap - MinimumGenerationGap - lastVerticalTravel,
                            maxGap - incomingGap - lastVerticalTravel,
                            GapForHorizontalOffset(Mathf.Max(0f, incomingX + lastHorizontalTravel - LandingReachAllowance)) - incomingGap - lastVerticalTravel);
                        float outgoingBudget = Mathf.Min(outgoingGap - MinimumGenerationGap, maxGap - outgoingGap,
                            GapForHorizontalOffset(Mathf.Max(0f, outgoingX - LandingReachAllowance)) - outgoingGap) * 0.5f;
                        verticalTravel = Mathf.Max(0f, Mathf.Min(Mathf.Min(VerticalTravelRange, VerticalTravelLimit) * 0.5f, incomingBudget, outgoingBudget));
                    }
                }
                var branch = Instantiate(platformPrefab, position, Quaternion.identity, transform);
                int index = TotalSpawned++;
                branch.name = kind + " Branch " + index;
                if (kind == PlatformKind.Moving)
                {
                    float distance = axis == PlatformerMovementAxis.Horizontal ? horizontalTravel : verticalTravel;
                    branch.AddComponent<PlatformerMovingPlatform>().Configure(axis,
                        Mathf.Min(platformMovementSpeed, player.bounceSpeed * 0.2f), distance, player.horizontalLimit);
                    branch.GetComponent<SpriteRenderer>().color = movingPlatformColor;
                }
                else if (kind == PlatformKind.Breakable)
                {
                    branch.AddComponent<PlatformerBreakablePlatform>().breakDelay = platformBreakDelay;
                    branch.GetComponent<SpriteRenderer>().color = breakablePlatformColor;
                }
                Platforms.Add(branch.transform);
                SpawnExtras(branch);
                if (showGenerationDebug)
                    Debug.Log($"[TreeClimb Generation] index={index} type={kind} spawn=({position.x:F3}, {position.y:F3}) deltaY={position.y - lastPosition.y:F3}", branch);
                lastPosition = position;
                lastHorizontalTravel = horizontalTravel;
                lastVerticalTravel = verticalTravel;
            }
        }

        Vector2 CalculateNextPosition(Vector2 previous, float range)
        {
            float gap = RandomRange(MinimumBaseGap, BaseGapLimit);
            float step = Mathf.Min(maximumHorizontalStep, HorizontalReach(gap));
            return new Vector2(ChooseHorizontalPosition(previous, range, step), previous.y + gap);
        }

        public float GetHorizontalDelta(float from, float to)
        {
            float delta = to - from;
            return player.screenWrapping ? Mathf.Repeat(delta + player.horizontalLimit, player.horizontalLimit * 2f) - player.horizontalLimit : delta;
        }

        float ChooseHorizontalPosition(Vector2 previous, float range, float reach)
        {
            range = Mathf.Max(0f, range);
            int preferred = random.NextDouble() < horizontalDirectionPersistence ? horizontalDirection : -horizontalDirection;
            // Intersect reachable intervals, including neighboring copies of the wrap
            // domain. Fall back to a smaller offset only if the requested one cannot fit.
            for (int relaxed = 0; relaxed < 2; relaxed++)
            {
                float minimum = relaxed == 0 ? Mathf.Min(minimumHorizontalOffset, reach) : 0f;
                for (int side = 0; side < 2; side++)
                {
                    int direction = side == 0 ? preferred : -preferred;
                    float a = previous.x + direction * minimum;
                    float b = previous.x + direction * reach;
                    float lower = Mathf.Min(a, b), upper = Mathf.Max(a, b);
                    float period = player.horizontalLimit * 2f;
                    int copies = player.screenWrapping ? 1 : 0;
                    float total = 0f, selected = 0f;
                    bool found = false;
                    for (int image = -copies; image <= copies; image++)
                    {
                        float lo = Mathf.Max(lower, -range + image * period);
                        float hi = Mathf.Min(upper, range + image * period);
                        if (lo > hi) continue;
                        float weight = Mathf.Max(0.00001f, hi - lo);
                        total += weight;
                        if (!found || random.NextDouble() < weight / total)
                            selected = RandomRange(lo, hi) - image * period;
                        found = true;
                    }
                    if (!found) continue;
                    horizontalDirection = direction;
                    return selected;
                }
            }
            // Zero steering capacity or contradictory settings: preserve a landing point.
            return Mathf.Clamp(previous.x, -range, range);
        }

        PlatformKind ChoosePlatformKind()
        {
            int score = CurrentRunScore;
            float staticWeight = score >= staticPlatformMinimumScore ? Mathf.Max(0f, staticPlatformWeight) : 0f;
            float movingWeight = score >= movingPlatformMinimumScore ? Mathf.Max(0f, movingPlatformWeight) : 0f;
            float breakableWeight = score >= breakablePlatformMinimumScore ? Mathf.Max(0f, breakablePlatformWeight) : 0f;
            float total = staticWeight + movingWeight + breakableWeight;
            if (total <= 0f) return PlatformKind.Static;
            float choice = (float)contentRandom.NextDouble() * total;
            if (choice < staticWeight) return PlatformKind.Static;
            return choice < staticWeight + movingWeight ? PlatformKind.Moving : PlatformKind.Breakable;
        }

        float GapForHorizontalOffset(float offset)
        {
            float speed = Mathf.Max(0f, player.moveSpeed - Mathf.Abs(player.windStrength));
            if (speed <= 0f) return player.JumpHeight * jumpHeightSafety;
            float airtime = 2f * player.moveSpeed / Mathf.Max(0.1f, player.acceleration) +
                offset / Mathf.Max(0.01f, speed * horizontalReachSafety);
            airtime = Mathf.Max(player.bounceSpeed / player.Gravity, airtime);
            return Mathf.Max(0.05f, player.bounceSpeed * airtime - 0.5f * player.Gravity * airtime * airtime);
        }

        float HorizontalReach(float verticalGap)
        {
            float landingTime = (player.bounceSpeed + Mathf.Sqrt(Mathf.Max(0f,
                player.bounceSpeed * player.bounceSpeed - 2f * player.Gravity * verticalGap))) / player.Gravity;
            float controllableSpeed = Mathf.Max(0f, player.moveSpeed - Mathf.Abs(player.windStrength));
            float travel = controllableSpeed * Mathf.Max(0f,
                landingTime - 2f * player.moveSpeed / Mathf.Max(0.1f, player.acceleration)) * horizontalReachSafety;
            return Mathf.Max(0f, travel);
        }

        void SpawnExtras(GameObject branch)
        {
            ProceduralPlatformsGenerated++;
            var stage = CurrentEnemyStage; // Evaluated once for each new generated slot, never retroactively.
            // Leave the first few jumps safe. Extras share the branch's lifetime.
            if (branch.transform.position.y < 4f) return;
            if (contentRandom.NextDouble() < powerupSpawnChance)
            {
                CreateExtra(branch, "Leaf seed", new Vector2(0.35f, 0.35f), 0.7f,
                    new Color(0.4f, 1f, 0.35f)).AddComponent<LeafPowerup>();
            }
            EnemyPlatformsEvaluated++;
            // Consume the same single chance sample even in zero-chance stages, preserving platform/type RNG.
            double chance = contentRandom.NextDouble();
            if (stage == null || chance >= Mathf.Clamp01(stage.enemyChance)) return;
            EnemyChancePasses++;
            SpawnEnemy(branch, stage);
#if UNITY_EDITOR
            if (showEnemyDifficultyDebug)
                Debug.Log($"[Enemy Difficulty] slot={TotalSpawned - 1} score={EnemyProgressionScore} stage={stage.minimumScore} chance={stage.enemyChance:P0} weights={stage.staticWeight}/{stage.movingWeight}/{stage.shootingWeight} generated={StaticEnemiesGenerated}/{MovingEnemiesGenerated}/{ShootingEnemiesGenerated} skipped placement={EnemyPlacementSkips} safety={EnemySafetySkips} weights={EnemyWeightSkips}", branch);
#endif
        }

        void SpawnEnemy(GameObject branch, EnemyDifficultyStage stage)
        {
            // Validate the common supporting space before choosing among eligible stage weights.
            var platform = branch.GetComponent<BoxCollider2D>();
            if (platform == null || platform.size.x <= 0f || platform.size.y <= 0f || branch.GetComponentInChildren<PlatformEnemy>() != null)
            { EnemyPlacementSkips++; return; }
            float staticWeight = staticEnemyPrefab != null ? Mathf.Max(0f, stage.staticWeight) : 0f;
            float movingWeight = movingEnemyPrefab != null ? Mathf.Max(0f, stage.movingWeight) : 0f;
            float shootingWeight = shootingEnemyPrefab != null && enemyProjectilePrefab != null ? Mathf.Max(0f, stage.shootingWeight) : 0f;
            float total = staticWeight + movingWeight + shootingWeight;
            if (total <= 0f) { EnemyWeightSkips++; return; }
            double choice = enemyRandom.NextDouble() * total;
            var type = choice < staticWeight ? PlatformEnemyType.Static : choice < staticWeight + movingWeight ? PlatformEnemyType.Moving : PlatformEnemyType.Shooting;
            bool moving = type == PlatformEnemyType.Moving;
            int index = TotalSpawned - 1;
            if (type != PlatformEnemyType.Static && lastLethalPlatformIndex >= 0 &&
                index - lastLethalPlatformIndex <= Mathf.Max(0, minimumSafePlatformsBetweenLethalEnemies))
            { EnemySafetySkips++; return; }
            var prefab = moving ? movingEnemyPrefab : type == PlatformEnemyType.Shooting ? shootingEnemyPrefab : staticEnemyPrefab;
            if (type == PlatformEnemyType.Shooting && prefab.GetComponent<PlatformEnemyShooter>() == null) { EnemyPlacementSkips++; return; }
            var enemyShape = prefab.GetComponent<BoxCollider2D>();
            float halfWidth = enemyShape.size.x * Mathf.Abs(prefab.transform.lossyScale.x) * 0.5f;
            float halfHeight = enemyShape.size.y * Mathf.Abs(prefab.transform.lossyScale.y) * 0.5f;
            Vector2 center = branch.transform.TransformPoint(platform.offset);
            float platformHalfWidth = platform.size.x * Mathf.Abs(branch.transform.lossyScale.x) * 0.5f;
            float platformHalfHeight = platform.size.y * Mathf.Abs(branch.transform.lossyScale.y) * 0.5f;
            if (halfWidth > platformHalfWidth || halfWidth > player.horizontalLimit) { EnemyPlacementSkips++; return; }
            float left = Mathf.Max(center.x - platformHalfWidth + halfWidth, -player.horizontalLimit + halfWidth);
            float right = Mathf.Min(center.x + platformHalfWidth - halfWidth, player.horizontalLimit - halfWidth);
            if (left > right) { EnemyPlacementSkips++; return; }
            float x = Mathf.Clamp(center.x, left, right), horizontal = 0f, vertical = 0f;
            var axis = enemyRandom.NextDouble() < enemyHorizontalMovementChance ? PlatformerMovementAxis.Horizontal : PlatformerMovementAxis.Vertical;
            if (type != PlatformEnemyType.Static)
            {
                // Preserve a full player-width landing strip throughout the entire sweep.
                // Put the obstacle on the far side of the arrival, leaving the near side free.
                if (branch.GetComponentInChildren<LeafPowerup>() != null) { EnemyPlacementSkips++; return; }
                float playerWidth = player.GetComponent<BoxCollider2D>().size.x * Mathf.Abs(player.transform.lossyScale.x);
                float available = platformHalfWidth * 2f - halfWidth * 2f - playerWidth - 0.15f;
                if (available < 0f) { EnemyPlacementSkips++; return; }
                horizontal = moving && axis == PlatformerMovementAxis.Horizontal ? Mathf.Min(enemyHorizontalTravelRange, available) : 0f;
                var mover = branch.GetComponent<PlatformerMovingPlatform>();
                float sweepY = mover != null && mover.axis == PlatformerMovementAxis.Vertical ? mover.TravelDistance : 0f;
                float sweepX = mover != null && mover.axis == PlatformerMovementAxis.Horizontal ? mover.TravelDistance : 0f;
                float verticalRoom = pendingPosition.y - center.y - sweepY - VerticalTravelLimit * 0.5f - platformHalfHeight * 2f - halfHeight * 2f - minimumPlatformClearance;
                vertical = moving && axis == PlatformerMovementAxis.Vertical ? Mathf.Min(enemyVerticalTravelRange, Mathf.Max(0f, verticalRoom)) : 0f;
                if (moving && (axis == PlatformerMovementAxis.Horizontal ? horizontal : vertical) <= 0.01f) { EnemyPlacementSkips++; return; }
                float arrival = GetHorizontalDelta(lastPosition.x, center.x);
                float side = arrival >= 0f ? 1f : -1f;
                x = center.x + side * (platformHalfWidth - halfWidth - horizontal * 0.5f);
                float landingX = center.x - side * (platformHalfWidth - playerWidth * 0.5f - 0.05f);
                float dy = center.y - lastPosition.y + lastVerticalTravel + sweepY;
                if (Mathf.Abs(GetHorizontalDelta(lastPosition.x, landingX)) + lastHorizontalTravel + sweepX > HorizontalReach(dy)) { EnemyPlacementSkips++; return; }
                if (x - horizontal * 0.5f < left || x + horizontal * 0.5f > right) { EnemyPlacementSkips++; return; }
            }
            if (type == PlatformEnemyType.Static) StaticEnemiesGenerated++;
            else
            {
                lastLethalPlatformIndex = index;
                if (moving) MovingEnemiesGenerated++; else ShootingEnemiesGenerated++;
            }
            var enemy = Instantiate(prefab);
            enemy.transform.position = new Vector3(x, center.y + platformHalfHeight + halfHeight, 0f);
            enemy.transform.SetParent(branch.transform, true);
            enemy.name = type + " enemy";
            if (moving) enemy.ConfigureMoving(axis, enemyMovementSpeed, horizontal, vertical, enemyStompBounceStrength, movingEnemyKillScore);
            else if (type == PlatformEnemyType.Shooting)
            {
                enemy.ConfigureShooting(shootingEnemyStompBounce, shootingEnemyKillScore);
                enemy.GetComponent<PlatformEnemyShooter>().Configure(enemyProjectilePrefab, enemyFireInterval, enemyProjectileSpeed,
                    enemyProjectileLifetime, enemyProjectileSize, enemyShootingRange);
            }
            else enemy.Configure(horizontalKnockback, verticalKnockback, enemyKnockbackCooldown, enemySteeringSuppression);
        }

        GameObject CreateExtra(GameObject branch, string name, Vector2 size, float heightAboveTop, Color color)
        {
            var extra = new GameObject(name, typeof(SpriteRenderer), typeof(BoxCollider2D));
            // Set world size before parenting so the branch's thin Y scale doesn't squash the extra.
            var branchCollider = branch.GetComponent<BoxCollider2D>();
            // Collider bounds can still describe the prefab's old position before the first physics step.
            float top = branch.transform.TransformPoint(branchCollider.offset + Vector2.up * branchCollider.size.y * 0.5f).y;
            extra.transform.position = new Vector3(branch.transform.position.x,
                top + heightAboveTop, 0);
            extra.transform.localScale = new Vector3(size.x, size.y, 1f);
            extra.transform.SetParent(branch.transform, true);
            var renderer = extra.GetComponent<SpriteRenderer>();
            var branchRenderer = branch.GetComponent<SpriteRenderer>();
            renderer.sprite = branchRenderer.sprite;
            renderer.sharedMaterial = branchRenderer.sharedMaterial;
            renderer.sortingOrder = 1;
            renderer.color = color;
            var collider = extra.GetComponent<BoxCollider2D>();
            collider.size = Vector2.one;
            collider.isTrigger = true;
            return extra;
        }

        float RandomRange(float min, float max) => Mathf.Lerp(min, max, (float)random.NextDouble());
    }
}
