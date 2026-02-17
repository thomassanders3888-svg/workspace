public class SaveManager { 
    public void Save(string path, string json) { 
        System.IO.File.WriteAllText(path, json); 
    }
    public string Load(string path) { 
        return System.IO.File.Exists(path) ? System.IO.File.ReadAllText(path) : null; 
    }
}
