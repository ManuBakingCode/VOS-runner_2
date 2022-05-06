using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class headClimbed : MonoBehaviour
{
    public RaycastHit hit;
    public bool overWall = false;
    public LayerMask physicsMask = 0;
    public CapsuleCollider bodyCollider;
    private void FixedUpdate()
    {
        detectCollision();
    }

    private void detectCollision()
    {
        if (Physics.Raycast(transform.position + transform.GetComponent<SphereCollider>().center, Vector3.down, out hit, bodyCollider.height + 0.00001f, physicsMask))
        {
            Debug.DrawRay(transform.position + transform.GetComponent<SphereCollider>().center, transform.TransformDirection(Vector3.down) * hit.distance, Color.green);
            if (hit.collider.tag == "Wall" || hit.collider.tag == "Ground")
            {
                overWall = true;
                return;
            }
        }
        overWall = false;
    }
}
