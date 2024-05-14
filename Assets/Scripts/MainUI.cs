using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainUI : MonoBehaviour
{
    [SerializeField] private BoardManager _GamePlay;
    [SerializeField] private GameUI _GameUI;
        
    public void ShowMainUIOnOff(bool isShow)
    {
        this.gameObject.SetActive(isShow);
    }

    public void ShowMainUI()
    {
        this.gameObject.SetActive(true);
    }

    public void UnShowMainUI()
    {
        this.gameObject.SetActive(false);
    }

    public void OnClickGameStartButton()
    {
        AudioManager.Instance.PlayButtonSound();
        UnShowMainUI();
        _GameUI.ShowGameUI();
        _GamePlay.enabled = true;
        _GamePlay.GameStart();

    }
}