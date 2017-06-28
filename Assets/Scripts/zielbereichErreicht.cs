using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class zielbereichErreicht : MonoBehaviour {
    public Canvas endbildschirm;
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "zielbereich")
        {
            Debug.Log("LadeBild");
            endbildschirm.enabled = true;
        }
    }
}
