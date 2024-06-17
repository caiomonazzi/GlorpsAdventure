using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenuManager : MonoBehaviour
{
    [Header("Components")]
    public GameObject menuButtonsContainer; // Reference to the container holding menu buttons
    Button[] menuButtons; // Array to hold all menu buttons
    int currentButtonIndex = 0; // Index of the currently selected button
    public AnimatorManager mainMenuAnimatorManager; //Animation manager
    bool isAnyKeyDown; //splash screen status
    public GameObject loadGameBtn;
    bool menuLoaded = false; // Flag to indicate if menu is loaded
    bool directionalKeyPressed = false; // Flag to track if directional key has been pressed
    int directionalKeyCount = 0; // Counter for directional key presses

    private void Start()
    {
        if (PlayerPrefs.GetString("Saved_Level") != "")
            loadGameBtn.SetActive(true);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(AudioManager.Instance.music);
        }
        else
        {
            Debug.LogError("AudioManager instance is null.");
        }

        // Get all buttons from the menuButtonsContainer
        menuButtons = menuButtonsContainer.GetComponentsInChildren<Button>();

    }

    private void Update()
    {
        if (!isAnyKeyDown && Input.anyKeyDown) //if any key down
        {
            SplashScreenClose(); //Splash screen disable
                                 
        }

        // Handle directional navigation and actions only if menu is loaded
        if (menuLoaded)
        {
            HandleNavigationInput();
        }
    }

    // Handle keyboard navigation for menu buttons
    void HandleNavigationInput()
    {
        // Navigate up with W or Up Arrow
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            Navigate(-1); // Move selection up
            directionalKeyPressed = true; // Set directional key pressed
            directionalKeyCount++; // Increment directional key count
        }

        // Navigate down with S or Down Arrow
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            Navigate(1); // Move selection down
            directionalKeyPressed = true; // Set directional key pressed
            directionalKeyCount++; // Increment directional key count
        }

        // Confirm selection with Space if directional key has been pressed twice
        if (Input.GetKeyDown(KeyCode.Space) && directionalKeyPressed && directionalKeyCount >= 2)
        {
            // Execute button click on current selected button
            menuButtons[currentButtonIndex].onClick.Invoke();
        }

        // Go back with ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Implement go back logic here
            Debug.Log("Go Back");
        }
    }



    // Method to navigate through menu buttons
    void Navigate(int direction)
    {
        // Deselect current button
        menuButtons[currentButtonIndex].OnDeselect(null);

        // Update current index based on direction
        currentButtonIndex = (currentButtonIndex + direction + menuButtons.Length) % menuButtons.Length;

        // Select new button
        menuButtons[currentButtonIndex].OnSelect(null);
    }

    // Splash screen disable method
    void SplashScreenClose()
    {
        isAnyKeyDown = true;
        mainMenuAnimatorManager.PlayPlayableDirector(mainMenuAnimatorManager.timelineAssets[1], UnityEngine.Playables.DirectorWrapMode.None); // Play main menu animation
        menuLoaded = true; // Menu is fully loaded

        // Select the first button once menu is fully loaded
        menuButtons[currentButtonIndex].Select();
    }

    // New game method
    public void NewGame()
    {
        // Check if menu is loaded before proceeding
        if (menuLoaded)
        {
            ScenesManager.Instance.LoadLoadingScene("Lvl_0"); // Load level 1
        }
    }

    // Load game method
    public void LoadGame()
    {
        // Check if menu is loaded before proceeding
        if (menuLoaded)
        {
            ScenesManager.Instance.continueGame = true;
            ScenesManager.Instance.LoadLoadingScene(PlayerPrefs.GetString("Saved_Level"));
        }
    }

    //Game quit
    public void Quit()
    {
        Application.Quit();
    }

}