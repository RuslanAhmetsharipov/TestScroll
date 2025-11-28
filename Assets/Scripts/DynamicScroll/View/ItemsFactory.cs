using System.Collections.Generic;
using UnityEngine;

namespace DynamicScroll.View
{
    public class ItemsFactory : MonoBehaviour
    {
        [SerializeField] private ItemView _itemPrefab;
        [SerializeField] private Transform _itemsParent;

        private readonly Queue<ItemView> _items = new Queue<ItemView>();

        public ItemView Create()
        {
            if (_items.TryPeek(out var itemView))
            {
                _items.Dequeue();
            }

            var instance = itemView ?? Instantiate(_itemPrefab, _itemsParent);
            return instance;
        }

        public void Return(ItemView instance)
        {
            _items.Enqueue(instance);
        }
    }
}