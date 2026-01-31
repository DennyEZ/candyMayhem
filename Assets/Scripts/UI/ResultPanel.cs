using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using Sirenix.OdinInspector;

namespace Match3.UI
{
    /// <summary>
    /// Manages a game result panel (Win or Lose).
    /// Handles animations and standard button events.
    /// </summary>
    public class ResultPanel : MonoBehaviour
    {
        [Title("UI Elements")]
        public Button NextLevelButton;
        public Button RestartButton;
        public Button QuitButton;
        public Button HomeButton;
        
        [Title("Animation")]
        public CanvasGroup PanelGroup;
        public float FadeDuration = 0.5f;
        
        // Events
        public UnityEvent OnNextLevelClicked;
        public UnityEvent OnRestartClicked;
        public UnityEvent OnQuitClicked;
        public UnityEvent OnHomeClicked;
        
        private void Awake()
        {
            // Auto-find canvas group if missing
            if (PanelGroup == null)
                PanelGroup = GetComponent<CanvasGroup>();
                
            // Bind buttons
            if (NextLevelButton != null)
                NextLevelButton.onClick.AddListener(() => OnNextLevelClicked?.Invoke());
                
            if (RestartButton != null)
                RestartButton.onClick.AddListener(() => OnRestartClicked?.Invoke());
                
            if (QuitButton != null)
                QuitButton.onClick.AddListener(() => OnQuitClicked?.Invoke());

            if (HomeButton != null)
                HomeButton.onClick.AddListener(() => OnHomeClicked?.Invoke());
        }
        
        /// <summary>
        /// Shows the panel with animation.
        /// </summary>
        [Button]
        public void Show()
        {
            gameObject.SetActive(true);
            
            if (PanelGroup != null)
            {
                PanelGroup.alpha = 0f;
                PanelGroup.DOFade(1f, FadeDuration).SetUpdate(true); // Ignore time scale
            }
            
            // Animate buttons
            AnimateButton(NextLevelButton, 0.1f);
            AnimateButton(RestartButton, 0.2f);
            AnimateButton(QuitButton, 0.3f);
            AnimateButton(HomeButton, 0.3f);
        }
        
        /// <summary>
        /// Hides the panel.
        /// </summary>
        [Button]
        public void Hide()
        {
            if (PanelGroup != null)
            {
                PanelGroup.DOFade(0f, 0.2f).SetUpdate(true).OnComplete(() => 
                {
                    gameObject.SetActive(false);
                });
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
        
        private void AnimateButton(Button btn, float delay)
        {
            if (btn == null) return;
            
            btn.transform.localScale = Vector3.zero;
            btn.transform.DOScale(Vector3.one, 0.4f)
                .SetDelay(delay)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }
    }
}
