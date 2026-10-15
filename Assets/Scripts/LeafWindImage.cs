using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A uGUI Image that builds a lightweight vertically segmented mesh for StylizedLeafWind.
/// Eight strips let the shader bend a leaf's silhouette without 2D Animation bones.
/// </summary>
[AddComponentMenu("UI/Leaf Wind Image")]
public sealed class LeafWindImage : Image
{
    private const int VerticalSegmentCount = 8;
    private const AdditionalCanvasShaderChannels RequiredShaderChannels = AdditionalCanvasShaderChannels.TexCoord1;

    protected override void OnEnable()
    {
        base.OnEnable();
        EnableRequiredCanvasChannels();
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        base.OnPopulateMesh(vertexHelper);

        int vertexCount = vertexHelper.currentVertCount;
        if (vertexCount == 0)
        {
            return;
        }

        // Find the actual sprite region before rebuilding the standard quad as a narrow grid.
        // This supports PSD mosaics and SpriteAtlases without an extra texture.
        UIVertex vertex = default;
        Vector2 atlasMin = new(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 atlasMax = new(float.NegativeInfinity, float.NegativeInfinity);

        for (int index = 0; index < vertexCount; index++)
        {
            vertexHelper.PopulateUIVertex(ref vertex, index);
            Vector2 atlasUv = new(vertex.uv0.x, vertex.uv0.y);
            atlasMin = Vector2.Min(atlasMin, atlasUv);
            atlasMax = Vector2.Max(atlasMax, atlasUv);
        }

        Vector2 atlasSize = atlasMax - atlasMin;
        Rect pixelAdjustedRect = GetPixelAdjustedRect();
        if (pixelAdjustedRect.width <= 0.0001f || pixelAdjustedRect.height <= 0.0001f)
        {
            return;
        }

        vertexHelper.Clear();

        for (int row = 0; row <= VerticalSegmentCount; row++)
        {
            float localY = row / (float)VerticalSegmentCount;
            float positionY = Mathf.Lerp(pixelAdjustedRect.yMin, pixelAdjustedRect.yMax, localY);

            for (int column = 0; column <= 1; column++)
            {
                float localX = column;
                UIVertex gridVertex = UIVertex.simpleVert;
                gridVertex.position = new Vector3(Mathf.Lerp(pixelAdjustedRect.xMin, pixelAdjustedRect.xMax, localX), positionY, 0f);
                gridVertex.color = color;
                gridVertex.uv0 = atlasMin + Vector2.Scale(atlasSize, new Vector2(localX, localY));

                // uv1.xy is local position; z is pivot height; w converts UV strength to pixels.
                gridVertex.uv1 = new Vector4(localX, localY, rectTransform.pivot.y, pixelAdjustedRect.width);
                vertexHelper.AddVert(gridVertex);
            }
        }

        for (int row = 0; row < VerticalSegmentCount; row++)
        {
            int lowerLeft = row * 2;
            int upperLeft = lowerLeft + 2;
            vertexHelper.AddTriangle(lowerLeft, upperLeft, lowerLeft + 1);
            vertexHelper.AddTriangle(lowerLeft + 1, upperLeft, upperLeft + 1);
        }
    }

    private void EnableRequiredCanvasChannels()
    {
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            parentCanvas.additionalShaderChannels |= RequiredShaderChannels;
        }
    }
}
