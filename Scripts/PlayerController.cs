
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
        void Start()
        {
            player = Networking.LocalPlayer;
        }

        public override void InputUse(bool value, UdonInputEventArgs args)
        {
            deck.Push();
        }

        public override void InputMoveHorizontal(float value, UdonInputEventArgs args)
        {
            deck.Turn(value);
        }
    }

}