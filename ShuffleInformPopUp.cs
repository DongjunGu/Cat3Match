using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShuffleInformPopUp : MonoBehaviour
{
    [SerializeField] private Image _Background;


    private void OnEnable()
    {
        Color color = _Background.color;
        color.a = 1.0f;
        _Background.color = color;

        FadeStart();
    }

    private void FadeStart()
    {
        LeanTween.alpha(_Background.rectTransform, 0f, 1.5f).setEase(LeanTweenType.linear).setOnComplete(FadeFinished);
    }

    void FadeFinished()
    {
        Color color = _Background.color;
        color.a = 1.0f;
        _Background.color = color;

        this.gameObject.SetActive(false);
    }

}