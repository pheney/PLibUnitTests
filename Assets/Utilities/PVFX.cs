using UnityEngine;
using PLib.Rand;

/// <summary>
/// Supress warning 0618 (method is deprecated)
/// </summary>
#pragma warning disable 0618
namespace PLib.VFX
{
    /// <summary>
    /// Fx utility methods and extensions
    /// </summary>
    public static class VFX
    {
        //////////////////////////////////////////
        //	static object creation and access	//
        //////////////////////////////////////////

        #region Single World Object and Renders

        private static GameObject UUFXGO;

        /// <summary>
        /// Returns a singleton GameObject that is used for attaching drawing classes onto.
        /// </summary>
        /// <returns>The singleton GameObject.</returns>
        private static GameObject getUUFXGO()
        {
            if (!UUFXGO)
            {
                UUFXGO = new GameObject("_UUFX");
            }
            return UUFXGO;
        }

        private static ParticleSystem pSystem;

        /// <summary>
        /// Returns the particle system attached to the singleton GameObject.
        ///	This is a general "universal" particle system that is used and modified as needed.
        /// </summary>
        /// <returns>The "world" particle system attached to the singleton GameObject.</returns>
        private static ParticleSystem getParticleSystem()
        {
            if (!pSystem)
            {
                GameObject go = getUUFXGO();
                go.AddComponent<ParticleSystem>();
                pSystem = go.GetComponent<ParticleSystem>();
            }
            return pSystem;
        }

        private static LineRenderer lineRenderer;

        /// <summary>
        /// Returns the line renderer attached to the singleton GameObject.
        ///	This is a general "universal" line renderer that is used and modified as needed.
        /// </summary>
        /// <returns>The "world" line renderer attached to the singleton GameObject.</returns>
        private static LineRenderer getLineRenderer()
        {
            if (!lineRenderer)
            {
                GameObject go = getUUFXGO();
                go.AddComponent<LineRenderer>();
                lineRenderer = go.GetComponent<LineRenderer>();
            }
            return lineRenderer;
        }

        private static Material lineMaterial;

        /// <summary>
        /// Returns the line material attached to the singleton GameObject.
        ///	This is a general "universal" line material that is used and modified as needed.
        /// </summary>
        /// <returns>The "world" line material attached to the singleton GameObject.</returns>
        private static Material getLineMaterial()
        {
            if (!lineMaterial)
            {
                var shaderText =
                    "Shader \"Alpha Additive\" {" +
                    "Properties { _Color (\"Main Color\", Color) = (1,1,1,0) }" +
                    "SubShader {" +
                    "    Tags { \"Queue\" = \"Transparent\" }" +
                    "    Pass {" +
                    "        Blend One One ZWrite Off ColorMask RGB" +
                    "        Material { Diffuse [_Color] Ambient [_Color] }" +
                    "        Lighting On" +
                    "        SetTexture [_Dummy] { combine primary double, primary }" +
                    "    }" +
                    "}" +
                    "}";
                lineMaterial = new Material(shaderText);
            }
            return lineMaterial;
        }

        #endregion

        //////////////////////////
        //	Static FX Methods	//
        //////////////////////////

        #region Drawing with Singleton Renderers

        /// <summary>
        /// Draws the a line in world space from startPosition to endPosition, using the indicated width and color.
        /// </summary>
        /// <param name="startPosition">Start position.</param>
        /// <param name="endPosition">End position.</param>
        /// <param name="rayWidth">Ray width.</param>
        /// <param name="drawColor">Draw color.</param>
        public static void DrawRay(Vector3 startPosition, Vector3 endPosition, float rayWidth, Color drawColor)
        {
            LineRenderer lRenderer = getLineRenderer();
            lRenderer.SetPosition(0, startPosition);
            lRenderer.SetPosition(1, endPosition);
            lRenderer.SetWidth(rayWidth, rayWidth);
            lRenderer.material = getLineMaterial();
            lRenderer.material.color = drawColor;
        }

        /// <summary>
        /// Sets the world line renderer visibility
        /// </summary>
        /// <param name="visible">If set to <c>true</c> visible.</param>
        public static void DrawRay(bool visible)
        {
            LineRenderer lRenderer = getLineRenderer();
            lRenderer.enabled = visible;
        }

        /// <summary>
        /// Draws the an arc in world space from startPosition to endPosition, using the indicated color.
        /// </summary>
        /// <param name="startPosition">Start position.</param>
        /// <param name="endPosition">End position.</param>
        /// <param name="drawColor">Draw color.</param>
        public static void DrawArc(Vector3 startPosition, Vector3 endPosition, Color drawColor)
        {
            ParticleSystem pSystem = getParticleSystem();

            float arcHeight = 3;		//	meters (local constant)
            float segLength = .1f;	//	meters (local constant)
            float particleSize = .3f;
            float particleLife = .2f;
            float speed = .1f;

            float distance = Vector3.Distance(startPosition, endPosition);
            float vertDistance = endPosition.y - startPosition.y;
            Vector3 planarDirection = endPosition - startPosition;
            planarDirection.Scale(Vector3.right + Vector3.forward);
            planarDirection.Normalize();
            int numSegments = (int)Mathf.Clamp(distance / segLength, 1, 360);

            pSystem.Emit(startPosition, PRand.RandomVector3() * speed, particleSize, particleLife, drawColor);

            for (int i = 1; i < numSegments - 1; i++)
            {
                float verticalPosition = arcHeight * Mathf.Sin(Mathf.PI * i / numSegments);
                Vector3 pointPosition = planarDirection * i * segLength;
                pointPosition.y = verticalPosition + i * vertDistance / numSegments;
                pSystem.Emit(startPosition + pointPosition, PRand.RandomVector3() * speed, particleSize, particleLife, drawColor);
            }
            pSystem.Emit(endPosition, PRand.RandomVector3() * speed, particleSize, particleLife, drawColor);
        }

        #endregion

    }
}