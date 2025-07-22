
using UnityEngine;

namespace Gameplay.Objects
{
    public class TestSuckableObject : MonoBehaviour
    {
        public Rigidbody tableTop;
        public Rigidbody[] legs;
        public float delay = 0.1f;

        private void Start()
        {
            StartCoroutine(EnablePhysicsAfterDelay());
        }

        private System.Collections.IEnumerator EnablePhysicsAfterDelay()
        {
            yield return new WaitForSeconds(delay);

            tableTop.isKinematic = false;
            foreach (var leg in legs)
            {
                leg.isKinematic = false;
            }
        }
    }
}