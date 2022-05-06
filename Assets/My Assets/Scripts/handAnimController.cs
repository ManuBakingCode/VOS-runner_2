using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class handAnimController : MonoBehaviour
{
    private Animator animController;
    private XRController controller;
    public float speed;
    float gripValue;
    float triggerValue;
    float thumbValue;


    private void Start()
    {
        controller = transform.GetComponentInParent<XRController>();
        animController = transform.GetComponent<Animator>();
    }
    private void Update()
    {
        AnimateHand();
    }

    private void AnimateHand()
    {
        float gripTarget;
        float triggerTarget;
        float thumbTarget;
        bool thumb;
        controller.inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.grip, out gripTarget);
        controller.inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out triggerTarget);
        controller.inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out thumb);
        if (thumb == true) 
        {
            thumbTarget = 1;
        }
        else
        {
            thumbTarget = 0;
        }


        if (gripValue != gripTarget)
        {
            gripValue = Mathf.MoveTowards(gripValue, gripTarget, Time.deltaTime * speed);
            animController.SetFloat("Grip", gripValue);

        }
        if (triggerValue != triggerTarget)
        {
            triggerValue = Mathf.MoveTowards(triggerValue, triggerTarget, Time.deltaTime * speed);
            animController.SetFloat("Trigger", triggerValue);
        }        
        if (thumbValue != thumbTarget)
        {
            thumbValue = Mathf.MoveTowards(thumbValue, thumbTarget, Time.deltaTime * speed);
            animController.SetFloat("Thumb", thumbValue);
        }

    }
}
