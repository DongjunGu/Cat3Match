using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [SerializeField] private BoardManager _BoardManager;
    [SerializeField] private MainUI _MainUI;
    [SerializeField] private Text _ScoreText;

    public void OnClickPrevButton()
    {
        UnShowGameUI();
        _MainUI.ShowMainUI();
        _BoardManager.enabled = false;
    }

    public void ShowGameUI()
    {
        this.gameObject.SetActive(true);
    }

    public void UnShowGameUI()
    {
        this.gameObject.SetActive(false);  
    }

    public void SetScore(int value)
    {
        if(value > 0)
        {
            _ScoreText.text = string.Format("{0:#,###}", value);
        }
        else
        {
            _ScoreText.text = "0";
        }
    }
}
