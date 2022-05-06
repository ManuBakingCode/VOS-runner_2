using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraCollision : MonoBehaviour
{
    private GameObject body;
    private 
    void Start()
    {
        body = gameObject.transform.parent.gameObject.transform.parent.gameObject;
    }
    private void OnTriggerEnter(Collider other)
    {
    }
}
