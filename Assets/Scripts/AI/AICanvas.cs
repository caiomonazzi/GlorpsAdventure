using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class AICanvas : MonoBehaviour
{
    #region Variables:
    AIStats aIStats;

    [Header("Elements")]
    public Image hpBar;
    #endregion

    #region methods:
    private void Start()
    {
        aIStats = GetComponentInParent<AIStats>();
        UpdateUI();
    }

    //Method for updating the UI
    public void UpdateUI()
    {
        hpBar.fillAmount = aIStats.HP.current / aIStats.HP.max;
    }
    #endregion
}
