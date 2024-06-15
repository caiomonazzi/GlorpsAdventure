using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorKey : Item
{
    [Header("Settings")]
    public int keyID;

    public void OnPickedUp()
    {
        if (!PlayerStats.Instance.doorKeys.ContainsKey(keyID))
        {
            PlayerStats.Instance.doorKeys.Add(keyID, true);
        }
        else
        {
            Debug.LogWarning($"Key with ID {keyID} already exists in doorKeys.");
        }

        UIManager.Instance.UpdateUI(); //Update UI
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        onPickedUp += OnPickedUp;
        base.OnTriggerEnter2D(collision);
    }
}