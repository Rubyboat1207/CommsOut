﻿using Sandbox;
using System.ComponentModel;

namespace NoComms;

public partial class Pawn : AnimatedEntity
{
	[Net, Predicted]
	public Weapon ActiveWeapon { get; set; }

	[ClientInput]
	public Vector3 InputDirection { get; set; }
	
	[ClientInput]
	public Angles ViewAngles { get; set; }

	
	SpotLightEntity light;

	float LocalEyeOffset = 0;
	

	/// <summary>
	/// Position a player should be looking from in world space.
	/// </summary>
	[Browsable( false )]
	public Vector3 EyePosition
	{
		get => Transform.PointToWorld( EyeLocalPosition );
		set => EyeLocalPosition = Transform.PointToLocal( value );
	}

	/// <summary>
	/// Position a player should be looking from in local to the entity coordinates.
	/// </summary>
	[Net, Predicted, Browsable( false )]
	public Vector3 EyeLocalPosition { get; set; }

	/// <summary>
	/// Rotation of the entity's "eyes", i.e. rotation for the camera when this entity is used as the view entity.
	/// </summary>
	[Browsable( false )]
	public Rotation EyeRotation
	{
		get => Transform.RotationToWorld( EyeLocalRotation );
		set => EyeLocalRotation = Transform.RotationToLocal( value );
	}

	/// <summary>
	/// Rotation of the entity's "eyes", i.e. rotation for the camera when this entity is used as the view entity. In local to the entity coordinates.
	/// </summary>
	[Net, Predicted, Browsable( false )]
	public Rotation EyeLocalRotation { get; set; }

	public BBox Hull;

	[BindComponent] public PawnController Controller { get; }
	[BindComponent] public PawnAnimator Animator { get; }

	public override Ray AimRay => new Ray( EyePosition, EyeRotation.Forward );

	/// <summary>
	/// Called when the entity is first created 
	/// </summary>
	public override void Spawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		// light = new SpotLightEntity();
	}

	public void SetActiveWeapon( Weapon weapon )
	{
		ActiveWeapon?.OnHolster();
		ActiveWeapon = weapon;
		ActiveWeapon.OnEquip( this );
	}

	public void Respawn()
	{
		Components.Create<PawnController>();
		Components.Create<PawnAnimator>();

		SetActiveWeapon( new Pistol() );
	}

	public void DressFromClient( IClient cl )
	{
		var c = new ClothingContainer();
		c.LoadFromClient( cl );
		c.DressEntity( this );
	}

	public override void Simulate( IClient cl )
	{
		SimulateRotation();
		Controller?.Simulate( cl );
		
		Animator?.Simulate();
		ActiveWeapon?.Simulate( cl );

		LocalEyeOffset = MathX.Lerp(LocalEyeOffset, ducking ? 32f : 64f, Time.Delta * 10);
		
		EyeLocalPosition = Vector3.Up * (LocalEyeOffset * Scale);

		// if(light == null) {
		// 	return;
		// }

		// light.Position = Vector3.Lerp(light.Position, Position + (Vector3.Up * (30f * Scale)) + (Rotation.Forward * 30f), Time.Delta * 10);
		// light.Rotation = Rotation.Lerp(light.Rotation, Camera.Rotation, Time.Delta * 10);

		if(!Input.Down("duck")) {
			TryUnDuck();
		}

		var girth = 32 * 0.5f;

		var mins = new Vector3(-girth, -girth, 0) * Scale;
		var maxs = new Vector3(+girth, +girth, 72) * Scale;

		if( ducking ) {
			maxs = maxs.WithZ( 36 * Scale );
		}

		Hull.Maxs = maxs;
		Hull.Mins = mins;

	}

	public override void BuildInput()
	{
		InputDirection = Input.AnalogMove;

		if ( Input.StopProcessing )
			return;

		var look = Input.AnalogLook;

		if ( ViewAngles.pitch > 90f || ViewAngles.pitch < -90f )
		{
			look = look.WithYaw( look.yaw * -1f );
		}

		var viewAngles = ViewAngles;
		viewAngles += look;
		viewAngles.pitch = viewAngles.pitch.Clamp( -89f, 89f );
		viewAngles.roll = 0f;
		ViewAngles = viewAngles.Normal;
	}

	bool IsThirdPerson { get; set; } = false;
	
	Vector3 GetEstimatedCameraPosition() {
			Vector3 targetPos;
			var pos = Position + Vector3.Up * (ducking ? 32 : 64);
			var rot = Camera.Rotation * Rotation.FromAxis( Vector3.Up, -16 );

			float distance = 80.0f * Scale;
			targetPos = pos + rot.Right * ((CollisionBounds.Mins.x + 50) * Scale);
			targetPos += rot.Forward * -distance;

			var tr = Trace.Ray( pos, targetPos )
				.WithAnyTags( "solid" )
				.Ignore( this )
				.Radius( 8 )
				.Run();
			
			return tr.EndPosition;
	}

	public override void FrameSimulate( IClient cl )
	{
		SimulateRotation();

		Camera.Rotation = ViewAngles.ToRotation();
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );

		if ( Input.Pressed( InputButton.View ) )
		{
			IsThirdPerson = !IsThirdPerson;
		}

		if ( IsThirdPerson )
		{
			Camera.FirstPersonViewer = null;
			Camera.Position = GetEstimatedCameraPosition();
		}
		else
		{
			Camera.FirstPersonViewer = this;
			Camera.Position = EyePosition;
		}
	}

	public TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
	{
		return TraceBBox( start, end, Hull.Mins, Hull.Maxs, liftFeet );
	}

	public TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f )
	{
		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		var tr = Trace.Ray( start, end )
					.Size( mins, maxs )
					.WithAnyTags( "solid", "playerclip", "passbullets" )
					.Ignore( this )
					.Run();

		return tr;
	}

	protected void SimulateRotation()
	{
		EyeRotation = ViewAngles.ToRotation();
		Rotation = ViewAngles.WithPitch( 0f ).ToRotation();
	}



	public Pawn() {
		Hull = new
		(
			new Vector3( -16, -16, 0 ),
			new Vector3( 16, 16, 64 )
		);
	}
}
