using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    private Vector3 respawnPoint;
    // Start is called before the first frame update
    void Start()
    {
        respawnPoint = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.transform.position.y < -30)
        {
            gameObject.transform.position = respawnPoint;
        }
    }
}
