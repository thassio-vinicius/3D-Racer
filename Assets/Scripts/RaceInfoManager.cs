using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceInfoManager : MonoBehaviour
{
    public static RaceInfoManager instance;
    public string trackToLoad;
    public CarController racerToUse;
    public int noOfAI;
    public int noOfLaps;
    public bool enteredRace;
    public Sprite trackSprite, racerSprite;
    public string trackToUnlock;
    private void Awake()
    {
        if (instance == null)
        {

            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
