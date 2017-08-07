using UnityEngine;
using palette = PLib.Palette.Palette;

namespace PLib.Gizmo
{
    [AddComponentMenu("Tools/Gizmo")]
    public class Gizmo : MonoBehaviour
    {

        public bool show = true;
        public float radius = .5f;  //	meters
        public bool solid = false;

        public int gizmoColorIndex = 0;
        public GizmoColor[] gizmoColors = new GizmoColor[] { new GizmoColor(palette.Normalize(Color.red, 200f / 255)) };

        //	cache	//

        private Transform _transform;
        private bool _IsSelected = false;

        //	LIFECYCLE

        void OnDrawGizmos()
        {
            if (!show) return;
            if (_IsSelected)
            {
                _IsSelected = false;
                return;
            }

            DrawTheGizmo(gizmoColors[gizmoColorIndex].color, radius);
        }

        //	LIFECYCLE

        void OnDrawGizmosSelected()
        {
            if (!show) return;

            DrawTheGizmo(gizmoColors[gizmoColorIndex].highColor, radius + 0.05f);

            _IsSelected = true;
        }

        //	Helper

        private void DrawTheGizmo(Color drawColor, float radius)
        {
            if (!_transform) _transform = transform;

            Gizmos.color = drawColor;
            float camDist = Vector3.Distance(Camera.current.transform.position, _transform.position);
            if (camDist >= 10f)
            {
                radius = radius * (100 - camDist) / 100;
            }

            if (solid)
            {
                Gizmos.DrawSphere(_transform.position, radius);
            }
            else
            {
                Gizmos.DrawWireSphere(_transform.position, radius);
            }
        }
    }

    [System.Serializable]
    public class GizmoColor
    {
        public Color color, highColor;
        public GizmoColor(Color color)
        {
            this.color = color;
            UpdateHighColor();
        }
        public void UpdateHighColor()
        {
            this.highColor = palette.Saturate(color, 1);
        }
    }
}