using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Symmetric Image")]
public class SymmetricImage : Image {
    public enum SourceSide {
        Left,
        Right
    }

    [SerializeField] private SourceSide sourceSide = SourceSide.Left;
    public SourceSide Side { get => sourceSide; set => sourceSide = value; }

    protected override void OnPopulateMesh(VertexHelper vh) {
        vh.Clear();

        Rect r = GetPixelAdjustedRect();
        float halfWidth = r.width * 0.5f;

        Vector2 leftMin = new Vector2(r.xMin, r.yMin);
        Vector2 leftMax = new Vector2(r.xMin + halfWidth, r.yMax);
        Vector2 rightMin = new Vector2(r.xMin + halfWidth, r.yMin);
        Vector2 rightMax = new Vector2(r.xMax, r.yMax);

        bool sourceOnLeft = sourceSide == SourceSide.Left;

        // ОРИГИНАЛ (использует ВСЮ текстуру)
        AddQuad(vh,
            sourceOnLeft ? leftMin : rightMin,
            sourceOnLeft ? leftMax : rightMax,
            new Vector2(0f, 0f),
            new Vector2(1f, 1f));

        // ЗЕРКАЛО (та же текстура, но X инвертирован)
        AddQuad(vh,
            sourceOnLeft ? rightMin : leftMin,
            sourceOnLeft ? rightMax : leftMax,
            new Vector2(1f, 0f),
            new Vector2(0f, 1f));
    }

    void AddQuad(
        VertexHelper vh,
        Vector2 posMin,
        Vector2 posMax,
        Vector2 uvMin,
        Vector2 uvMax) {
        int start = vh.currentVertCount;

        UIVertex v = UIVertex.simpleVert;
        v.color = color;

        v.position = new Vector3(posMin.x, posMin.y);
        v.uv0 = new Vector2(uvMin.x, uvMin.y);
        vh.AddVert(v);

        v.position = new Vector3(posMin.x, posMax.y);
        v.uv0 = new Vector2(uvMin.x, uvMax.y);
        vh.AddVert(v);

        v.position = new Vector3(posMax.x, posMax.y);
        v.uv0 = new Vector2(uvMax.x, uvMax.y);
        vh.AddVert(v);

        v.position = new Vector3(posMax.x, posMin.y);
        v.uv0 = new Vector2(uvMax.x, uvMin.y);
        vh.AddVert(v);

        vh.AddTriangle(start, start + 1, start + 2);
        vh.AddTriangle(start + 2, start + 3, start);
    }
}