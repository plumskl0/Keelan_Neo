using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatePlate : MonoBehaviour {

    private int offset = 10;
    private int plateParts = 18;

	// Use this for initialization
	void Start () {
        createPlate();
	}

    // Update is called once per frame
    private void Update()
    {
        
    }

    private void createPlate()
    {
        for (int i = 0; i < plateParts; i++)
        {
            Transform part = transform.Find("Part");
            Transform newPart = Instantiate(part, 
                new Vector3(part.position.x, part.position.y, part.position.z), 
                Quaternion.Euler(part.rotation.x, part.rotation.y + offset * i, part.rotation.z));
            newPart.SetParent(transform);
        }
    }
}
