 ```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    public Image progressBarFill;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI tipsText;
    public Slider loadingBar;
    private string[] tips = { "Welcome to the game!", "Loading...", "Please wait..." };
    private float currentProgress;

    IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            currentProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f); // Adjust the progress bar to reach 100%
            UpdateProgress(currentProgress);
            if (currentProgress == 1.0f && !asyncLoad.allowSceneActivation)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }
    }

    void UpdateProgress(float percent)
    {
        progressBarFill.rectTransform.localScale = new Vector3(percent, 1, 1);
        loadingBar.value = percent * 100f;
        progressText.text = $"{(int)(percent * 100f)}%";
        tipsText.text = tips[(int)currentProgress % tips.Length];
    }

    public void StartLoading(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }
}
```
