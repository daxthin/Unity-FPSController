using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepsManager : MonoBehaviour
{
    // Private variables
    [SerializeField] private AudioClip[] _footsteps;
    [SerializeField] private float _footstepsInterval = 1;
    [SerializeField] private float _footstepsRunIntervalMultiplier = 1;
    [SerializeField] private float _footstepsCrouchInterval = 1;
    private AudioSource _audioSource;
    private FootstepsGenerator _footstepsGenerator;
    private FPSController _fpsController;
    private bool _isWalking;
    private float _interval;

    // Properties
    public bool IsWalking
    {
        set { _isWalking = value; }
        get { return _isWalking; }
    }
    public float Interval
    {
        set { _interval = value; }
        get { return _interval; }
    }
    public AudioClip[] Footsteps
    {
        set { _footsteps = value; }
        get { return _footsteps; }
    }
    public AudioSource AudioSource
    {
        set { _audioSource = value; }
        get { return _audioSource; }
    }

    public bool Active
    {
        get;
        set;
    }

    void Start()
    {
        _audioSource = gameObject.GetComponent<AudioSource>();
        _footstepsGenerator = gameObject.GetComponent<FootstepsGenerator>();
        _interval = _footstepsInterval;
        _fpsController = gameObject.GetComponent<FPSController>();
    }

    void Update()
    {

        if (Input.GetButton("Horizontal") || Input.GetButton("Vertical"))
            if (!_isWalking && _fpsController.IsGrounded)
                _footstepsGenerator.PlayFootsteps();


        if (Input.GetKey(KeyCode.LeftControl))
        {
            _interval = _footstepsInterval / _footstepsRunIntervalMultiplier;
        }
        else if (_fpsController.IsCrouching)
        {
            _interval = _footstepsInterval * _footstepsCrouchInterval;
        }
        else
        {
            _interval = _footstepsInterval;
        }
    }
}
