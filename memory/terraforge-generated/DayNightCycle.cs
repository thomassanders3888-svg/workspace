 This code snippet is designed to simulate a day-night cycle in Unity using C#. The `DayNightCycle` class manages the transition between day and night by controlling various aspects of the environment including time of day, sun position, weather effects, and lighting conditions.

```csharp
// DayNightCycle.cs
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public float dayLength = 240.0f; // Duration of a day in seconds
    public Transform sunTransform;
    public Gradient skyboxGradient;
    public bool useWeatherEffects = true;
    private float timeOfDay;

    void Update()
    {
        timeOfDay += Time.deltaTime / dayLength * Mathf.PI * 2.0f; // Progress time of day

        // Adjust sun rotation based on the time of day
        sunTransform.localRotation = Quaternion.Euler(-timeOfDay / Mathf.PI * 180.0f, 0.0f, 0.0f);

        // Blend skybox colors based on the time of day
        RenderSettings.skybox.SetColor("_Tint", new Color(0.5f, 0.75f, 0.9f) * Mathf.Sin(timeOfDay / Mathf.PI));
        RenderSettings.skybox.SetColor("_TopColor", skyboxGradient.Evaluate(Mathf.Sin(timeOfDay / Mathf.PI)));

        // Adjust ambient light based on the time of day
        RenderSettings.ambientLight = new Color(1.0f, 1.0f, 1.0f) * Mathf.Cos(timeOfDay / Mathf.PI);

        if (useWeatherEffects)
        {
            // Implement weather effects such as rain or snow based on the time of day
            // This is a placeholder for weather effect implementation
        }
    }

    void OnEnable()
    {
        // Initialize any properties or set up event listeners here
    }

    void OnDisable()
    {
        // Clean up or reset properties when the script is disabled
    }
}
```

This code snippet defines a class `DayNightCycle` that manages the day-night cycle in Unity. It updates the time of day, rotates the sun based on this time, blends the skybox colors to simulate different times of the day, and adjusts the ambient light accordingly. Optional weather effects can be implemented by adding logic within the `if (useWeatherEffects)` block. The `dayLength` parameter controls how long a
