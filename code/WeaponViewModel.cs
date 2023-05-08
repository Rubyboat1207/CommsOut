using Sandbox;
using System;

namespace MyGame;

public partial class WeaponViewModel : BaseViewModel
{
	protected Weapon Weapon { get; init; }

	public Vector3 TargetOffset;

	private Vector3 Offset;

	public WeaponViewModel( Weapon weapon )
	{
		Weapon = weapon;
		EnableShadowCasting = false;
		EnableViewmodelRendering = true;

		TargetOffset = new(0,0,0);
	}

	public override void PlaceViewmodel()
	{
		if ( Game.IsRunningInVR )
			return;
		
		// TargetOffset += new Vector3(Random.Shared.Float(-1, 1), Random.Shared.Float(-1, 1), Random.Shared.Float(-1, 1));

		Offset = Vector3.Lerp(Offset, ToRotated(TargetOffset), Time.Delta * 10);

		Position = Camera.Position + Offset;

		Rotation = Rotation.Lerp(Rotation, Camera.Rotation, Time.Delta * 40);

		Camera.Main.SetViewModelCamera( 80f, 1, 500 );
	}

	public Vector3 ToRotated(Vector3 vec) {
		return Transform.Rotation.Forward * vec.x + Transform.Rotation.Right * vec.y + Transform.Rotation.Up * vec.z;
	}
}
