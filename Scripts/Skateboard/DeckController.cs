
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonSkate.Skateboard
{
    public class DeckController : UdonSharpBehaviour
    {
        public WheelController[] wheels;

        public float forwardFriction = 0.005f;
        public float sideFriction = 0.2f;

        private Rigidbody rb;

        private bool grounded = false;

        void Start()
        {
            _init();
        }


        void Update()
        {
            _setGrounded();
            if (grounded)
            {
                Vector3 normal = _calculateNormal();
                _applySurfaceForce(normal);
                Vector3 forwardRot = _calculateForwardRotation(normal);
                Debug.DrawLine(transform.position, transform.position - forwardRot, Color.magenta);
                rb.rotation = Quaternion.LookRotation(forwardRot, normal);
            }
        }

        private void _init()
        {
            rb = GetComponent<Rigidbody>();
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
                    grounded = true;
                    return;
                }

            }
            grounded = false;
        }
    }
}