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
            Debug.Log("Coroutine started. c is null? " + (c == null));

            Debug.Log("Start DestroyWhenFinished() coroutine");
            StartCoroutine(DestroyWhenFinished());

        }
        
        private IEnumerator DestroyWhenFinished()
        {
            Debug.Log("DestroyWhenFinished() called at " + Time.time);
            do
            {
                yield return new WaitForSeconds(Random.value+1);
            } while (c != null);

            Debug.Log("DestroyWhenFinished() complete at " + Time.time);
            Destroy(gameObject, Random.value+0.1f);
        }
    }
}
