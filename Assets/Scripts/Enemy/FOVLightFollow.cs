using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FOVLightFollow : MonoBehaviour
{
    public Transform source;              // arraste o VisionOrigin (ou o objeto do FOV)
    public float angleOffset = 0f;        // use 90, -90 ou 180 se ficar “de lado”
    public float distance = 6f;           // alcance do cone
    public float fovAngle = 60f;          // largura do cone
    public float innerPercent = 0.6f;     // quão suave no centro (0–1)

    Light2D l;

    void Awake() { l = GetComponent<Light2D>(); }

    void LateUpdate()
    {
        if (!source) return;

        // Segue posição e rotação do FOV
        transform.position = source.position;
        transform.rotation = source.rotation * Quaternion.Euler(0,0,angleOffset);

        // Mantém parâmetros do cone coerentes
        if (l)
        {
            l.pointLightOuterRadius = distance;
            l.pointLightOuterAngle  = fovAngle;
            l.pointLightInnerAngle  = fovAngle * innerPercent;
        }
    }
}
