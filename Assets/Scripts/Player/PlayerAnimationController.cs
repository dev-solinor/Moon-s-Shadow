using UnityEngine;

/// <summary>
/// Drives the Animator based on movement state
/// </summary>
public class PlayerAnimationController : MonoBehaviour
{
    // Serialized References

    [Header("References")]
    [SerializeField] private PlayerInputHandler _input;
    [SerializeField] private CharacterController _controller;

    [Header("Smoothing")]
    [SerializeField] private float _speedDampTime = 0.1f;

    // Private

    private Animator _animator;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");

    // Lifecycle

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        float speed = _input.MoveInput.magnitude;

        if (speed > 0.1f)
            speed = _input.SprintHeld ? 1f : 0.5f;
        else
            speed = 0f;

        _animator.SetFloat(SpeedHash, speed, _speedDampTime, Time.deltaTime);
        _animator.SetBool(IsGroundedHash, _controller.isGrounded);
    }
}