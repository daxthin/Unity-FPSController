using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class FPSController : MonoBehaviour
{
    // Inspector fields
    [Header("Movement")]
    [SerializeField] private float m_walkSpeed = 6.0f;
    [SerializeField] private float m_runSpeed = 6.0f;
    [SerializeField] private float m_walkRunTransition = 5f;
    [SerializeField] private float m_airAcceleration = 15;
    [SerializeField] private float m_maxAirAcceleration = 15;
    [SerializeField] private float m_moveSmoothness = 8f;

    [Header("Camera Sway Section")]
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _maxAngle = 1.5f;
    private float curAngle = 0f;

    [Header("Physics")]
    [SerializeField] private float m_forceMultiplier = 8f;
    [SerializeField] private float m_SlopeDistance = 1.0f;
    [SerializeField] private float m_slopeSlideSpeed = 6f;

    [Header("Slope Bouncing Fix")]
    [SerializeField] private float m_slopeForceDown = 20f;
    [SerializeField] private float m_slopeForceRayLenght = 1.5f;

    [Header("Mouse")]
    [SerializeField] private float m_mouseSensivity = 2.0f;
    [SerializeField] private float m_clampYRot = 90f;

    [Header("Jump")]
    [SerializeField] private float m_jumpHeight = 1.8f;
    [SerializeField] private float m_gravity = -40.0f;

    [Header("Crouch")]
    [SerializeField] private float m_crouchSpeed = 4;
    [SerializeField] private float m_crouchHeight = 0.5f;
    [SerializeField] private float m_normalHeight = 2.8f;

    [Header("Setup")]
    [SerializeField] private bool m_debug;
    [SerializeField] private bool m_lockCursor;
    [SerializeField] private LayerMask m_whatIsGround;
    [SerializeField] private KeyCode m_runKey = KeyCode.LeftCommand;
    [SerializeField] private KeyCode m_crouchKey = KeyCode.C;
    [SerializeField] private GameObject m_playerCamera;
    [SerializeField] private Transform m_root;
    [SerializeField] private GUIStyle m_style;

    // Private fields 
    private float m_speed;
    private bool m_isRunning;
    private bool m_isCrouching;
    private Vector3 m_characterVelocity;
    private CharacterController m_controller = null;
    private float m_rightMove;
    private float m_forwardMove;
    private RaycastHit m_slopeHit;
    private Vector3 m_velocity;
    private float m_playerTopVelocity = 0.0f;

    // Properties
    public bool IsCrouching
    {
        get
        {
            return m_isCrouching;
        }
    }

    public bool IsGrounded
    {
        get
        {
            return GroundCheck();
        }
    }

    public bool IsOnSlope
    {
        get
        {
            return CheckSlope();
        }
    }

    public bool SlopeAngleIsLow
    {
        get
        {
            return CheckSlopeAngle();
        }
    }


    // Methods
    void Start()
    {
        m_controller = gameObject.GetComponent<CharacterController>();
        m_speed = m_walkSpeed;
    }

    void Update()
    {
        // Camera setup
        CameraLook.SetupCamera(m_mouseSensivity, m_clampYRot);
        CameraLook.MouseLook(m_playerCamera.transform, this.transform);

        
        Crouch();
        CameraSway();
        SetMovementInput();
        CalculateMovement();

        /* Calculate top velocity */
        Vector3 udp = m_characterVelocity;
        if (udp.magnitude > m_playerTopVelocity)
            m_playerTopVelocity = udp.magnitude;

        // Move the character controller
        m_controller.Move(m_characterVelocity * Time.deltaTime);

        if (m_lockCursor)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                m_lockCursor = false;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            if (Input.GetMouseButton(0))
                m_lockCursor = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = false;
        }
    }

    private void SetMovementInput()
    {
        m_rightMove = Input.GetAxisRaw("Horizontal");
        m_forwardMove = Input.GetAxisRaw("Vertical");
    }

    private void AirMove()
    {
        Vector3 wishdir = transform.TransformDirection(m_rightMove, 0, m_forwardMove);

        // Add acceleration to the player so player can change directions 
        m_characterVelocity.x += wishdir.x *  m_airAcceleration * Time.deltaTime;
        m_characterVelocity.z += wishdir.z *  m_airAcceleration * Time.deltaTime;

        // Clamp only horizontal velocity
        Vector3 horizontalvelocity = Vector3.ProjectOnPlane(m_characterVelocity, Vector3.up);
        horizontalvelocity = Vector3.ClampMagnitude(horizontalvelocity, m_maxAirAcceleration);
        m_characterVelocity = horizontalvelocity + (Vector3.up * m_characterVelocity.y);
    }

    private void CalculateMovement()
    {
        Vector3 wishdir = transform.TransformDirection(m_rightMove, 0, m_forwardMove) * m_speed;

        // Check if player is on slope to fix bouncing
        if (CheckSlope())
        {
            m_controller.Move(Vector3.down * m_controller.height / 2 * m_slopeForceDown * Time.deltaTime);
        }

        // Check slope angle for slide down
        if (CheckSlopeAngle())
        {
            SlopePhysics();
        }

        // Prevent y velocity goes down to infinite
        if (GroundCheck() && m_velocity.y < 0)
        {
            m_velocity.y = -10f;
        }

        // Apply jump force
        if (GroundCheck() && Input.GetKeyDown(KeyCode.Space))
        {
            m_velocity.y = Mathf.Sqrt(m_jumpHeight * -2f * m_gravity);
        }


        
        // Change between Ground Movement and Air Movement
        if (GroundCheck())
        {
            m_characterVelocity = Vector3.Lerp(m_characterVelocity, wishdir, m_moveSmoothness * Time.deltaTime);
        }
        else if (!m_isCrouching)
        {
            AirMove();
        }

        // Run input
        if (Input.GetKey(m_runKey))
        {
            m_isRunning = true;
            if (!m_isCrouching)
                m_speed = Mathf.Lerp(m_speed, m_runSpeed,
                m_walkRunTransition * Time.deltaTime);
        }
        else
        {
            m_isRunning = false;
            if (!m_isCrouching)
                m_speed = Mathf.Lerp(m_speed, m_walkSpeed,
                m_walkRunTransition * Time.deltaTime);
        }

        // Apply gravity force
        m_velocity.y += m_gravity * Time.deltaTime;
        m_characterVelocity.y = m_velocity.y;
    }

    private void Crouch()
    {
        if (Input.GetKey(m_crouchKey))
        {
            m_controller.height = Mathf.MoveTowards(m_controller.height,
                m_crouchHeight, 42 * Time.deltaTime);

            m_speed = m_crouchSpeed;
            m_isCrouching = true;
        }
        else
        {
            m_controller.height = Mathf.MoveTowards(m_normalHeight,
                m_controller.height, 10 * Time.deltaTime);

            if (!m_isRunning && m_isCrouching)
            {
                m_speed = m_walkSpeed;
            }

            m_isCrouching = false;
        }
    }

    private void SlopePhysics()
    {
        Vector3 slopeDirection = Vector3.up - m_slopeHit.normal * Vector3.Dot(Vector3.up, m_slopeHit.normal);
        float slideSpeed = m_speed + m_slopeSlideSpeed + Time.deltaTime;

        m_characterVelocity = slopeDirection * -slideSpeed;

        m_characterVelocity.y += m_characterVelocity.y - m_slopeHit.point.y;

        m_controller.Move(m_characterVelocity * Time.deltaTime);
    }

    private bool CheckSlopeAngle()
    {
        if (!GroundCheck()) return false;

        if (Physics.Raycast(transform.position, Vector3.down, out m_slopeHit, m_SlopeDistance))
        {
            float m_slopeAngle = Vector3.Angle(m_slopeHit.normal, Vector3.up);
            if (m_slopeAngle > m_controller.slopeLimit) return true;
        }
        return false;
    }

    private bool CheckSlope()
    {

        if (!GroundCheck()) return false;

        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, m_SlopeDistance))
            if (hit.normal != Vector3.up)
                return true;
        return false;
    }

    private bool GroundCheck()
    {
        if (m_controller.isGrounded)
        {
            return true;
        }
        return false;
    }

    private void CameraSway()
    {
        // lean left
        if (Input.GetKey(KeyCode.A))
        {
            curAngle = Mathf.MoveTowardsAngle(curAngle, _maxAngle, _speed * Time.deltaTime);
        }
        // lean right
        else if (Input.GetKey(KeyCode.D))
        {
            curAngle = Mathf.MoveTowardsAngle(curAngle, -_maxAngle, _speed * Time.deltaTime);
        }
        // reset lean
        else
        {
            curAngle = Mathf.MoveTowardsAngle(curAngle, 0f, _speed * Time.deltaTime);
        }

        m_root.localRotation = Quaternion.AngleAxis(curAngle, Vector3.forward);
    }

    private void OnControllerColliderHit(ControllerColliderHit collision)
    {
        //checks if there is rigidbody
        if (collision.rigidbody == null) { return; }
        Vector3 pushDir = m_characterVelocity;
        //Adds force to the object
        collision.rigidbody.AddForce(pushDir * m_speed * m_forceMultiplier
            * Time.deltaTime, ForceMode.Impulse);
    }

    private void OnGUI()
    {
        if (m_debug)
        {
            var ups = m_controller.velocity;
            ups.y = 0;
            GUI.Label(new Rect(10, 15, 400, 100), "Speed: " + Mathf.Round(ups.magnitude * 100) / 100 + " ups", m_style);
            GUI.Label(new Rect(10, 30, 400, 100), "Top Speed: " + Mathf.Round(m_playerTopVelocity * 100) / 100 + "ups", m_style);

            GUI.Label(new Rect(10, 45, 400, 100), "Is Grounded: " + GroundCheck(), m_style);
            GUI.Label(new Rect(10, 60, 400, 100), "Is On Slope: " + CheckSlope(), m_style);
            GUI.Label(new Rect(10, 75, 400, 100), "Slope angle is low: " + CheckSlopeAngle(), m_style);
        }
    }

}
