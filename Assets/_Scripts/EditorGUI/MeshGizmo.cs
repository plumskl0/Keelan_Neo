using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGizmo : MonoBehaviour {

    private void OnDrawGizmos()
    {
        MeshCollider area = GetComponent<MeshCollider>();
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireMesh(area.sharedMesh, -1, area.transform.position, area.transform.rotation, area.transform.localScale);
    }

}
