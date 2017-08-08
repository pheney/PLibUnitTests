using System.Collections;
using UnityEngine;

namespace PLib
{
    public class PCoroutine : MonoBehaviour
    {
        private Coroutine c;

        /// <summary>
        /// Starts a coroutine using the delegate.
        /// </summary>
        public void StartCoroutineDelegate(IEnumerator ienumerator)
        {
            Debug.Log("StartCoroutineDelegate() called");
            c = StartCoroutine(ienumerator);

            Debug.Log("DestroyWhenFinished() called");
            StartCoroutine(DestroyWhenFinished());

        }

        private IEnumerator DestroyWhenFinished()
        {
            Debug.Log("Enter DestroyWhenFinished() at " + Time.time);
            do
            {
                yield return new WaitForSeconds(Random.value + 1);
            } while (c != null);

            Debug.Log("EXIT DestroyWhenFinished() at " + Time.time);
            Destroy(gameObject, Random.value + 0.1f);
        }
    }
}
