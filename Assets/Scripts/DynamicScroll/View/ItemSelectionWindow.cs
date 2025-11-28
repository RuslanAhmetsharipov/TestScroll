using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Foundation;
using UnityEngine;
using UnityEngine.UI;

namespace DynamicScroll.View
{
    public class ItemSelectionWindow : MonoBehaviour
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _backgroundButton;
        [SerializeField] private RectTransform _contentTransform;
        [SerializeField] private float _animationTime = 0.3f;
        [SerializeField] private int _animationFrameRate = 60;
        [SerializeField] private ItemsFactory _itemsFactory;
        [SerializeField] private VisualRegistry _visualRegistry;
        [SerializeField] private ItemsContainer _itemsContainer;
        
        private ReactiveProperty<ItemView> _selectedItem = new ReactiveProperty<ItemView>(null);
        private int _totalItemCount;
        private int _selectedIndex = -1;
        private ItemView[] _itemViews;
        private Action _onClose;
        private CancellationTokenSource _cts;
        
        private void Start()
        {
            _closeButton?.onClick.AddListener(Close);
            _backgroundButton?.onClick.AddListener(Close);
            _totalItemCount = _itemsContainer.GetTotalItemsCount() - 1;
            GenerateItems();
        }

        private void OnDestroy()
        {
            _closeButton?.onClick.RemoveListener(Close);
            _backgroundButton?.onClick.RemoveListener(Close);
            if (_cts is { IsCancellationRequested: false })
            {
                _cts.Cancel();
            }
        }

        private void Update()
        {
            int currentIndex = _selectedIndex;
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                currentIndex--;
                SelectItem(currentIndex < 0 ? 0 : currentIndex);
                return;
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                currentIndex++;
                SelectItem(currentIndex < _totalItemCount ? currentIndex : _totalItemCount);
            }
        }

        private void OnItemSelectClicked(ItemView itemView)
        {
            if (_selectedItem.Value == itemView)
            {
                _selectedItem.Value.SetSelected(false);
                _selectedItem.Value = null;
                _selectedIndex = -1;
                return;
            }

            var index = Array.IndexOf(_itemViews, itemView);
            if (index == -1)
                Debug.LogError($"ItemView {itemView} not found");
            
            SelectItem(index);
        }

        private void GenerateItems()
        {
            if (_itemViews != null)
                return;

            _itemViews = new ItemView[_totalItemCount];
            for (int i = 0; i < _totalItemCount; i++)
            {
                _itemViews[i] = _itemsFactory.Create();
                _itemViews[i].SetData(_visualRegistry, OnItemSelectClicked, _itemsContainer.GetItem(i));
            }
        }

        private void Close()
        {
            gameObject.SetActive(false);
        }

        private void ShowCurrentItem()
        {
            _selectedItem.Value?.SetSelected(true);
        }

        private void SelectItem(int index)
        {
            _selectedItem.Value?.SetSelected(false);
            if (_selectedIndex == index)
            {
                _selectedIndex = -1;
                return;
            }

            _selectedIndex = index;
            var calculatedHeight = 0f;
            for (int i = 0; i < _selectedIndex; i++)
            {
                calculatedHeight += _itemViews[i].CurrentHeight;
            }

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            _selectedItem.Value = _itemViews[_selectedIndex];
            MoveScrollTo(calculatedHeight).Forget();
        }

        private async UniTaskVoid MoveScrollTo(float height)
        {
            var speed = new Vector2(0f, (height - _contentTransform.anchoredPosition.y) / _animationTime);
            var timeStepMilliseconds = (int)(1f / _animationFrameRate * 1000);
            var startPosition = _contentTransform.anchoredPosition;
            var finalPosition = new Vector2(_contentTransform.anchoredPosition.x, height);
            var cumulativeTime = 0f;
            while (Mathf.Abs(_contentTransform.anchoredPosition.y - height) > 0.1f)
            {
                cumulativeTime += timeStepMilliseconds;
                _contentTransform.anchoredPosition = Vector2.Lerp(startPosition, finalPosition, cumulativeTime/(_animationTime * 1000));
                await UniTask.Delay(timeStepMilliseconds, cancellationToken: _cts.Token);
            }

            ShowCurrentItem();
        }
    }
}