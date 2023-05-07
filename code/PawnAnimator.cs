using Sandbox;
using System;

namespace MyGame;

public class PawnAnimator : EntityComponent<Pawn>, ISingletonComponent
{
	public void Simulate()
	{
		var helper = new CitizenAnimationHelper( Entity );
		helper.WithVelocity( Entity.Velocity );
		helper.WithLookAt( Entity.EyePosition + Entity.EyeRotation.Forward * 100 );
		helper.HoldType = CitizenAnimationHelper.HoldTypes.None;
		helper.IsGrounded = Entity.GroundEntity.IsValid();
		helper.DuckLevel = MathX.Lerp( helper.DuckLevel, Entity.Tags.Has( "ducked" ) ? 1 : 0, Time.Delta * 10.0f );

		if ( Entity.Controller.HasEvent( "jump" ) )
		{
			helper.TriggerJump();
		}
	}
}
