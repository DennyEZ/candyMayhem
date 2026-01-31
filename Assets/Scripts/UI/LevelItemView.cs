using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Match3.UI
{
    public class LevelItemView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _levelInfoText;
        [SerializeField] private Image _lockIcon;
        [SerializeField] private GameObject _starContainer;
        
        public event Action OnClick;
        
        public void Initialize(int levelNumber, bool isLocked)
        {
            if (_levelInfoText == null)
            {
                Debug.LogError($"LevelItemView: _levelInfoText is not assigned on {gameObject.name}!");
            }
            else
            {
                _levelInfoText.text = levelNumber.ToString();
                _levelInfoText.gameObject.SetActive(!isLocked);
            }

            SetLocked(isLocked);
            
            if (_button == null)
            {
                Debug.LogError($"LevelItemView: _button is not assigned on {gameObject.name}!");
                return;
            }

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => OnClick?.Invoke());
        }
        
        public void SetLocked(bool isLocked)
        {
            _button.interactable = !isLocked;
            
            if (_lockIcon != null)
                _lockIcon.gameObject.SetActive(isLocked);
                
            if (_levelInfoText != null)
                _levelInfoText.gameObject.SetActive(!isLocked);
                
            // Optional: Dim color if locked
            var colors = _button.colors;
            colors.normalColor = isLocked ? Color.gray : Color.white;
            _button.colors = colors;
        }
    }
}
