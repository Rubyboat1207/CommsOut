using Sandbox;

namespace MyGame;

partial class Pawn : AnimatedEntity {
    public bool ducking { get; private set;}

    public void Duck() {
        ducking = true;

        if(GroundEntity != null && !GroundEntity.IsValid) {
            Position += new Vector3(0,0,32);
        }

        Tags.Add("ducked");
    }

    public virtual void TryUnDuck()
    {
        var pm = TraceBBox( Position, Position, Hull.Mins, Hull.Maxs + 32);

        if ( pm.StartedSolid ) return;

        Components.Get<PawnController>().MoveModifierHandler.Remove("crouch");

        ducking = false;
        Tags.Remove("ducked");
    }
}