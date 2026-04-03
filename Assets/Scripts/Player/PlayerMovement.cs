using UnityEngine;

/// <summary>
/// Handles character movement and jumping
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    // Serialized Settings

    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 4f;
    [SerializeField] private float _sprintSpeed = 7f;
    [SerializeField] private float _rotationSpeed = 10f;

    [Header("Jump & Gravity")]
    [SerializeField] private float _jumpHeight = 1.5f;
    [SerializeField] private float _gravity = -19.62f;

    [Header("References")]
    [SerializeField] private CharacterController _controller;
    [SerializeField] private PlayerInputHandler _input;
    [SerializeField] private Transform _meshRoot;

    // Private

    private Camera _cam;
    private Vector3 _velocity;

    // Lifecycle

    private void Awake()
    {
        _cam = Camera.main;
    }

    private void Update()
    {
        ApplyGravity();
        Move();
        Jump();
    }

    // Movement

    private void Move()
    {
        Vector2 input = _input.MoveInput;

        if (input == Vector2.zero) return;

        Vector3 camForward = Vector3.ProjectOnPlane(_cam.transform.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(_cam.transform.right, Vector3.up).normalized;
        Vector3 direction = (camForward * input.y + camRight * input.x).normalized;

        float speed = _input.SprintHeld ? _sprintSpeed : _walkSpeed;
        _controller.Move(direction * speed * Time.deltaTime);

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            _meshRoot.rotation = Quaternion.Slerp(_meshRoot.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }
    }

    private void Jump()
    {
        if (_input.JumpPressed && _controller.isGrounded)
        {
            _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
        }
    }

    private void ApplyGravity()
    {
        if (_controller.isGrounded && _velocity.y < 0f)
        {
            _velocity.y = -2f;
        }

        _velocity.y += _gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }
}