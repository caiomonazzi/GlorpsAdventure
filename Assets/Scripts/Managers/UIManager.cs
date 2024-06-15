using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    public static UIManager Instance; //Singleton

    //Ceched components
    PlayerStats playerStats;

    [Header("Components")]
    List<GameObject> HPUIObjects = new List<GameObject>(); //UI HP list
    public Transform hpParent; //HP parent, position for spawn
    public GameObject hpIconPrefab; //HP prefab for spawn
    public Sprite hpActiveSprite, hpDisableSprite; //Sprites for HP( 1 hpActive - you have 1 hp, 1 hpDisabled - you have taken damage  )

    public Text keyText, shotsText; // UI text

    [Header("SpaceShip Pieces UI")]
    public Text spaceShipPiecesText; // Text component for displaying collected pieces

    [Header("Screens GameObjects")]
    public GameObject dialogGO, shopGO;
    public GameObject pauseGo;
    public GameObject gameoverGO;

    [Header("Status")]
    public bool isPause;

    public event EventHandler dialogClosed; //Close dialog event

    //Singleton method
    void SingletonInit()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Awake()
    {
        SingletonInit();
    }

    private void Start()
    {
        playerStats = PlayerStats.Instance; //Set playerstats in static object of PlayerStats
        UpdateUI(); //UpdateUI
    }

    //Update ui method
    public void UpdateUI()
    {
        UpdateHP(); //Update HP 

        keyText.text = playerStats.doorKeys.Count.ToString();

        playerStats.weaponController = FindAnyObjectByType<WeaponController>();


        UpdateWeaponUI();

        UpdateSpaceShipPiecesUI();
    }

    // Update spaceship pieces UI method
    public void UpdateSpaceShipPiecesUI()
    {
        if (spaceShipPiecesText == null)
        {
            Debug.LogError("spaceShipPiecesText is not assigned in the UIManager.");
            return;
        }

        int collectedPiecesCount = playerStats.collectedPieces.Count;
        string collectedPiecesInfo = "";

        if (collectedPiecesCount == 0)
        {
            collectedPiecesInfo = "0";
        }
        else
        {
            collectedPiecesInfo = collectedPiecesCount.ToString() + " (";

            for (int i = 0; i < playerStats.collectedPieces.Count; i++)
            {
                collectedPiecesInfo += playerStats.collectedPieces[i].pieceName;
                if (i < playerStats.collectedPieces.Count - 1)
                {
                    collectedPiecesInfo += ", ";
                }
            }

            collectedPiecesInfo += ")";
        }

        spaceShipPiecesText.text = collectedPiecesInfo;
    }

    // Update weapon UI method
    public void UpdateWeaponUI()
    {
        if (playerStats.weaponController != null && playerStats.weaponController.currentWeapon != null)
        {
            shotsText.text = playerStats.weaponController.shotsRemaining.ToString();
        }
        else
        {
            shotsText.text = "No Weapon";
        }
    }


    //Update hp method
    public void UpdateHP()
    {
        //Loop for clear old hp
        for (int i = 0; i < HPUIObjects.Count; i++)
        {
            Destroy(HPUIObjects[i]);
        }
        HPUIObjects.Clear(); //Clear list

        //Loop for spawn new 
        for (int i = 0; i < playerStats.HP.max; i++)
        {
            Image hpIcon = Instantiate(hpIconPrefab, hpParent).GetComponent<Image>(); //Spawn prefab

            if (playerStats.HP.current > i) //check player hp
            {
                hpIcon.sprite = hpActiveSprite; //Set Active hp
            }
            else
            {
                hpIcon.sprite = hpDisableSprite; //Set disable hp
            }
            HPUIObjects.Add(hpIcon.gameObject); //Add object to list 
        }
    }

    //Pause method
    public void Pause()
    {
        isPause = !isPause; //Reverse pause status
        pauseGo.SetActive(!pauseGo.activeSelf); //Reverse pause screen active status 

        if (isPause)
        {
            UpdateSpaceShipPiecesUI(); // Update spaceship pieces UI when paused
        }
    }
    //UI GameOver method
    public void GameOver()
    {
        gameoverGO.SetActive(true); //gameover screen enable
    }

    //Load main menu method
    public void LoadMainMenu()
    {
        ScenesManager.Instance.LoadLoadingScene("MainMenu"); //Load main menu scene
    }

}
