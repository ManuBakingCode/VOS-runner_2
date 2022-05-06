using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(XRController))]
public class PhysicsPoser : MonoBehaviour
{
    // Value
    public float physiscsRange = 0.1f;
    public LayerMask physicsMask = 0;
    private Vector3 prevPos;
    private Vector3 enterPos;
    private Quaternion enterRot;
    public float clamberDrag = 10;
    private bool touchingWall = false;
    private bool exiting = false;
    private Vector3 handVelocity;
    public bool grabbing;
    private Vector3 diff;

    [Range(0, 1)] public float slowDownVelocity = 1;
    [Range(0, 1)] public float slowDownAngularVelocity = 1;

    [Range(0, 1000)] public float maxPositionChange = 500f;
    [Range(0, 1000)] public float maxRotationChange = 500f;

    // References
    private Rigidbody rigid = null;
    private XRController controller = null;
    private XRBaseInteractor interactor = null;
    public Transform handCenter = null;
    public GameObject head;
    public Rigidbody vrRig = null;
    public CapsuleCollider bodyCollider = null;
    public PhysicsPoser otherHand;

    // Runtime
    private Vector3 targetPosition = Vector3.zero;
    private Quaternion targetRotation = Quaternion.identity;

    private void Awake()
    {
        // Get the stuff
        rigid = GetComponent<Rigidbody>();
        controller = GetComponent<XRController>();
        interactor = GetComponent<XRBaseInteractor>();
    }

    private void Start()
    {
        // As soon as we start, move to the hand
        UpdateTracking(controller.inputDevice);
        MoveUsingTransform();
        RotateUsingTransform();
    }

    private void Update()
    {
        // Update our target location
        UpdateTracking(controller.inputDevice);
    }

    private void UpdateTracking(InputDevice inputDevice)
    {
        // Get the rotation and position from the device
        inputDevice.TryGetFeatureValue(CommonUsages.devicePosition, out targetPosition);
        inputDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out targetRotation);
    }

    private void FixedUpdate()
    {
        // Move via transform if we're holding an object, or not within physics range
        if (IsHoldingObject() || !WithinPhysicsRange())
        {
            MoveUsingTransform();
            RotateUsingTransform();
        }
        else
        {
            MoveUsingPhysics();
            RotateUsingPhysics();
        }
        // Else move using physics

        updateGravity();
        snapHandBack();
    }

    public bool IsHoldingObject()
    {
        return interactor.selectTarget;
    }

    public bool WithinPhysicsRange()
    {
        return Physics.CheckSphere(handCenter.transform.position, physiscsRange, physicsMask, QueryTriggerInteraction.Ignore);
    }

    private void MoveUsingPhysics()
    {
        // Prevents overshooting
        rigid.velocity *= slowDownVelocity;

        // Get target velocity
        Vector3 velocity = FindNewVelocity();

        // Check if it's valid
        if (IsValidVelocity(velocity.x))
        {
            // Figure out the max we can move, then move via velocity
            float maxChange = maxPositionChange * Time.deltaTime;
            rigid.velocity = Vector3.MoveTowards(rigid.velocity, velocity, maxChange);
        }
    }


    private void RotateUsingPhysics()
    {
        // Prevents overshooting
        rigid.angularVelocity *= slowDownAngularVelocity;

        // Get target velocity
        Vector3 angularVelocity = FindNewAngularVelocity();

        // Check if it's valid
        if (IsValidVelocity(angularVelocity.x))
        {
            float maxChange = maxRotationChange * Time.deltaTime;
            rigid.angularVelocity = Vector3.MoveTowards(rigid.angularVelocity, angularVelocity, maxChange);
        }
        // Figure out the max we can rotate, then move via velocity
    }


    private bool IsValidVelocity(float value)
    {
        // Is it an actual number, or is a broken number?
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }

    private void MoveUsingTransform()
    {
        // Prevents jitter
        rigid.velocity = Vector3.zero;
        transform.localPosition = targetPosition;
    }

    private void RotateUsingTransform()
    {
        // Prevents jitter
        rigid.angularVelocity = Vector3.zero;
        transform.localRotation = targetRotation;
    }

    private void OnDrawGizmos()
    {
        // Show the range at which the physics takeover
        Gizmos.DrawWireSphere(handCenter.transform.position, physiscsRange);
    }

    private void OnValidate()
    {
        // Just in case
        if (TryGetComponent(out Rigidbody rigidbody))
        {
            rigidbody.useGravity = false;
        }
    }

    private Vector3 FindNewVelocity()
    {
        // Figure out the difference we can move this frame
        Vector3 worldPosition = transform.root.TransformPoint(targetPosition);
        Vector3 difference = worldPosition - rigid.position;
        return difference / Time.deltaTime;
    }

    private Vector3 FindNewAngularVelocity()
    {
        // Figure out the difference in rotation
        Quaternion worldRotation = transform.root.rotation * targetRotation;
        Quaternion difference = worldRotation * Quaternion.Inverse(rigid.rotation);
        difference.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);

        // Do the weird thing to account for have a range of -180 to 180
        if (angleInDegrees > 180)
            angleInDegrees -= 360;

        // Figure out the difference we can move this frame
        return (rotationAxis * angleInDegrees * Mathf.Deg2Rad) / Time.deltaTime;
    }

    private void OnTriggerStay(Collider other)
    {
        float triggerValue;
        Vector3 deviceRealPos;
        controller.inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out triggerValue);
        controller.inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out deviceRealPos);
        diff = deviceRealPos - gameObject.transform.localPosition;

        if (triggerValue >= 0.5f && other.tag == "Wall" && touchingWall == false)
        {
            beginClimb();
            return;
        }
        if (triggerValue >= 0.5f && other.tag == "Wall" && touchingWall == true)
        {
            Climb(controller.inputDevice);
            return;

        }
        if (triggerValue <= 0.5f && other.tag == "Wall" && exiting == false)
        {
            letGo();
            return;
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Wall")
        {
            touchingWall = false;
            exiting = true;
            grabbing = false;
            bodyCollider.enabled = true;
        }
    }

    private void beginClimb()
    {
        enterPos = gameObject.transform.position;
        enterRot = gameObject.transform.rotation;
        touchingWall = true;
        exiting = false;
        grabbing = true;
        if (diff.magnitude >= .3f)
        {
            vrRig.MovePosition(vrRig.position - diff);
        }
    }

    private void Climb(InputDevice inputDevice)
    {
        gameObject.transform.position = enterPos;
        gameObject.transform.rotation = enterRot;
        bodyCollider.enabled = false;
        controller.inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceVelocity, out handVelocity);
        if (grabbing == true && otherHand.grabbing == false)
        {
            vrRig.MovePosition(vrRig.position + -(handVelocity * Time.fixedDeltaTime * 1f));
        }
        if (grabbing == true && otherHand.grabbing == true)
        {
            vrRig.MovePosition(vrRig.position + -(handVelocity * Time.fixedDeltaTime * .5f));
        }

    }

    private void letGo()
    {
        touchingWall = false;
        exiting = true;
        grabbing = false;
        bodyCollider.enabled = true;

        gameObject.transform.position = gameObject.transform.position;
        gameObject.transform.rotation = gameObject.transform.rotation;

        if (otherHand.grabbing == false && head.GetComponent<headClimbed>().overWall == true)
        {
            float goUp = bodyCollider.height - (head.GetComponent<headClimbed>().hit.distance);
            Vector3 bodyPos = new Vector3(head.transform.localPosition.x, goUp, head.transform.localPosition.z);
            vrRig.MovePosition(vrRig.position + bodyPos);
        }

        controller.inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceVelocity, out handVelocity);
        vrRig.AddForce(-handVelocity, ForceMode.Impulse);
    }

    private void updateGravity()
    {
        if (grabbing || otherHand.grabbing)
        {
            vrRig.velocity = Vector3.zero;
            vrRig.useGravity = false;
        }
        else
        {
            vrRig.useGravity = true;
        }
    }

    private void snapHandBack()
    {
        if (diff.magnitude >= .35f)
        {
            RotateUsingTransform();
            MoveUsingTransform();
        }

    }

}