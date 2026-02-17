using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LocalizationManager : MonoBehaviour {
    public static LocalizationManager Instance { get; private set; }
    
    public enum Language { English, Spanish, French, German, Chinese, Japanese, Russian, Portuguese }
    public Language currentLanguage = Language.English;
    
    [System.Serializable]
    public class LocaleText {
        public string key;
        public string english;
        public string spanish;
        public string french;
        public string german;
        public string chinese;
        public string japanese;
        public string russian;
        public string portuguese;
        
        public string GetText(Language lang) => lang switch {
            Language.Spanish => spanish,
            Language.French => french,
            Language.German => german,
            Language.Chinese => chinese,
            Language.Japanese => japanese,
            Language.Russian => russian,
            Language.Portuguese => portuguese,
            _ => english
        };
    }
    
    public List<LocaleText> translations = new();
    private Dictionary<string, LocaleText> translationMap = new();
    private List<TextMeshProUGUI> localizedTexts = new();
    
    void Awake() {
        Instance = this;
        InitializeMap();
        LoadLanguage();
    }
    
    void InitializeMap() {
        foreach (var t in translations) translationMap[t.key] = t;
    }
    
    void LoadLanguage() {
        int saved = PlayerPrefs.GetInt("Language", (int)Language.English);
        currentLanguage = (Language)saved;
    }
    
    public void SetLanguage(Language lang) {
        currentLanguage = lang;
        PlayerPrefs.SetInt("Language", (int)lang);
        UpdateAllUI();
    }
    
    public string GetText(string key) {
        if (translationMap.TryGetValue(key, out LocaleText text)) {
            return text.GetText(currentLanguage);
        }
        return $"[{key}]";
    }
    
    public void RegisterLocalizedText(TextMeshProUGUI text, string key) {
        text.SetTag("LocalizedKey", key);
        localizedTexts.Add(text);
        text.text = GetText(key);
    }
    
    void UpdateAllUI() {
        foreach (var text in localizedTexts) {
            if (text == null) continue;
            string key = text.GetTag("LocalizedKey");
            if (!string.IsNullOrEmpty(key)) text.text = GetText(key);
        }
        OnLanguageChanged?.Invoke(currentLanguage);
    }
    
    public event System.Action<Language> OnLanguageChanged;
}

public static class TextMeshProExtensions {
    private static Dictionary<TextMeshProUGUI, Dictionary<string, string>> tags = new();
    
    public static void SetTag(this TextMeshProUGUI text, string key, string value) {
        if (!tags.ContainsKey(text)) tags[text] = new();
        tags[text][key] = value;
    }
    
    public static string GetTag(this TextMeshProUGUI text, string key) {
        if (tags.TryGetValue(text, out var d) && d.TryGetValue(key, out string v)) return v;
        return null;
    }
}
