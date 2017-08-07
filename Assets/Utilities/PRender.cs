using UnityEngine;

namespace PLib
{
    public static partial class PRender
    {
        public static void Show(this GameObject source, bool show = true)
        {
            Debug.LogWarning("No Unit Test for PRender.Show()");
            Renderer[] r = source.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < r.Length; i++)
            {
                r[i].enabled = show;
            }
        }
    }
}
