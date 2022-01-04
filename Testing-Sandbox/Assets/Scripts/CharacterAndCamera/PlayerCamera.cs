using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerInput), typeof(Rigidbody))]
public class PlayerCamera : MonoBehaviour
{
    public float cameraSensitivity;
    const string MouseSensitivityKey = "mouseSensitivity";

    [SerializeField] Slider _mouseSensitivitySlider;

    [SerializeField] Camera _camera;
    [SerializeField] GameObject _viewModel;
    [SerializeField] GameObject _viewModelCam;
    [SerializeField] float _minFov = 75f;
    [SerializeField] float _maxFov = 110f;
    [SerializeField] float _tiltSpeed = 10f;
    PlayerInput _pInput;
    Rigidbody _rb;
    Vector3 _currentPlayerRotation, _currentCameraRotation;
    float _tiltAngleTarget = 0f;

    public static PlayerCamera Instance { get; private set; }

    private void Awake()
    {
        if(Instance == null)
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
        if(cameraSensitivity == 0f)
        {
            cameraSensitivity = 2f;
        }
    }

    private void Update()
    {
        //Set Y rotation of character
        _currentPlayerRotation = _rb.rotation.eulerAngles;
        _currentPlayerRotation.y += _pInput.CameraInput.y * cameraSensitivity;
        _rb.rotation = Quaternion.Euler(_currentPlayerRotation);

        //Set X rotation of camera
        _currentCameraRotation.x += _pInput.CameraInput.x * cameraSensitivity;
        _currentCameraRotation.x = Mathf.Clamp(_currentCameraRotation.x, -85f, 85f);

        //Handle Camera Tilting
        _currentCameraRotation.z = Mathf.Lerp(_currentCameraRotation.z, _tiltAngleTarget, Time.deltaTime * _tiltSpeed);

        //Assign rotation to camera
        _camera.transform.localRotation = Quaternion.Euler(_currentCameraRotation);

        //Match Viewmodel
        if (_viewModel)
        {
            //Position
            var newVMPos = _camera.transform.position;
            newVMPos.y -= 1f;
            _viewModel.transform.position = newVMPos;

            //Rotation
            _viewModel.transform.rotation = _camera.transform.rotation;
            var viewModelYRot = new Vector3(_currentCameraRotation.x * 0.2f, _viewModelCam.transform.localEulerAngles.y, _viewModelCam.transform.localEulerAngles.z);
            _viewModelCam.transform.localRotation = Quaternion.Euler(viewModelYRot);
        }
    }

    public void SetFOV(float newFOV)
    {
        if(newFOV > _maxFov)
        {
            _camera.fieldOfView = _maxFov;
        }
        else if(newFOV < _minFov)
        {
            _camera.fieldOfView = _minFov;
        }
        else
        {
            _camera.fieldOfView = newFOV;
        }
    }

    internal void SetCameraTilt(object p)
    {
        throw new NotImplementedException();
    }

    public void MomentumFOVShift(float percentageValue)
    {
        float range = _maxFov - _minFov;
        SetFOV(_minFov + (range * percentageValue));
    }

    public void SetCameraTilt(float targetXRotation)
    {
        _tiltAngleTarget = targetXRotation;
    }

    public void SetMouseSensitivity(float value)
    {
        cameraSensitivity = value;
    }
}
