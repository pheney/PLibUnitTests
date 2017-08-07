using UnityEngine;

namespace PLib.TestHelper
{
    public class TestHelper
    {
        public static string TEST_TAG = "UnitTest";

        #region Show a single variable

        public static string ShowVariable(string label, int value)
        {
            return label + ": " + value;
        }

        public static string ShowVariable(string label, float value)
        {
            return label + ": ~" + value + "f";
        }

        public static string ShowVariable(string label, bool value)
        {
            return label + " is " + value;
        }

        public static string ShowVariable(string label, string value)
        {
            return label + ": '" + value + "'";
        }

        public static string ShowVariable(string label, Color value)
        {
            return label + ": " + value.ToString();
        }

        public static string ShowVariable(string label, Vector3 value)
        {
            return label + ": " + value.ToString();
        }

        #endregion

        #region Show multiple variables

        public static string ShowVariables(Color expected, Color actual)
        {
            return expected.ToString() + " = " + actual.ToString();
        }

        public static string ShowVariables(int expected, int actual)
        {
            return expected + " = " + actual;
        }

        public static string ShowVariables(float expected, float actual)
        {
            return expected + " ~ " + actual;
        }

        #endregion

        #region Show camparisons between variables

        public static string ShowGComparison(float expected, float actual)
        {
            return expected + " > " + actual;
        }

        public static string ShowLComparison(float expected, float actual)
        {
            return expected + " < " + actual;
        }

        public static string ShowEComparison(float expected, float actual)
        {
            return expected + " == " + actual;
        }

        public static string ShowNEComparison(float expected, float actual)
        {
            return expected + " != " + actual;
        }

        #endregion

        public static string ShowVariables(string expected, string actual)
        {
            return "'" + expected + "' matches '" + actual + "'";
        }

        public static string ShowVariables(UnityEngine.Vector3 expected, UnityEngine.Vector3 actual)
        {
            return expected.ToString() + " == " + actual.ToString();
        }

        public static string LabelAs(string variableName)
        {
            return System.DateTime.Now.ToLongTimeString() + ", Test variable: " + variableName;
        }

        /// <summary>
        /// Deletes all existing cameras. Creates a new main camera.
        /// </summary>
        public static void ResetAllCameras()
        {
            Camera[] junk = GameObject.FindObjectsOfType<Camera>();
            for (int i = 0; i < junk.Length; i++)
            {
                MonoBehaviour.DestroyImmediate(junk[0].gameObject);
            }

            GameObject c = new GameObject("_CameraForUnitTest");
            c.AddComponent<Camera>();
            c.tag = "MainCamera";
            c.transform.position = Vector3.back * 10;
            c.transform.LookAt(Vector3.forward);
        }

        public static void CleanUpGameObjects()
        {
            GameObject[] junk = GameObject.FindObjectsOfType<GameObject>();
            for (int i = 0; i < junk.Length; i++)
            {
                MonoBehaviour.DestroyImmediate(junk[0].gameObject);
            }
        }
    }
}