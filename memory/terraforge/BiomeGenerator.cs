using System;
using UnityEngine;

namespace TerraForge.WorldGen
{
    /// <summary>
    /// Biome generation using Perlin noise and climate simulation
    /// </summary>
    public class BiomeGenerator : MonoBehaviour
    {
        [Header("Biome Settings")]
        public float temperatureScale = 0.001f;
        public float humidityScale = 0.001f;
        public float heightScale = 0.002f;
        
        [Header("Climate Zones")]
        public float equatorTemperature = 30f;
        public float poleTemperature = -20f;
        
        // Noise seeds
        private int temperatureSeed;
        private int humiditySeed;
        private int heightSeed;
        
        void Awake()
        {
            // Randomize seeds
            temperatureSeed = UnityEngine.Random.Range(0, 100000);
            humiditySeed = UnityEngine.Random.Range(0, 100000);
            heightSeed = UnityEngine.Random.Range(0, 100000);
        }
        
        /// <summary>
        /// Get biome at world position
        /// </summary>
        public BiomeType GetBiomeAt(Vector3 worldPos)
        {
            return GetBiomeAt(worldPos.x, worldPos.z);
        }
        
        public BiomeType GetBiomeAt(float x, float z)
        {
            float temperature = GetTemperature(x, z);
            float humidity = GetHumidity(x, z);
            float height = GetHeight(x, z);
            
            return DetermineBiome(temperature, humidity, height);
        }
        
        float GetTemperature(float x, float z)
        {
            // Base temperature based on latitude (z coordinate as proxy)
            float latitude = Mathf.Abs(z / 10000f); // Normalize
            float baseTemp = Mathf.Lerp(equatorTemperature, poleTemperature, latitude);
            
            // Add noise variation
            float noise = PerlinNoise(x * temperatureScale + temperatureSeed, 
                                     z * temperatureScale + temperatureSeed);
            float variation = Mathf.Lerp(-10f, 10f, noise);
            
            return baseTemp + variation;
        }
        
        float