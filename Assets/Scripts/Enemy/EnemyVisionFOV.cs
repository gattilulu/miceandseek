using UnityEngine;

/// <summary>
/// SENSOR de visão 2D do inimigo:
/// - DETECÇÃO: raio + ângulo (frente = direção de movimento) + linha de visão (obstacleMask)
/// - DESENHO: cone (FOV) como Mesh recortado por obstáculos, com cor/alpha configuráveis
/// - NÃO dispara Game Over; apenas expõe IsSeeingPlayer para a AI (EnemyAI)
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyVisionFOV : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Ponto de origem da visão (se vazio, usa o próprio transform).")]
    [SerializeField] Transform visionOrigin;

    [Header("Parâmetros da Visão")]
    [Tooltip("Alcance máximo do cone (unidades do Unity).")]
    [SerializeField] float viewRadius = 5f;
    [Tooltip("Abertura total do cone em graus.")]
    [SerializeField] float viewAngle = 70f;
    [Tooltip("Resolução angular do cone (mais = mais suave; também mais caro).")]
    [SerializeField, Range(8, 360)] int rayCount = 90;
    [Tooltip("Layer que bloqueia a visão (paredes/obstáculos).")]
    [SerializeField] LayerMask obstacleMask;
    [Tooltip("Tag usada para localizar o Player.")]
    [SerializeField] string playerTag = "Player";

    [Header("Detecção do Player")]
    [SerializeField] bool debugLogSeen = false;
    [SerializeField] Color seenTint = new Color(1f, 0.4f, 0.4f);

    [Header("Render do Cone (FOV)")]
    [SerializeField] Color fovColor = new Color(1f, 1f, 0f, 0.2f);
    [SerializeField] Material fovMaterialOverride;
    [SerializeField] bool drawFOV = true;
    [SerializeField] float meshUpdateInterval = 0.05f;

    // --- Internos ---
    Transform player;
    PlayerHide playerHide;
    SpriteRenderer enemySR;
    Rigidbody2D rb;

    Mesh fovMesh;
    MeshFilter fovMeshFilter;
    MeshRenderer fovMeshRenderer;

    float meshTimer;
    bool playerVisible;

    // Direção "frente" baseada no movimento
    Vector2 facing = Vector2.right;     // fallback quando parado

    // ====== API PÚBLICA ======
    public bool  IsSeeingPlayer => playerVisible;
    public float ViewRadius     { get => viewRadius; set => viewRadius = Mathf.Max(0.01f, value); }
    public float ViewAngle      { get => viewAngle;  set => viewAngle  = Mathf.Clamp(value, 1f, 359f); }


    Vector2? facingOverride = null;
    public void SetFacingOverride(Vector2? dir) { facingOverride = dir; }

    void Awake()
    {
        // Refs
        rb = GetComponent<Rigidbody2D>();
        enemySR = GetComponent<SpriteRenderer>();
        if (!visionOrigin) visionOrigin = transform;

        var pGO = GameObject.FindWithTag(playerTag);
        if (pGO)
        {
            player = pGO.transform;
            playerHide = player.GetComponent<PlayerHide>();
        }

        if (obstacleMask.value == 0)
            Debug.LogWarning("[EnemyVisionFOV] ObstacleMask está vazio. O FOV não será bloqueado por paredes.");

        // Mesh do FOV
        GameObject fovGO = new GameObject("FOV_Mesh");
        fovGO.transform.SetParent(transform, false); // pode ficar no inimigo mesmo
        fovMeshFilter = fovGO.AddComponent<MeshFilter>();
        fovMeshRenderer = fovGO.AddComponent<MeshRenderer>();

        if (!fovMaterialOverride)
        {
            var defaultMat = new Material(Shader.Find("Sprites/Default"));
            defaultMat.color = fovColor;
            fovMeshRenderer.sharedMaterial = defaultMat;
        }
        else
        {
            fovMeshRenderer.sharedMaterial = fovMaterialOverride;
            fovMeshRenderer.sharedMaterial.color = fovColor;
        }

        // Desenhar atrás da sprite do inimigo
        fovMeshRenderer.sortingLayerID = enemySR.sortingLayerID;
        fovMeshRenderer.sortingOrder   = enemySR.sortingOrder - 1;

        fovMesh = new Mesh { name = "FOV_RuntimeMesh" };
        fovMesh.MarkDynamic();
        fovMeshFilter.mesh = fovMesh;
    }

    void OnEnable()
    {
        if (fovMeshRenderer) fovMeshRenderer.enabled = drawFOV;
    }

    void Update()
    {


         // Atualiza a "frente": override > velocidade
        if (facingOverride.HasValue) facing = facingOverride.Value.normalized;
        else
        {
            Vector2 v = rb ? rb.linearVelocity : Vector2.zero;
            if (v.sqrMagnitude > 0.0001f) facing = v.normalized;
        }

        // 1) DETECÇÃO
        playerVisible = IsPlayerVisible();

        // Feedback opcional
        if (enemySR) enemySR.color = playerVisible ? seenTint : Color.white;
        if (playerVisible && debugLogSeen) Debug.Log("[EnemyVision] Player visível!");

        // 2) DESENHO do cone (com throttling)
        if (drawFOV)
        {
            if (meshUpdateInterval <= 0f) BuildFOVMesh();
            else
            {
                meshTimer += Time.deltaTime;
                if (meshTimer >= meshUpdateInterval)
                {
                    meshTimer = 0f;
                    BuildFOVMesh();
                }
            }
        }

        // 3) Sincronizar toggles/cor em runtime
        if (fovMeshRenderer && fovMeshRenderer.enabled != drawFOV)
            fovMeshRenderer.enabled = drawFOV;

        if (fovMeshRenderer && fovMeshRenderer.sharedMaterial &&
            fovMeshRenderer.sharedMaterial.color != fovColor)
        {
            fovMeshRenderer.sharedMaterial.color = fovColor;
        }
    }

    bool IsPlayerVisible()
    {
        if (!player) return false;
        if (playerHide != null && playerHide.IsHidden) return false;

        Vector2 originPos = visionOrigin.position;
        Vector2 toPlayer  = (Vector2)player.position - originPos;

        if (toPlayer.sqrMagnitude > viewRadius * viewRadius) return false;

        // Frente = direção de movimento (fallback já está em 'facing')
        float angleTo = Vector2.Angle(facing, toPlayer.normalized);
        if (angleTo > viewAngle * 0.5f) return false;

        // Linha de visão
        RaycastHit2D hit = Physics2D.Linecast(originPos, player.position, obstacleMask);
        return hit.collider == null;
    }

    void BuildFOVMesh()
    {
        if (!fovMesh) return;

        int vertexCount = rayCount + 2; // origem + 1 por passo + último
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles    = new int[rayCount * 3];
        Color[] colors     = new Color[vertexCount];

        Vector3 origin = visionOrigin.position;

        // Vértice 0 = origem, em espaço local do inimigo
        vertices[0] = transform.InverseTransformPoint(origin);
        colors[0]   = fovColor;

        float startAngle = -viewAngle * 0.5f;
        float angleStep  = viewAngle / Mathf.Max(1, rayCount);

        int vIndex = 1;
        for (int i = 0; i <= rayCount; i++, vIndex++)
        {
            float ang = startAngle + i * angleStep;

            // Direção no mundo: gira a "frente" baseada no movimento
            Vector2 dirWorld = (Quaternion.Euler(0, 0, ang) * (Vector3)facing);

            // Raycast até bater ou até o limite do raio
            Vector3 endPoint;
            RaycastHit2D hit = Physics2D.Raycast(origin, dirWorld, viewRadius, obstacleMask);
            endPoint = hit.collider ? (Vector3)hit.point : origin + (Vector3)dirWorld * viewRadius;

            // Espaço local do inimigo (o mesh é filho dele)
            vertices[vIndex] = transform.InverseTransformPoint(endPoint);
            colors[vIndex]   = fovColor;

            if (i > 0)
            {
                int triIndex = (i - 1) * 3;
                triangles[triIndex + 0] = 0;
                triangles[triIndex + 1] = vIndex - 1;
                triangles[triIndex + 2] = vIndex;
            }
        }

        fovMesh.Clear();
        fovMesh.vertices  = vertices;
        fovMesh.triangles = triangles;
        fovMesh.colors    = colors;
        fovMesh.RecalculateBounds();
    }

    void OnDrawGizmosSelected()
    {
        var o = visionOrigin ? visionOrigin : transform;

        // Usa a mesma "frente" do runtime para visualizar no editor
        Vector2 gizmoFacing = Vector2.right;
        var rbTmp = Application.isPlaying ? GetComponent<Rigidbody2D>() : null;
        if (rbTmp && rbTmp.linearVelocity.sqrMagnitude > 0.0001f) gizmoFacing = rbTmp.linearVelocity.normalized;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(o.position, viewRadius);

        Vector3 left  = Quaternion.Euler(0, 0, +viewAngle / 2f) * gizmoFacing;
        Vector3 right = Quaternion.Euler(0, 0, -viewAngle / 2f) * gizmoFacing;
        Gizmos.DrawLine(o.position, o.position + left  * viewRadius);
        Gizmos.DrawLine(o.position, o.position + right * viewRadius);
    }
}
