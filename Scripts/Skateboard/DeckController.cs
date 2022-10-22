
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonSkate.Skateboard
{
    public class DeckController : UdonSharpBehaviour
    {
        public WheelController[] wheels;

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
            rb.AddForce(-Vector3.Project(rb.velocity, -normal), ForceMode.Impulse);
            rb.AddForce(-Vector3.Project(Physics.gravity, -normal), ForceMode.Acceleration);

            if (Physics.Raycast(transform.position, -normal, out RaycastHit hitInfo, 0.3f))
            {
                if (Vector3.Distance(transform.position, hitInfo.point) > 0.2f)
                {
                    rb.AddForce(-Vector3.Project(Physics.gravity, -normal), ForceMode.Acceleration);
                }
            }
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