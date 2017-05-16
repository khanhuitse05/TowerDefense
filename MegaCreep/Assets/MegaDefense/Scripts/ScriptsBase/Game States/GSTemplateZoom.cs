using System.Collections;
using UnityEngine;

public class GSTemplateZoom : GSTemplate
{
    CanvasGroup canvasGroup;
    protected Vector3 scaleEffect = new Vector3(3, 3, 3);
    protected float timeIn = 0.4f;
    protected float timeOut = 0.4f;
    protected override void Awake()
    {
        base.Awake();
        canvasGroup = guiMain.GetComponent<CanvasGroup>();
    }
  
    public override void onEnter()
    {
        base.onEnter();
        StartCoroutine(FadeOut());
    }
    public override void onExit()
    {
        parameters.Clear();
        onSuspend();
        StartCoroutine(FadeIn());
    }
    public virtual void ShowContent()
    {
    }

    public virtual void HideContent()
    {
    }
    IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(timeOut);
        guiMain.SetActive(true);
        canvasGroup.interactable = false;
        guiMain.transform.localScale = scaleEffect;
        iTween.ScaleTo(guiMain, Vector3.one, timeIn);
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.fixedDeltaTime / timeIn;
            yield return null;
        }
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        ShowContent();
    }
    IEnumerator FadeOut()
    {
        HideContent();
        canvasGroup.interactable = false;
        iTween.ScaleTo(guiMain, scaleEffect, timeIn);
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.fixedDeltaTime / timeOut;
            yield return null;
        }
        guiMain.SetActive(false);
    }
}
