
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon.Common;
using UdonSkate.Skateboard;

namespace UdonSkate
{
    public class PlayerController : UdonSharpBehaviour
    {
        public DeckController deck;
        public VRCPlayerApi player;
        public float playerWalkSpeed = 10.0f;
        public float playerRunSpeed = 15.0f;
        public float playerStrafeSpeed = 10.0f;
        public float playerJumpImpulse = 5.0f;
        void Start()
        {
            player = Networking.LocalPlayer;
            player.SetRunSpeed(playerRunSpeed);
            player.SetWalkSpeed(playerWalkSpeed);
            player.SetStrafeSpeed(playerStrafeSpeed);
            player.SetJumpImpulse(playerJumpImpulse);
        }

        public override void InputUse(bool value, UdonInputEventArgs args)
        {
            deck.Push();
        }

        public override void InputMoveHorizontal(float value, UdonInputEventArgs args)
        {
            deck.Turn(value);
        }

        public override void InputJump(bool value, UdonInputEventArgs args)
        {
            if (!value)
            {
                return;
            }
            deck.Unmount(player);
        }
    }

}