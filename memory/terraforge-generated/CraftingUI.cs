using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingUI : MonoBehaviour {
    public GameObject craftingPanel;
    public Transform recipesParent;
    public GameObject recipeButtonPrefab;
    public TextMeshProUGUI selectedRecipeName;
    public TextMeshProUGUI selectedRecipeDescription;
    public Button craftButton;
    
    private RecipeData selectedRecipe;
    
    void Start() {
        craftButton.onClick.AddListener(OnCraftClick);
        craftButton.interactable = false;
    }
    
    public void ShowCrafting() {
        craftingPanel.SetActive(true);
        RefreshRecipes();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    void RefreshRecipes() {
        foreach (Transform child in recipesParent) {
            Destroy(child.gameObject);
        }
        
        var recipes = CraftingSystem.Instance.GetUnlockedRecipes();
        foreach (var recipe in recipes) {
            var btn = Instantiate(recipeButtonPrefab, recipesParent).GetComponent<Button>();
            btn.GetComponentInChildren<TextMeshProUGUI>().text = recipe.resultName;
            btn.onClick.AddListener(() => SelectRecipe(recipe));
        }
    }
    
    void SelectRecipe(RecipeData recipe) {
        selectedRecipe = recipe;
        selectedRecipeName.text = recipe.resultName;
        selectedRecipeDescription.text = recipe.GetDescription();
        craftButton.interactable = HasIngredients(recipe);
    }
    
    bool HasIngredients(RecipeData recipe) {
        return true;
    }
    
    void OnCraftClick() {
        if (selectedRecipe != null) {
            CraftingSystem.Instance.Craft(selectedRecipe);
            craftButton.interactable = HasIngredients(selectedRecipe);
        }
    }
}

[System.Serializable]
public class RecipeData {
    public string recipeId;
    public string resultName;
    public ResourceType[] ingredients;
    public int[] ingredientCounts;
    public int resultCount = 1;
    public int requiredLevel = 1;
    
    public string GetDescription() {
        string desc = "Requires:\n";
        for (int i = 0; i < ingredients.Length; i++) {
            desc += $"- {ingredients[i]} x{ingredientCounts[i]}\n";
        }
        return desc + $"\nMakes: {resultCount} {resultName}";
    }
}
