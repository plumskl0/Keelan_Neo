using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour {
    public Transform player;
    private bool followMode = true; //Bestimmt ob die Minimap sich entsprechend der Autoposition verschiebt

    public bool FollowMode
    {
        get
        {
            return followMode;
        }

        set
        {
            Debug.Log(string.Format("Setze Follow Mode der Minimap auf {0}", value));
            followMode = value;
        }
    }

    private void LateUpdate()
    {
        if (FollowMode)
        {
            Vector3 newPosition = player.position;
            newPosition.y = transform.position.y;
            transform.position = newPosition;

            transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
        }

    }
}
