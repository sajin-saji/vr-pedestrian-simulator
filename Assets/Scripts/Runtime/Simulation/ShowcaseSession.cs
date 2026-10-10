using System.Collections;
using UnityEngine;
using VRPedestrianSimulator.Player;
using VRPedestrianSimulator.Traffic;

namespace VRPedestrianSimulator.Simulation
{
    /// <summary>
    /// Runs a crossing session: three levels, score (minus 10 per passing car
    /// while crossing), collision and level progression.
    /// </summary>
    public sealed class ShowcaseSession : MonoBehaviour
    {
        public PedestrianFirstPersonController player;
        public DemoTrafficSpawner traffic;
        private readonly Vector3 playerStart = new Vector3(0f, 0.02f, -7.1f);
        private bool isCrossing;
        private bool completed;
        private bool gameOver;
        private bool transitioning;
        private int level = 1;
        private int score = 100;
        private string announcement = "Approach the crosswalk. Look both ways.";
        private GUIStyle titleStyle;
        private GUIStyle textStyle;

        private void Start() { Restart(); }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R)) Restart();
            if (completed && Input.GetKeyDown(KeyCode.Space)) Restart();
        }

        public void BeginCrossing()
        {
            if (gameOver || transitioning || completed || isCrossing) return;
            isCrossing = true;
            announcement = "Crossing started — reach the opposite pavement.";
        }

        public void RegisterPassingVehicle()
        {
            if (!isCrossing || gameOver || transitioning || completed) return;
            score = Mathf.Max(0, score - 10);
            if (score == 0) EndSession("Score reached zero. Press R to retry.");
        }

        public void RegisterCollision()
        {
            if (gameOver || transitioning || completed) return;
            EndSession("Vehicle collision detected. Press R to retry.");
        }

        public void FinishCrossing()
        {
            if (!isCrossing || gameOver || transitioning || completed) return;
            isCrossing = false;
            transitioning = true;
            if (level < 3)
            {
                announcement = "Level " + level + " complete — preparing next scenario...";
                StartCoroutine(AdvanceLevel());
            }
            else
            {
                completed = true;
                transitioning = false;
                announcement = "All 3 demo scenarios completed. Space or R to restart.";
            }
        }

        private IEnumerator AdvanceLevel()
        {
            yield return new WaitForSeconds(2.2f);
            level++;
            score = 100;
            transitioning = false;
            announcement = "Level " + level + ": higher speed and denser traffic.";
            if (player != null) player.ResetTo(playerStart);
            if (traffic != null) { traffic.ClearVehicles(); traffic.SetDifficulty(level); }
        }

        private void EndSession(string reason)
        {
            gameOver = true;
            isCrossing = false;
            announcement = reason;
            if (player != null) { player.enabled = false; player.SetCursorLocked(false); }
        }

        public void Restart()
        {
            StopAllCoroutines();
            level = 1;
            score = 100;
            isCrossing = false;
            completed = false;
            gameOver = false;
            transitioning = false;
            announcement = "Approach the crosswalk. Look both ways.";
            if (player != null)
            {
                player.enabled = true;
                player.ResetTo(playerStart);
                player.SetCursorLocked(true);
            }
            if (traffic != null) { traffic.ClearVehicles(); traffic.SetDifficulty(level); }
        }

        private void OnGUI()
        {
            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 19, fontStyle = FontStyle.Bold };
                titleStyle.normal.textColor = Color.white;
                textStyle = new GUIStyle(GUI.skin.label) { fontSize = 14 };
                textStyle.normal.textColor = Color.white;
            }

            GUI.Box(new Rect(16, 16, 465, 152), GUIContent.none);
            GUI.Label(new Rect(30, 22, 450, 30), "PEDESTRIAN SIMULATOR — DESKTOP DEMO", titleStyle);
            GUI.Label(new Rect(30, 59, 440, 24), "Scenario: " + level + " / 3     Score: " + score +
                      "     Traffic: " + (traffic != null ? (traffic.CurrentCarSpeed * 3.6f).ToString("F0") : "—") + " km/h", textStyle);
            GUI.Label(new Rect(30, 87, 445, 30), announcement, textStyle);
            GUI.Label(new Rect(30, 119, 440, 27), "WASD move · Mouse look · Shift run · R restart · Esc cursor", textStyle);
        }
    }
}
