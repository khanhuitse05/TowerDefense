using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TDTK;

public enum _ItemState { Selected, Normal, Unavailable }
public class PerkItemUI : MonoBehaviour
{
    public GameObject start;
    public Transform startRoot;

    public Image imageIcon;
    _ItemState state = _ItemState.Normal;
    public Perk perk { get; set; }
    Button button;
    public void Init(Perk _perk)
    {
        button = gameObject.GetComponent<Button>();
        perk = _perk;
        imageIcon.sprite = _perk.icon;
        for (int i = 0; i < _perk.level; i++)
        {
            Utils.Spawn(start, startRoot);
        }
    }
    public void UpgragePerk()
    {
        Utils.Spawn(start, startRoot);
    }

    private Color colorSelected = new Color(1f, 75f / 255f, 75f / 255f, 1f);
    private Color colorNormal = new Color(0.5f, 0.5f, 0.5f, 196f / 255f);
    private Color colorUnavailable = new Color(.15f, .15f, .15f, 1);

    public void SetToSelected()
    {
        ColorBlock colors = button.colors;
        colors.normalColor = colorSelected;
        button.colors = colors;
        state = _ItemState.Selected;
    }
    public void SetToNormal()
    {
        ColorBlock colors = button.colors;
        colors.normalColor = colorNormal;
        button.colors = colors;
        if (perk.IsAvailable())
        {
            imageIcon.color = Color.white;
            state = _ItemState.Normal;
        }
        else
        {
            imageIcon.color = colorUnavailable;
            state = _ItemState.Unavailable;
        }
    }
}
