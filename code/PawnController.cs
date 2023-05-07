﻿using Sandbox;
using System;
using System.Collections.Generic;
using MyGame.Utility;

namespace MyGame;

public class PawnController : EntityComponent<Pawn>
{
	public int StepSize => 24;
	public int GroundAngle => 45;
	public int JumpSpeed => 100;
	public float Gravity => 800f;

	public float walkSpeed = 200.0f;
	// public float runMult = 1.5f;
	// public float crouchMult = 0.5f;

	public Modifier<float> MoveModifierHandler { get; private set; }
	public Modifier<bool> JumpModifierHandler { get; private set; }
	

	HashSet<string> ControllerEvents = new( StringComparer.OrdinalIgnoreCase );

	bool Grounded => Entity.GroundEntity.IsValid();

	public void Simulate( IClient cl )
	{
		ControllerEvents.Clear();

		var movement = Entity.InputDirection.Normal;
		var angles = Entity.ViewAngles.WithPitch( 0 );
		var moveVector = Rotation.From( angles ) * movement * 320f;
		var groundEntity = CheckForGround();

		ComputeMovementModifiers();

		MoveModifierHandler.ForEach(m => Log.Info(m.Name));

		if ( groundEntity.IsValid() )
		{
			if ( !Grounded )
			{
				Entity.Velocity = Entity.Velocity.WithZ( 0 );
				AddEvent( "grounded" );
			}

			float speed = walkSpeed;

			MoveModifierHandler.ForEach(m => speed *= m.Modification);

			Entity.Velocity = Accelerate( Entity.Velocity, moveVector.Normal, moveVector.Length, speed, 12.5f );
			Entity.Velocity = ApplyFriction( Entity.Velocity, 7.0f );
		}
		else
		{
			Entity.Velocity = Accelerate( Entity.Velocity, moveVector.Normal, moveVector.Length, 100, 20f );
			Entity.Velocity += Vector3.Down * Gravity * Time.Delta;
		}

		bool canJump = true;
		JumpModifierHandler.ForEach(m => canJump = m.Modification);

		if ( Input.Down( "jump" ) && canJump)
		{
			
			DoJump();
		}

		var mh = new MoveHelper( Entity.Position, Entity.Velocity );
		mh.Trace = mh.Trace.Size( Entity.Hull ).Ignore( Entity );

		if ( mh.TryMoveWithStep( Time.Delta, StepSize ) > 0 )
		{
			if ( Grounded )
			{
				mh.Position = StayOnGround( mh.Position );
			}
			Entity.Position = mh.Position;
			Entity.Velocity = mh.Velocity;
		}

		Entity.GroundEntity = groundEntity;
	}

	void ComputeMovementModifiers() {
		if(Input.Pressed( "run" )) {
			MoveModifierHandler.Add(new ModifierItem<float>("run", 1.5f, "crouch", "aimDownSights"));
		}else if(Input.Released("run")) {
			MoveModifierHandler.Remove("run");
		}
		if(Input.Pressed("duck")) {
			Entity.Duck();
			MoveModifierHandler.Add(new ModifierItem<float>("crouch", 0.5f));
		}else if(Input.Released("duck")) {
			Entity.TryUnDuck();
		}
	}

	void DoJump()
	{
		if ( Grounded )
		{
			Entity.Velocity = ApplyJump( Entity.Velocity, "jump" );
		}
	}

	Entity CheckForGround()
	{
		if ( Entity.Velocity.z > 100f )
			return null;

		var trace = Entity.TraceBBox( Entity.Position, Entity.Position + Vector3.Down, 2f );

		if ( !trace.Hit )
			return null;

		if ( trace.Normal.Angle( Vector3.Up ) > GroundAngle )
			return null;

		return trace.Entity;
	}

	Vector3 ApplyFriction( Vector3 input, float frictionAmount )
	{
		float StopSpeed = 100.0f;

		var speed = input.Length;
		if ( speed < 0.1f ) return input;

		// Bleed off some speed, but if we have less than the bleed
		// threshold, bleed the threshold amount.
		float control = (speed < StopSpeed) ? StopSpeed : speed;

		// Add the amount to the drop amount.
		var drop = control * Time.Delta * frictionAmount;

		// scale the velocity
		float newspeed = speed - drop;
		if ( newspeed < 0 ) newspeed = 0;
		if ( newspeed == speed ) return input;

		newspeed /= speed;
		input *= newspeed;

		return input;
	}

	Vector3 Accelerate( Vector3 input, Vector3 wishdir, float wishspeed, float speedLimit, float acceleration )
	{
		if ( speedLimit > 0 && wishspeed > speedLimit )
			wishspeed = speedLimit;

		var currentspeed = input.Dot( wishdir );
		var addspeed = wishspeed - currentspeed;

		if ( addspeed <= 0 )
			return input;

		var accelspeed = acceleration * Time.Delta * wishspeed;

		if ( accelspeed > addspeed )
			accelspeed = addspeed;

		input += wishdir * accelspeed;

		return input;
	}

	Vector3 ApplyJump( Vector3 input, string jumpType )
	{
		AddEvent( jumpType );

		return input + Vector3.Up * JumpSpeed;
	}

	Vector3 StayOnGround( Vector3 position )
	{
		var start = position + Vector3.Up * 2;
		var end = position + Vector3.Down * StepSize;

		// See how far up we can go without getting stuck
		var trace = Entity.TraceBBox( position, start );
		start = trace.EndPosition;

		// Now trace down from a known safe position
		trace = Entity.TraceBBox( start, end );

		if ( trace.Fraction <= 0 ) return position;
		if ( trace.Fraction >= 1 ) return position;
		if ( trace.StartedSolid ) return position;
		if ( Vector3.GetAngle( Vector3.Up, trace.Normal ) > GroundAngle ) return position;

		return trace.EndPosition;
	}

	public bool HasEvent( string eventName )
	{
		return ControllerEvents.Contains( eventName );
	}

	void AddEvent( string eventName )
	{
		if ( HasEvent( eventName ) )
			return;

		ControllerEvents.Add( eventName );
	}

	public PawnController() {
		MoveModifierHandler = new Modifier<float>();
		JumpModifierHandler = new Modifier<bool>();
	}
	
}
