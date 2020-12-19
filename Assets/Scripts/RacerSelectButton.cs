using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RacerSelectButton : MonoBehaviour
{
    public Image racerImage;
    public CarController racerToSet;
    // Start is called before the first frame update
public void SelectRacer()
{
    RaceInfoManager.instance.racerToUse = racerToSet;
    MainMenu.instance.racerSelectImage.sprite = racerImage.sprite;
    RaceInfoManager.instance.racerSprite = racerImage.sprite;

    MainMenu.instance.CloseRacerSelect();
}
}
