
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonSkate.Skateboard
{

    public class DeckController : UdonSharpBehaviour
    {
        private Rigidbody _rigidbody;
        void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

    }
}