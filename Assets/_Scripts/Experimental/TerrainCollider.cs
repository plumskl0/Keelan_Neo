using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainCollider : MonoBehaviour
{

    private SharedFields sharedData = SharedFields.Instance;



    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Ball"))
        {
            //Debug.LogError("Lebensverlust bei Terrain erkannt");
            sharedData.LostLife = true;
            sharedData.CarResetNeeded = true;
            sharedData.BallResetNeeded = true;
            col.transform.parent.Find("Car Prototype v8").GetComponent<PlateAgent>().LostLife = true;
        }

        else if (col.gameObject.CompareTag("Trainingsball"))    //falls ein Trainingsball aufprallt dürfen die Spielmechanismen nicht geändert werden
        {
            //Suche das zum Ball gehörende Auto und sage ihm, dass es den Ball verloren hat
            col.transform.parent.Find("TrainingCarv8").GetComponent<PlateAgent>().LostLife = true;
            //Debug.LogError("Treffe Boden bei y : " + col.transform.parent.Find("TrainingCarv8").GetComponent<PlateAgent>().ballTransform.position.y);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        //wenn der Teller das Terrain berührt ist das Auto wahrscheinlich umgefallen -> muss wieder aufgestellt werden
        if (other.gameObject.CompareTag("Teller"))
        {
            //Debug.Log("Lebensverlust bei Terrain erkannt");
            sharedData.LostLife = true;
            sharedData.CarResetNeeded = true;
        }
    }
}
