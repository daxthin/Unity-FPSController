using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepsManager : MonoBehaviour
{
    // Private variables
    [SerializeField] private AudioClip[] m_footsteps;
    [SerializeField] private float m_footstepsInterval = 1;
    [SerializeField] private float m_footstepsRunIntervalMultiplier = 1;
    [SerializeField] private float m_footstepsCrouchInterval = 1;
    private AudioSource m_audioSource;
    private FootstepsGenerator m_footstepsGenerator;
    private FPSController m_fpsController;
    private bool m_isWalking;
    private float m_interval;

    // Properties
    public bool IsWalking
    {
        set { m_isWalking = value; }
        get { return m_isWalking; }
    }
    public float Interval
    {
        set { m_interval = value; }
        get { return m_interval; }
    }
    public AudioClip[] Footsteps
    {
        set { m_footsteps = value; }
        get { return m_footsteps; }
    }
    public AudioSource AudioSource
    {
        set { m_audioSource = value; }
        get { return m_audioSource; }
    }

    public bool Active
    {
        get;
        set;
    }

    void Start()
    {
        m_audioSource = gameObject.GetComponent<AudioSource>();
        m_footstepsGenerator = gameObject.GetComponent<FootstepsGenerator>();
        m_interval = m_footstepsInterval;
        m_fpsController = gameObject.GetComponent<FPSController>();
    }

    void Update()
    {

        if (Input.GetButton("Horizontal") || Input.GetButton("Vertical"))
            if (!m_isWalking && m_fpsController.IsGrounded)
                m_footstepsGenerator.PlayFootsteps();


        if (Input.GetKey(KeyCode.LeftControl))
        {
            m_interval = m_footstepsInterval / m_footstepsRunIntervalMultiplier;
        }
        else if (m_fpsController.IsCrouching)
        {
            m_interval = m_footstepsInterval * m_footstepsCrouchInterval;
        }
        else
        {
            m_interval = m_footstepsInterval;
        }
    }
}
