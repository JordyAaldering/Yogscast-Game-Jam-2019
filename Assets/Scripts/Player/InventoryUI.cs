#pragma warning disable 0649
using UnityEngine;

namespace Player
{
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] private Inventory inventory;
        
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(4f, Screen.height - 154f, 150f, 150f));
            foreach (InventoryItem item in inventory.items)
            {
                GUILayout.Label($"{item.name}: {item.amount}");
            }
            
            GUILayout.EndArea();
        }
    }
}