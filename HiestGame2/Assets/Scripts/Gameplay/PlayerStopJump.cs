using Platformer.Core;
using Platformer.Mechanics;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when the player releases the jump button early.
    /// This allows short-hop behavior.
    /// </summary>
    public class PlayerStopJump : Simulation.Event<PlayerStopJump>
    {
        public BaseController player;

        public override void Execute()
        {
            if (player != null)
            {
                player.stopJump = true;
            }
        }
    }
}