using System.Collections;
using UnityEngine;

namespace PLib.Pooling
{
    public class PCoroutine : MonoBehaviour
    {
        private Coroutine c;

        /// <summary>
        /// 2017-8-8
        /// Creates a temporary GameObject and attaches a PCoroutine 
        /// MonoBehaviour. This is used to run coroutines for delayed
        /// actions. The GameObject deletes itself once complete.
        /// </summary>
        public static PCoroutine GetCoroutineRunner(string name)
        {
            //  Create a game object with no renderer or geometry
            GameObject g = new GameObject("_CoroutineRunner_" + name + "_" + Time.time);

            //  Attach a PCoroutine MonoBehavior to the GameObject
            PCoroutine pcoroutine = g.AddComponent<PCoroutine>();

            //  Return a reference to the PCoroutine
            return pcoroutine;
        }

        /// <summary>
        /// 2017-8-8
        /// Starts a coroutine using the delegate.
        /// </summary>
        public void StartCoroutineDelegate(IEnumerator ienumerator)
        {
            c = StartCoroutine(ienumerator);
            StartCoroutine(DestroyWhenFinished());
        }

        /// <summary>
        /// 2017-8-8
        /// Monitors the status of the running coroutine by checking 
        /// every few seconds (1~3). Once the coroutine has terminated,
        /// this destroys this GameObject.
        /// </summary>
        private IEnumerator DestroyWhenFinished()
        {
            do
            {
                yield return new WaitForSeconds(Random.value + 1);
            } while (c != null);            

            if (Application.isEditor) DestroyImmediate(gameObject);
            else Destroy(gameObject, Random.value + 0.1f);
        }
    }
}
