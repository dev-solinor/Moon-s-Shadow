using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Reads and exposes raw player input values.
/// Single Responsibility: input reading only — no movement logic here.
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    // Public Properties

    public Vector2 MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool SprintHeld { get; private set; }

    // Serialized References

    [Header("Input Actions")]
    [SerializeField] private InputActionReference _moveAction;
    [SerializeField] private InputActionReference _jumpAction;
    [SerializeField] private InputActionReference _sprintAction;

    // Lifecycle

    private void OnEnable()
    {
        _moveAction.action.Enable();
        _jumpAction.action.Enable();
        _sprintAction.action.Enable();
    }

    // Prevents actions from firing on disabled objects
    private void OnDisable()
    {
        _moveAction.action.Disable();
        _jumpAction.action.Disable();
        _sprintAction.action.Disable();
    }

    private void Update()
    {
        MoveInput = _moveAction.action.ReadValue<Vector2>();
        JumpPressed = _jumpAction.action.WasPressedThisFrame();
        SprintHeld = _sprintAction.action.IsPressed();
    }
}