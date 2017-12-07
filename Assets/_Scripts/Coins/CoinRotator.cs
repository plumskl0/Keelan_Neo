using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinRotator : MonoBehaviour {

    public float speed = 200f;
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(speed * Time.deltaTime, 0f, 0f);
	}
}
