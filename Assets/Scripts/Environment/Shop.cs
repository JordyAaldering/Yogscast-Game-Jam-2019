#pragma warning disable 0649
using Player;
using TMPro;
using UnityEngine;

namespace Environment
{
    public class Shop : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI buyText;
        [SerializeField] private Inventory inventory;

        private bool inside, buy;

        private void Update()
        {
            if (inside && Input.GetButtonDown("Interact"))
                buy = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            inside = true;
            buyText.text = inventory.GetBuyText();
            buyText.enabled = true;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (buy && inventory.TryBuy())
            {
                buyText.text = inventory.GetBuyText();
                buy = false;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            inside = false;
            buyText.enabled = false;
        }
    }
}
