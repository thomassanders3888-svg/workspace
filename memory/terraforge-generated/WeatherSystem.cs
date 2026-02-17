using UnityEngine;

public class WeatherSystem : MonoBehaviour {
    public enum WeatherType { Clear, Rain, Snow, Storm }
    public WeatherType currentWeather = WeatherType.Clear;
    public ParticleSystem rainParticles;
    public ParticleSystem snowParticles;
    public float weatherChangeInterval = 300f;
    private float timer;
    
    void Update() {
        timer += Time.deltaTime;
        if (timer >= weatherChangeInterval) {
            ChangeWeather();
            timer = 0;
        }
    }
    
    void ChangeWeather() {
        currentWeather = (WeatherType)Random.Range(0, 4);
        rainParticles.gameObject.SetActive(currentWeather == WeatherType.Rain || currentWeather == WeatherType.Storm);
        snowParticles.gameObject.SetActive(currentWeather == WeatherType.Snow);
    }
}
