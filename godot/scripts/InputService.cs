using Godot;
using System;

public partial class InputService : Node
{
	public InputState CurrentInputState { get; private set; }
	public Vector2 PlayerPosition { get; set; } = Vector2.Zero; // This should be updated by the player node
	private bool EnableTouchControls { get; set; } = false;
	private bool EnableMouseAiming { get; set; } = true;

	public override void _Ready()
	{
		GD.Print("InputService ready.");
		// TODO - get correct input device based on connected devices
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		CurrentInputState = new InputState(EnableMouseAiming);
		if (EnableTouchControls)
		{
			// TODO - update movement and aim vectors based on touch input
		}
		if (EnableMouseAiming)
		{
			// TODO - update aim vector based on mouse input and player position
			// AimVector = (MousePosition - PlayerPosition).Normalized();
		}
		// GD.Print(CurrentInputState.ToString());
	}

	public struct InputState
	{
		public Vector2 MovementVector { get; set; }
		public Vector2 AimVector { get; set; }
		public bool IsShooting { get; set; }
		public bool IsReloading { get; set; }
		public bool IsSwitchingWeapon { get; set; }
		public bool IsUsingGadget { get; set; }
		public bool IsPressingPickup { get; set; }
		public bool IsDroppingWeapon { get; set; }
		public bool IsAiming { get; set; }

		public InputState(bool enableMouseAiming)
		{
			var _moveVect = Input.GetVector("move_left", "move_right", "move_up", "move_down");
			MovementVector = (_moveVect.Length() > .2f) ? _moveVect : Vector2.Zero;
			AimVector = Input.GetVector("aim_left",  "aim_right",  "aim_up",  "aim_down");
			IsShooting = Input.IsActionPressed("shoot");
			IsReloading = Input.IsActionPressed("reload");
			IsSwitchingWeapon = Input.IsActionPressed("switch_weapon");
			IsUsingGadget = Input.IsActionPressed("use_gadget");
			IsPressingPickup = Input.IsActionPressed("pickup");
			IsDroppingWeapon = Input.IsActionPressed("drop_weapon");
			IsAiming = enableMouseAiming ? Input.IsActionPressed("aim") : AimVector.Length() > 0.8f;
		}

		public override string ToString()
		{
			return $"Movement: {MovementVector}, Aim: {AimVector}, Shooting: {IsShooting}, Reloading: {IsReloading}, SwitchingWeapon: {IsSwitchingWeapon}, UsingGadget: {IsUsingGadget}, PressingPickup: {IsPressingPickup}, DroppingWeapon: {IsDroppingWeapon}, Aiming: {IsAiming}";
		}
	}
}
