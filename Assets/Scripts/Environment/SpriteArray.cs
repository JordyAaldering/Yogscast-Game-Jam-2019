using UnityEngine;

namespace Environment
{
    [CreateAssetMenu(menuName="Utilities/Sprite Array", fileName="New Sprite Array")]
    public class SpriteArray : ScriptableObject
    {
        public Sprite[] sprites = new Sprite[0];

        public Sprite GetRandom()
        {
            return sprites[Random.Range(0, sprites.Length)];
        }
    }
}
