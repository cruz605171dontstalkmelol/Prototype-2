using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectCollisionsX : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (tag != "Frizbee") {
            Destroy(gameObject);
        }
    }
}
