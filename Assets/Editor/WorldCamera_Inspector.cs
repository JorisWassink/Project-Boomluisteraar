using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.WSA;
using Application = UnityEngine.Application;

[CustomEditor(typeof(WorldMapCamera))]
public class WorldCamera_Inspector : Editor
{
   void OnSceneGUI()
    {
        var cam = (WorldMapCamera)target;
        Rect r = cam.bounds;   // x/width = world X, y/height = world Z
        Vector2 view = ViewSize(cam.GetComponent<Camera>());

        float size = 0.1f * Mathf.Min(r.width, r.height);

        Vector3 tl = new Vector3(r.xMin, 0, r.yMax);
        Vector3 tr = new Vector3(r.xMax, 0, r.yMax);
        Vector3 bl = new Vector3(r.xMin, 0, r.yMin);
        Vector3 br = new Vector3(r.xMax, 0, r.yMin);

        r.yMax = Mathf.Max(r.yMax + Edge(Color.yellow, tl, tr, Vector3.forward, size), r.yMin + view.y);
        r.xMin = Mathf.Min(r.xMin - Edge(Color.red,    bl, tl, Vector3.left,    size), r.xMax - view.x);
        r.xMax = Mathf.Max(r.xMax + Edge(Color.blue,   br, tr, Vector3.right,   size), r.xMin + view.x);
        r.yMin = Mathf.Min(r.yMin - Edge(Color.cyan,   bl, br, Vector3.back,    size), r.yMax - view.y);

        cam.bounds = r;
        cam.viewportSize = view;
    }

    // draws one edge + arrow, returns how far it was dragged along dir
    float Edge(Color color, Vector3 a, Vector3 b, Vector3 dir, float size)
    {
        Handles.color = color;
        Handles.DrawLine(a, b, size * 0.25f);

        Vector3 mid = (a + b) / 2;
        EditorGUI.BeginChangeCheck();
        Vector3 p = Handles.Slider(mid, dir, size, Handles.ArrowHandleCap, 0f);
        if (!EditorGUI.EndChangeCheck()) return 0f;

        Undo.RecordObject(target, "Resize Bounds");
        EditorUtility.SetDirty(target);
        return Vector3.Dot(p - mid, dir);
    }

    // width (x) and height (z) of the camera's footprint on y = 0
    static Vector2 ViewSize(Camera c)
    {
        var ground = new Plane(Vector3.up, Vector3.zero);
        Vector3 min = Vector3.positiveInfinity, max = Vector3.negativeInfinity;

        foreach (var v in new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) })
        {
            Ray ray = c.ViewportPointToRay(v);
            if (!ground.Raycast(ray, out float d)) continue;
            Vector3 hit = ray.GetPoint(d);
            min = Vector3.Min(min, hit);
            max = Vector3.Max(max, hit);
        }
        return new Vector2(max.x - min.x, max.z - min.z);
    }
}
