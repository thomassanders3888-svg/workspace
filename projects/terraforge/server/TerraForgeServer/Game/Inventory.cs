using System;
using System.Collections.Generic;
using System.Linq;

namespace TerraForgeServer.Game
{
    public class Item
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public float Weight { get; set; }

        public Item(string name, int count, float weight)
        {
            Name = name;
            Count = count;
            Weight = weight;
        }

        public float TotalWeight => Count * Weight;
    }

    public class Inventory
    {
        public List<Item> Items { get; private set; } = new List<Item>();

        public float TotalWeight => Items.Sum(i => i.TotalWeight);

        public void AddItem(string name, int count, float weight)
        {
            var existing = Items.FirstOrDefault(i => i.Name == name);
            if (existing != null)
            {
                existing.Count += count;
            }
            else
            {
                Items.Add(new Item(name, count, weight));
            }
        }

        public bool RemoveItem(string name, int count)
        {
            var existing = Items.FirstOrDefault(i => i.Name == name);
            if (existing == null || existing.Count < count)
            {
                return false;
            }

            existing.Count -= count;
            if (existing.Count <= 0)
            {
                Items.Remove(existing);
            }

            return true;
        }

        public bool HasItem(string name, int count = 1)
        {
            var existing = Items.FirstOrDefault(i => i.Name == name);
            return existing != null && existing.Count >= count;
        }
    }
}
