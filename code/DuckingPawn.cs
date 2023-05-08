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
        var pm = Trace.Ray( new Ray(Position, Vector3.Up), 92 * Scale).Run();

        Log.Info(pm.Hit);

        if ( pm.Hit ) return;

        Components.Get<PawnController>().MoveModifierHandler.Remove("crouch");

        ducking = false;
        Tags.Remove("ducked");
    }
}