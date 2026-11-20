using UnityEngine;
using UnityEngine.UI;

namespace Bakvand.UI
{
    [AddComponentMenu("UI/Opaque Polygon Graphic"), RequireComponent(typeof(CanvasRenderer))]
    public sealed class OpaquePolygonGraphic : MaskableGraphic
    {
        [SerializeField, HideInInspector] private Vector2[] points;
        [SerializeField, HideInInspector] private int[] triangles;

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            if (points == null || triangles == null)
                return;

            Rect rect = GetPixelAdjustedRect();
            Color32 fill = color;
            fill.a = 255;
            foreach (Vector2 point in points)
            {
                Vector2 vertex = rect.min + Vector2.Scale(rect.size, point);
                mesh.AddVert(vertex, fill, Vector2.zero);
            }

            for (int i = 0; i < triangles.Length; i += 3)
                mesh.AddTriangle(triangles[i], triangles[i + 1], triangles[i + 2]);
        }
    }
}
