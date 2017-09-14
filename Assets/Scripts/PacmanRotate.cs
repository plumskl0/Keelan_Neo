using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacmanRotate : MonoBehaviour {

	public Transform PacmanTransform, SensorTransform;
	private bool Collisons = false;
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		Collisons = Physics.Linecast (PacmanTransform.position, SensorTransform.position, 1 << LayerMask.NameToLayer ("solid"));

		if (Collisons)
			transform.localScale = new Vector3 (transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
	}
}
