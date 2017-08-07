using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
using System.Text;
using PLib.Math;
using PLib.Rand;
using PLib.Logging;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Supress warning 0162 (Unreachable code)
/// </summary>
#pragma warning disable 0162
namespace PLib.General
{
    /// <summary>
    /// 2016-6-17
    /// General utility methods and extensions
    /// </summary>
    public static class PUtil
    {
        //////////////////////////////////////
        //	Aiming and Altitude Info		//
        //////////////////////////////////////

        #region Altitude

        /// <summary>
        /// Gets the altitude the object is above "ground". "Ground" is all objects on the layer provided.
        /// The returned value is the worldspace Y value difference in this object's position, and the
        /// object this is directly over.
        /// </summary>
        /// <returns>The altitude above ground.</returns>
        /// <param name="layer">The ground layer.</param>
        public static float GetAltitudeAGL(this Transform item, int layer)
        {
            float agl = -1;
            RaycastHit info;
            if (Physics.Raycast(item.position, Vector3.down, out info, 1 << layer))
            {
                agl = item.position.y - info.point.y;
            }

            return agl;
        }

        /// <summary>
        /// Gets the altitude the object is above "ground". "Ground" is all objects on the layer provided.
        /// The returned value is the worldspace Y value difference in this object's position, and the
        /// object this is direclty over.q
        /// </summary>
        /// <returns>The altitude above ground.</returns>
        /// <param name="groundLayerName">The ground layer.</param>
        public static float GetAltitudeAGL(this Transform item, string groundLayerName)
        {
            return GetAltitudeAGL(item, LayerMask.NameToLayer(groundLayerName));
        }

        #endregion
        #region Aiming

        /// <summary>
        /// 2016-3-21 -- Refactored to LookAwayFrom()
        /// Look directly away from the target. Opposite of LookAt()
        /// </summary>
        /// <param name="target">The target transform.</param>
        public static void LookAwayFrom(this Transform source, Transform target)
        {
            Quaternion lookRotation = Quaternion.LookRotation(source.position - target.position);
            source.rotation = lookRotation;
        }

        /// <summary>
        /// 2016-3-21 -- Added overloading for GameObject
        /// Look directly away from the target. Opposite of LookAt()
        /// </summary>
        /// <param name="target">The target transform.</param>
        public static void LookAwayFrom(this GameObject source, GameObject target)
        {
            source.transform.LookAwayFrom(target.transform);
        }

        /// <summary>
        /// 2016-10-19
        /// Returns true if the angle between this object's facing direction (the object's forward vector)
        /// and the direction to the target is less than the maximum allowable error.
        /// </summary>
        /// <returns><c>true</c>, if within allowable error, <c>false</c> otherwise.</returns>
        public static bool IsAimedWithinTolerance(this Transform firer, Transform target, float maxErrorAngle)
        {
            return IsAimedWithinTolerance(firer.position, firer.forward, target.position, maxErrorAngle);
        }

        /// <summary>
        /// DEPRECATED
        /// 2016-10-19
        public static bool IsAimedWithinTolerance(this Transform firer, Vector3 target, float maxErrorAngle)
        {
            throw new Exception("IsAimedWitinTolerance(Transform, Vector3, float) signature is deprecated. Convert to new signature.");
        }

        /// <summary>
        /// 2016-3-21
        /// Returns true if the angle between this object and the target is less than the maximu allowable error.
        /// </summary>
        /// <returns><c>true</c>, if within allowable error, <c>false</c> otherwise.</returns>
        /// <param name="target">Target.</param>
        /// <param name="maxErrorAngle">Max error angle.</param>
        public static bool IsAimedWithinTolerance(Vector3 source, Vector3 target, Vector3 direction, float maxErrorAngle)
        {
            return Vector3.Angle(direction, target - source) <= maxErrorAngle;
        }

        /// <summary>
        /// 2016-12-7 -- WORKS
        /// Returns aim point to hit the target.
        /// Assumes firer is stationary.
        /// Accounts for target movement and speed of the shot.
        ///	Simplified version of GetInterceptPoint(). Makes the following assumptions:
        ///	    1) firer is not moving
        ///	    2) target is moving in it's "forward" direction
        /// </summary>
        public static Vector3 LeadTarget(Transform firer, Transform target, float targetSpeed, float shotSpeed)
        {
            Vector3 relativeVelocity = target.forward * targetSpeed;
            Vector3 relativePosition = target.position - firer.position;
            return target.TransformDirection(GetInterceptPoint(relativePosition, relativeVelocity, shotSpeed));
        }

        /// <summary>
        /// 2016-10-19
        /// Returns the intercept point (relative to the target) for a projectile and a target, 
        /// using the target and firer's relative position and relative velocity,
        /// as well as the speed of the projectile.
        /// Returns (0, 0, 0) when the intercept point is the same as the target location.
        /// Returns (Infinity, Infinity, Infinity) when there is no intercept point.
        /// Ref: http://wiki.unity3d.com/index.php/Calculating_Lead_For_Projectiles
        /// </summary>
        public static Vector3 GetInterceptPoint(Vector3 relativePosition, Vector3 relativeVelocity, float shotSpeed)
        {
            float flightTime = GetInterceptTime(relativePosition, relativeVelocity, shotSpeed);
            if (flightTime < 0) return Vector3.zero;
            if (flightTime.Equals(Mathf.Infinity)) return Vector3.one * Mathf.Infinity;
            return flightTime * relativeVelocity;
        }

        /// <summary>
        /// 2016-10-19
        /// Returns the intercept time based on a relative position, relative velocity and shot speed.
        /// Returns INFINITY when there are no solutions.
        /// Ref: http://wiki.unity3d.com/index.php/Calculating_Lead_For_Projectiles
        /// </summary>
        public static float GetInterceptTime(Vector3 relativePosition, Vector3 relativeVelocity, float shotSpeed)
        {
            //  edge case where shotspeed is zero
            //if (Mathf.Approximately(shotSpeed,0)) return -1;

            //  edge case where relative position is zero
            //if (relativePosition.Equals(Vector3.zero)) return 0;

            //  edge case where relative velocity is zero
            if (relativeVelocity.Equals(Vector3.zero)) return 1/shotSpeed;

            float a = relativeVelocity.sqrMagnitude - Mathf.Pow(shotSpeed, 2);
            float b = 2f * Vector3.Dot(relativeVelocity, relativePosition);
            float c = relativePosition.sqrMagnitude;

            //  when there is no solution possible
            if (!PMath.IsQuadraticSolvable(a, b, c)) return Mathf.Infinity;

            //  find the solutions (there are "always" 2 -- sometimes they are both the same)
            Vector2 solutions = PMath.SolveQuadratic(a, b, c);

            //  when both solutions are positive, return the smaller of the two
            if (solutions.x >= 0 && solutions.y >= 0) return solutions.MinComponentMagnitude();

            //  when only one solution is positive, return the larger of the two
            if (solutions.x >= 0 || solutions.y >= 0) return solutions.MaxComponentMagnitude();
            
            //  both solutions are negative
            return Mathf.Infinity;
        }

        /// <summary>
        /// 2016-10-18
        /// Returns a quaternion direction that represents a firing angle.
        /// This direction is aimed at the target provided, and off EXACTLY by the error angle provided.
        /// So if you say 10 degrees, then the returned angle will be off by 10 degrees.
        /// </summary>
        public static Quaternion AimAtWithError(Transform firer, Transform target, float errorAngle)
        {
            //  generate the goal-aim-direction
            Quaternion goalDirection = Quaternion.LookRotation(target.position - firer.position);

            //  generate the aim-error

            //  Aim error is exactly the errorAngle value on the y-axis (left/right)
            Quaternion error = Quaternion.AngleAxis(errorAngle, Vector3.up);

            //  Rotate the error around the z-axis (forward)
            Quaternion forwardRotation = Quaternion.AngleAxis(360 * UnityEngine.Random.value, Vector3.forward);
            Quaternion errorDirection = forwardRotation * error;

            //  apply the error to the direction
            return goalDirection * errorDirection;
        }

        /// <summary>
        /// 2016-10-18
        /// Returns a quaternion direction that represents a firing angle.
        /// This direction is aimed at the target provided, and off EXACTLY by the horizontal and vertical
        /// angles provided.
        /// So if you say 10 & 5 degrees, then the returned angle will be off on the horizontal
        /// axis by 10 degrees and on the (local) vertical axis by 5 degrees.
        /// </summary>
        public static Quaternion AimAtWithError(Transform firer, Transform target, float errorHorizontal, float errorVertical)
        {
            //  generate the goal-aim-direction
            Vector3 directionVector = target.position - firer.position;
            Quaternion direction = Quaternion.LookRotation(directionVector);

            //  generate the horizontal and vertical projections of the direction vector
            Vector3 hProjection = directionVector;
            hProjection.x = 0;
            hProjection.Normalize();
            Vector3 vProjection = directionVector;
            vProjection.y = 0;
            vProjection.Normalize();

            //  generate the error components
            Quaternion errorH = Quaternion.AngleAxis(errorHorizontal, firer.up);
            Quaternion errorV = Quaternion.AngleAxis(errorVertical, firer.right);

            //  apply the error to the direction
            Quaternion result = direction * errorV;
            return result * errorH;
        }

        /// <summary>
        /// 2016-3-11
        /// Returns a quaternion direction that represents a firing angle.
        /// This direction is aimed at the target provided, and by a random amount, which is between the minimum
        /// and maximum values provided.
        /// </summary>
        public static Quaternion AimAtWithErrorBetween(Transform firer, Transform target, float minError, float maxError)
        {
            return AimAtWithError(firer, target, UnityEngine.Random.Range(minError, maxError));
        }

        #endregion

        //////////////////////////
        //	Environment			//
        //////////////////////////

        #region Environment

        public static void SetFog(FogData fogData)
        {
            RenderSettings.fog = fogData.fogEnabled;
            RenderSettings.fogColor = fogData.fogColor;
            RenderSettings.fogMode = fogData.fogMode;
            RenderSettings.fogDensity = fogData.fogDensity;
            RenderSettings.fogStartDistance = fogData.linearFogStart;
            RenderSettings.fogEndDistance = fogData.linearFogEnd;
            RenderSettings.ambientLight = fogData.ambientColor;
        }

        #endregion

        //////////////////////////
        //	Screen space		//
        //////////////////////////

        #region Screen space

        /// <summary>
        /// 2016-12-28
        /// Returns a Vector2 with the length and width of the screen as
        /// seen by the provided camera. Uses Camera.main by default.
        /// Unit test not implemented.
        /// </summary>
        public static Vector2 ScreenSize(Camera cam = null)
        {
            if (cam == null)
            {
                cam = Camera.main;
            }

            Vector2 size = Vector2.one *2;
            size.x *= cam.aspect;
            size *= cam.orthographicSize;

            return size;
        }

        /// <summary>
        /// Determines if the specified GameObject is on screen. Use the optional parameter to check against
        /// a camera other than Camera.main.
        /// </summary>
        /// <returns><c>true</c> if the specified position is on screen; otherwise, <c>false</c>.</returns>
        /// <param name="position">Position.</param>
        public static bool IsOnScreen(this GameObject gameObject, Camera cam = null)
        {
            return gameObject.transform.IsOnScreen(cam);
        }

        /// <summary>
        /// Determines if the specified Transform is on screen. Use the optional parameter to check against
        /// a camera other than Camera.main.
        /// </summary>
        /// <returns><c>true</c> if the specified position is on screen; otherwise, <c>false</c>.</returns>
        /// <param name="position">Position.</param>
        public static bool IsOnScreen(this Transform transform, Camera cam = null)
        {
            return transform.position.IsOnScreen(cam);
        }

        /// <summary>
        /// 2016-3-22
        /// Determines if the specified position is on screen. Use the optional parameter to check against
        /// a camera other than Camera.main.
        /// </summary>
        /// <returns><c>true</c> if the specified position is on screen; otherwise, <c>false</c>.</returns>
        /// <param name="position">Position.</param>
        public static bool IsOnScreen(this Vector3 position, Camera cam = null)
        {
            if (!cam) cam = Camera.main;

            Vector3 screenPosition = cam.WorldToScreenPoint(position);
            return screenPosition.x.IsBetween(0, cam.pixelWidth)
                && screenPosition.y.IsBetween(0, cam.pixelHeight);
        }

        /// <summary>
        /// Returns a Vector3 containing the object's position on the screen in pixels across and down
        /// from the top left corner of the screen. The .z value is always 0.
        /// </summary>
        /// <returns>The to Vector3 representation of the source location on the screen (in pixels).</returns>
        public static Vector3 PositionToScreenSpace(this GameObject source, Camera cam = null)
        {
            return source.transform.PositionToScreenSpace(cam);
        }

        /// <summary>
        /// 2016-3-22 -- renamed from "ToScreenSpace()"
        /// Returns a Vector3 containing the object's position on the screen in pixels across and down
        /// from the top left corner of the screen. The .z value is always 0.
        /// </summary>
        /// <returns>The to Vector3 representation of the source location on the screen (in pixels).</returns>
        public static Vector3 PositionToScreenSpace(this Transform source, Camera cam = null)
        {
            return source.position.PositionToScreenSpace(cam);
        }

        /// <summary>
        /// Returns a Vector3 containing the object's position on the screen in pixels across and down
        /// from the top left corner of the screen. The .z value is always 0.
        /// </summary>
        /// <returns>The to Vector3 representation of the source location on the screen (in pixels).</returns>
        public static Vector3 PositionToScreenSpace(this Vector3 source, Camera cam = null)
        {
            if (!cam) cam = Camera.main;
            return cam.WorldToScreenPoint(source);
        }

        /// <summary>
        /// 2016-10-29
        /// Determines if the camera can see specified renderer.
        /// </summary>
        public static bool CanSee(this Camera source, Renderer other)
        {
            //  CalculateFrustumPlanes:
            //  Returns 6 planes that define the camera view frustum.
            Plane[] frustum = GeometryUtility.CalculateFrustumPlanes(source);

            //  TestPlanesAABB:
            //  Returns true if a Bounds object is contained within the provided
            //  plane array (the camera frustum).

            //  Renderer.bounds:
            //  Returns an axis-aligned bounding box for the renderer.
            return GeometryUtility.TestPlanesAABB(frustum, other.bounds);
        }
        
        /// <summary>
        /// Determines if the camera can see the specified point.
        /// </summary>
        /// <returns><c>true</c> if the camera can see the specified point; otherwise, <c>false</c>.</returns>
        /// <param name="other">A point in 3D space</param>
        public static bool CanSee(this Camera source, Vector3 point)
        {
            Plane[] frustum = GeometryUtility.CalculateFrustumPlanes(source);
            return GeometryUtility.TestPlanesAABB(frustum, new Bounds(point, Vector3.zero));
        }

        /// <summary>
        /// Determines if the camera can see renderer the specified game object.
        /// If the game object lacks a renderer, this uses the game object's position
        /// to determine if it can be seen by the camera.
        /// </summary>
        /// <returns><c>true</c> if the camera can see the game object; otherwise, <c>false</c>.</returns>
        /// <param name="other">Game object.</param>
        public static bool CanSee(this Camera source, GameObject other)
        {
            if (!other.IsOnScreen()) return false;

            Renderer r = other.GetComponentInChildren<Renderer>();
            if (r) return source.CanSee(r);
            else return source.CanSee(other.transform.position);
        }

        #endregion

        //////////////////
        //	Sorting		//
        //////////////////

        #region Sorting

        /// <summary>
        /// Sorts the list from nearest to furthest, by distance measured from a target point.
        /// </summary>
        /// <returns>The sorted list.</returns>
        /// <param name="list">List of items to sort</param>
        /// <param name="target">Target.</param>
        public static List<GameObject> SortByDistanceFrom(this List<GameObject> list, GameObject target)
        {
            list.Sort((lh, rh) =>
            {
                return CompareDistanceToTarget(lh, rh, target);
            });
            return list;
        }

        /// <summary>
        /// Sorts the list from nearest to furthest, by distance measured from a target point.
        /// </summary>
        /// <returns>The sorted list.</returns>
        /// <param name="list">List of items to sort</param>
        /// <param name="target">Target.</param>
        public static List<Transform> SortByDistanceFrom(this List<Transform> list, Transform target)
        {
            list.Sort((lh, rh) =>
            {
                return CompareDistanceToTarget(lh, rh, target);
            });
            return list;
        }

        /// <summary>
        /// Sorts the list from nearest to furthest, by distance measured from a target point.
        /// </summary>
        /// <returns>The sorted list.</returns>
        /// <param name="list">List of items to sort</param>
        /// <param name="target">Target.</param>
        public static List<Vector3> SortByDistanceFrom(this List<Vector3> list, Vector3 target)
        {
            list.Sort((lh, rh) =>
            {
                return CompareDistanceToTarget(lh, rh, target);
            });
            return list;
        }

        /// <summary>
        /// 2016-3-22
        /// Sorts the list from nearest to furthest, by range from shooter to target.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<RaycastHit> SortByRange(this List<RaycastHit> list)
        {
            list.Sort((lh, rh) =>
            {
                return lh.distance.CompareTo(rh.distance);
            });
            return list;
        }

        /// <summary>
        /// 2016-10-19
        /// Sorts the list from smallest to largest, by accuracy (the distance from
        /// impact point to target position).
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<RaycastHit> SortByAccuracy(this List<RaycastHit> list)
        {
            list.Sort((lh, rh) =>
            {
                return CompareRaycastHitByAccuracy(lh, rh);
            });
            return list;
        }

        /// <summary>
        /// 2016-3-22
        /// Compares two sets of RaycastHit data using the "accuracy" of their hit data.
        ///     Accuracy = SqrMagnitude(raycatHit.point - raycastHit.transform.position)
        /// Note that this is the distance from the impact point, to the position
        /// of the IMPACTED object -- NOT the distance from the shooter.
        /// </summary>
        /// <returns>The raycast hit.</returns>
        /// <param name="a">The first raycast data.</param>
        /// <param name="b">The second raycast data.</param>
        public static int CompareRaycastHitByAccuracy(RaycastHit a, RaycastHit b)
        {
            float distA = Vector3.SqrMagnitude(a.point - a.transform.position);
            float distB = Vector3.SqrMagnitude(b.point - b.transform.position);
            return distA.CompareTo(distB);
        }

        /// <summary>
        /// Compares the instanceIDs of the two objects and sorts.
        /// PU.SetTargetForDistanceComparison()
        /// </summary>
        /// <returns>1 when First ID is less than Second ID, 
        /// -1 when Second ID is less than First ID, 
        /// 0 when they are the same (this should never happen).</returns>
        /// <param name="lhs">First item</param>
        /// <param name="rhs">Second item</param>
        public static int CompareInstanceID(Transform lhs, Transform rhs)
        {
            return lhs.GetInstanceID().CompareTo(rhs.GetInstanceID());
        }

        /// <summary>
        /// 2016-10-19
        /// Compares two points to a third point.
        /// </summary>
        public static int CompareDistanceToTarget(Vector3 a, Vector3 b, Vector3 target)
        {
            float distA = Vector3.SqrMagnitude(target - a);
            float distB = Vector3.SqrMagnitude(target - b);
            return distA.CompareTo(distB);
        }

        /// <summary>
        /// 2016-10-19
        /// Compares the position of two Transforms to a third Transform's position.
        /// </summary>
        public static int CompareDistanceToTarget(Transform a, Transform b, Transform target)
        {
            return CompareDistanceToTarget(a.position, b.position, target.position);
        }

        /// <summary>
        /// 2016-10-19
        /// Compares the position of two GameObjects to a third GameObject's position.
        /// </summary>
        public static int CompareDistanceToTarget(GameObject a, GameObject b, GameObject target)
        {
            return CompareDistanceToTarget(a.transform, b.transform, target.transform);
        }

        #endregion

        ////////////////////////////
        //	Lists and Arrays	////
        ////////////////////////////

        #region Array Type Conversion

        /// <summary>
        /// Creates a new list using the elements in the array. Returns the new list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this T[] source)
        {
            List<T> tList = new List<T>();
            foreach (T t in source)
            {
                tList.Add(t);
            }
            return tList;
        }

        public static string[] ToStringArray<T>(this T[] array)
        {
            PLog.LogNoUnitTests();
            string[] result = new string[array.Length];
            for(int i = 0; i < array.Length; i++)
            {
                result[i] = array[i].ToString();
            }
            return result;
        }

        /// <summary>
        /// Returns a new array containing the Transform elements of the objects in the source array.
        /// </summary>
        /// <returns>The transform array of the current array.</returns>
        public static Transform[] ToTransformArray(this GameObject[] array)
        {
            Transform[] tArray = new Transform[array.Length];

            for (int i = array.Length; i > 0; i--)
            {
                tArray[i - 1] = array[i - 1].transform;
            }

            return tArray;
        }

        /// <summary>
        /// Returns a new array containing the position vectors of the objects in the source array.
        /// </summary>
        /// <returns>The transform array of the current array.</returns>
        public static Vector3[] ToPositionArray(this Transform[] array)
        {
            Vector3[] tArray = new Vector3[array.Length];

            for (int i = array.Length; i > 0; i--)
            {
                tArray[i - 1] = array[i - 1].position;
            }

            return tArray;
        }

        /// <summary>
        /// 2016-5-10 -- TODO Refactor to ToQuaternionArray
        /// Returns a new array containing the Quaternion rotation elements of the objects in the source array.
        /// </summary>
        /// <returns>The transform array of the current array.</returns>
        public static Quaternion[] ToRotationArray(this Transform[] array)
        {
            Quaternion[] tArray = new Quaternion[array.Length];

            for (int i = array.Length; i > 0; i--)
            {
                tArray[i - 1] = array[i - 1].rotation;
            }

            return tArray;
        }

        /// <summary>
        /// Returns a new array containing the Euler rotation vectors of the objects in the source array.
        /// </summary>
        /// <returns>The transform array of the current array.</returns>
        public static Vector3[] ToEulerArray(this Quaternion[] array)
        {
            Vector3[] tArray = new Vector3[array.Length];

            for (int i = array.Length; i > 0; i--)
            {
                tArray[i - 1] = array[i - 1].eulerAngles;
            }

            return tArray;
        }

        #endregion
        #region Lists and Arrays

        /// <summary>
        /// 2016-5-11
        /// Converts the contents to a string. Values are space-separated,
        /// unless an optional parameter is supplied, which is used as the
        /// separator instead.
        /// </summary>
        public static string ContentsToString<T>(this List<T> source, string separator = "")
        {
            string sep = separator + " ";
            StringBuilder b = new StringBuilder();

            for (int i = 0; i < source.Count; i++)
            {
                b.Append(source[i].ToString());
                if (i < source.Count - 1) b.Append(sep);
            }
            return b.ToString();
        }

        /// <summary>
        /// 2016-5-11
        /// Converts the contents to a string. Values are space-separated,
        /// unless an optional parameter is supplied, which is used as the
        /// separator instead.
        /// </summary>
        public static string ContentsToString<T>(this T[] source, string separator = "")
        {
            string sep = separator + " ";
            StringBuilder b = new StringBuilder();

            for (int i = 0; i < source.Length; i++)
            {
                b.Append(source[i].ToString());
                if (i < source.Length - 1) b.Append(sep);
            }
            return b.ToString();
        }

        /// <summary>
        /// Removes the nulls from the list.
        /// </summary>
        public static List<T> RemoveNulls<T>(this List<T> list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
                if (list[i] == null) list.RemoveAt(i);
            return list;
        }

        /// <summary>
        /// 2016-3-23
        /// Removes duplicates from the list. This can be expensive.
        /// </summary>
        public static List<T> RemoveDuplicates<T>(this List<T> list)
        {
            List<int> hashes = new List<int>();
            for (int i = list.Count - 1; i >= 0; i--)
            {
                int id = list[i].GetHashCode();
                if (hashes.Contains(id))
                {
                    list.RemoveAt(i);
                }
                else
                {
                    hashes.Add(id);
                }
            }
            return list;
        }

        /// <summary>
        /// 2016-5-17
        /// </summary>
        public static T GetLast<T>(this List<T> list)
        {
            T item = list.GetRandomFromBottom(1);
            return item;
        }
        /// <summary>
        /// Removes the last item from the list, and returns it.
        /// </summary>
        /// <returns>The last item.</returns>
        public static T RemoveLast<T>(this List<T> list)
        {
            T item = list.GetLast();
            list.RemoveAt(list.Count - 1);
            return item;
        }

        /// <summary>
        /// 2016-5-17
        /// </summary>
        public static T GetFirst<T>(this List<T> list)
        {
            T item = list[0];
            return item;
        }

        /// <summary>
        /// Removes the first item from the list, and returns it.
        /// </summary>
        /// <returns>The first item.</returns>
        public static T RemoveFirst<T>(this List<T> list)
        {
            T item = list.GetFirst();
            list.RemoveAt(0);
            return item;
        }

        /// <summary>
        /// Ensures the added item is unique in the list. Does
        /// not add the item if it is already in the list.
        /// </summary>
        /// <param name="list">List.</param>
        /// <param name="item">The item to be added</param>
        public static List<T> AddUnique<T>(this List<T> list, T item)
        {
            if (list.Contains(item)) return list;
            list.Add(item);
            return list;
        }

        /// <summary>
        /// Adds all items from the parameter "list" that aren't already in
        /// the source list. Does not add duplicates.
        /// </summary>
        /// <param name="source">The source list.</param>
        /// <param name="list">The list items to be added</param>
        public static List<T> AddUniques<T>(this List<T> source, List<T> list)
        {
            foreach (T t in list) source.AddUnique(t);
            return source;
        }

        /// <summary>
        /// Assigns every item in the array to the item parameter.
        /// </summary>
        public static T[] AssignAll<T>(this T[] array, T item)
        {
            for (int i = array.Length; i > 0; i--)
                array[i - 1] = item;
            return array;
        }

        /// <summary>
        /// Indicates if the array contains the item.
        /// </summary>
        public static bool Contains<T>(this T[] array, T item)
        {
            foreach (T t in array)
            {
                if (t.Equals(item)) return true;
            }
            return false;
        }

        /// <summary>
        /// 2016-3-23
        /// Indicates if the array contains an item with the given instance ID.
        /// </summary>
        public static bool ContainsInstanceId(this GameObject[] array, int id)
        {
            foreach (var t in array)
            {
                if (t.GetInstanceID() == id) return true;
            }
            return false;
        }

        /// <summary>
        /// 2016-3-23
        /// Indicates if the array contains an item with the given instance ID.
        /// </summary>
        public static bool ContainsInstanceId(this Transform[] array, int id)
        {
            foreach (var t in array)
            {
                if (t.GetInstanceID() == id) return true;
            }
            return false;
        }

        /// <summary>
        /// 2016-3-23
        /// Indicates if the list contains an item with the given instance ID.
        /// </summary>
        public static bool ContainsInstanceId(this List<GameObject> list, int id)
        {
            foreach (var t in list)
            {
                if (t.GetInstanceID() == id) return true;
            }
            return false;
        }

        /// <summary>
        /// 2016-3-23
        /// Indicates if the list contains an item with the given instance ID.
        /// </summary>
        public static bool ContainsInstanceId(this List<Transform> list, int id)
        {
            foreach (var t in list)
            {
                if (t.GetInstanceID() == id) return true;
            }
            return false;
        }

        /// <summary>
        /// 2016-3-23
        /// Removes the item with the given instance ID, if it is in the list.
        /// </summary>
        public static GameObject RemoveByInstanceId(this List<GameObject> list, int id)
        {
            GameObject found = null;
            foreach (GameObject t in list)
            {
                if (t.GetInstanceID() == id)
                {
                    found = t;
                    list.Remove(t);
                    break;
                }
            }
            return found;
        }

        /// <summary>
        /// 2016-3-23
        /// Removes the item with the given instance ID, if it is in the list.
        /// </summary>
        public static Transform RemoveByInstanceId(this List<Transform> list, int id)
        {
            Transform found = null;
            foreach (Transform t in list)
            {
                if (t.GetInstanceID() == id)
                {
                    found = t;
                    list.Remove(t);
                    break;
                }
            }
            return found;
        }

        /// <summary>
        /// 2016-3-23
        /// Adds the item to the array at index 0.
        /// All existing items in the array are shifted up by 1.
        /// The last item (at index Length -1) is lost.
        /// </summary>
        public static T[] Push<T>(this T[] array, T item)
        {
            for (int i = array.Length - 1; i > 1; i--)
            {
                array[i] = array[i - 1];
            }
            array[0] = item;
            return array;
        }

        /// <summary>
        /// 2016-5-17
        /// </summary>
        public static T GetLast<T>(this T[] array)
        {
            return array[array.Length - 1];
        }

        /// <summary>
        /// 2016-5-17
        /// </summary>
        public static T GetFirst<T>(this T[] array)
        {
            return array[0];
        }
        
        /// <summary>
        /// Returns true if the entire array is true
        /// </summary>
        public static bool AllTrue(this bool[] source)
        {
            bool allTrue = true;
            foreach (bool s in source)
            {
                allTrue &= s;
                if (!allTrue) return false;
            }
            return true;
        }

        #endregion

        ////////////////////
        //	Queues		////
        ////////////////////

        #region Queues

        /// <summary>
        ///	2016-1-13
        /// Churn the specified source.
        ///	Moves the first item to the end. Returns a reference to the item that moved.
        /// </summary>
        /// <param name="source">the queue.</param>
        public static T Churn<T>(this Queue<T> source)
        {
            T first = source.Dequeue();
            source.Enqueue(first);
            return first;
        }

        #endregion

        ////////////////////////
        //	Game Objects	////
        ////////////////////////

        #region Boundaries

        /// <summary>
        /// 2016-10-29
        /// Returns a bounding object that encapsulates all of this object's child
        /// objects.
        /// </summary>
        public static Bounds GetCombinedBoundsOfChildren(this GameObject source)
        {
            Renderer[] childRenderers = source.GetComponentsInChildren<Renderer>();
            Bounds combinedBounds = new Bounds();
            foreach (Renderer r in childRenderers)
            {
                combinedBounds.Encapsulate(r.bounds);
            }
            return combinedBounds;
        }

        /// <summary>
        /// 2016-10-29
        /// Detatches this object from it's parent.
        /// </summary>
        public static void Unparent(this GameObject source)
        {
            source.transform.parent = null;
        }

        /// <summary>
        /// 2016-10-29
        /// Detatches the nth child object from this object and returns the
        /// child object.
        /// </summary>
        public static GameObject DetachChild (this GameObject source, int index)
        {
            GameObject child = null;
            if (source.transform.childCount > index)
            {
                child = source.transform.GetChild(index).gameObject;
                child.transform.parent = null;
            }
            return child;
        }

        /// <summary>
        /// 2016-10-29
        /// Sets this object's parent to the parameter object
        /// </summary>
        public static void SetParent(this GameObject source, GameObject parent)
        {
            source.transform.parent = parent.transform;
        }

        /// <summary>
        /// 2016-10-29
        /// Appends the parameter object to this object. The parameter object
        /// becomes a child of this object.
        /// </summary>
        public static void AddChild(this GameObject source, GameObject child)
        {
            child.transform.parent = source.transform;
        }

        #endregion
        #region Messaging

        public enum MessageSendType { SEND, SEND_DOWN, SEND_UP, SEND_UP_AND_DOWN };

        /// <summary>
        /// Sends a message to all elements in the array. The optional parameter indicates
        /// the propagation method.
        /// 
        /// SEND uses SendMessage()
        /// SEND_DOWN uses BroadcastMessage()
        /// SEND_UP uses SendMessageUpwards()
        /// SEND_UP_AND_DOWN uses both BroadcaseMessage() and SendMessageUpwards()
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="sendMethod">Send method.</param>
        public static void SendMessageToElements<T>(this GameObject[] array,
                                                  string message, T data,
                                                  MessageSendType sendMethod = PUtil.MessageSendType.SEND,
                                                  SendMessageOptions sendOptions = SendMessageOptions.DontRequireReceiver)
        {
            foreach (var e in array) e.SendMessageToElements(message, data, sendMethod, sendOptions);
        }

        /// <summary>
        /// Sends a message to all elements in the array. The optional parameter indicates
        /// the propagation method.
        /// 
        /// SEND uses SendMessage()
        /// SEND_DOWN uses BroadcastMessage()
        /// SEND_UP uses SendMessageUpwards()
        /// SEND_UP_AND_DOWN uses both BroadcaseMessage() and SendMessageUpwards()
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="sendMethod">Send method.</param>
        public static void SendMessageToElements<T>(this Transform[] array,
                                                  string message, T data,
                                                  MessageSendType sendMethod = PUtil.MessageSendType.SEND,
                                                  SendMessageOptions sendOptions = SendMessageOptions.DontRequireReceiver)
        {
            foreach (var e in array) e.SendMessageToElements(message, data, sendMethod, sendOptions);
        }

        /// <summary>
        /// Sends a message to all elements in the list. The optional parameter indicates
        /// the propagation method.
        /// 
        /// SEND uses SendMessage()
        /// SEND_DOWN uses BroadcastMessage()
        /// SEND_UP uses SendMessageUpwards()
        /// SEND_UP_AND_DOWN uses both BroadcaseMessage() and SendMessageUpwards()
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="sendMethod">Send method.</param>
        public static void SendMessageToElements<T>(this List<GameObject> list,
                                                  string message, T data,
                                                  MessageSendType sendMethod = PUtil.MessageSendType.SEND,
                                                  SendMessageOptions sendOptions = SendMessageOptions.DontRequireReceiver)
        {
            foreach (var e in list) e.SendMessageToElements(message, data, sendMethod, sendOptions);
        }

        /// <summary>
        /// Sends a message to all elements in the list. The optional parameter indicates
        /// the propagation method.
        /// 
        /// SEND uses SendMessage()
        /// SEND_DOWN uses BroadcastMessage()
        /// SEND_UP uses SendMessageUpwards()
        /// SEND_UP_AND_DOWN uses both BroadcaseMessage() and SendMessageUpwards()
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="sendMethod">Send method.</param>
        public static void SendMessageToElements<T>(this List<Transform> list,
                                                  string message, T data,
                                                  MessageSendType sendMethod = PUtil.MessageSendType.SEND,
                                                  SendMessageOptions sendOptions = SendMessageOptions.DontRequireReceiver)
        {
            foreach (var e in list) e.SendMessageToElements(message, data, sendMethod, sendOptions);
        }

        /// <summary>
        /// Sends a message to all components of this element. The optional parameter indicates
        /// the propagation method.
        /// 
        /// SEND uses SendMessage()
        /// SEND_DOWN uses BroadcastMessage()
        /// SEND_UP uses SendMessageUpwards()
        /// SEND_UP_AND_DOWN uses both BroadcaseMessage() and SendMessageUpwards()
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="sendMethod">Send method.</param>
        public static void SendMessageToElements<T>(this GameObject item, string message, T data,
                                                    MessageSendType sendMethod = PUtil.MessageSendType.SEND,
                                                    SendMessageOptions sendOptions = SendMessageOptions.DontRequireReceiver)
        {
            item.transform.SendMessageToElements(message, data, sendMethod, sendOptions);
        }

        /// <summary>
        /// Sends a message to all components of this element. The optional parameter indicates
        /// the propagation method.
        /// 
        /// SEND uses SendMessage()
        /// SEND_DOWN uses BroadcastMessage()
        /// SEND_UP uses SendMessageUpwards()
        /// SEND_UP_AND_DOWN uses both BroadcaseMessage() and SendMessageUpwards()
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="sendMethod">Send method.</param>
        public static void SendMessageToElements<T>(this Transform item, string message, T data,
                                                    MessageSendType sendMethod = PUtil.MessageSendType.SEND,
                                                    SendMessageOptions sendOptions = SendMessageOptions.DontRequireReceiver)
        {
            switch (sendMethod)
            {
                case MessageSendType.SEND:
                    //  send the message to all components on this game object
                    if (data == null) item.SendMessage(message, sendOptions);
                    else item.SendMessage(message, data, sendOptions);
                    break;
                case MessageSendType.SEND_UP:
                    //  send the message to all components on this game object AND
                    //  send the message to all PARENT components of this game object
                    if (data == null) item.SendMessageUpwards(message, sendOptions);
                    else item.SendMessageUpwards(message, data, sendOptions);
                    break;
                case MessageSendType.SEND_DOWN:
                    //  send the message to all components on this game object AND
                    //  send the message to all CHILD components of this game object
                    if (data == null) item.BroadcastMessage(message, sendOptions);
                    else item.BroadcastMessage(message, data, sendOptions);
                    break;
                case MessageSendType.SEND_UP_AND_DOWN:
                    //  send the message to all components on this game object AND
                    //  send the message to all PARENT components of this game object AND
                    //  send the message to all CHILD components of this game object
                    if (data == null) item.BroadcastMessage(message, sendOptions);
                    else item.BroadcastMessage(message, data, sendOptions);

                    //  object comparison -- ensure the 'parent' isn't this object itself
                    if (item.transform.parent.gameObject != item)
                    {
                        if (data == null) item.transform.parent.SendMessageUpwards(message, sendOptions);
                        else item.transform.parent.SendMessageUpwards(message, data, sendOptions);
                    }
                    break;
            }
        }

        #endregion
        #region Navigation

        /// <summary>
        /// Gets the child by instance identifier. Returns null if there is no match.
        /// </summary>
        /// <returns>The child transform.</returns>
        /// <param name="array">The parent transform.</param>
        /// <param name="id">Instance ID of the child to find.</param>
        public static Transform GetChildByInstanceId(this Transform source, int id)
        {
            foreach (Transform t in source)
            {
                if (t.GetInstanceID() == id) return t;
            }
            return null;
        }

        /// <summary>
        /// Gets the root object.
        /// </summary>
        /// <returns>The root GameObject.</returns>
        public static GameObject GetRoot(this GameObject gameObject)
        {
            return gameObject.transform.GetRoot().gameObject;
        }

        /// <summary>
        /// Gets the root object.
        /// </summary>
        /// <returns>The root Transform.</returns>
        public static Transform GetRoot(this Transform transform)
        {
            Transform t = transform;
            while (t.parent) t = t.parent;
            return t;
        }

        /// <summary>
        /// Gets the root object.
        /// </summary>
        /// <returns>The root Transform.</returns>
        public static Transform GetRoot(this Collider collider)
        {
            return collider.transform.GetRoot();
        }

        /// <summary>
        /// Gets the root object.
        /// </summary>
        /// <returns>The root Transform.</returns>
        public static Transform GetRoot(this Collision collisionData)
        {
            return collisionData.transform.GetRoot();
        }

        /// <summary>
        /// Finds all the children of this object that have the indiated tag.
        /// </summary>
        /// <returns>An array containing the transforms of all children with the tag.</returns>
        /// <param name="tag">Tag to match.</param>
        public static Transform[] FindChildrenWithTag(this Transform rootTransform, string tag)
        {
            List<Transform> matchList = new List<Transform>();

            foreach (Transform t in rootTransform.GetComponentsInChildren<Transform>())
            {
                if (t.tag.Equals(tag)) matchList.Add(t);
            }

            return matchList.ToArray();
        }

        /// <summary>
        /// Finds all the children of this object that have the indiated tag.
        /// </summary>
        /// <returns>An array containing the transforms of all children with the tag.</returns>
        /// <param name="tag">Tag to match.</param>
        public static Transform[] FindChildrenWithTag(this GameObject gameObject, string tag)
        {
            return gameObject.transform.FindChildrenWithTag(tag);
        }

        #endregion
        #region Identification and Tags

        /// <summary>
        /// Determines if this transform has same tag as the specified transform.
        /// </summary>
        /// <returns><c>true</c> if the tags match; otherwise, <c>false</c>.</returns>
        /// <param name="tran">This transform.</param>
        /// <param name="other">Item being compared to.</param>
        public static bool HasSameTagAs(this Transform tran, Transform other)
        {
            return tran.tag.Equals(other.tag);
        }

        /// <summary>
        /// Indicates if the other object has the same tag as this object.
        /// </summary>
        /// <param name="other">Other object.</param>
        public static bool HasSameTagAs(this GameObject gameObject, GameObject other)
        {
            return gameObject.tag.Equals(other.tag);
        }

        /// <summary>
        /// Indicates if the other object has the same root tag as this object.
        /// </summary>
        /// <param name="other">Other object.</param>
        public static bool HasSameRootTagAs(this GameObject gameObject, GameObject other)
        {
            return gameObject.GetRoot().tag.Equals(other.GetRoot().tag);
        }

        #endregion
        #region Layers

        /// <summary>
        /// Indicates if the other object is on the same layer as this object.
        /// </summary>
        /// <param name="other">Other object.</param>
        public static bool OnSameLayerAs(this GameObject gameObject, GameObject other)
        {
            return gameObject.layer == other.layer;
        }

        /// <summary>
        /// Returns a list of GameObjects on the indicated layer.
        /// </summary>
        /// <returns>A List of GameObjects on the layer.</returns>
        /// <param name="layerName">Name of the layer to search.</param>
        public static List<GameObject> FindOnLayer(string layerName)
        {
            GameObject[] found = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
            List<GameObject> foundOnLayer = new List<GameObject>();
            int layer = LayerMask.NameToLayer(layerName);
            foreach (GameObject g in found)
            {
                if (g.layer.Equals(layer))
                {
                    foundOnLayer.Add(g);
                }
            }
            return foundOnLayer;
        }

        /// <summary>
        /// Returns true if the source layer mask contains the 
        /// layer parameter.
        /// </summary>
        public static bool ContainsLayer(this LayerMask source, LayerMask layer)
        {
            //  Use bit-wise AND
            //return (source & (1 << layer)) == layer;

            //  Use bit-wise OR
            return (source | (1 << layer)) == source;
        }

        #endregion
        #region Distances and distance-based comparison

        /// <summary>
        /// Returns the Transform that is farther from this object. If they are the same, one is randomly selected.
        /// </summary>
        /// <returns>The farther object.</returns>
        /// <param name="left">First object</param>
        /// <param name="right">Second object</param>
        public static Transform FindFarther(this Transform me, Transform left, Transform right)
        {
            float leftRange = Vector3.SqrMagnitude(me.position - left.position);
            float rightRange = Vector3.SqrMagnitude(me.position - right.position);

            if (leftRange > rightRange) return left;
            if (leftRange > rightRange) return right;
            return PRand.RandomBool() ? left : right;
        }

        /// <summary>
        /// Returns the Transform that is closest from this object. If they are the same, one is randomly selected.
        /// </summary>
        /// <returns>The closer object.</returns>
        /// <param name="left">First object</param>
        /// <param name="right">Second object</param>
        public static Transform FindCloser(this Transform me, Transform left, Transform right)
        {
            if (me.FindFarther(left, right).Equals(left))
            {
                return right;
            }
            else
            {
                return left;
            }
        }

        /// <summary>
        /// Returns the sqr magnitude of the position of the transforms, e.g., Vector3.SqrMagnitude(lh.position - rh.position);
        /// </summary>
        /// <param name="lh"></param>
        /// <param name="rh"></param>
        /// <returns></returns>
        public static float SqrDistance(Transform lh, Transform rh)
        {
            return Vector3.SqrMagnitude(lh.position - rh.position);
        }

        /// <summary>
        /// Returns the sqr magnitude of the position of the game objects, e.g., Vector3.SqrMagnitude(lh.transform.position - rh.transform.position);
        /// </summary>
        /// <param name="lh"></param>
        /// <param name="rh"></param>
        /// <returns></returns>
        public static float SqrDistance(GameObject lh, GameObject rh)
        {
            return SqrDistance(lh.transform, rh.transform);
        }

        #endregion

        ////////////////////////
        //	Components		////
        ////////////////////////

        #region Components

        /// <summary>
        /// Gets the copy of the component. Usage: var copy = myComp.GetCopyOf(someOtherComponent);
        /// </summary>
        /// <returns>The copy.</returns>
        /// <param name="other">Other.</param>
        public static T GetCopyOf<T>(this Component source, T other) where T : Component
        {
            BindingFlags flags = BindingFlags.Public
                                | BindingFlags.NonPublic
                                | BindingFlags.Default
                                | BindingFlags.Instance
                                | BindingFlags.DeclaredOnly;
            Type type = source.GetType();
            PropertyInfo[] propertyInfo = type.GetProperties(flags);
            foreach (PropertyInfo info in propertyInfo)
            {
                if (info.CanWrite)
                {
                    try
                    {
                        info.SetValue(source, info.GetValue(other, null), null);
                    }
                    catch { }	//	ignore NotImplementedExceptions
                }
            }
            FieldInfo[] fieldInfo = type.GetFields(flags);
            foreach (FieldInfo info in fieldInfo)
            {
                info.SetValue(source, info.GetValue(other));
            }
            return source as T;
        }

        /// <summary>
        /// Adds a copy of the component. Usage: Health myHealth = gameObject.AddComponent<Health>(enemy.health);
        /// </summary>
        /// <returns>The copied component.</returns>
        /// <param name="toAdd">Source component to add.</param>
        public static T AddComponent<T>(this GameObject source, T toAdd) where T : Component
        {
            return source.AddComponent<T>().GetCopyOf(toAdd) as T;
        }

        /// <summary>
        /// 2016-5-12
        /// </summary>
        public static List<MethodInfo> GetMethods(this MonoBehaviour mb, Type returnType, Type[] paramTypes, BindingFlags flags)
        {
            return mb.GetType().GetMethods(flags)
                .Where(m => m.ReturnType == returnType)
                .Select(m => new { m, Params = m.GetParameters() })
                .Where(x =>
                {
                    return paramTypes == null ? // in case we want no params
                        x.Params.Length == 0 :
                        x.Params.Length == paramTypes.Length &&
                        x.Params.Select(p => p.ParameterType).ToArray().IsEqualTo(paramTypes);
                })
                .Select(x => x.m)
                .ToList();
        }

        /// <summary>
        /// 2016-5-12
        /// </summary>
        public static List<MethodInfo> GetMethods(this GameObject go, Type returnType, Type[] paramTypes, BindingFlags flags)
        {
            var mbs = go.GetComponents<MonoBehaviour>();
            List<MethodInfo> list = new List<MethodInfo>();
            foreach (var mb in mbs)
            {
                list.AddRange(mb.GetMethods(returnType, paramTypes, flags));
            }
            return list;
        }

        /// <summary>
        ///	2016-5-12 -- this may be a duplicate method
        /// Determines if is equal to the specified list other.
        /// </summary>
        public static bool IsEqualTo<T>(this IList<T> list, IList<T> other)
        {
            if (list.Count != other.Count) return false;
            for (int i = 0, count = list.Count; i < count; i++)
            {
                if (!list[i].Equals(other[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 2016-6-29
        /// Returns the class name of the MonoBehavior, or the specific
        /// name of the component, e.g., "MeshRenderer" or "DestroyOnLoad" etc
        /// </summary>
        public static string ComponentName(this Component source)
        {
            return source.GetType().ToString().Split('.').Last();
        }

        /// <summary>
        /// Searches all children, no matter how deep.
        /// Uses Breadth-First Search (BFS) by default.
        /// Optionally can use Depth-First Search (DFS).
        /// </summary>
        public static Transform DeepFind(this Transform parent, string name, DeepFindSearch searchType = DeepFindSearch.BFS, bool includeInactive = false)
        {
            if (searchType.Equals(DeepFindSearch.DFS)) return DeepFindDFS(parent, name, includeInactive);
            else return DeepFindBFS(parent, name, includeInactive);
        }

        // Breadth-first search, including inactive elements.
        private static Transform DeepFindBFS(Transform aParent, string aName, bool includeInactive)
        {
            if (aParent.name.Equals(aName)) return aParent;

            Transform result = null;

            int childCount = aParent.childCount;

            for (int i = 0; i < childCount; i++)
            {
                Transform child = aParent.GetChild(i);
                if (!includeInactive && !child.gameObject.activeSelf) continue;
                if (child.name.Equals(aName)) return child;
            }

            //  At this point, the object is NOT found
            for (int i = 0; i < childCount; i++)
            {
                Transform child = aParent.GetChild(i);
                if (!includeInactive && !child.gameObject.activeSelf) continue;
                result = DeepFindBFS(child, aName, includeInactive);
                if (result != null) return result;
            }
            return null;
        }

        //Depth-first search, including inactive elements
        private static Transform DeepFindDFS(Transform aParent, string aName, bool includeInactive)
        {
            if (aParent.name.Equals(aName)) return aParent;

            int childCount = aParent.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = aParent.GetChild(i);
                if (!includeInactive && !child.gameObject.activeSelf) continue;
                if (child.name.Equals(aName)) return child;
                child = DeepFindDFS(child, aName, includeInactive);
                if (child) return child;
            }
            return null;
        }

        public enum DeepFindSearch { BFS, DFS }

        /// <summary>
        /// 2017-5-17
        /// Searches all children, no matter how deep.
        /// Uses Breadth-First Search (BFS) by default.
        /// Optionally can use Depth-First Search (DFS).
        /// Returns the first child with a matching tag.
        /// </summary>
        public static Transform DeepFindByTag(this Transform parent, string tag, DeepFindSearch searchType = DeepFindSearch.BFS, bool includeInactive = false)
        {
            if (searchType.Equals(DeepFindSearch.DFS)) return DeepFindDFSByTag(parent, tag, includeInactive);
            else return DeepFindBFSByTag(parent, tag, includeInactive);
        }

        private static Transform DeepFindBFSByTag(Transform parent, string tag, bool includeInactive)
        {
            int childCount = parent.childCount;

            //  check all children for the tag
            for (int i = 0; i < childCount; i++)
            {
                Transform child = parent.GetChild(i);

                //  skip inactive child objects, unless includeInactive is true
                if (!includeInactive && !child.gameObject.activeSelf) continue;

                //  Check tag, return if found
                if (child.tag.Equals(tag)) return child;
            }

            Transform result = null;

            //  To get here, none of the tag's matched
            for (int i = 0; i < childCount; i++)
            {
                Transform child = parent.GetChild(i);

                //  skip inactive child objects, unless includeInactive is true
                if (!includeInactive && !child.gameObject.activeSelf) continue;

                //  Execute this search on the child object
                result = DeepFindBFSByTag(child, tag, includeInactive);

                if (result != null) return result;
            }

            return result;
        }

        private static Transform DeepFindDFSByTag(Transform parent, string tag, bool includeInactive)
        {
            int childCount = parent.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = parent.GetChild(i);

                //  skip inactive child objects, unless includeInactive is true
                if (!includeInactive && !child.gameObject.activeSelf) continue;

                //  Check tag, return if found
                if (child.tag.Equals(tag)) return child;

                //  When NOT found, execute this search on the child object
                child = DeepFindDFSByTag(child, tag, includeInactive);

                if (child) return child;
            }
            return null;
        }

        /// <summary>
        /// 2017-05-17
        /// Returns a list of all child transforms that have the provided tag,
        /// regardless how deep the child is.
        /// </summary>
        public static List<Transform> DeepFindChildrenByTag(this Transform parent, string tag)
        {
            List<Transform> found = new List<Transform>();
            Transform child;
            for (int i = parent.childCount;i>0;i--)
            {
                child = parent.GetChild(i-1);

                //  Check tag of the child
                if (child.tag.Equals(tag))
                {
                    //  Add the child to the 'found' list
                    found.Add(child);
                }

                //  Regardless of the child's tag,
                //  execute this search on the child
                found.AddRange(child.DeepFindChildrenByTag(tag));

            }
            return found;
        }

        #endregion

        ////////////////////////
        //	Enumerations	////
        ////////////////////////

        #region enumerations

        /// <summary>
        /// Returns a string array containing the enumerations.
        /// </summary>
        /// <returns>The string array.</returns>
        public static string[] ToStringArray(this System.Type enumeratedType)
        {
            return System.Enum.GetNames(enumeratedType);
        }

        /// <summary>
        /// Returns a string containing the enumeration type.
        /// </summary>
        /// <returns>The string.</returns>
        public static string EnumToString(this System.Type enumeratedType)
        {
            return enumeratedType.ToString();
        }

        /// <summary>
        /// Converts the enumeration at the indiated index, to a string.
        /// </summary>
        /// <returns>The string value of the enumeration at index.</returns>
        /// <param name="index">Index to return as a string.</param>
        public static string EnumToString(this System.Type enumeratedType, int index)
        {
            return System.Enum.GetName(enumeratedType, index);
        }

        #endregion

        ////////////////////////
        //	Attributes  	////
        ////////////////////////

        #region Attributes

        public class SelectableLabelAttribute : PropertyAttribute
        {
            public string text;

            public SelectableLabelAttribute(string text)
            {
                this.text = text;
            }
        }

        #if UNITY_EDITOR

        [CustomPropertyDrawer(typeof(SelectableLabelAttribute))]
        public class SelectableLabelDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.SelectableLabel(position, selectableLabelAttribute.text);
            }

            private SelectableLabelAttribute selectableLabelAttribute
            {
                get
                {
                    return (SelectableLabelAttribute)attribute;
                }
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {


                return selectableLabelAttribute.text.Split('\n').Length * base.GetPropertyHeight(property, label);
            }
        }

        #endif

        #endregion
    }

    [System.Serializable]
    public class FogData
    {
        public bool fogEnabled;
        public Color fogColor;
        public FogMode fogMode;
        public float fogDensity;
        public float linearFogStart;
        public float linearFogEnd;
        public Color ambientColor;
    }
}
