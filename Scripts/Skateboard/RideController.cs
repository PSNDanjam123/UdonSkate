
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonSkate.Skateboard
{
    public class RideController : UdonSharpBehaviour
    {
        public DeckController deck;

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            deck.Mount(player);
        }

    }
}