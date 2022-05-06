using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerController : MonoBehaviour
{

    public Transform head, player, cam;
    public SphereCollider headCollider;
    private CapsuleCollider bodyCollider;
    public XRController leftController;
    private ContactPoint lowerContact;
    public LayerMask physicsMask = 0;
    public RaycastHit hit;
    public XRController controllerRight;
    public int jumpForce;
    private bool buttonA;
    private Rigidbody vrRigidibody;
    private bool jumped;
    private int jumpDelay;
    [SerializeField]
    private bool isGrounded;

    private void Start()
    {
        vrRigidibody = gameObject.GetComponent<Rigidbody>();
        jumpDelay = 5;
    }
    private void Awake()
    {
        bodyCollider = transform.GetComponent<CapsuleCollider>();
    }
    private void checkGrounded()
    {
        Vector3 capsuleCenter = new Vector3(bodyCollider.center.x + 0.05f, bodyCollider.center.y * 2 - headCollider.radius, bodyCollider.center.z);

        if (Physics.Raycast(transform.position + capsuleCenter, Vector3.down, out hit, capsuleCenter.y + 0.00001f, physicsMask))
        {
            Debug.DrawRay(transform.position + capsuleCenter, transform.TransformDirection(Vector3.down) * hit.distance, Color.red);
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    void FixedUpdate()
    {
        changeHeight();
        checkGrounded();
        moveHeadBody();
        jump();
        wallRun();
    }

    private void moveHeadBody()
    {
        Vector3 headCenter = new Vector3(head.transform.localPosition.x, head.transform.localPosition.y, head.transform.localPosition.z);
        headCollider.center = headCenter;
        Vector3 bodyCenter = new Vector3(head.transform.localPosition.x, headCollider.center.y / 2, head.transform.localPosition.z);
        bodyCollider.center = bodyCenter;
    }

    private void changeHeight()
    {
        bodyCollider.height = head.transform.localPosition.y;
        bodyCollider.center = new Vector3(bodyCollider.center.x, headCollider.center.y / 2, bodyCollider.center.z);
    }

    private void wallRun()
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 2))
        {
            Debug.DrawRay(cam.transform.position, cam.transform.forward * hit.distance, Color.yellow);
        }
    }

    private void jump()
    {
        controllerRight.inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonA);
        jumped = false;
        if (buttonA == true && isGrounded == true)
        {
            vrRigidibody.AddForce(new Vector3(head.forward.x, jumpForce, head.forward.z), ForceMode.Impulse);
        }
    }
}

