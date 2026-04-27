using UnityEngine;
using UnityEngine.InputSystem;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    // Player 1 (WASD)
    public class PlayerController : BaseController
    {
        protected override void ReadInput()
        {
            var kb = Keyboard.current;
            if (kb == null) { move.x = 0; return; }

            // WASD horizontal
            move.x = 0;
            if (kb.aKey.isPressed) move.x = -1;
            if (kb.dKey.isPressed) move.x = 1;

            // Jump with W
            if (jumpState == JumpState.Grounded && kb.wKey.wasPressedThisFrame)
                jumpState = JumpState.PrepareToJump;
            else if (kb.wKey.wasReleasedThisFrame)
            {
                stopJump = true;
                Schedule<PlayerStopJump>().player = this;
            }
        }
    }
}