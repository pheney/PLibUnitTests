using System.Text;
using UnityEngine;
using PLib.Math;

namespace PLib.Logging
{
    /// <summary>
    /// Console logging
    /// </summary>
    public static class PLog
    {
        public static bool EchoToConsole = true;
        private static string ConsoleSeparator = " > ";

        public static string NOT_IMPLEMENTED = "Not implemented.";
        public static string NOT_UNIT_TESTS = "No unit tests conducted.";

        /// <summary>
        /// 2016-6-2
        /// </summary>
        public static string PropertiesToString(this object source)
        {
            StringBuilder s = new StringBuilder();
            s.Append(source.ToString() + " properties");
            foreach (var p in source.GetType().GetProperties())
            {
                s.Append("\n" + p + " = " + p.GetValue(source, null));
            }
            return s.ToString();
        }

        /// <summary>
        /// Returns the name of the GameObject.
        /// </summary>
        public static string NiceName(this GameObject gameObject)
        {
            return gameObject.name.Split('(')[0];
        }

        /// <summary>
        /// Creates a tag for console logging. Format is:
        /// [GameObject name] [GameObject instanceID] [info] [ConsoleSeparator]
        /// </summary>
        /// <param name="info">Additional Info (usually the name of the calling method).</param>
        public static string ConsoleTag(this GameObject source, string info = "")
        {
            return source.NiceName()
                + " (ID " + source.GetInstanceID() + ")"
                + (info.Length > 0 ? ":" + info : "")
                + ConsoleSeparator;
        }

        /// <summary>
        /// Prepends the runtime to the string
        /// </summary>
        /// <returns>The string with runtime added.</returns>
        /// <param name="decimalPlaces">Decimal places to use for the runtime.</param>
        public static string PrependRuntime(this string source, int decimalPlaces = 3)
        {
            return PMath.Trunc(Time.time, decimalPlaces) + ":" + source;
        }

        /// <summary>
        /// Returns the method name where this was called.
        /// </summary>
        /// <returns>The method name.</returns>
        public static string MethodName(this Component script)
        {
            //	requires System.Reflection
            //return MethodBase.GetCurrentMethod().Name;
            return StackTraceUtility.ExtractStackTrace().Split(':')[3].Split('(')[0] + "()";
        }

        public static string StaticMethodName()
        {
            return StackTraceUtility.ExtractStackTrace().Split('\n')[1].Split(')')[0] + ")";
        }

        public static void LogNoUnitTests()
        {
            Debug.LogWarning(StackTraceUtility.ExtractStackTrace().Split('\n')[1].Split(')')[0] + ") " + NOT_UNIT_TESTS);
        }

        /// <summary>
        /// Echo the specified message to the console.
        /// </summary>
        /// <param name="message">Message.</param>
        public static void Echo(string message)
        {
            if (EchoToConsole) Debug.Log(message);
        }
    }
}