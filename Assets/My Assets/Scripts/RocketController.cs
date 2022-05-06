using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class RocketController : MonoBehaviour
{

    private XRController controller;
    public Transform player;
    public float boostTime;
    private float timePassed;
    public PhysicsPoser lHand;
    public PhysicsPoser rHand;

    void Start()
    {
        controller = GetComponent<XRController>();
    }

    private void disableBoost()
    {
        timePassed += Time.deltaTime;
        if (timePassed > boostTime)
        {
            ConstantForce force = player.gameObject.GetComponent<ConstantForce>();
            Destroy(force);
        }
    }

    private void boost()
    {/*
        if (player.gameObject.GetComponent<ConstantForce>() == null && player.gameObject.GetComponent<PlayerController>().isGrounded == false)
        {
            timePassed = 0;
            Debug.Log("Trigger key pressed");
            ConstantForce force;
            force = player.gameObject.AddComponent<ConstantForce>();
            force.relativeForce = gameObject.transform.forward * 20;
        }*/
    }


    void FixedUpdate()
    {

        float triggerValue;
        controller.inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out triggerValue);
        if (triggerValue > 0.5 && !lHand.grabbing && !rHand.grabbing)
        {
            boost();
        }
        disableBoost();
    }
}
