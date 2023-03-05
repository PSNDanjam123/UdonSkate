
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonSkate.Skateboard
{
    public class Wheel : UdonSharpBehaviour
    {
        public LayerMask RaycastLayerMask;
        public MeshFilter WheelMesh;
        public Rigidbody m_rigidbody;
        private RaycastHit hitInfo;
        private bool collisionHit;
        private Bounds bounds;
        private Vector3 center;
        private float radius;

        void Start()
        {
            bounds = WheelMesh.mesh.bounds;
            radius = bounds.size.y / 2;
            center = bounds.center;
        }

        void FixedUpdate()
        {
            var hit = Raycast();
            if (!hit)
            {
                return;
            }

            var currentVelocity = m_rigidbody.GetPointVelocity(hitInfo.point);
            var velocity = CalculateCollisionResponse(currentVelocity, hitInfo.normal);
            var gravityVelocity = CalculateCollisionResponse(Physics.gravity, hitInfo.normal);
            var worldCenter = transform.TransformPoint(center);

            var multiplier = 0.33f;

            ApplyCollisionResponse(gravityVelocity * multiplier, hitInfo.point, hitInfo.normal, ForceMode.Acceleration);
            ApplyCollisionResponse(velocity * multiplier, hitInfo.point, hitInfo.normal, ForceMode.Impulse);

            ApplyPositionCorrection(hitInfo.point - worldCenter, radius, hitInfo.distance);
        }

        bool Raycast()
        {
            var worldCenter = transform.TransformPoint(center);
            return collisionHit = Physics.Raycast(worldCenter, Vector3.down, out hitInfo, radius, RaycastLayerMask);
        }

        void ApplyCollisionResponse(Vector3 force, Vector3 point, Vector3 normal, ForceMode forceMode)
        {
            var right = m_rigidbody.transform.right;
            var forward = Vector3.Cross(normal, right);
            var rightMag = Vector3.Project(force, right).magnitude;
            var forwardMag = Vector3.Project(force, forward).magnitude;

            var redist = 0.8f;
            forwardMag += rightMag * redist;
            rightMag -= rightMag * redist;

            force += (right * rightMag) + (forward * forwardMag);

            m_rigidbody.AddForceAtPosition(force, point, forceMode);
        }

        Vector3 CalculateCollisionResponse(Vector3 currentVelocity, Vector3 collisionNormal)
        {
            float angle = Vector3.Angle(currentVelocity.normalized, -collisionNormal);
            if (angle > 90)
            {
                return Vector3.zero;
            }
            return -Vector3.Project(currentVelocity, -collisionNormal);
        }

        void ApplyPositionCorrection(Vector3 direction, float radius, float hitDistance)
        {
            var difference = radius - hitDistance;
            if (difference <= 0)
            {
                return;
            }
            m_rigidbody.transform.position += (-direction * difference);
        }

    }
}