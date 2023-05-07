using MyGame.Utility;

namespace MyGame;
public interface IAimDownSights {
    Vector3 AimSightsTargetPose { get; }
    Vector3 NormalPose { get; }
    bool IsAimingDownSights { get; set; }
    float aimDownSightsModifier {get; set;}

    public void AimDownSights(ref Vector3 viewModelTarget, PawnController pawn) {
        IsAimingDownSights = !IsAimingDownSights;
        if(viewModelTarget != null) {
            viewModelTarget = IsAimingDownSights ? this.AimSightsTargetPose : NormalPose;
        }

        SetMovementModifier(pawn);
    }

    public void AimDownSights(PawnController pawn) {
        IsAimingDownSights = !IsAimingDownSights;
        SetMovementModifier(pawn);
    }


    void SetMovementModifier(PawnController pawn) {
        if(IsAimingDownSights) {
            pawn.MoveModifierHandler.Add(new ModifierItem<float>("aimDownSights", aimDownSightsModifier));
            pawn.JumpModifierHandler.Add(new ModifierItem<bool>("aimDownSights", false));
        }else {
            pawn.MoveModifierHandler.Remove("aimDownSights");
            pawn.JumpModifierHandler.Remove("aimDownSights");
        }
    }
}