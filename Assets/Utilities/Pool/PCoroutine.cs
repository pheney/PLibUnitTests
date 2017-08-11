using System.Collections;
using UnityEngine;

namespace PLib.Pooling
{
    public class PCoroutine : MonoBehaviour
    {
        /// <summary>
        /// 2017-8-10
        /// Stand alone, fire and forget coroutine runner.
        /// Creates a temporary GameObject and attaches a PCoroutine 
        /// MonoBehaviour. This is used to run coroutines for delayed
        /// actions. The GameObject deletes itself once complete.
        /// </summary>
        public static void CreateCoroutineRunner(IEnumerator ienumerator, string name = "")
        {
            //  Create a game object with no renderer or geometry
            GameObject g = new GameObject("_CoroutineRunner_" + name + "_" + Time.time);

            //  Attach a PCoroutine MonoBehavior to the GameObject
            PCoroutine pcoroutine = g.AddComponent<PCoroutine>();

            //  Start the coroutines
            pcoroutine.StartCoroutine(pcoroutine.DestroyWhenFinished(ienumerator));
        }

        /// <summary>
        /// 2017-8-10
        /// Destroys this GameObject once the parameter coroutine finishes
        /// executing.
        /// </summary>
        private IEnumerator DestroyWhenFinished(IEnumerator ienumerator)
        {
            yield return StartCoroutine(ienumerator);

            if (Application.isEditor) DestroyImmediate(gameObject);
            else Destroy(gameObject, Random.value + 1);
        }
    }
}
