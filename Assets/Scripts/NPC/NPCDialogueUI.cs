using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace NPCSystem
{
    /// <summary>
    /// NPC会話用UIコンポーネント
    /// </summary>
    public class NPCDialogueUI : MonoBehaviour
    {
        [Header("UI要素")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Image npcPortrait;
        
        [Header("アニメーション設定")]
        [SerializeField] private float textSpeed = 0.05f;
        [SerializeField] private bool useTypewriterEffect = true;
        
        [Header("UI設定")]
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private Color npcNameColor = Color.yellow;
        
        // イベント
        public event Action OnNextClicked;
        public event Action OnCloseClicked;
        
        // タイプライター効果用
        private string fullText = "";
        private string currentText = "";
        private float textTimer = 0f;
        private bool isTyping = false;
        
        private void Start()
        {
            InitializeUI();
        }
        
        /// <summary>
        /// UIの初期化
        /// </summary>
        private void InitializeUI()
        {
            // ボタンの設定
            if (nextButton != null)
            {
                nextButton.onClick.AddListener(OnNextButtonClicked);
            }
            
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            }
            
            // 初期状態でパネルを非表示
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }
            
            // テキスト色の設定
            if (dialogueText != null)
            {
                dialogueText.color = textColor;
            }
        }
        
        private void Update()
        {
            // タイプライター効果の更新
            if (isTyping)
            {
                UpdateTypewriterEffect();
            }
        }
        
        /// <summary>
        /// 会話テキストを表示
        /// </summary>
        public void DisplayDialogueText(string text, string npcName = "")
        {
            if (dialogueText == null) return;
            
            fullText = text;
            
            if (useTypewriterEffect)
            {
                // タイプライター効果で表示
                StartTypewriterEffect();
            }
            else
            {
                // 即座に表示
                dialogueText.text = text;
            }
        }
        
        /// <summary>
        /// タイプライター効果を開始
        /// </summary>
        private void StartTypewriterEffect()
        {
            currentText = "";
            textTimer = 0f;
            isTyping = true;
            
            // 次のボタンを無効化
            if (nextButton != null)
            {
                nextButton.interactable = false;
            }
        }
        
        /// <summary>
        /// タイプライター効果の更新
        /// </summary>
        private void UpdateTypewriterEffect()
        {
            textTimer += Time.deltaTime;
            
            if (textTimer >= textSpeed)
            {
                textTimer = 0f;
                
                if (currentText.Length < fullText.Length)
                {
                    currentText = fullText.Substring(0, currentText.Length + 1);
                    dialogueText.text = currentText;
                }
                else
                {
                    // タイプライター効果完了
                    isTyping = false;
                    
                    // 次のボタンを有効化
                    if (nextButton != null)
                    {
                        nextButton.interactable = true;
                    }
                }
            }
        }
        
        /// <summary>
        /// 会話パネルを表示
        /// </summary>
        public void ShowPanel()
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
            }
        }
        
        /// <summary>
        /// 会話パネルを非表示
        /// </summary>
        public void HidePanel()
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }
        }
        
        /// <summary>
        /// 次のボタンがクリックされた時の処理
        /// </summary>
        private void OnNextButtonClicked()
        {
            if (isTyping)
            {
                // タイプライター効果中は即座に完了
                CompleteTypewriterEffect();
            }
            else
            {
                // イベントを発火
                OnNextClicked?.Invoke();
            }
        }
        
        /// <summary>
        /// 閉じるボタンがクリックされた時の処理
        /// </summary>
        private void OnCloseButtonClicked()
        {
            // イベントを発火
            OnCloseClicked?.Invoke();
        }
        
        /// <summary>
        /// タイプライター効果を即座に完了
        /// </summary>
        private void CompleteTypewriterEffect()
        {
            currentText = fullText;
            dialogueText.text = currentText;
            isTyping = false;
            
            if (nextButton != null)
            {
                nextButton.interactable = true;
            }
        }
        
        /// <summary>
        /// NPCのポートレートを設定
        /// </summary>
        public void SetNPCPortrait(Sprite portrait)
        {
            if (npcPortrait != null && portrait != null)
            {
                npcPortrait.sprite = portrait;
            }
        }
        
        /// <summary>
        /// テキスト色を設定
        /// </summary>
        public void SetTextColor(Color color)
        {
            if (dialogueText != null)
            {
                dialogueText.color = color;
            }
        }
        
        /// <summary>
        /// テキスト表示速度を設定
        /// </summary>
        public void SetTextSpeed(float speed)
        {
            textSpeed = Mathf.Max(0.01f, speed);
        }
        
        /// <summary>
        /// タイプライター効果の有効/無効を設定
        /// </summary>
        public void SetTypewriterEffect(bool enabled)
        {
            useTypewriterEffect = enabled;
        }
    }
} 