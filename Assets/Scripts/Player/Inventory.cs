#pragma warning disable 0649
using MarchingSquares;
using UnityEngine;

namespace Player
{
    public class Inventory : MonoBehaviour
    {
        [SerializeField] private InventoryItem[] items;
        [SerializeField] private Tool[] tools;

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
        public int level = 0;

        public InventoryItem costType;
        public int costAmount = 0;

        public int maxRadius = 2;
        public int maxStencils = 1;
    }
}
