using Fusion;
using UnityEngine;

namespace UI
{
    public class UIController : MonoBehaviour
    {
        public static UIController Instance { get; private set; }

        [Header("Elimination UI")]
        [SerializeField] private GameObject losePanel;
        [SerializeField] private GameObject winPanel;

        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            losePanel?.SetActive(false);

            winPanel?.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
        
        public void ShowEliminated()
        {
            losePanel?.SetActive(true);
        }
        public void ShowWin()
        {
            winPanel?.SetActive(true);
        }
    }
}