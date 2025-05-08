using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class CameraOcclusionHandler : MonoBehaviour
{
    [Header("Configuración de Objetivo")]
    [Tooltip("Transform del objetivo que la cámara debe mantener siempre visible (centro de la arena).")]
    public Transform target;

    [Header("Capas de Obstáculos")]
    [Tooltip("Capa(s) que contienen los objetos que pueden bloquear la vista.")]
    public LayerMask obstacleLayers;

    [Header("Referencia de Cámara (opcional)")]
    [Tooltip("Si no se asigna, se usará Camera.main.")]
    public Camera cam;

    // Lista interna de renderers actualmente escondidos
    private List<Renderer> hiddenRenderers = new List<Renderer>();

    void Awake()
    {
        if (cam == null)
            cam = Camera.main;
    }

    void LateUpdate()
    {
        if (target == null || cam == null)
            return;

        // 1. Raycast desde el target hacia la cámara
        Vector3 origin = target.position;
        Vector3 direction = cam.transform.position - origin;
        float distance = direction.magnitude;

        RaycastHit[] hits = Physics.RaycastAll(origin, direction, distance, obstacleLayers);

        // Conjunto de renderers que actualmente obstruyen
        var currentHits = new HashSet<Renderer>();
        foreach (var hit in hits)
        {
            // Obtener Renderer del collider
            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend == null)
                continue;

            // Ignora al propio target (si lo tiene) o componentes no deseados
            if (rend.transform.IsChildOf(target))
                continue;

            currentHits.Add(rend);

            // Si aún no estaba oculto, ocultarlo ahora
            if (!hiddenRenderers.Contains(rend))
            {
                rend.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                hiddenRenderers.Add(rend);
            }
        }

        // 2. Restaurar los que ya no obstruyen
        for (int i = hiddenRenderers.Count - 1; i >= 0; i--)
        {
            var rend = hiddenRenderers[i];
            if (!currentHits.Contains(rend))
            {
                // Restaurar modo de render original
                rend.shadowCastingMode = ShadowCastingMode.On;
                hiddenRenderers.RemoveAt(i);
            }
        }
    }
}
