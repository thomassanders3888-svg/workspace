using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TerraForge.Persistence
{
    /// <summary>
    /// Save/load system with compression and versioning
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }
        
        [Header("Save Settings")]
        public string SaveFileName = "save.dat";
        public bool useCompression = true;
        public int currentVersion = 1;
        
        private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);
        
        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        /// <summary>
        /// Save game data asynchronously
        /// </summary>
        public async Task<bool> SaveGame(GameSaveData data)
        {
            try
            {
                data.Version = currentVersion;
                data.SaveTime = DateTime.UtcNow;
                
                string json = JsonUtility.ToJson(data, true);
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                
                if (useCompression)
