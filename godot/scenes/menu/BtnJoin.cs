using Godot;

public partial class BtnJoin : Button
{
	public override void _Ready()
	{
		Pressed += () =>
		{
			GetNode<NetworkService>("/root/NetworkService").JoinGame();
		};
	}
}
