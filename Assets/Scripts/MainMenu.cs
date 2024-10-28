using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Reference to the pause menu UI
    public GameObject pauseMenuUI;

    // Reference to the zombie board instance
    private PotionBoard potionBoard;

    void Start()
    {
        potionBoard = PotionBoard.Instance;
    }

    // Method to go back to the main menu
    public void mainMenu()
    {
        Time.timeScale = 1; // Ensure time is unpaused before loading
        SceneManager.LoadSceneAsync(0);
    }

    // Method to play Level 1
    public void PlayLevel1()
    {
        Time.timeScale = 1; // Ensure time is unpaused before loading
        SceneManager.LoadSceneAsync(1);
    }

    // Method to play Level 2
    public void PlayLevel2()
    {
        Time.timeScale = 1; // Ensure time is unpaused before loading
        SceneManager.LoadSceneAsync(2);
    }

    // Quit the game
    public void Quit()
    {
        Application.Quit();
    }
    public void PauseGame()
    {
        // Pause potions in PotionBoard
        if (PotionBoard.Instance != null)
        {
            foreach (GameObject potion in PotionBoard.Instance.potionsToPause)
            {
                if (potion != null) // Check if the object has not been destroyed
                {
                    potion.SetActive(false); // Deactivate each potion
                }
            }
        }

        // Pause potions in PotionBoard2
        if (PotionBoard2.Instance2 != null)
        {
            foreach (GameObject potion in PotionBoard2.Instance2.potionsToPause2)
            {
                if (potion != null) // Check if the object has not been destroyed
                {
                    potion.SetActive(false); // Deactivate each potion
                }
            }
        }

        // Pause the game
        Time.timeScale = 0; // Pause game time
        pauseMenuUI.SetActive(true); // Show pause menu UI
    }



    public void ResumeGame()
    {
        // Resume potions in PotionBoard
        if (PotionBoard.Instance != null)
        {
            foreach (GameObject potion in PotionBoard.Instance.potionsToPause)
            {
                if (potion != null) // Check if the object has not been destroyed
                {
                    potion.SetActive(true); // Reactivate each potion
                }
            }
        }

        // Resume potions in PotionBoard2
        if (PotionBoard2.Instance2 != null)
        {
            foreach (GameObject potion in PotionBoard2.Instance2.potionsToPause2)
            {
                if (potion != null) // Check if the object has not been destroyed
                {
                    potion.SetActive(true); // Reactivate each potion
                }
            }
        }

        // Resume the game
        Time.timeScale = 1; // Resume game time
        pauseMenuUI.SetActive(false); // Hide pause menu UI
    }



}
