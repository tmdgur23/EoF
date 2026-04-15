using UnityEngine;

public class FixSceneShaders : MonoBehaviour
{
    [ContextMenu("Convert Scene Materials To URP Lit")]
    public void ConvertSceneMaterials()
    {
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null)
        {
            Debug.LogError("URP Lit 셰이더를 못 찾음");
            return;
        }

        Renderer[] renderers = FindObjectsByType<Renderer>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        int matCount = 0;

        foreach (Renderer r in renderers)
        {
            foreach (Material mat in r.sharedMaterials)
            {
                if (mat == null) continue;

                mat.shader = urpLit;
                matCount++;
            }
        }

        Debug.Log($"셰이더 변경 완료: {matCount}개 머티리얼");
    }
}