#pragma warning disable 0649
using UnityEngine;

namespace Environment
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteObject : MonoBehaviour
    {
        [SerializeField] private SpriteArray sprites;

        private void Awake()
        {
            GetComponent<SpriteRenderer>().sprite = sprites.GetRandom();
        }
    }
}
