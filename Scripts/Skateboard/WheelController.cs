
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonSkate.Skateboard
{
    public class WheelController : UdonSharpBehaviour
    {
        public DeckController deck;
        public float radius;

        private RaycastHit hitInfo;
        private bool collision = false;

        private Rigidbody rb;

        void Start()
        {
            _init();
        }

        void Update()
        {
            _raycast();
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

        private void _raycast()
        {
            collision = Physics.Raycast(transform.position, -transform.up, out hitInfo, radius);
            Debug.DrawLine(transform.position, transform.position + (-transform.up * radius), Color.white);
        }

    }
}