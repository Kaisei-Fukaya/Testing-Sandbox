using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PlayerInput : MonoBehaviour
{
    public Vector3 MovementInput { get; private set; }
    public Vector3 CameraInput { get; private set; }
    public bool IsPressingMovementKey { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool CrouchPressed { get; private set; }

    float _timeSinceJumpPressed;


    public static PlayerInput Instance { get; private set; }

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
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {

            UpdateLogic();
    }

    void ClearAllInputs()
    {
        MovementInput = Vector3.zero;
        CameraInput = Vector3.zero;
        IsPressingMovementKey = false;
        JumpPressed = false;
        CrouchPressed = false;
    }

    void UpdateLogic()
    {
        //Store Movement Input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        MovementInput = new Vector3(moveX, 0f, moveZ);


        //Store if pressing movement key
        if (Input.GetAxisRaw("Horizontal") != 0f || Input.GetAxisRaw("Vertical") != 0f)
        {
            IsPressingMovementKey = true;
            float forwardMovement = moveZ;
            if (forwardMovement < 0f)
            {
                forwardMovement = 0f;
            }
            //PlayerData.Instance.ReduceBattery(1f * Time.deltaTime);
        }
        else
        {
            IsPressingMovementKey = false;
        }

        //Store crouch input
        if (Input.GetButton("Crouch"))
        {
            CrouchPressed = true;
        }
        else
        {
            CrouchPressed = false;
        }

        //Store if jump key pressed this frame
        if (Input.GetButtonDown("Jump"))
        {
            JumpPressed = true;
        }

        //Store Camera Input
        float cameraY = Input.GetAxis("Mouse X");
        float cameraX = -Input.GetAxis("Mouse Y");

        CameraInput = new Vector3(cameraX, cameraY, 0f);


        //Timeout Jump Input
        if (JumpPressed)
        {
            _timeSinceJumpPressed += Time.deltaTime;
        }

        if (_timeSinceJumpPressed > 0.5f)
        {
            ResetJump();
            _timeSinceJumpPressed = 0f;
        }
    }

    public void ResetJump() => JumpPressed = false;
}
