#pragma warning disable 0649
using UnityEngine;

namespace Player
{
    public class Inventory : MonoBehaviour
    {
        [SerializeField] private InventoryItem[] items;
        
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
}
