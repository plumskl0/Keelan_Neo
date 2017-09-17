using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door_1_Sensor : MonoBehaviour
{

    private CoinBooth booth;
    private bool doorUsed = false;

    private Animator _animator;

    // Use this for initialization
    void Start()
    {
        _animator = GetComponent<Animator>();
        booth = GetComponent<CoinBooth>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (booth.BoothUsedOnce && !doorUsed)
        {
            if (other.tag == "Player")
            {
                if (booth.WasPayed)
                    _animator.SetBool("open", true);
                doorUsed = true;
            }
        }
    }

}
