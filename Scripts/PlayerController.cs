
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon.Common;
using UdonSkate.Skateboard;
using UnityEngine;

namespace UdonSkate
{
    public class PlayerController : UdonSharpBehaviour
    {
        public DeckController deck;
        public VRCPlayerApi player;

        public AudioSource AudioWind;
        public float playerWalkSpeed = 10.0f;
        public float playerRunSpeed = 15.0f;
        public float playerStrafeSpeed = 10.0f;
        public float playerJumpImpulse = 5.0f;


        private float moveHorizontal = 0.0f;
        void Start()
        {
            player = Networking.LocalPlayer;
            player.SetRunSpeed(playerRunSpeed);
            player.SetWalkSpeed(playerWalkSpeed);
            player.SetStrafeSpeed(playerStrafeSpeed);
            player.SetJumpImpulse(playerJumpImpulse);
        }

        public void Update()
        {
            transform.position = player.GetPosition();
            if (!AudioWind.isPlaying)
            {
                AudioWind.volume = 0;
                AudioWind.Play();
            }
            var speed = player.GetVelocity().magnitude / 400.0f;
            if (speed < 20)
            {
                speed = 0;
            }
            if (deck.STATE_RIDING)
            {
                speed = deck.GetComponent<Rigidbody>().velocity.magnitude / 400.0f;
            }
            AudioWind.volume = Mathf.Clamp(Mathf.Lerp(AudioWind.volume, speed, 0.1f), 0, 1);
            AudioWind.pitch = Mathf.Clamp((Mathf.Lerp(AudioWind.pitch, speed * 5, 0.1f)), 0.5f, 1);
        }

        public void FixedUpdate()
        {
            if (Input.GetKeyDown("s"))
            {
                deck.Ollie();
            }
            deck.Turn(moveHorizontal);
        }

        public override void InputUse(bool value, UdonInputEventArgs args)
        {
            deck.Push();
        }

        public override void InputMoveHorizontal(float value, UdonInputEventArgs args)
        {
            moveHorizontal = value;
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