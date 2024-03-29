﻿
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace UdonSkate.Skateboard
{
    public class DeckController : UdonSharpBehaviour
    {
        public float forwardFriction = 0.005f;
        public float sideFriction = 0.6f;

        public float playerWeight = 70.0f;

        public WheelCollider[] wheels;

        private Rigidbody rb;   // rigidbody of the skateboard
        private VRC_Pickup vRC_Pickup;  // pickup component

        public VRCStation vRC_Station; // station follower

        private VRCPlayerApi player;

        private bool canMount = true;
        private float mountCooldown = 0.0f;
        private float mountCooldownDuration = 0.5f;

        private Vector3 normal;
        private Vector3 forward;


        /** STATES */

        public bool STATE_RIDING = false;
        public bool STATE_GROUNDED = false;
        public bool STATE_PICKED_UP = false;

        /** AUDIO **/
        public AudioSource AudioSkateboardRolling;
        public AudioSource AudioSkateboardOllie;
        public AudioSource AudioSkateboardHit;

        void Start()
        {
            _init();
        }


        void Update()
        {
            _setPickupable();
            _calculateNormal();
            _calculateForwardRotation();
            _setGrounded();
            if (STATE_RIDING)
            {
                vRC_Station.gameObject.transform.position = transform.position;
                vRC_Station.gameObject.transform.rotation = Quaternion.Lerp(
                    vRC_Station.gameObject.transform.rotation,
                    Quaternion.LookRotation(
                        new Vector3(forward.x, 0, forward.z),
                        -Physics.gravity),
                    0.1f);
            }
            else
            {
                vRC_Station.gameObject.transform.position = new Vector3(1000, 1000, 1000);
            }
            if (!canMount)
            {
                if (mountCooldown > 0)
                {
                    mountCooldown -= Time.deltaTime;
                }
                else
                {
                    canMount = true;
                    mountCooldown = 0;
                }
            }
        }

        void FixedUpdate()
        {
            if (STATE_PICKED_UP)
            {
                return; // no need to process anything as player is holding
            }

            if (STATE_GROUNDED)
            {
                /** Ground Rotation */
                rb.angularVelocity = Vector3.zero;
                rb.rotation = Quaternion.Lerp(rb.rotation, Quaternion.LookRotation(forward, normal), 0.5f);
            }

            /** Audio **/
            var speed = rb.velocity.magnitude / 10;
            if (!STATE_GROUNDED)
            {
                speed = 0;
            }
            AudioSkateboardRolling.volume = Mathf.Clamp(Mathf.Lerp(AudioSkateboardRolling.volume, speed, 0.2f), 0, 1);
            AudioSkateboardRolling.pitch = Mathf.Clamp(Mathf.Lerp(AudioSkateboardRolling.pitch, speed, 0.2f), 0.4f, 1);
            if (STATE_GROUNDED)
            {
                if (!AudioSkateboardRolling.isPlaying)
                {
                    AudioSkateboardRolling.volume = 0;
                    AudioSkateboardRolling.Play();
                }
            }
            else
            {
                if (AudioSkateboardRolling.isPlaying && AudioSkateboardRolling.volume == 0)
                {
                    AudioSkateboardRolling.Stop();
                }
            }
        }

        public void Push()
        {
            if (!STATE_GROUNDED || !STATE_RIDING || rb.velocity.magnitude > 40)
            {
                return;
            }
            var force = forward * player.GetWalkSpeed() * playerWeight / 8f;
            if (Vector3.Angle(rb.velocity, force) > 90)
            {
                force = -force;
            }
            force -= Vector3.Project(force, -Physics.gravity);
            rb.AddForce(force, ForceMode.Impulse);
        }

        public void Turn(float amount)
        {
            if (!STATE_RIDING)
            {
                return;
            }
            float turnForce = playerWeight * 0.2f;
            rb.AddTorque(rb.gameObject.transform.up * turnForce * amount, ForceMode.Impulse);
        }

        public void Mount(VRCPlayerApi player)
        {
            if (STATE_RIDING || !STATE_GROUNDED || !canMount)
            {
                return;
            }
            rb.mass += playerWeight; // weight of rider
            vRC_Station.transform.rotation = player.GetRotation();
            vRC_Station.UseStation(player);
            STATE_RIDING = true;
        }
        public void Unmount(VRCPlayerApi player)
        {
            if (!STATE_RIDING)
            {
                return;
            }
            rb.mass -= playerWeight; // weight of rider
            vRC_Station.ExitStation(player);
            player.SetVelocity(rb.velocity + -Physics.gravity.normalized * player.GetJumpImpulse());
            player.Immobilize(false);
            canMount = false;
            mountCooldown = mountCooldownDuration;
            STATE_RIDING = false;
        }

        public void Ollie()
        {
            if (!STATE_RIDING || !STATE_GROUNDED)
            {
                return;
            }
            rb.AddForce(Vector3.up * player.GetJumpImpulse() * playerWeight, ForceMode.Impulse);
            AudioSkateboardOllie.volume = 1.0f;
            AudioSkateboardOllie.Play();
        }

        public override void OnPickup()
        {
            STATE_PICKED_UP = true;
        }
        public override void OnDrop()
        {
            STATE_PICKED_UP = false;
        }


        private void _init()
        {
            rb = GetComponent<Rigidbody>();
            vRC_Pickup = GetComponent<VRC_Pickup>();
            player = Networking.LocalPlayer;
            rb.centerOfMass = -transform.up * 0.02f;
            AudioSkateboardHit.volume = 0;
            AudioSkateboardOllie.volume = 0;
            AudioSkateboardRolling.volume = 0;
        }


        private Vector3 _calculateForwardRotation()
        {
            var newForward = transform.forward;
            return forward = newForward;

        }

        private Vector3 _calculateNormal()
        {
            if (!Physics.Raycast(transform.position, -transform.up, out RaycastHit hitInfo, 0.5f))
            {
                return normal = transform.up;
            }
            return normal = hitInfo.normal;

        }

        private void _setGrounded()
        {
            var wasGrounded = STATE_GROUNDED;
            STATE_GROUNDED = Physics.Raycast(transform.position, -transform.up, out RaycastHit hitInfo, 0.1f);
            if (STATE_GROUNDED && !wasGrounded && !AudioSkateboardHit.isPlaying)
            {
                AudioSkateboardHit.volume = Mathf.Clamp(Vector3.Project(rb.velocity, -transform.up).magnitude / 10, 0, 0.5f);
                AudioSkateboardHit.Play();
            }
        }

        private void _setPickupable()
        {
            vRC_Pickup.pickupable = !STATE_RIDING;
        }
    }
}