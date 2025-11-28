using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Foundation;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DynamicScroll.View
{
    public class ItemView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private GameObject _selectIndicator;
        [SerializeField] private Image _image;
        [SerializeField] private TMP_Text _shortDescription;
        [SerializeField] private TMP_Text _mainDescription;
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private float _animationTime = 0.3f;

        private float _startY;
        private Action<ItemView> _onClick;
        private float _currentAddedHeight;
        private CancellationTokenSource _cancellationToken;
        
        public string CurrentItemId { get; private set; }
        public bool IsSelected { get; private set; }
        public float CurrentAddedHeight => _currentAddedHeight;

        private void Start()
        {
            _startY = _rectTransform.sizeDelta.y;
        }

        private void OnDestroy()
        {
            if (_cancellationToken is { IsCancellationRequested: false })
            {
                _cancellationToken.Cancel();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _onClick?.Invoke(this);
        }
        
        public void SetData(VisualRegistry visualRegistry, Action<ItemView> onClicked, ItemData itemData)
        {
            _onClick = onClicked;
            _image.sprite = visualRegistry.GetSprite(itemData.SpriteId);
            _shortDescription.text = itemData.ShortDescription;
            _mainDescription.text = itemData.LongDescription;
            CurrentItemId = itemData.ItemId;
        }

        public void SetSelected(bool isSelected)
        {
            _cancellationToken?.Cancel();
            _cancellationToken = new CancellationTokenSource();
            if (isSelected)
            {
                OpenItem().Forget();
                _selectIndicator.SetActive(true);
            }
            else
            {
                CloseItem().Forget();
                _selectIndicator.SetActive(false);
            }
        }

        public float GetPosition()
        {
            return _rectTransform.anchoredPosition.y;
        }

        private async UniTask OpenItem()
        {
            IsSelected = true;
            var heightToAdd = _mainDescription.rectTransform.rect.height;
            var speed = heightToAdd / _animationTime;
            var sizeDelta = _rectTransform.sizeDelta;
            var cumulativeTime = 0f;
            while (_rectTransform.sizeDelta.y - heightToAdd < _startY)
            {
                cumulativeTime += Time.deltaTime;
                _rectTransform.sizeDelta = new Vector2(sizeDelta.x, sizeDelta.y + speed * cumulativeTime);
                await UniTask.Yield(_cancellationToken.Token);
            }

            _currentAddedHeight = heightToAdd;
        }

        private async UniTask CloseItem()
        {
            IsSelected = false;
            var heightToRemove = _mainDescription.rectTransform.rect.height;
            var speed = heightToRemove / _animationTime;
            var sizeDelta = _rectTransform.sizeDelta;
            var cumulativeTime = 0f;
            while (_rectTransform.sizeDelta.y - _startY > 0f)
            {
                cumulativeTime += Time.deltaTime;
                _rectTransform.sizeDelta = new Vector2(sizeDelta.x, sizeDelta.y - speed * cumulativeTime);
                await UniTask.Yield(_cancellationToken.Token);
            }

            _currentAddedHeight = 0f;
        }
    }
}