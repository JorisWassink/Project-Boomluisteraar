
using UnityEngine;

[ExecuteInEditMode] 
public class LinkLightToShader : MonoBehaviour
{
    public Material targetMaterial;

    void Update()
    {
        if (targetMaterial != null)
        {
           
            targetMaterial.SetVector("_LightPos", transform.position);
        }
    }
}