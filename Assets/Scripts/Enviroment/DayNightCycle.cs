using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class DayNightCycle : MonoBehaviour
{
    [Header("Sol / Luna")]
    [Tooltip("Directional Light que actúa como Sol/Luna")]
    public Light sunLight;
    [Tooltip("Segundos que dura un día completo (0→1 en _timeOfDay)")]
    public float fullDayLength = 120f;
    [Tooltip("Ángulo inicial en grados (0 = amanecer en el horizonte este)")]
    [Range(0f, 360f)] public float startAngle = 0f;

    [Header("Global Volume")]
    [Tooltip("Volume global con overrides de Color Adjustments y Bloom")]
    public Volume globalVolume;

    // :::::: Definiciones por código ::::::
    private AnimationCurve intensityCurve;
    private Gradient sunColorGradient;
    private Gradient ambientColorGradient;
    private Gradient fogColorGradient;

    // Post-proceso
    private ColorAdjustments _colorAdj;
    private Bloom _bloom;

    [Range(0f, 1f)]
    private float _timeOfDay = 0f;

    void Awake()
    {
        // 1) Referencia automática al sol si no está asignado
        if (sunLight == null)
            sunLight = RenderSettings.sun;

        // 2) Definir la curva de intensidad [0→1] con 5 keys y suavizado
        intensityCurve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.25f, 0.5f),
            new Keyframe(0.5f, 1f),
            new Keyframe(0.75f, 0.5f),
            new Keyframe(1f, 0f)
        );
        for (int i = 0; i < intensityCurve.length; i++)
            intensityCurve.SmoothTangents(i, 0f);

        // 3) Definir Gradient del Sol
        sunColorGradient = new Gradient();
        sunColorGradient.colorKeys = new[] {
            new GradientColorKey(new Color32(247,233,192,255), 0f),
            new GradientColorKey(Color.white,                  0.5f),
            new GradientColorKey(new Color32(247,233,192,255), 1f)
        };
        sunColorGradient.alphaKeys = new[] {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(1f, 1f)
        };

        // 4) Definir Gradient Ambiente
        ambientColorGradient = new Gradient();
        ambientColorGradient.colorKeys = new[] {
            new GradientColorKey(new Color32(26, 42, 64,255),   0f),
            new GradientColorKey(new Color32(181,219,232,255),  0.5f),
            new GradientColorKey(new Color32(26, 42, 64,255),   1f)
        };
        ambientColorGradient.alphaKeys = new[] {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(1f, 1f)
        };

        // 5) Definir Gradient de Niebla
        fogColorGradient = new Gradient();
        fogColorGradient.colorKeys = new[] {
            new GradientColorKey(new Color32(12, 26, 42,255),   0f),
            new GradientColorKey(new Color32(174,203,232,255),  0.5f),
            new GradientColorKey(new Color32(12, 26, 42,255),   1f)
        };
        fogColorGradient.alphaKeys = new[] {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(1f, 1f)
        };

        // 6) Capturar overrides de post-proceso
        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out _colorAdj);
            globalVolume.profile.TryGet(out _bloom);
        }
    }

    void Update()
    {
        if (sunLight == null) return;

        // 1) Avanza y normaliza el tiempo [0–1]
        _timeOfDay += Time.deltaTime / fullDayLength;
        if (_timeOfDay >= 1f) _timeOfDay -= 1f;

        // 2) Rotar Sol/Luna
        float angle = _timeOfDay * 360f + startAngle;
        sunLight.transform.rotation = Quaternion.Euler(angle, 170f, 0f);

        // 3) Intensidad y color del Sol
        float intensity = intensityCurve.Evaluate(_timeOfDay);
        sunLight.intensity = intensity;
        sunLight.color = sunColorGradient.Evaluate(_timeOfDay);

        // 4) Luz ambiental
        RenderSettings.ambientLight = ambientColorGradient.Evaluate(_timeOfDay);

        // 5) Niebla
        RenderSettings.fogColor = fogColorGradient.Evaluate(_timeOfDay);
        RenderSettings.fogDensity = Mathf.Lerp(0.003f, 0.012f, 1f - intensity);

        // 6) Reflexiones
        RenderSettings.reflectionIntensity = Mathf.Lerp(0.2f, 1f, intensity);

        // 7) Exposición del skybox Procedural
        float exposure = Mathf.Lerp(0.2f, 1.3f, intensity);
        RenderSettings.skybox.SetFloat("_Exposure", exposure);

        // 8) Post-proceso dinámico
        if (_colorAdj != null)
        {
            _colorAdj.postExposure.value = Mathf.Lerp(-0.5f, 0f, intensity);
            _colorAdj.contrast.value = Mathf.Lerp(-20f, -10f, intensity);
            _colorAdj.saturation.value = Mathf.Lerp(-10f, 15f, intensity);
            var nightFilter = new Color(160 / 255f, 188 / 255f, 224 / 255f);
            _colorAdj.colorFilter.value = Color.Lerp(nightFilter, Color.white, intensity);
        }
        if (_bloom != null)
        {
            float b = Mathf.Lerp(0.4f, 0.6f, Mathf.Abs(intensity - 0.5f) * 2f);
            _bloom.intensity.value = b;
        }
    }
}
