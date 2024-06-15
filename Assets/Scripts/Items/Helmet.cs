using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Helmet : Item
{

    public void OnPickedUp()
    {
        PlayerStats playerStats = PlayerStats.Instance;

        playerStats.HP = new DoubleInt(5, 5);
        UIManager.Instance.UpdateUI();
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        onPickedUp += OnPickedUp;
        base.OnTriggerEnter2D(collision);
    }

}
