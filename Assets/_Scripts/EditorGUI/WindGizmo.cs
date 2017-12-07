using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindGizmo : MonoBehaviour {

    private void OnDrawGizmos()
    {
        BoxCollider area = GetComponent<BoxCollider>();
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(area.center, area.size);
    }

}
