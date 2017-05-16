using UnityEngine;
using System.Collections;

public class TextureScroll : MonoBehaviour
{

    public Material mat;

    void Awake()
    {
        mat = transform.GetComponent<Renderer>().material;
    }

    public Vector2 uvAnimationRate = new Vector2(1.0f, 0.0f);
    Vector2 uvOffset = Vector2.zero;

    void Update()
    {
        uvOffset += (uvAnimationRate * Time.deltaTime);
        mat.SetTextureOffset("_MainTex", uvOffset);
    }

}
