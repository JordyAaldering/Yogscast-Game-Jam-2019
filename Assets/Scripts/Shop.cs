#pragma warning disable 0649
using Player;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    [SerializeField] private Text buyText;
    
    private Inventory inventory;

    private void Awake()
    {
        inventory = FindObjectOfType<Inventory>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        buyText.text = inventory.GetBuyText();
        buyText.enabled = true;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (Input.GetButtonDown("Submit"))
        {
            if (inventory.TryBuy())
            {
                buyText.text = inventory.GetBuyText();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        buyText.enabled = false;
    }
}
