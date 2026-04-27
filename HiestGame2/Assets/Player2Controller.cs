using UnityEngine;
using UnityEngine.InputSystem;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    // Player 2 (Arrow Keys)
    public class Player2Controller : BaseController
    {
        protected override void ReadInput()
        {
            var kb = Keyboard.current;
            if (kb == null) { move.x = 0; return; }

            // Arrow horizontal
            move.x = 0;
            if (kb.leftArrowKey.isPressed) move.x = -1;
            if (kb.rightArrowKey.isPressed) move.x = 1;

            // Jump with Up Arrow
            if (jumpState == JumpState.Grounded && kb.upArrowKey.wasPressedThisFrame)
                jumpState = JumpState.PrepareToJump;
            else if (kb.upArrowKey.wasReleasedThisFrame)
            {
                stopJump = true;
                Schedule<PlayerStopJump>().player = this;
            }
        }
    }
}