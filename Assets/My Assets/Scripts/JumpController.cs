using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class JumpController : MonoBehaviour
{
    public XRController controllerRight;
    public Transform player;
    public float boostTime;
    public int jumpForce;
    private bool buttonA;
    private Rigidbody vrRigidibody;
    private bool jumped;
    private int jumpDelay;

    private void Start()
    {
        vrRigidibody = gameObject.GetComponent<Rigidbody>();
        jumpDelay = 1;
    }

    void FixedUpdate()
    {
        controllerRight.inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonA);
        jumped = false;
        /*
        if (buttonA == true && gameObject.GetComponent<PlayerController>().isGrounded == true)
        {
            boost();
        }*/
    }

    private void boost()
    {
        vrRigidibody.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
        jumped = true;
        StartCoroutine(SpamBlockco());
    }

    public IEnumerator SpamBlockco()
    {
        if (jumped == true)
        {
            yield return new WaitForSeconds(jumpDelay);
        }
        yield return null;
        jumped = false;
    }
}
