using Sandbox;

namespace MyGame;

public partial class Pistol : Weapon, IAimDownSights
{
	public override string ModelPath => "weapons/rust_pistol/rust_pistol.vmdl";
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

	
	public Vector3 AimSightsTargetPose { get {
		return new Vector3(-5, -16.5f, 2);
	} }
	public Vector3 NormalPose { get; init; }
	public bool IsAimingDownSights { get; set; }
	public float aimDownSightsModifier { get => 0.5f; set {} }

	public Pistol() {
		NormalPose = new Vector3(0,0,0);
	}

	[ClientRpc]
	protected virtual void ShootEffects()
	{
		Game.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		Pawn.SetAnimParameter( "b_attack", true );
		ViewModelEntity?.SetAnimParameter( "fire", true );
	}

	public override void PrimaryAttack()
	{
		ShootEffects();
		Pawn.PlaySound( "rust_pistol.shoot" );
		ShootBullet( 0f, 100, 20, 1 );
	}

	public override void SecondaryAttack() {
		Log.Info("hello!");

		if(ViewModelEntity == null) {
			((IAimDownSights) this).AimDownSights(Owner.Components.Get<PawnController>());
			return;
		}

		((IAimDownSights) this).AimDownSights(ref ((WeaponViewModel) ViewModelEntity).TargetOffset, Owner.Components.Get<PawnController>());
	}

	protected override void Animate()
	{
		Pawn.SetAnimParameter( "holdtype", (int)CitizenAnimationHelper.HoldTypes.Pistol );
	}

	public override void OnEquip(Pawn pawn) {
		base.OnEquip(pawn);

	}
}
