using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class CameraSwitcher : MonoBehaviour
{
    public static CameraSwitcher instance;
    public GameObject[] cameras;
    private int currentCam;
    public CameraController topDownCam;
    public Cinemachine.CinemachineVirtualCamera cineCam;
    // Start is called before the first frame update
    private void Awake() 
    {
        instance = this;
    }
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            currentCam++;

            if(currentCam >= cameras.Length)
            {
                currentCam = 0;
            }

            for(int i = 0; i < cameras.Length; i++)
            {
                if(i == currentCam)
                {
                    cameras[i].SetActive(true);
                }
                else
                {
                    cameras[i].SetActive(false);
                }
            }
        }
    }

    public void SetTarget(CarController playerCar)
    {
        topDownCam.target = playerCar;
        cineCam.m_Follow = playerCar.transform;
        cineCam.m_LookAt = playerCar.transform;
    }
}
