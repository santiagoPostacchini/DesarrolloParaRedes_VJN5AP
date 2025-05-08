using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Sol")]
    public Light sunLight;
    [Tooltip("Segundos que dura un día completo")]
    public float fullDayLength = 120f;
    [Range(0f, 360f)] public float startAngle = 0f;

    [Header("Curvas y Gradientes")]
    public AnimationCurve intensityCurve;      // 0–1 según hora del día
    public Gradient sunColorGradient;   // Color de la luz solar
    public Gradient ambientColorGradient; // Color de la luz ambiental
    public Gradient fogColorGradient;     // Color de la niebla

    [Header("Valores base")]
    public float minExposure = 0.2f, maxExposure = 1.3f;
    public float minFogDensity = 0.005f, maxFogDensity = 0.05f;
    public float minReflection = 0.2f, maxReflection = 1f;

    private float _timeOfDay; // 0–1

    void Update()
    {
        // 1. Avanzar tiempo
        _timeOfDay += Time.deltaTime / fullDayLength;
        if (_timeOfDay >= 1f) _timeOfDay -= 1f;

        // 2. Rotar "sol"
        float sunAngle = _timeOfDay * 360f + startAngle;
        sunLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

        // 3. Intensidad y color del sol
        float intensity = intensityCurve.Evaluate(_timeOfDay);
        sunLight.intensity = intensity;
        sunLight.color = sunColorGradient.Evaluate(_timeOfDay);

        // 4. Luz ambiental
        RenderSettings.ambientLight = ambientColorGradient.Evaluate(_timeOfDay);

        // 5. Niebla
        RenderSettings.fogColor = fogColorGradient.Evaluate(_timeOfDay);
        RenderSettings.fogDensity = Mathf.Lerp(minFogDensity, maxFogDensity, 1 - intensity);

        // 6. Skybox (Exposure)
        float exposure = Mathf.Lerp(minExposure, maxExposure, intensity);
        RenderSettings.skybox.SetFloat("_Exposure", exposure);

        // 7. Reflexiones
        RenderSettings.reflectionIntensity = Mathf.Lerp(minReflection, maxReflection, intensity);
    }
}
