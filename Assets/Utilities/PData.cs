using UnityEngine;

namespace PLib.Data
{
    /// <summary>
    /// 2016-6-16
    /// Data saving and loading library.
    /// </summary>
    public static class PData
    {
        //////////////////////////////
        //	preferences and options	//
        //////////////////////////////

        #region Player Preferences

        /// <summary>
        /// Save the specified data into PlayerPrefs using the key provided.
        /// </summary>
        public static void Save(string key, int data)
        {
            PlayerPrefs.SetInt(key, data);
        }

        /// <summary>
        /// Save the specified data into PlayerPrefs using the key provided.
        /// </summary>
        public static void Save(string key, float data)
        {
            PlayerPrefs.SetFloat(key, data);
        }

        /// <summary>
        /// Save the specified data into PlayerPrefs using the key provided.
        /// </summary>
        public static void Save(string key, string data)
        {
            PlayerPrefs.SetString(key, data);
        }

        /// <summary>
        /// Load the specified key into the data parameter.
        /// </summary>
        public static void Load(string key, out int data)
        {
            data = PlayerPrefs.GetInt(key);
        }

        /// <summary>
        /// Load the specified key into the data parameter.
        /// </summary>
        public static void Load(string key, out float data)
        {
            data = PlayerPrefs.GetFloat(key);
        }

        /// <summary>
        /// Load the specified key into the data parameter.
        /// </summary>
        public static void Load(string key, out string data)
        {
            data = PlayerPrefs.GetString(key);
        }

        #endregion
    }
}