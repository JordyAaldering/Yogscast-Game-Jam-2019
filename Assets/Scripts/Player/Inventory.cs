#pragma warning disable 0649
using MarchingSquares;
using UnityEngine;

namespace Player
{
    public class Inventory : MonoBehaviour
    {
        public InventoryItem[] items;
        public Tool[] tools;

        private int _toolLevel = 0;
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
            return true;
        }
        
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(4f, Screen.height - 154f, 150f, 150f));
            foreach (InventoryItem item in items)
            {
                GUILayout.Label($"{item.name}: {item.amount}");
            }
            
            GUILayout.EndArea();
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
