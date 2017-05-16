using UnityEngine;
using UnityEngine.UI;

namespace TDTK
{
    public class UIHUD : MonoBehaviour
    {

        public Text txtLife;
        public Text txtWave;
        public Text txtScore;
        public GameObject scoreObj;

        public Text txtTimer;
        public GameObject buttonSpawn;

        public Text txtRsc;

        static UIHUD _instance;
        public static UIHUD instance { get { return _instance; } }
        bool init = false;
        // Use this for initialization
        void Start()
        {
            _instance = this;
            init = true;
            txtTimer.text = "Start Game";

            scoreObj.SetActive(false);
            OnLife(0);
            OnNewWave(1);
            OnResourceChanged(0);

            if (SpawnManager.AutoStart())
            {
                buttonSpawn.SetActive(false);
                OnSpawnTimer(SpawnManager.GetAutoStartDelay());
            }
        }

        void OnEnable()
        {
            GameControl.onLifeE += OnLife;

            SpawnManager.onNewWaveE += OnNewWave;
            SpawnManager.onEnableSpawnE += OnEnableSpawn;
            SpawnManager.onSpawnTimerE += OnSpawnTimer;
            ResourceManager.onRscChangedE += OnResourceChanged;
        }
        void OnDisable()
        {
            GameControl.onLifeE -= OnLife;

            SpawnManager.onNewWaveE -= OnNewWave;
            SpawnManager.onEnableSpawnE -= OnEnableSpawn;
            SpawnManager.onSpawnTimerE -= OnSpawnTimer;
            ResourceManager.onRscChangedE -= OnResourceChanged;
        }

        void OnLife(int changedvalue)
        {
            int cap = GameControl.GetPlayerLifeCap();
            string text = (cap > 0) ? "/" + cap : "";
            txtLife.text = GameControl.GetPlayerLife() + text;
        }

        void OnNewWave(int waveID)
        {
            int totalWaveCount = SpawnManager.GetTotalWaveCount();
            string text = totalWaveCount > 0 ? "/" + totalWaveCount : "";
            txtWave.text = "WAVE " + waveID + text;
            if (GameControl.IsGameStarted()) buttonSpawn.SetActive(false);
        }

        void OnResourceChanged(int valueChangedList)
        {
            if (init)
            {
                int _rsc = ResourceManager.GetResource();
                txtRsc.text = _rsc.ToString();
            }
        }

        public void OnSpawnButton()
        {
            timerDuration = 0;
            SpawnManager.Spawn();
            buttonSpawn.SetActive(false);
        }

        void OnEnableSpawn()
        {
            buttonSpawn.SetActive(true);
        }

        private float timerDuration = 0;
        void OnSpawnTimer(float duration) { timerDuration = duration; }
        void FixedUpdate()
        {
            if (timerDuration > 0)
            {
                if (timerDuration < 60)
                    txtTimer.text = "Next Wave in " + (Mathf.Ceil(timerDuration)).ToString("f0") + "s";
                else
                    txtTimer.text = "Next Wave in " + (Mathf.Floor(timerDuration / 60)).ToString("f0") + "m";
                timerDuration -= Time.fixedDeltaTime;
            }
        }



        public Toggle toggleFastForward;
        public void ToggleFF()
        {
            if (toggleFastForward.isOn) Time.timeScale = UI.GetFFTime();
            else Time.timeScale = 1;
        }

        public void OnPauseButton()
        {
            _GameState gameState = GameControl.GetGameState();
            if (gameState == _GameState.Over) return;

            if (toggleFastForward.isOn) toggleFastForward.isOn = false;

            if (gameState == _GameState.Pause)
            {
                GameControl.ResumeGame();
                UIPauseMenu.Hide();
            }
            else
            {
                GameControl.PauseGame();
                UIPauseMenu.Show();
            }
        }
    }
}