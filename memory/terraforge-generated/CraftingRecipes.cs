using UnityEngine.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CraftingRecipes", menuName = "TerraForge/CraftingRecipes")]
public class CraftingRecipes : ScriptableObject {
    public List<Recipe> recipes = new();
    
    [System.Serializable]
    public class Ingredient {
        public ResourceType resource;
        public int amount;
    }
    
    [System.Serializable]
    public class Recipe {
        public string recipeId;
        public string name;
        public string description;
        public Ingredient[] ingredients;
        public string resultItemId;
        public int resultAmount = 1;
        public int requiredLevel = 1;
        public Sprite resultIcon;
        public bool unlockedByDefault = true;
    }
    
    void OnEnable() {
        if (recipes.Count == 0) InitializeDefaultRecipes();
    }
    
    void InitializeDefaultRecipes() {
        recipes.Add(new Recipe {
            recipeId = "wood_plank",
            name = "Wood Plank",
            ingredients = new[] { new Ingredient { resource = ResourceType.Wood, amount = 1 } },
            resultItemId = "wood_plank",
            resultAmount = 4
        });
        
        recipes.Add(new Recipe {
            recipeId = "wood_pickaxe",
            name = "Wooden Pickaxe",
            ingredients = new[] { 
                new Ingredient { resource = ResourceType.Wood, amount = 5 }
            },
            resultItemId = "wood_pickaxe",
            requiredLevel = 1
        });
        
        recipes.Add(new Recipe {
            recipeId = "stone_pickaxe",
            name = "Stone Pickaxe",
            ingredients = new[] { 
                new Ingredient { resource = ResourceType.Wood, amount = 2 },
                new Ingredient { resource = ResourceType.Stone, amount = 3 }
            },
            resultItemId = "stone_pickaxe",
            requiredLevel = 2
        });
        
        recipes.Add(new Recipe {
            recipeId = "iron_ingot",
            name = "Iron Ingot",
            ingredients = new[] { 
                new Ingredient { resource = ResourceType.Ore, amount = 2 },
                new Ingredient { resource = ResourceType.Wood, amount = 1 }
            },
            resultItemId = "iron_ingot",
            requiredLevel = 5
        });
        
        recipes.Add(new Recipe {
            recipeId = "brick",
            name = "Clay Brick",
            ingredients = new[] { new Ingredient { resource = ResourceType.Clay, amount = 4 } },
            resultItemId = "brick",
            resultAmount = 4
        });
        
        recipes.Add(new Recipe {
            recipeId = "torch",
            name = "Torch",
            ingredients = new[] { 
                new Ingredient { resource = ResourceType.Wood, amount = 1 },
                new Ingredient { resource = ResourceType.Ore, amount = 1 }
            },
            resultItemId = "torch",
            resultAmount = 4
        });
    }
    
    public Recipe GetRecipe(string id) => recipes.Find(r => r.recipeId == id);
    public List<Recipe> GetRecipesForLevel(int level) => recipes.FindAll(r => r.requiredLevel <= level);
}
