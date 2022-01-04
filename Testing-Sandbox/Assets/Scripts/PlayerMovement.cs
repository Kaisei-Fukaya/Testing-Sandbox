using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInput), typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public float baseMoveSpeed;
    public float maxMomentum;
    public float decelerationSpeed;
    public float accelerationRate = 0.005f;
    public float jumpPower;
    public float jumpBonusAmount;
    public float airDrag;
    public float slideDrag;
    public float slopeDownForce;
    public float crouchSpeed;
    public LayerMask groundMask;
    public AudioClip jumpSound;
    float _moveSpeed;
    float _momentumMultiplier;
    float _timeSinceLanded;
    float _jumpBonus;
    bool _isGrounded;
    bool _frameOfJumpFlag;
    bool _frameOfSlideFlag;
    bool _isSliding;
    float _targetScale = 1f;
    Vector3 _lastVelocity;
    PlayerInput _pInput;
    Rigidbody _rb;
    [SerializeField] float _cameraTiltDegrees = 5f;
    [SerializeField] GameObject _footstepObject;
    [SerializeField] AudioClip[] _slideSound;

    public static PlayerMovement Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        _pInput = GetComponent<PlayerInput>();
        _rb = GetComponent<Rigidbody>();
        _moveSpeed = baseMoveSpeed;
        _momentumMultiplier = 1f;
        Physics.gravity = new Vector3(0f, -27.468f, 0f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), 0.5f);
    }

    private void FixedUpdate()
    {
        //Check grounded
        if(Physics.OverlapSphere(new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z), 0.5f, groundMask).Length != 0)
        {
            _isGrounded = true;
            _timeSinceLanded += Time.deltaTime;
        }
        else
        {
            _isGrounded = false;
            _timeSinceLanded = 0f;
        }


        //Handle movement
        Vector3 currentVelocity = _rb.velocity;
        currentVelocity.y = 0f;

        _moveSpeed = baseMoveSpeed * (_momentumMultiplier + _jumpBonus/10f);

        //Increase momentum until max speed, reset when not moving
        if (_pInput.IsPressingMovementKey && currentVelocity.magnitude > 0.1f && !_isSliding)
        {
            if (_momentumMultiplier < maxMomentum)
            {
                _momentumMultiplier += accelerationRate;

                float percentage = GetPercentageMomentum();                             //Get percentage momentum
                PlayerCamera.Instance.MomentumFOVShift(percentage);                     //Set camera fov based on momentum
            }
            else
            {
                _momentumMultiplier = maxMomentum;
            }
        }
        else
        {
            _momentumMultiplier = Mathf.Lerp(_momentumMultiplier, 1f, Time.deltaTime * decelerationSpeed);
            float percentage = GetPercentageMomentum();
            PlayerCamera.Instance.MomentumFOVShift(percentage);
        }

        //Tilt camera
        PlayerCamera.Instance.SetCameraTilt(_cameraTiltDegrees * -_pInput.MovementInput.x);

        //Handle Jump
        if (_pInput.JumpPressed && _isGrounded)
        {
            //Jump
            if(_timeSinceLanded < 0.1f)
            {
                _jumpBonus += jumpBonusAmount;                                                          //Bonus Added for jumping soon after landing
            }
            else
            {
                _jumpBonus = 0f;
            }
            _rb.AddForce(transform.up * (jumpPower + _jumpBonus), ForceMode.Impulse);             

            //Reset flag
            _pInput.ResetJump();

            //Flag jumped this frame
            _frameOfJumpFlag = true;

            //Cancel sliding
            if (_isSliding)
            {
                _isSliding = false;
            }

        }


        //Handle Slide
        if (_isGrounded)
        {
            if (_pInput.CrouchPressed && !_isSliding)
            {
                StartSliding();
            }
            else if (!_pInput.CrouchPressed && _isSliding)
            {
                StopSliding();
            }
        }

        //Scale player
        float currentYScale = transform.localScale.y;

        if (transform.localScale.y != _targetScale)
        {
            currentYScale = Mathf.Lerp(currentYScale, _targetScale, Time.deltaTime * crouchSpeed);
            transform.localScale = new Vector3(1f, currentYScale, 1f);
        }

        //Add downforce on slope when moving to prevent bouncing
        if (!_pInput.JumpPressed /*&& _pInput.MovementInput.x == 0f && _pInput.MovementInput.z == 0f*/)
        {
            RaycastHit hit;
            Debug.DrawRay(transform.position, -transform.up, Color.red);
            if (Physics.Raycast(transform.position, -transform.up, out hit, 2f))
            {
                if (hit.normal != transform.up)
                {
                    //print(hit.normal);
                    _rb.AddForce(Vector3.down * slopeDownForce * Time.fixedDeltaTime, ForceMode.VelocityChange);
                }
            }
        }

        //Set velocity
        Vector3 horizontalMoveForce = transform.right *  _pInput.MovementInput.x * (_moveSpeed*0.8f);   //Horizontal speed is reduced for more natural feel
        Vector3 forwardMoveForce = transform.forward * _pInput.MovementInput.z * _moveSpeed;

        currentVelocity = horizontalMoveForce + forwardMoveForce;

        currentVelocity = Vector3.ClampMagnitude(currentVelocity, _moveSpeed);                          //Clamp magnitude so that moving diagonally is not faster
        currentVelocity.y = _rb.velocity.y;                                                             //Re-combine with vertical velocity

        if (_isGrounded)
        {
            if (!_isSliding)
            {
                _rb.velocity = currentVelocity;
            }
            else
            {
                if (_frameOfSlideFlag)
                {
                    _lastVelocity = currentVelocity;                                                    //Store velocity just before sliding
                    _lastVelocity.y = 0f;
                    _frameOfSlideFlag = false;
                }
                else
                {
                    currentVelocity.x = currentVelocity.x / 2;
                    currentVelocity.z = currentVelocity.z / 3;
                    _rb.velocity = _lastVelocity + currentVelocity;

                    //Slide drag
                    _lastVelocity -= _lastVelocity * slideDrag * Time.fixedDeltaTime;
                }
            }
        }
        else if(!_frameOfJumpFlag)
        {
            currentVelocity.x = currentVelocity.x / 2;
            currentVelocity.z = currentVelocity.z / 3;
            _rb.velocity = _lastVelocity + currentVelocity;

            //Air drag
            _lastVelocity -= _lastVelocity * airDrag * Time.fixedDeltaTime;
        }
        else
        {
            _lastVelocity = currentVelocity;                                                    //store velocity just before jumping
            _lastVelocity.y = 0f;
            _frameOfJumpFlag = false;
            if (_isSliding)
            {
                StopSliding();
            }
        }
    }

    private void StartSliding()
    {
        _isSliding = true;
        _frameOfSlideFlag = true;
        _targetScale = 0.6f;
    }

    private void StopSliding()
    {
        _isSliding = false;
        _targetScale = 1f;
    }

    float GetPercentageMomentum()
    {
        return Mathf.InverseLerp(1f, maxMomentum, _momentumMultiplier);
    }

    public float GetPlayerVelocityMagnitude()
    {
        Vector3 val = _rb.velocity;
        val.y = 0f;                 //Only return magnitude x and z movement
        return val.magnitude;
    }


}
