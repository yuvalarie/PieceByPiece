using System.Collections;
using UnityEngine;

namespace Core.Managers
{
    public class EndSceneSequenceManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] allStatues;
        [SerializeField] private GameObject[] walls;
        [SerializeField] private Camera uiCamera;
        
        private void OnEnable()
        {
            EventManager.GameOver += OnEndGame;
        }
        
        private void OnDisable()
        {
            EventManager.GameOver -= OnEndGame;
        }

        private void OnEndGame()
        {
            foreach (var statue in allStatues)
            {
                if (statue != null)
                {
                    BreakStatue(statue);
                }
                else
                {
                    Debug.LogWarning("Statue is null, skipping.");
                }
            }
            StartCoroutine(EndSequenceCoroutine());
            //EventManager.Instance.OnEndSequenceOver();
        }
        
        private IEnumerator EndSequenceCoroutine()
        {
            yield return new WaitForSeconds(4f); // Wait for 1 second before breaking the walls
            BreakWalls();
            yield return new WaitForSeconds(5f); // Optional: small delay for effect
            EventManager.Instance.OnEndSequenceOver();
        }

        private void BreakStatue(GameObject statue)
        {
            foreach (Transform child in statue.transform)
            {
                // check if the child has Fixed Joint components and destroy them
                FixedJoint[] fixedJoints = child.GetComponents<FixedJoint>();
                foreach (var joint in fixedJoints)
                {
                    if (joint != null)
                        Destroy(joint);
                }
                // check if the child has a Rigidbody component
                Rigidbody rb = child.GetComponent<Rigidbody>();
                if (rb!=null)
                    rb.isKinematic = false;
            }
        }

        private void BreakWalls()
        {
            Debug.Log("Breaking walls...");
            uiCamera.enabled = false;
            foreach (var wall in walls)
            {
                if (wall != null)
                {
                    // add Rigidbody to the wall
                    wall.AddComponent(typeof(Rigidbody));
                    Rigidbody rb = wall.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = false; // Set the Rigidbody to non-kinematic to allow physics interaction
                        rb.useGravity = true; // Enable gravity so the wall falls
                        Debug.Log("Wall broken: " + wall.name);
                    }
                    else
                    {
                        Debug.LogWarning("Rigidbody component could not be added to the wall.");
                    }

                }
                else
                {
                    Debug.LogWarning("Wall is null, skipping.");
                }
            }
        }
        
    }
}