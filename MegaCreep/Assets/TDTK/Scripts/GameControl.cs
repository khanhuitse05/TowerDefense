using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TDTK;
using UnityEngine.SceneManagement;

namespace TDTK
{

    public enum _GameState { Play, Pause, Over }

    [RequireComponent(typeof(ResourceManager))]
    [RequireComponent(typeof(DamageTable))]

    public class GameControl : MonoBehaviour
    {

        public delegate void GameMessageHandler(string msg);
        public static event GameMessageHandler onGameMessageE;
        public static void DisplayMessage(string msg) { if (onGameMessageE != null) onGameMessageE(msg); }

        public delegate void GameOverHandler(int _star); //(star > 0) if win
        public static event GameOverHandler onGameOverE;

        public delegate void LifeHandler(int value);
        public static event LifeHandler onLifeE;

        private bool gameStarted = false;
        public static bool IsGameStarted() { return instance.gameStarted; }
        public static bool IsGameOver() { return instance.gameState == _GameState.Over ? true : false; }
        public _GameState gameState = _GameState.Play;
        public static _GameState GetGameState() { return instance.gameState; }

        public bool capLife = true;
        public int playerLifeCap = 0;
        public int playerLife = 10;
        public static int GetPlayerLife() { return instance.playerLife; }
        public static int GetPlayerLifeCap()
        {
            return instance.capLife ? instance.playerLifeCap + PerkManager.GetLifeCapModifier() : -1;
        }

        public bool enableLifeGen = false;
        public int lifeRegenRate = 0;

        public float sellTowerRefundRatio = 0.5f;

        public Transform rangeIndicator;
        private GameObject rangeIndicatorObj;

        public bool loadAudioManager = false;

        private float timeStep = 0.015f;

        public static GameControl instance;
        Transform thisT;

        void Awake()
        {

            Time.fixedDeltaTime = timeStep;

            instance = this;
            thisT = transform;

            ObjectPoolManager.Init();

            BuildManager buildManager = (BuildManager)FindObjectOfType(typeof(BuildManager));
            buildManager.Init();

            NodeGenerator nodeGenerator = (NodeGenerator)FindObjectOfType(typeof(NodeGenerator));
            if (nodeGenerator != null) nodeGenerator.Awake();
            PathFinder pathFinder = (PathFinder)FindObjectOfType(typeof(PathFinder));
            if (pathFinder != null) pathFinder.Awake();

            PathTD[] paths = FindObjectsOfType(typeof(PathTD)) as PathTD[];
            for (int i = 0; i < paths.Length; i++) paths[i].Init();

            for (int i = 0; i < buildManager.buildPlatforms.Count; i++) buildManager.buildPlatforms[i].Init();

            gameObject.GetComponent<ResourceManager>().Init();

            PerkManager perkManager = (PerkManager)FindObjectOfType(typeof(PerkManager));
            if (perkManager != null) perkManager.Init();

            if (loadAudioManager)
            {
                Instantiate(Resources.Load("AudioManager", typeof(GameObject)));
            }

            if (rangeIndicator)
            {
                rangeIndicator = (Transform)Instantiate(rangeIndicator);
                rangeIndicator.parent = thisT;
                rangeIndicatorObj = rangeIndicator.gameObject;
            }
            ClearSelectedTower();

            Time.timeScale = 1;
        }

        // Use this for initialization
        void Start()
        {


            UnitTower[] towers = FindObjectsOfType(typeof(UnitTower)) as UnitTower[];
            for (int i = 0; i < towers.Length; i++) BuildManager.PreBuildTower(towers[i]);

            //ignore collision between shootObject so they dont hit each other
            int soLayer = LayerManager.LayerShootObject();
            Physics.IgnoreLayerCollision(soLayer, soLayer, true);

            if (capLife) playerLife = GetPlayerLifeCap();
            if (onLifeE != null) onLifeE(0);

            if (enableLifeGen) StartCoroutine(LifeRegenRoutine());
        }

        void OnEnable()
        {
            UnitCreep.onDestinationE += OnUnitReachDestination;

            Unit.onDestroyedE += OnUnitDestroyed;
        }
        void OnDisable()
        {
            UnitCreep.onDestinationE -= OnUnitReachDestination;

            Unit.onDestroyedE -= OnUnitDestroyed;
        }

        void OnUnitDestroyed(Unit unit)
        {
            if (unit.IsCreep())
            {
                if (unit.GetUnitCreep().lifeValue > 0) GainLife(unit.GetUnitCreep().lifeValue);
            }
            else if (unit.IsTower())
            {
                if (unit.GetUnitTower() == selectedTower) _ClearSelectedTower();
            }
        }

        void OnUnitReachDestination(UnitCreep unit)
        {
            playerLife = Mathf.Max(0, playerLife - unit.lifeCost);

            if (onLifeE != null) onLifeE(-unit.lifeCost);
            if (gameState == _GameState.Play)
            {
                if (playerLife <= 0)
                {
                    gameState = _GameState.Over;
                    if (onGameOverE != null) onGameOverE(0);
                }
            }
        }

        IEnumerator LifeRegenRoutine()
        {
            float temp = 0;
            while (true)
            {
                yield return new WaitForSeconds(1);
                temp += lifeRegenRate + PerkManager.GetLifeRegenModifier();
                int value = 0;
                while (temp >= 1)
                {
                    value += 1;
                    temp -= 1;
                }
                if (value > 0) _GainLife(value);
            }
        }

        public static void GainLife(int value) { instance._GainLife(value); }
        public void _GainLife(int value)
        {
            playerLife += value;
            if (capLife) playerLife = Mathf.Min(playerLife, GetPlayerLifeCap());
            if (onLifeE != null) onLifeE(value);
        }

        public static void StartGame()
        {
            //if game is not yet started, start it now
            instance.gameStarted = true;
        }

        public static void GameWon() { instance.StartCoroutine(instance._GameWon()); }
        public IEnumerator _GameWon()
        {
            ResumeGame(); //call to reset ff speed
            yield return new WaitForSeconds(0.0f);
            if (gameState == _GameState.Play)
            {
                gameState = _GameState.Over;
                int _cap = GameControl.GetPlayerLifeCap();
                int _life = GameControl.GetPlayerLife();
                int _star = 3;
                if (_cap > 0)
                {
                    float percent = (float)_life / _cap;
                    if (percent < 0.5f)
                    {
                        _star = 1;
                    }
                    else if (percent < 0.9f)
                    {
                        _star = 2;
                    }
                }
                if (onGameOverE != null) onGameOverE(_star);
            }
        }

        UnitTower selectedTower;
        public static UnitTower GetSelectedTower() { return instance.selectedTower; }

        public static UnitTower Select(Vector3 pointer)
        {
            int layer = LayerManager.LayerTower();

            LayerMask mask = 1 << layer;
            Ray ray = Camera.main.ScreenPointToRay(pointer);
            RaycastHit hit;
            if (!Physics.Raycast(ray, out hit, Mathf.Infinity, mask)) return null;

            SelectTower(hit.transform.GetComponent<UnitTower>());

            return instance.selectedTower;
        }

        public static void SelectTower(UnitTower tower) { instance._SelectTower(tower); }
        public void _SelectTower(UnitTower tower)
        {
            _ClearSelectedTower();

            selectedTower = tower;

            if (tower.type == _TowerType.Block || tower.type == _TowerType.Resource) return;

            float range = tower.GetRange();

            Transform indicatorT = rangeIndicator;

            if (indicatorT != null)
            {
                indicatorT.parent = tower.thisT;
                indicatorT.position = tower.thisT.position;
                indicatorT.localScale = new Vector3(2 * range, 1, 2 * range);

                indicatorT.gameObject.SetActive(true);
            }
        }

        public static void ClearSelectedTower() { instance._ClearSelectedTower(); }
        public void _ClearSelectedTower()
        {
            selectedTower = null;

            rangeIndicatorObj.SetActive(false);
            rangeIndicator.parent = thisT;
        }

        public static void PauseGame()
        {
            instance.gameState = _GameState.Pause;
            Time.timeScale = 0;
        }
        public static void ResumeGame()
        {
            instance.gameState = _GameState.Play;
            UIHUD.instance.ToggleFF();
        }
        public static void ResetGame()
        {
            SceneControl.ResetLevel();
        }
        public static float GetSellTowerRefundRatio()
        {
            return instance.sellTowerRefundRatio;
        }

    }
}