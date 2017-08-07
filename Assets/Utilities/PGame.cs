using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace PLib
{
    /// <summary>
    /// Game related Interface defintions
    /// </summary>
    public static class PGame
    {
        //////////////////////////
        //	Common Messages		//
        //////////////////////////

        #region Common Messages

        public const string NO_MESSAGE = "no message";

        #endregion

        /// <summary>
        /// Gets the random name.
        /// </summary>
        /// <returns>A random name.</returns>
        /// <param name="wordCount">Word count.</param>
        public static string GetRandomName(int wordCount)
        {
            throw new System.NotImplementedException();
        }

        //	UI Extensions

        #region UI Extensions

        public static Toggle GetActive(this ToggleGroup group)
        {
            return group.ActiveToggles().FirstOrDefault();
        }

        #endregion
    }

    //////////////////////
    //	Interfaces		//
    //////////////////////

    #region Interfaces

    #region Transformable

    public static class Transformable
    {
        public const string UPDATE_VELOCITY = "OnUpdateVelocity";
        public const string UPDATE_DIRECTION = "OnUpdateRotation";
        public const string UPDATE_SCALE = "OnUpdateScale";

        public interface ITransformable
        {
            void OnUpdateVelocity(Vector3 updatedVelocity);
            void OnUpdateRotation(Quaternion updatedRotation);
            void OnUpdateScale(Vector3 updatedScale);
        }
    }

    #endregion
    #region Directable

    public static class Directable
    {
        public const string MOVE_TO = "OnMoveToPosition";
        public const string ORIENT_TO = "OnOrientToDirection";
        public const string ALIGN_WITH = "OnAlignWithTransform";

        public interface IDirectable
        {
            void OnMoveToPosition(Vector3 goalPosition);
            void OnOrientToDirection(Quaternion goalDirection);
            void OnAlignWithTransform(Transform goalTransform);
        }
    }

    #endregion


    #endregion

}