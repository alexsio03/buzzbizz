using Godot;
using System;

public partial class Player : CharacterBody3D
{
    public const float Speed = 5.0f;
    public const float JumpVelocity = 4.5f;
    public const float MouseSensitivity = 0.001f; // Adjust this for camera sensitivity

    private float _twistAngle = 0.0f;
    private float _pitchAngle = 0.0f;

    public Node3D TwistPivot;
    public Node3D PitchPivot;

    private const float PitchClampMin = -0.5f; // Adjust these limits to your preference
    private const float PitchClampMax = 0.5f;

    public override void _Ready()
    {
        Input.SetMouseMode(Input.MouseModeEnum.Captured);

        // Get references to TwistPivot and PitchPivot nodes
        TwistPivot = GetNode<Node3D>("TwistPivot");
        PitchPivot = GetNode<Node3D>("TwistPivot/PitchPivot");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseEvent)
        {
            if (Input.GetMouseMode() == Input.MouseModeEnum.Captured)
            {
                // Calculate twist and pitch based on mouse movement, without accumulating
                _twistAngle -= mouseEvent.Relative.X * MouseSensitivity;
                _pitchAngle -= mouseEvent.Relative.Y * MouseSensitivity;

                // Clamp the pitch angle to prevent over-rotation
                _pitchAngle = Mathf.Clamp(_pitchAngle, PitchClampMin, PitchClampMax);
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;

        // Toggle mouse capture mode
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            Input.SetMouseMode(Input.MouseModeEnum.Visible);
        }

        // Add gravity
        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta;
        }

        // Handle jump
        if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
        }

        // Handle movement
        Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * Speed;
            velocity.Z = direction.Z * Speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
        }

        Velocity = velocity;
        MoveAndSlide();

        // Apply the twist (Y-axis rotation) directly based on the current mouse input
        TwistPivot.Rotation = new Vector3(TwistPivot.Rotation.X, _twistAngle, TwistPivot.Rotation.Z);

        // Apply the pitch (X-axis rotation) directly with clamping
        PitchPivot.Rotation = new Vector3(_pitchAngle, PitchPivot.Rotation.Y, PitchPivot.Rotation.Z);
    }
}
