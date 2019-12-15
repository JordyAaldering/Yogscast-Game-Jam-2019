#pragma warning disable 0649
using System;
using MarchingSquares;
using UI;
using UnityEngine;

namespace Player
{
    [CreateAssetMenu(menuName="Player/Inventory", fileName="New Inventory")]
    public class Inventory : ScriptableObject
    {
        public InventoryItem[] items;
        public Tool[] tools;

        [SerializeField] private int _toolLevel = 0;
        private int toolLevel
        {
            get => _toolLevel;
            set
            {
                _toolLevel = value;
                VoxelMap map = FindObjectOfType<VoxelMap>();
                map.MaxRadius = tools[_toolLevel].maxRadius;
                map.MaxStencils = tools[_toolLevel].maxStencils;
            }
        }
        
        public Action OnPurchase = delegate { };

        private void Awake()
        {
            if (!Application.isPlaying)
                return;
            
            VoxelMap map = FindObjectOfType<VoxelMap>();
            map.MaxRadius = tools[_toolLevel].maxRadius;
            map.MaxStencils = tools[_toolLevel].maxStencils;
        }

        public void Add(int index)
        {
            items[index].amount++;
            CheckComplete();
        }

        public string GetBuyText()
        {
            if (toolLevel + 1 >= tools.Length)
                return "";
            
            Tool tool = tools[toolLevel + 1];
            InventoryItem item = items[tool.costTypeIndex];

            return $"{tool.name}\n{tool.costAmount} {item.name}\nBuy: [E]";
        }

        public bool TryBuy()
        {
            if (toolLevel + 1 >= tools.Length)
                return false;
            
            Tool tool = tools[toolLevel + 1];
            InventoryItem item = items[tool.costTypeIndex];
            
            if (item.amount < tool.costAmount)
                return false;

            item.amount -= tool.costAmount;
            toolLevel++;
            
            CheckComplete();
            OnPurchase();
            
            return true;
        }

        private void CheckComplete()
        {
            int diamondIndex = items.Length - 1;
            
            Transform complete = FindObjectOfType<CompleteGame>().gameObject.transform;
            bool active = items[diamondIndex].amount > 0;
            complete.GetChild(0).gameObject.SetActive(active);
            complete.GetChild(1).gameObject.SetActive(active);
        }
    }

    [System.Serializable]
    public class InventoryItem
    {
        public string name = "Name";
        public int amount = 0;
    }
    
    [System.Serializable]
    public class Tool
    {
        public string name = "Name";

        public int costTypeIndex = 0;
        public int costAmount = 0;

        public int maxRadius = 2;
        public int maxStencils = 1;
    }
}
