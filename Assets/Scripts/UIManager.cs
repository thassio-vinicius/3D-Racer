using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public TMP_Text lapCounterText, bestLapTimeText, currentLapTimeText, currentPositionText, allPositionsText, countdownText, goText, raceResult;
    public GameObject resultsScreen, pauseScreen, trackUnlockMessage;
    public bool isPaused;
    private void Awake() 
    {
        instance = this;    
    }
    // Start is called before the first frame update
    void Start()
    {
        allPositionsText.text = "out of " + (RaceInfoManager.instance.noOfAI + 1).ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            PauseUnpause();
        }
    }

    public void PauseUnpause()
    {
        isPaused = !isPaused;

        pauseScreen.SetActive(isPaused);

        if(isPaused)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }


    public void ExitRace()
    {
                Time.timeScale = 1f;

        RaceManager.instance.ExitRace();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

   
}
