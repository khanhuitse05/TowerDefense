using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace TDTK
{
    public class UIAbilityButton : MonoBehaviour
    {
        public List<AbilityButton> buttonList = new List<AbilityButton>();

        public RectTransform energyRect;
        public Text txtEnergy;

        public GameObject tooltipObj;
        public Text txtTooltipName;
        public Text txtTooltipCost;
        public Text txtTooltipDesp;

        public static UIAbilityButton instance;
        List<Ability> abList;

        void Awake()
        {
            instance = this;
        }
        // Use this for initialization
        void Start()
        {
            tooltipObj.SetActive(false);
            if (!AbilityManager.IsOn())
            {
                Hide();
                return;
            }

            abList = AbilityManager.GetAbilityList();
            if (abList.Count == 0)
            {
                Hide();
                return;
            }
            EventTrigger.Entry entryRequireTargetSelect = SetupTriggerEntry(true);
            EventTrigger.Entry entryDontRequireTargetSelect = SetupTriggerEntry(false);

            for (int i = 0; i < abList.Count; i++)
            {
                if (i == 0) buttonList[0].Init();
                else if (i > 0)
                {
                    buttonList.Add(buttonList[0].Clone("button" + (i + 1), new Vector3(i * 155, 0, 0)));
                }
                buttonList[i].InitAbility(abList[i]);

                EventTrigger trigger = buttonList[i].gameObject.GetComponent<EventTrigger>();
                if (abList[i].requireTargetSelection) trigger.triggers.Add(entryRequireTargetSelect);
                else trigger.triggers.Add(entryDontRequireTargetSelect);
            }
        }


        private EventTrigger.Entry SetupTriggerEntry(bool requireTargetSelection)
        {
            UnityEngine.Events.UnityAction<BaseEventData> call = new UnityEngine.Events.UnityAction<BaseEventData>(OnAbilityButton);
            EventTriggerType eventID = EventTriggerType.PointerClick;
#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
            if (requireTargetSelection) eventID = EventTriggerType.PointerDown;
#endif
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = eventID;
            entry.callback = new EventTrigger.TriggerEvent();
            entry.callback.AddListener(call);
            return entry;
        }

        void OnEnable()
        {
            AbilityManager.onAbilityActivatedE += OnAbilityActivated;
        }
        void OnDisable()
        {
            AbilityManager.onAbilityActivatedE -= OnAbilityActivated;
        }

        public void OnAbilityButton(UnityEngine.EventSystems.BaseEventData baseEvent)
        {
            OnAbilityButton(baseEvent.selectedObject);
        }
        public void OnAbilityButton(GameObject butObj)
        {
            //in drag and drop mode, player could be hitting the button while having an active tower selected
            //if that's the case, clear the selectedTower first. and we can show the tooltip properly
            if (UI.UseDragNDrop() && GameControl.GetSelectedTower() != null)
            {
                UI.ClearSelectedTower();
                return;
            }
            UI.ClearSelectedTower();
            int ID = GetButtonID(butObj);

            string exception = AbilityManager.SelectAbility(ID);
            if (exception != "") UIGameMessage.DisplayMessage(exception);
        }
        public void OnHoverAbilityButton(GameObject butObj)
        {
            if (GameControl.GetSelectedTower() != null) return;

            int ID = GetButtonID(butObj);
            Ability ability = AbilityManager.GetAbilityList()[ID];

            txtTooltipName.text = ability.name;
            txtTooltipCost.text = "" + ability.GetCostInt();
            txtTooltipDesp.text = ability.GetDesp();

            tooltipObj.SetActive(true);
        }
        public void OnExitHoverAbilityButton(GameObject butObj)
        {
            tooltipObj.SetActive(false);
        }
        int GetButtonID(GameObject butObj)
        {
            for (int i = 0; i < buttonList.Count; i++)
            {
                if (buttonList[i].gameObject == butObj) return i;
            }
            return 0;
        }

        void OnAbilityActivated(Ability ab)
        {
            int ID = ab.ID;
            StartCoroutine(buttonList[ID].OnCouldownRoutine());
        }

        void Update()
        {
            txtEnergy.text = AbilityManager.GetEnergy().ToString("f0") + "/" + AbilityManager.GetEnergyFull().ToString("f0");
            float valueX = Mathf.Clamp(AbilityManager.GetEnergy() / AbilityManager.GetEnergyFull() * 200, 4, 200);
            float valueY = Mathf.Min(valueX, 20);
            energyRect.sizeDelta = new Vector2(valueX, valueY);
        }

        public static bool isOn = true;
        public static void Show() { instance._Show(); }
        public void _Show()
        {
            isOn = true;
            gameObject.SetActive(isOn);
        }
        public static void Hide() { instance._Hide(); }
        public void _Hide()
        {
            isOn = false;
            gameObject.SetActive(isOn);
        }
    }
}