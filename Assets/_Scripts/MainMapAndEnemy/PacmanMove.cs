using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacmanMove : MonoBehaviour {

	private Rigidbody regPacman;
	public float PacmanSpeed = 10f;
	void Start () {
		regPacman = GetComponent<Rigidbody> ();
		
	}
	
	// Update is called once per frame
	void Update () {
		regPacman.velocity = new Vector2 (transform.localScale.x, 0) * PacmanSpeed;
	}
}
