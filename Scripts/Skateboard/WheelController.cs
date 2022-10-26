
using UdonSharp;
using UnityEngine;

namespace UdonSkate.Skateboard
{
    public class WheelController : UdonSharpBehaviour
    {
        public DeckController deck;
        public float radius;

        public float friction = 0.8f;

        private RaycastHit hitInfo;
        private RaycastHit lastHitInfo;
        private bool collision = false;

        private Rigidbody rb;

        void Start()
        {
            _init();
        }

        void FixedUpdate()
        {
            collision = Physics.Raycast(transform.position, -transform.up, out hitInfo, radius);
            if (!collision)
            {
                return;
            }

            // clipping
            var offset = radius - hitInfo.distance;
            rb.transform.Translate(transform.up * offset);

            // forces
            var normal = hitInfo.normal;
            var point = hitInfo.point;
            var velocity = -Vector3.Project(rb.velocity, -normal);
            var gravVelocity = -Vector3.Project(Physics.gravity, -normal);
            rb.AddForceAtPosition(velocity, point, ForceMode.Impulse);
            rb.AddForceAtPosition(gravVelocity, point, ForceMode.Acceleration);

            // friction
            var right = transform.right;
            if (Vector3.Angle(velocity, right) > 90)
            {
                right = -right;
            }
            rb.AddForceAtPosition(-Vector3.Project(rb.velocity, right) * friction, hitInfo.point, ForceMode.Impulse);

        }

        public bool Collision
        {
            get
            {
                return collision;
            }
        }

        public RaycastHit HitInfo
        {
            get
            {
                return hitInfo;
            }
        }
        private void _init()
        {
            rb = deck.GetComponent<Rigidbody>();
        }

    }
}