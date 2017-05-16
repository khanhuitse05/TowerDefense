using UnityEngine;
using UnityEngine.UI;

public class EmptyGraphic : Graphic
{
    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();
    }
}