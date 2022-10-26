
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace UdonSkate.Skateboard
{
    public class DeckController : UdonSharpBehaviour
    {
        public float pushForce = 5.0f;
        public float forwardFriction = 0.005f;
        public float sideFriction = 0.6f;

        private Rigidbody rb;   // rigidbody of the skateboard
        private VRC_Pickup vRC_Pickup;  // pickup component

        public VRCStation vRC_Station; // station follower

        private VRCPlayerApi player;

        private bool canMount = true;
        private float mountCooldown = 0.0f;
        private float mountCooldownDuration = 0.5f;


        /** STATES */

        private bool STATE_RIDING = false;
        private bool STATE_GROUNDED = false;
        private bool STATE_PICKED_UP = false;

        void Start()
        {
            _init();
        }


        void Update()
        {
            _setPickupable();
            _setGrounded();
            if (STATE_RIDING)
            {
                Vector3 normal = _calculateNormal();
                Vector3 forward = _calculateForwardRotation(normal);
                if (Vector3.Angle(rb.velocity, forward) > 90)
                {
                    forward = -forward;
                }
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
            var normal = _calculateNormal();
            var forward = _calculateForwardRotation(normal);
            rb.rotation = Quaternion.LookRotation(forward, normal);
        }

        public void Push()
        {
            if (!STATE_GROUNDED || !STATE_RIDING)
            {
                return;
            }
            Vector3 normal = _calculateNormal();
            Vector3 forward = _calculateForwardRotation(normal).normalized;
            if (Vector3.Angle(rb.velocity, forward) > 90)
            {
                forward = -forward;
            }

            forward -= Vector3.Project(forward, Physics.gravity);

            rb.AddForce(forward * pushForce, ForceMode.Impulse);
        }

        public void Turn(float amount)
        {
            if (!STATE_GROUNDED || !STATE_RIDING)
            {
                return;
            }
            float turnForce = 2.0f;
            Vector3 normal = _calculateNormal();
            rb.AddTorque(rb.gameObject.transform.up * turnForce * amount);
        }

        public void Mount(VRCPlayerApi player)
        {
            if (STATE_RIDING || !STATE_GROUNDED || !canMount)
            {
                return;
            }
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
            vRC_Station.ExitStation(player);
            player.SetVelocity(rb.velocity + -Physics.gravity.normalized * player.GetJumpImpulse());
            player.Immobilize(false);
            canMount = false;
            mountCooldown = mountCooldownDuration;
            STATE_RIDING = false;
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
        }


        private Vector3 _calculateForwardRotation(Vector3 normal)
        {
            return Vector3.Cross(Vector3.Cross(normal, transform.forward), normal);

        }

        private Vector3 _calculateNormal()
        {
            if (!Physics.Raycast(transform.position, -transform.up, out RaycastHit hitInfo, 0.5f))
            {
                return transform.up;
            }
            return hitInfo.normal;

        }

        private void _setGrounded()
        {
            STATE_GROUNDED = Physics.Raycast(transform.position, -transform.up, out RaycastHit hitInfo, 0.5f);
        }

        private void _setPickupable()
        {
            vRC_Pickup.pickupable = !STATE_RIDING;
        }
    }
}