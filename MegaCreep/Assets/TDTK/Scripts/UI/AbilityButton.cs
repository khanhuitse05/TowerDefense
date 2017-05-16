using System.Collections;
using System.Collections.Generic;
using TDTK;
using UnityEngine;
using UnityEngine.UI;

public class AbilityButton : MonoBehaviour {

    public Image imageIcon;
    public Text label;
    public GameObject iconTime;
    public GameObject iconEnergy;
    [HideInInspector]
    public Ability ability;

    public void Init()
    {
        isValid = true;
    }
    public void InitAbility(Ability _ability)
    {
        ability = _ability;
        imageIcon.sprite = _ability.icon;
        label.text = _ability.GetCostInt().ToString();
    }
    public AbilityButton Clone(string name, Vector3 posOffset)
    {
        GameObject newBut = (GameObject)MonoBehaviour.Instantiate(gameObject);
        newBut.name = name;
        AbilityButton abilityButton = newBut.GetComponent<AbilityButton>(); ;
        abilityButton.Init();

        newBut.transform.SetParent(transform.parent);
        newBut.transform.localPosition = transform.localPosition + posOffset;
        newBut.transform.localScale = transform.localScale;
        newBut.transform.localRotation = transform.localRotation;
        return abilityButton;
    }
    public void OnUse()
    {
        isValid = false;
        iconEnergy.SetActive(false);
        iconTime.SetActive(false);
        label.gameObject.SetActive(false);
    }
    public IEnumerator OnCouldownRoutine()
    {
        OnValidCouldown(false);
        if (ability.usedCount == ability.maxUseCount)
        {
            OnUse();
            yield break;
        }
        while (true)
        {
            string text = "";
            float duration = ability.currentCD;
            if (duration <= 0) break;

            if (duration > 60) text = Mathf.Floor(duration / 60).ToString("F0") + "m";
            else text = (Mathf.Ceil(duration)).ToString("F0") + "s";
            label.text = text;
            yield return new WaitForSeconds(0.1f);
        }
        OnValidCouldown(true);
    }
    public void OnValidCouldown(bool value)
    {
        if (value)
        {
            isValid = true;
            imageIcon.color = new Color(1f, 1f, 1f, 1f);
            iconEnergy.SetActive(true);
            iconTime.SetActive(false);
            label.text = ability.GetCostInt().ToString();
        }
        else
        {
            isValid = false;
            imageIcon.color = new Color(1f, .5f, .5f, 1f);
            iconEnergy.SetActive(false);
            iconTime.SetActive(true);
        }
    }
    public void OnValidEnergy(bool value)
    {
        if (value)
        {
            imageIcon.color = new Color(1f, 1f, 1f, 1f);
        }
        else
        {
            imageIcon.color = new Color(1f, .5f, .5f, 1f);
        }
    }
    bool isValid = true;
    private void Update()
    {
        if (isValid)
        {
            OnValidEnergy(AbilityManager.GetEnergy() >= ability.GetCost());
        }
    }
}
