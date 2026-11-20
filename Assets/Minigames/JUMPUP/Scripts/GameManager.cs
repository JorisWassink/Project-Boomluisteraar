using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TreeClimb
{
    public enum PlatformerRunState { NotStarted, Playing, GameOver }

    [DefaultExecutionOrder(-100)]
    public class GameManager : MonoBehaviour
    {
        public PlayerController player;
        public Camera viewCamera;
        public PlatformGenerator platformGenerator;
        public Text heightLabel;
        public Text finalHeightLabel;
        public GameObject gameOverPanel;
        public GameObject startPanel;
        public GameObject playingPanel;
        public GameObject touchControls;
        public GameObject calibrateButton;
        public Text controlModeLabel;
        [Header("Pause and debug UI")]
        public GameObject pausePanel;
        public GameObject debugControlsPanel;
        public GameObject debugToggleButton;
        public Text debugToggleLabel;
        public bool showDebugControls = true;
        public bool hideDebugControlsInReleaseBuilds;
        public bool IsPaused { get; private set; }
        public bool DebugControlsAvailable => showDebugControls &&
            (!hideDebugControlsInReleaseBuilds || Application.isEditor || Debug.isDebugBuild);
        bool debugExpanded;
        [Tooltip("Game Over after falling this far below the highest world Y reached this run.")]
        [Min(1f)] public float deathFallDistance = 10f;
        [Min(0.01f)] public float scoreScale = 100f;
        public float MaximumHeight { get; private set; }
        public int HeightScore { get; private set; }
        public int EnemyBonusScore { get; private set; }
        public int Score => (int)Math.Min(int.MaxValue, (long)HeightScore + EnemyBonusScore);
        public PlatformerRunState State { get; private set; }
        public bool IsGameOver => State == PlatformerRunState.GameOver;
        public PlatformerRunResult? LastResult { get; private set; }
        public event Action<PlatformerRunResult> RunFinished;
        public event Action ExitRequested;
        Vector2 startingPosition;
        CameraController cameraController;
        bool initialized;

        void Awake() => Initialize();
        void Initialize()
        {
            if (initialized) return;
            initialized = true;
            startingPosition = player.transform.position;
            cameraController = viewCamera.GetComponent<CameraController>();
            cameraController.CaptureStart();
            platformGenerator.gameManager = this;
            platformGenerator.Initialize();
            player.ResetRun(startingPosition, false);
            State = PlatformerRunState.NotStarted;
            // Editor defaults remain keyboard; standalone Android starts with tilt.
            if (Application.isMobilePlatform) player.SetControlMode(PlatformerControlMode.DeviceTilt);
            RefreshUI();
        }
        void Update()
        {
            if (State == PlatformerRunState.Playing && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (IsPaused) ResumeRun(); else PauseRun();
            }
            if (IsPaused) return;
            if (State == PlatformerRunState.GameOver)
            {
                if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) RestartRun();
                return;
            }
            if (State != PlatformerRunState.Playing) return;
            UpdateHeightScore();
            heightLabel.text = "Score: " + Score;
            // Preserve the original maximum-fall death condition, independent of camera position.
            if (player.HighestY - player.Body.position.y <= deathFallDistance) return;
            GameOver();
        }
        void UpdateHeightScore()
        {
            float previousMaximum = MaximumHeight;
            float currentHeight = Mathf.Max(0f, player.Body.position.y - startingPosition.y);
            MaximumHeight = Mathf.Max(MaximumHeight, player.HighestY - startingPosition.y, currentHeight);
            // Camera look-ahead keeps the player below the upper quarter in this prototype.
            // Use rising world height so following downward never blocks or creates score.
            if (player.Body.linearVelocity.y > 0f && currentHeight > previousMaximum)
                HeightScore = Mathf.Max(HeightScore, Mathf.FloorToInt(currentHeight * scoreScale));
        }
        // Fall death and lethal enemy contact share this single guarded transition.
        public void GameOver()
        {
            if (State != PlatformerRunState.Playing || IsPaused) return;
            UpdateHeightScore();
            State = PlatformerRunState.GameOver;
            player.Stop();
            platformGenerator.SetRunning(false);
            var result = new PlatformerRunResult(Score, MaximumHeight, CalculateXp(Score));
            LastResult = result;
            RefreshUI();
            RunFinished?.Invoke(result);
        }
        internal void AwardEnemyBonus(int points)
        {
            if (State != PlatformerRunState.Playing || IsPaused || points <= 0) return;
            EnemyBonusScore = (int)Math.Min(int.MaxValue, (long)EnemyBonusScore + points);
            heightLabel.text = "Score: " + Score;
        }
        public void StartRun()
        {
            Initialize();
            if (State == PlatformerRunState.Playing) return;
            BeginRun();
        }
        public void RestartRun() { Initialize(); BeginRun(); }
        void BeginRun()
        {
            IsPaused = debugExpanded = false;
            State = PlatformerRunState.Playing;
            MaximumHeight = 0f;
            HeightScore = EnemyBonusScore = 0;
            LastResult = null;
            player.ResetRun(startingPosition, true);
            cameraController.ResetRun();
            platformGenerator.ResetRun();
            Physics2D.SyncTransforms();
            RefreshUI();
        }
        public void QuitRun()
        {
            Initialize();
            IsPaused = debugExpanded = false;
            player.ResetRun(startingPosition, false);
            platformGenerator.ClearRun();
            cameraController.ResetRun();
            State = PlatformerRunState.NotStarted;
            HeightScore = EnemyBonusScore = 0;
            MaximumHeight = 0f;
            LastResult = null;
            RefreshUI();
            // Always provide a standalone start-screen fallback; the host may unload afterward.
            ExitRequested?.Invoke();
        }
        public void PauseRun()
        {
            if (State != PlatformerRunState.Playing || IsPaused) return;
            IsPaused = true;
            debugExpanded = false;
            player.SetPaused(true);
            platformGenerator.SetRunning(false);
            RefreshUI();
        }
        public void ResumeRun()
        {
            if (!IsPaused) return;
            IsPaused = debugExpanded = false;
            player.SetPaused(false);
            platformGenerator.SetRunning(State == PlatformerRunState.Playing);
            RefreshUI();
        }
        public void ToggleDebugControls()
        {
            if (!IsPaused || !DebugControlsAvailable) return;
            debugExpanded = !debugExpanded;
            RefreshUI();
        }
        public static int CalculateXp(int score) => score < 500 ? 5 : score < 1000 ? 10 :
            score < 2000 ? 15 : score < 3000 ? 20 : 30;
        public void UseKeyboard() => SetControlMode(PlatformerControlMode.Keyboard);
        public void UseDeviceTilt() => SetControlMode(PlatformerControlMode.DeviceTilt);
        public void UseTouchButtons() => SetControlMode(PlatformerControlMode.TouchButtons);
        public void CalibrateTilt() => player.CalibrateTilt();
        public void SetControlMode(PlatformerControlMode mode) { player.SetControlMode(mode); RefreshUI(); }
        void RefreshUI()
        {
            startPanel.SetActive(State == PlatformerRunState.NotStarted);
            playingPanel.SetActive(State == PlatformerRunState.Playing);
            gameOverPanel.SetActive(State == PlatformerRunState.GameOver);
            pausePanel.SetActive(State == PlatformerRunState.Playing && IsPaused);
            debugToggleButton.SetActive(DebugControlsAvailable);
            debugControlsPanel.SetActive(IsPaused && debugExpanded && DebugControlsAvailable);
            debugToggleLabel.text = (debugExpanded ? "[-] " : "[+] ") + "Debug Controls";
            touchControls.SetActive(State == PlatformerRunState.Playing && !IsPaused && player.controlMode == PlatformerControlMode.TouchButtons);
            calibrateButton.SetActive(player.controlMode == PlatformerControlMode.DeviceTilt);
            heightLabel.text = "Score: " + Score;
            controlModeLabel.text = "Controls: " + (player.controlMode == PlatformerControlMode.Keyboard ? "Keyboard" :
                player.controlMode == PlatformerControlMode.TouchButtons ? "Touch buttons" :
                player.TiltAvailable ? "Device tilt" : "Tilt unavailable - choose Touch");
            if (LastResult.HasValue)
                finalHeightLabel.text = "GAME OVER\n\nScore: " + Score + "\nXP earned: " + LastResult.Value.XpReward;
        }
        // Apply before starting; never change the main game's progression or save data.
        public void Configure(PlatformerGameplaySettings settings)
        {
            Initialize();
            if (State == PlatformerRunState.Playing) throw new InvalidOperationException("Configure before starting a run.");
            if (settings == null) return;
            if (settings.MoveSpeed.HasValue) player.moveSpeed = Mathf.Max(0.1f, settings.MoveSpeed.Value);
            if (settings.WindStrength.HasValue) player.windStrength = settings.WindStrength.Value;
            if (settings.BounceStrength.HasValue) player.bounceSpeed = Mathf.Max(0.1f, settings.BounceStrength.Value);
            if (settings.MinimumPlatformSpacing.HasValue) platformGenerator.minimumVerticalSpacing = Mathf.Max(0.1f, settings.MinimumPlatformSpacing.Value);
            if (settings.MaximumPlatformSpacing.HasValue) platformGenerator.maximumVerticalSpacing = Mathf.Max(0.1f, settings.MaximumPlatformSpacing.Value);
            if (settings.EnemySpawnChance.HasValue) platformGenerator.SetEnemySpawnChance(settings.EnemySpawnChance.Value);
            if (settings.PowerupSpawnChance.HasValue) platformGenerator.powerupSpawnChance = Mathf.Clamp01(settings.PowerupSpawnChance.Value);
            if (settings.ScreenWrapping.HasValue) player.screenWrapping = settings.ScreenWrapping.Value;
            if (settings.ControlMode.HasValue) SetControlMode(settings.ControlMode.Value);
        }

        public void ReturnToWorldMap()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("WorldMap");
        }
    }
}
