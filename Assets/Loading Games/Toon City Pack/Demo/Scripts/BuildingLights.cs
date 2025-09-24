using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class BuildingLights_Emissive : MonoBehaviour
{
    public int windowMaterialIndex = 0;
    public Color lightColor = new Color(1f, 0.85f, 0.6f);
    public bool areLightsOn = true;

    private MeshRenderer mr;
    private Color baseColor;

    void Awake()
    {
        mr = GetComponent<MeshRenderer>();
    }

    void Start()
    {
        // pega _BaseColor (URP) em vez de material.color
        var mat = mr.materials[windowMaterialIndex];
        baseColor = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : Color.white;
        SetLights(areLightsOn);
    }

    public void SetLights(bool isOn)
    {
        var mats = mr.materials;                  // instancia (ok se quer por instância)
        var m = mats[windowMaterialIndex];

        // garante URP/Lit
        if (m.shader.name != "Universal Render Pipeline/Lit")
            m.shader = Shader.Find("Universal Render Pipeline/Lit");

        // Base color mantém o tom do vidro
        m.SetColor("_BaseColor", baseColor);

        if (isOn)
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", lightColor);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        else
        {
            m.SetColor("_EmissionColor", Color.black);
            m.DisableKeyword("_EMISSION");
        }

        mr.materials = mats;
        RendererExtensions.UpdateGIMaterials(mr);            // atualiza GI em tempo real
    }
}
