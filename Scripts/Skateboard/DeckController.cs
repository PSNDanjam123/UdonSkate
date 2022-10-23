
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace UdonSkate.Skateboard
{
    public class DeckController : UdonSharpBehaviour
    {
        public WheelController[] wheels;

        public float pushForce = 5.0f;
        public float forwardFriction = 0.005f;
        public float sideFriction = 0.6f;

        private Rigidbody rb;   // rigidbody of the skateboard
        private VRC_Pickup vRC_Pickup;  // pickup component

        public VRCStation vRC_Station; // station follower

        private VRCPlayerApi player;

        private bool canMount = true;
        private float mountCooldown = 0.0f;
        private float mountCooldownDuration = 2.0f;


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
            if (STATE_PICKED_UP)
            {
                return; // no need to process anything as player is holding
            }
            if (STATE_GROUNDED)
            {
                Vector3 normal = _calculateNormal();
                _applySurfaceForce(normal);
                Vector3 forward = _calculateForwardRotation(normal);
                rb.rotation = Quaternion.LookRotation(forward, normal);
            }
            if (STATE_RIDING)
            {
                Vector3 normal = _calculateNormal();
                Vector3 forward = _calculateForwardRotation(normal);
                if (Vector3.Angle(rb.velocity, forward) > 90)
                {
                    forward = -forward;
                }
                vRC_Station.gameObject.transform.position = transform.position;
                vRC_Station.gameObject.transform.rotation = Quaternion.LookRotation(new Vector3(forward.x, 0, forward.z), -Physics.gravity);
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
            rb.AddForce(forward * pushForce, ForceMode.Impulse);
        }

        public void Turn(float amount)
        {
            if (!STATE_GROUNDED || !STATE_RIDING)
            {
                return;
            }
            float turnForce = 10.0f;
            Vector3 normal = _calculateNormal();
            rb.AddTorque(rb.gameObject.transform.up * turnForce * amount);
        }

        public void Mount(VRCPlayerApi player)
        {
            if (STATE_RIDING || !STATE_GROUNDED || !canMount)
            {
                return;
            }
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
            player.SetVelocity(Physics.gravity.normalized * 1.0f);
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
        }

        private void _applySurfaceForce(Vector3 normal)
        {
            // redirect force
            rb.AddForce(-Vector3.Project(rb.velocity, -normal), ForceMode.Impulse);
            rb.AddForce(-Vector3.Project(Physics.gravity * 0.3f, -normal), ForceMode.Acceleration);

            // add distance if getting too close to ground
            if (Physics.Raycast(transform.position, -normal, out RaycastHit hitInfo, 0.2f))
            {
                float dist = Vector3.Distance(transform.position, hitInfo.point);
                rb.AddForce(-Vector3.Project(Physics.gravity * (1 - dist / 0.2f), -normal), ForceMode.Acceleration);
                if (dist < 0.15f)
                {
                    rb.AddForce(-Vector3.Project(Physics.gravity * 0.1f, -normal), ForceMode.Impulse);
                }
            }

            // reduce sideways movement
            if (Vector3.Angle(rb.velocity.normalized, transform.right) > 90)
            {
                rb.AddForce(-Vector3.Project(rb.velocity, -transform.right) * sideFriction, ForceMode.Impulse);
            }
            else
            {
                rb.AddForce(-Vector3.Project(rb.velocity, transform.right) * sideFriction, ForceMode.Impulse);
            }

            // reduce forward movement
            if (Vector3.Angle(rb.velocity.normalized, transform.forward) > 90)
            {
                rb.AddForce(-Vector3.Project(rb.velocity, -transform.forward) * forwardFriction, ForceMode.Impulse);
            }
            else
            {
                rb.AddForce(-Vector3.Project(rb.velocity, transform.forward) * forwardFriction, ForceMode.Impulse);
            }

            // reduce torque
            rb.AddTorque(new Vector3(0, rb.angularVelocity.y * -0.5f, 0), ForceMode.Acceleration);
        }

        private Vector3 _calculateForwardRotation(Vector3 normal)
        {
            return Vector3.Cross(Vector3.Cross(normal, transform.forward), normal);

        }

        private Vector3 _calculateNormal()
        {
            Vector3 normal = Vector3.zero;
            foreach (var wheel in wheels)
            {
                if (!wheel.Collision)
                {
                    continue;
                }
                normal += wheel.HitInfo.normal;

            }
            if (normal == Vector3.zero)
            {
                normal = transform.up;
            }
            return normal;
        }

        private void _setGrounded()
        {
            foreach (var wheel in wheels)
            {
                if (wheel.Collision)
                {
                    STATE_GROUNDED = true;
                    return;
                }

            }
            STATE_GROUNDED = false;
        }

        private void _setPickupable()
        {
            vRC_Pickup.pickupable = !STATE_RIDING;
        }
    }
}