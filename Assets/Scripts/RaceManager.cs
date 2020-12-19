using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RaceManager : MonoBehaviour
{
    public Checkpoint[] allCheckpoints;
    public static RaceManager instance;
    public int totalLaps;
    public CarController playerCar;
    public List<CarController> allAICars = new List<CarController>();
    public int playerPosition;
    public float timeBetweenPosCheck = .2f;
    private float posCheckCounter;
    public float aiDefaultSpeed = 30f, playerDefaultSpeed = 30f, rubberBandSpeedMod = 3.5f, rubberBandAccel = .5f;
    public bool isStarting;
    public float timeBetweenStartCount = 1f;
    private float startCounter;
    public int countdownCurrent = 3;
    public int playerStartPos, aiNumberToSpawn;
    public Transform[] startPoints;
    public List<CarController> aiCars = new List<CarController>();
    public bool raceCompleted;
    public string raceCompleteScene;
    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        totalLaps = RaceInfoManager.instance.noOfLaps;
        aiNumberToSpawn = RaceInfoManager.instance.noOfAI;


        for (int i = 0; i < allCheckpoints.Length; i++)
        {
            allCheckpoints[i].cpNumber = i;
        }
        isStarting = true;
        startCounter = timeBetweenStartCount;

        UIManager.instance.countdownText.text = countdownCurrent + "!";

        playerStartPos = Random.Range(0, aiNumberToSpawn + 1);

        playerCar = Instantiate(RaceInfoManager.instance.racerToUse,
        startPoints[playerStartPos].transform.position,
        startPoints[playerStartPos].transform.rotation);

        playerCar.isAI = false;
        playerCar.GetComponent<AudioListener>().enabled = true;
        CameraSwitcher.instance.SetTarget(playerCar);


        for (int i = 0; i < aiNumberToSpawn + 1; i++)
        {
            if (i != playerStartPos)
            {
                int selectedCar = Random.Range(0, aiCars.Count);

                allAICars.Add(Instantiate(aiCars[selectedCar], startPoints[i].position, startPoints[i].rotation));

                if (aiCars.Count > aiNumberToSpawn - i)
                    aiCars.RemoveAt(selectedCar);

            }
        }



        UIManager.instance.currentPositionText.text = GetPlayerOrder(playerStartPos + 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (isStarting)
        {
            startCounter -= Time.deltaTime;
            if (startCounter <= 0)
            {
                countdownCurrent--;
                startCounter = timeBetweenStartCount;

                UIManager.instance.countdownText.text = countdownCurrent + "!";


                if (countdownCurrent == 0)
                {
                    isStarting = false;

                    UIManager.instance.countdownText.gameObject.SetActive(false);
                    UIManager.instance.goText.gameObject.SetActive(true);


                }
            }
        }
        else
        {
            posCheckCounter -= Time.deltaTime;
            if (posCheckCounter <= 0)
            {
                playerPosition = 1;

                foreach (CarController car in allAICars)
                {
                    if (car.currentLap > playerCar.currentLap)
                    {
                        playerPosition++;

                    }
                    else if (car.currentLap == playerCar.currentLap)
                    {
                        if (car.nextCheckpoint > playerCar.nextCheckpoint)
                        {
                            playerPosition++;

                        }
                        else if (car.nextCheckpoint == playerCar.nextCheckpoint)
                        {
                            if (Vector3.Distance(car.transform.position, allCheckpoints[car.nextCheckpoint].transform.position) < Vector3.Distance(playerCar.transform.position, allCheckpoints[car.nextCheckpoint].transform.position))
                            {
                                playerPosition++;
                            }
                        }
                    }
                }

                UIManager.instance.currentPositionText.text = GetPlayerOrder(playerPosition);
            }

            if (playerPosition == 1)
            {
                foreach (CarController car in allAICars)
                {
                    car.maxSpeed = Mathf.MoveTowards(car.maxSpeed, aiDefaultSpeed + rubberBandSpeedMod, rubberBandAccel * Time.deltaTime);
                }

                playerCar.maxSpeed = Mathf.MoveTowards(playerCar.maxSpeed, playerDefaultSpeed - rubberBandSpeedMod, rubberBandAccel * Time.deltaTime);
            }
            else
            {
                foreach (CarController car in allAICars)
                {
                    car.maxSpeed = Mathf.MoveTowards(car.maxSpeed, aiDefaultSpeed - (rubberBandSpeedMod * ((float)playerPosition / ((float)allAICars.Count + 1))), rubberBandAccel * Time.deltaTime);
                }

                playerCar.maxSpeed = Mathf.MoveTowards(playerCar.maxSpeed, playerDefaultSpeed + (rubberBandSpeedMod * ((float)playerPosition / ((float)allAICars.Count + 1))), rubberBandAccel * Time.deltaTime);

            }
        }
    }

    public string GetPlayerOrder(int position)
    {
        switch (position)
        {
            case 1:

                return "1st";
            case 2:
                return "2nd";
            case 3:
                return "3rd";
            default:
                return position + "th";
        }
    }

    public void FinishRace()
    {

        if (playerPosition == 1)
        {
            if (RaceInfoManager.instance.trackToUnlock != "")
            {
                if (!PlayerPrefs.HasKey(RaceInfoManager.instance.trackToUnlock + "_unlocked"))
                {
                    PlayerPrefs.SetInt(RaceInfoManager.instance.trackToUnlock + "_unlocked", 1);
                    UIManager.instance.trackUnlockMessage.SetActive(true);
                }
            }
        }

        raceCompleted = true;

        UIManager.instance.raceResult.text = "You finished " + GetPlayerOrder(playerPosition);

        UIManager.instance.resultsScreen.SetActive(true);
    }

    public void ExitRace()
    {
        SceneManager.LoadScene(raceCompleteScene);
    }
}
