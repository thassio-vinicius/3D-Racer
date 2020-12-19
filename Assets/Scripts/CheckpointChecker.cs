using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointChecker : MonoBehaviour
{
    public CarController theCar;
    private void OnTriggerEnter(Collider other) 
    {
        if(other.tag == "Checkpoint")
        {
           theCar.CheckpointHit(other.GetComponent<Checkpoint>().cpNumber);
        }
    }
}
