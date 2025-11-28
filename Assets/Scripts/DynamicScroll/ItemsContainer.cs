using UnityEngine;

namespace DynamicScroll
{
    [CreateAssetMenu(fileName = "ItemsContainer", menuName = "Configs/ItemsContainer", order = 0)]
    public class ItemsContainer: ScriptableObject
    {
        [SerializeField] private ItemData[] _items;

        public ItemData GetItem(int index)
        {
            return _items[index];
        }

        public int GetTotalItemsCount()
        {
            return _items.Length;
        }
    }
}