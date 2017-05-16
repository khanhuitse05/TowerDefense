using System.Collections;
using UnityEngine;

public class GSTemplateFade : GSTemplate
{
    CanvasGroup canvasGroup;
    protected float timeIn = 0.5f;
    protected float timeOut = 0.5f;
    protected override void Awake()
    {
        base.Awake();
        canvasGroup = guiMain.GetComponent<CanvasGroup>();
    }
    public override void onEnter()
    {
        base.onEnter();
        if (canvasGroup != null)
        {
            StartCoroutine(FadeOut());
        }
        else
        {
            guiMain.SetActive(false);
        }
    }
    public override void onExit()
    {
        parameters.Clear();
        onSuspend();
        if (canvasGroup != null)
        {
            StartCoroutine(FadeIn());
        }
        else
        {
            guiMain.SetActive(true);
        }
    }
    IEnumerator FadeIn()
    {
        guiMain.SetActive(true);
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.deltaTime / timeIn;
            yield return null;
        }
        canvasGroup.interactable = true;
    }
    IEnumerator FadeOut()
    {
        canvasGroup.interactable = false;
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime / timeOut;
            yield return null;
        }
        guiMain.SetActive(false);
    }
}
