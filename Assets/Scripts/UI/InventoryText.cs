#pragma warning disable 0649
using System.Text;
using MarchingSquares;
using Player;
using TMPro;
using UnityEngine;

namespace UI
{
    public class InventoryText : MonoBehaviour
    {
        [SerializeField] private Inventory inventory;
        [SerializeField] private TextMeshProUGUI inventoryText;

        private void Awake()
        {
            FindObjectOfType<VoxelMap>().OnVoxelEdit += UpdateText;
            inventory.OnPurchase += UpdateText;
            UpdateText();
        }
        
        private void UpdateText()
        {
            StringBuilder s = new StringBuilder($"{inventory.items[0].name}: {inventory.items[0].amount}");
            for (int i = 1; i < inventory.items.Length; i++)
            {
                InventoryItem item = inventory.items[i];
                s.Append($"\n{item.name}: {item.amount}");
            }

            inventoryText.text = s.ToString();
        }
    }
}
