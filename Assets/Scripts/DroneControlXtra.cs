using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class DroneControlXtra : MonoBehaviour
{
    public float thrustForce = 10f, raiseForce = 10f; // ???????? ?????? ????? ? ?????
    public float rotationSpeed = 100f; // ???????? ????????
    public float tiltSpeed = 50f; // ???????? ???????
    public float maxTiltAngle = 30f; // ???????????? ???? ???????
    private Rigidbody rb;
    public Transform target;
    public Vector3 offset;
    
    [SerializeField] private InputActionReference m_droneMovementInputAction, m_droneGasInputAction, m_droneInputButton;

    private bool isDroning;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        m_droneInputButton.action.performed += DroneButtonPressed;
    }

    private void DroneButtonPressed(InputAction.CallbackContext ctx)
    {
        isDroning = !isDroning;
        // FindObjectOfType<XRController>().GetComponent<>();
        print("123");
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
    }

    void HandleMovement()
    {
        // ?????????? ????????? ??????/????? ? ?????/??????
        // m_droneMovementInputAction
        // float moveVertical = Input.GetKey(KeyCode.W) ? -1 : Input.GetKey(KeyCode.S) ? 1 : 0;
        // float moveHorizontal = Input.GetKey(KeyCode.D) ? -1 : Input.GetKey(KeyCode.A) ? 1 : 0;
        
        float moveVertical = -m_droneMovementInputAction.action.ReadValue<Vector2>().y;
        float moveHorizontal = -m_droneMovementInputAction.action.ReadValue<Vector2>().x;

        // ?????????? ????????? ?????/????
        float moveUp = m_droneGasInputAction.action.ReadValue<Vector2>().y;

        float tiltAroundZ = -moveHorizontal * maxTiltAngle;
        float tiltAroundX = moveVertical * maxTiltAngle;

        Quaternion targetRotation = Quaternion.Euler(tiltAroundX, rb.rotation.eulerAngles.y, tiltAroundZ);
        rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.deltaTime * tiltSpeed);

        // ???????? ??????/????? ? ?????/??????
        Vector3 movement = new Vector3(moveHorizontal * thrustForce, moveUp * raiseForce, moveVertical * thrustForce) * Time.deltaTime;
        offset += transform.TransformDirection(movement);
        rb.MovePosition(target.position + offset);
    }

    void HandleRotation()
    {
        // ?????????? ????????? ?????? ??? Y (?????/??????)
        float rotateY = m_droneGasInputAction.action.ReadValue<Vector2>().x;

        // ?????????? ????????
        Vector3 rotation = new Vector3(0, rotateY, 0) * rotationSpeed * Time.deltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));
    }

    public void StartInput()
    {
        // FindObjectOfType<XR>()
    }

    public void EndInput()
    {
        
    }
}