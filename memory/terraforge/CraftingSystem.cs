using System;
using System.Collections.Generic;
using UnityEngine;

namespace TerraForge.Crafting
{
    /// <summary>
    /// Crafting system with recipes and queue processing
    /// </summary>
    public class CraftingSystem : MonoBehaviour
    {
        [Header("Crafting Queue")]
        public int maxQueueSize = 5;
        
        // Events
        public event Action<CraftingRecipe> OnRecipeComplete;
        public event Action<CraftingRecipe> OnRecipeStarted;
        
        // Data
        private Queue<CraftingJob> craftingQueue = new Queue<CraftingJob>();
        private CraftingJob currentJob;
        private float craftTimer = 0f;
        private bool isCrafting = false;
        
        [System.Serializable]
        public class CraftingJob
        {
            public CraftingRecipe Recipe;
            public int Quantity;
            public float Progress;
            
            public CraftingJob(CraftingRecipe recipe, int quantity)
            {
                Recipe = recipe;
                Quantity = quantity;
                Progress = 0f;
            }
        }
        
        void Update()
        {
            if (isCrafting && currentJob != null)
            {
                ProcessCrafting();
            }
            else if (!isCrafting && craftingQueue.Count > 0)
            {
                StartNextJob();
            }
        }
        
        void ProcessCrafting()
        {
            craftTimer += Time.deltaTime;
            currentJob.Progress = craftTimer / currentJob.Recipe.CraftTime;
            
            if (craftTimer >= currentJob.Recipe.CraftTime)
            {
                CompleteCrafting();
            }
        }
        
        void StartNextJob()
        {
            if (craftingQueue.Count == 0) return;
            
            currentJob = craftingQueue.Dequeue();
            isCrafting = true;
            craftTimer = 0f;
            OnRecipeStarted?.Invoke(currentJob.Recipe);
        }
        
        void CompleteCrafting()
        {
            isCrafting = false;
            
            // Give output items
            for (int i = 0; i < currentJob.Quantity; i++)
            {
                foreach (var output in currentJob.Recipe.Outputs)
                {
                    // Add to inventory
                    // InventorySystem.Instance.AddItem(output.ItemType, output.Quantity);
                }
            }
            
            CraftingRecipe completedRecipe = currentJob.Recipe;
            currentJob = null;
            OnRecipeComplete?.Invoke(completedRecipe);
        }
        
        /// <summary>
        /// Try to craft a recipe
        /// </summary>
        public bool TryCraft(CraftingRecipe recipe, int quantity = 1)
        {
            if (!CanCraft(recipe, quantity))
            {
                return false;
            }
            
            if (craftingQueue.Count >= maxQueueSize)
            {
                return false;
            }
            
            // Consume inputs
            foreach (var input in recipe.Inputs)
            {
                // InventorySystem.Instance.RemoveItem(input.ItemType, input.Quantity * quantity);
            }
            
            // Add to queue
            craftingQueue.Enqueue(new CraftingJob(recipe, quantity));
            return true;
        }
        
        public bool CanCraft(CraftingRecipe recipe, int quantity = 1)
        {
            // Check if all inputs are available
            foreach (var input in recipe.Inputs)
            {
                int required = input.Quantity * quantity;
                // if (InventorySystem.Instance.GetItemCount(input.ItemType) < required)
                //     return false;
            }
            return true;
        }
        
        public void CancelCurrentJob()
        {
            if (!isCrafting) return;
            
            // Return inputs
            if (currentJob != null)
            {
                foreach (var input in currentJob.Recipe.Inputs)
                {
                    int returnAmount = (int)(input.Quantity * (1f - currentJob.Progress));
                    // if (returnAmount > 0)
                    //     InventorySystem.Instance.AddItem(input.ItemType, returnAmount);
                }
            }
            
            isCrafting = false;
            currentJob = null;
            craftTimer = 0f;
        }
        
        public void ClearQueue()
        {
            craftingQueue.Clear();
        }
        
        public float GetCurrentProgress()
        {
            if (!isCrafting || currentJob == null) return 0f;
            return currentJob.Progress;
        }
    }
    
    [CreateAssetMenu(fileName = "Recipe", menuName = "TerraForge/Crafting Recipe")]
    public class CraftingRecipe : ScriptableObject
    {
        public string RecipeName;
        public string Description;
        public float CraftTime = 1f;
        public Sprite Icon;
        
        [System.Serializable]
        public class Ingredient
        {
            public ItemType ItemType;
            public int Quantity;
        }
        
        public List<Ingredient> Inputs = new List<Ingredient>();
        public List<Ingredient> Outputs = new List<Ingredient>();
        
        [Tooltip("Required crafting station level")]
        public int RequiredStationLevel = 0;
    }
}