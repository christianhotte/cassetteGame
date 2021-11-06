using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // This script controls the core game loop as well as the UI.

    // Lets the script be accessed from other scripts without using GetComponent  
    public static GameManager Instance { get; set; }

    // Cassette Lists to compare player guesses to correct answer
    [SerializeField] private List<CassetteTape> cassetteAnswer = new List<CassetteTape>();
    private List<CassetteTape> playerGuess = new List<CassetteTape>();

    // Reference to UI screens
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;

    public void AddGuess(CassetteTape tape)
    {
        playerGuess.Add(tape);

        if (playerGuess.Count == cassetteAnswer.Count)
        {
            CheckWin();
        }
    }

    #region UI

    public void CloseWindow(GameObject window)
    {
        window.SetActive(false);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit");
    }

    #endregion

    private void Awake()
    {
        Instance = this;
    }

    private void CheckWin()
    {
        // checks if the two lists are equal, order matters.
        var isEqual = playerGuess.SequenceEqual(cassetteAnswer);

        // checks if isEqual is true or false then prints the corresponding string to the console.
        if (isEqual)
        {
            winScreen.SetActive(true);
        }
        else
        {
            loseScreen.SetActive(true);
        }
    }

}
