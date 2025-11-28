using System;
using System.Collections.Generic;
using UnityEngine;

namespace Foundation
{
    [CreateAssetMenu(fileName = "VisualRegistry", menuName = "Configs/VisualRegistry", order = 0)]
    public class VisualRegistry : ScriptableObject
    {
        [SerializeField] private SpriteInfoContainer[] _spritesSerialized;
        private Dictionary<string, Sprite> _spritesAtlas;

        private void OnValidate()
        {
            _spritesAtlas = new Dictionary<string, Sprite>();
            foreach (var spriteInfoContainer in _spritesSerialized)
            {
                _spritesAtlas.Add(spriteInfoContainer.SpriteId, spriteInfoContainer.Sprite);
            }
        }

        public Sprite GetSprite(string spriteName)
        {
            return _spritesAtlas[spriteName];
        }
    }

    [Serializable]
    public class SpriteInfoContainer
    {
        [SerializeField] private string _spriteId;
        [SerializeField] private Sprite _sprite;

        public Sprite Sprite => _sprite;
        public string SpriteId => _spriteId;
    }
}