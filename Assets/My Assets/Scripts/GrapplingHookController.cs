using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrapplingHookController : MonoBehaviour
{
    public LayerMask physicsMask = 0;
    public Vector3 grapplePoint;
    public int maxHookDistance;
    public Transform player, hookTip, head;
    public bool hooked;
    public Material lineMat;
    public float variableX;

    private XRController controller;
    private LineRenderer hookVisual;
    private SpringJoint hookJoint;
    private RaycastHit hit;
    private Renderer grabParentEmission;
    private bool pointingToHook = false;
    private Transform hookCollider;
    private bool correctHit = false;
    private float changeHookDistance;
    private float triggerValue;
    private Vector2 rightStick;
    private DeviceBasedContinuousMoveProvider playerMovement;


    [SerializeField]
    private float distanceFromPoint;

    private void Start()
    {
        controller = GetComponent<XRController>();
        maxHookDistance = 30;
        hooked = false;
        hookVisual = player.GetComponent<LineRenderer>();
        hookVisual.positionCount = 0;
        playerMovement = player.GetComponent<DeviceBasedContinuousMoveProvider>();
    }

    private void Update()
    {
        handleControllerInputs();
        lookingForHook();
        hookLogic();
    }

    private void LateUpdate()
    {
        whileHooked();
    }

    private void whileHooked()
    {
        if (hooked)
        {
            drawHookLine();
            changeCompression();
            swingMovement();
        }
        else
        {
            hookVisual.positionCount = 0;
            Destroy(hookJoint);
            playerMovement.enabled = true;
        }
    }

    private void handleControllerInputs()
    {
        controller.inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out triggerValue);
        controller.inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out rightStick);
    }

    private void lookingForHook()
    {
        pointingToHook = false;
        if (Physics.Raycast(hookTip.position, hookTip.forward, out hit, maxHookDistance))
        {
            hookCollider = hit.transform;
            if (!correctHit)
            {
                pointingToHook = true;
                correctHit = true;
                hookCollider.GetComponentInParent<Renderer>().material.EnableKeyword("_EMISSION");
            }
        }
        else if (correctHit && hookCollider.tag == "Hookable")
        {
            hookCollider.GetComponentInParent<Renderer>().material.DisableKeyword("_EMISSION");
            correctHit = false;
            hookCollider = null;
        }
    }

    private void hookLogic()
    {
        if (triggerValue > 0.5f && !hooked)
        {
            shootHook();
        }
        if (triggerValue <= 0.5f && hooked)
        {
            player.GetComponent<DeviceBasedContinuousMoveProvider>().enabled = true;
            hooked = false;
        }
    }

    private void shootHook()
    {
        if (Physics.Raycast(hookTip.position, hookTip.forward, out hit, maxHookDistance))
        {
            if (hit.collider.gameObject.tag == "Hookable")
            {
                hooked = true;
                player.GetComponent<DeviceBasedContinuousMoveProvider>().enabled = false;

                playerMovement.moveSpeed = 4;

                hit.transform.GetComponentInParent<Renderer>().material.DisableKeyword("_EMISSION");
                grapplePoint = hit.collider.gameObject.transform.position;
                hookJoint = player.gameObject.AddComponent<SpringJoint>();

                distanceFromPoint = Vector3.Distance(player.position, grapplePoint);

                hookJoint.autoConfigureConnectedAnchor = false;
                hookJoint.connectedAnchor = grapplePoint;
                hookJoint.maxDistance = distanceFromPoint * 0.6f;
                hookJoint.minDistance = distanceFromPoint * 0.25f;
                hookJoint.spring = 4;
                hookJoint.damper = 7;
                hookJoint.massScale = 4;
            }
        }
    }



    private void swingMovement()
    {

        Vector2 swingDirection = new Vector2(head.forward.x, head.forward.z);
        print("swing direction head: " + swingDirection);


        player.GetComponent<Rigidbody>().AddForce(swingDirection * variableX);

    }

    private void drawHookLine()
    {
        hookVisual.positionCount = 2;
        hookVisual.SetPosition(0, hookTip.transform.position);
        hookVisual.SetPosition(1, grapplePoint);
        hookVisual.startColor = Color.black;
        hookVisual.endColor = Color.yellow;
        hookVisual.material = lineMat;
        hookVisual.startWidth = 0.01f;
        hookVisual.endWidth = 0.02f;
    }

    private void changeCompression()
    {
        hookJoint.maxDistance = hookJoint.maxDistance - rightStick.y / 30;
    }


}
