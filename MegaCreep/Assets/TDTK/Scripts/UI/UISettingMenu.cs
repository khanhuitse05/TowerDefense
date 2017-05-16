using UnityEngine;
using UnityEngine.UI;

public class UISettingMenu : MonoBehaviour
{

    public GameObject guiUI;
    private static UISettingMenu instance;

    public Slider sliderMusicVolume;
    public Slider sliderSFXVolume;
    void Awake()
    {
        instance = this;
        transform.localPosition = Vector3.zero;
        sliderMusicVolume.value = AudioManager.GetMusicVolume() * 100;
        sliderSFXVolume.value = AudioManager.GetSFXVolume() * 100;
    }

    void Start()
    {
        Hide();
    }

    public void OnCloseButton()
    {
        Hide();
    }

    public static bool isOn = true;
    public static void Show() { instance._Show(); }
    public void _Show()
    {
        isOn = true;
        guiUI.SetActive(isOn);
    }
    public static void Hide() { instance._Hide(); }
    public void _Hide()
    {
        isOn = false;
        guiUI.SetActive(isOn);
    }

    public void OnMusicVolumeSlider()
    {
        if (Time.timeSinceLevelLoad > 0.5f)
            AudioManager.SetMusicVolume(sliderMusicVolume.value / 100);
    }
    public void OnSFXVolumeSlider()
    {
        if (Time.timeSinceLevelLoad > 0.5f)
            AudioManager.SetSFXVolume(sliderSFXVolume.value / 100);
    }
}
