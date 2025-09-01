using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class WeaponEnhanceItemDisplay : MonoBehaviour
{
    [Header("UI要素")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText; // 説明文用（新規追加）
    [SerializeField] private TMP_Text effectText;       // 効果文用（自動生成されるステータス）
    [SerializeField] private Button button;
    
    [Header("強化値表示用")]
    [SerializeField] private TMP_Text enhancementLevelText; // 強化レベル表示用（例: "+3"）
    [SerializeField] private GameObject enhancementIndicator; // 強化アイコンや背景（オプション）

    [Header("武器データ")]
    private WeaponItem weaponItem;
    private Action<WeaponItem> onWeaponSelected;

    void Awake()
    {
        // ボタンコンポーネントを取得
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError($"❌ {gameObject.name}: Buttonコンポーネントが見つかりません！");
        }
    }
    
    /// <summary>
    /// 武器強化用のセットアップ（WeaponItemDisplayパターンを完全適用）
    /// </summary>
    public void Setup(WeaponItem weapon, Action<WeaponItem> onSelected)
    {
        weaponItem = weapon;
        onWeaponSelected = onSelected;

        Debug.Log($"🔧 WeaponEnhanceItemDisplay: Setup開始 - {weapon?.weaponName}");

        // UI更新
        UpdateDisplay();

        // ボタンイベント設定（WeaponItemDisplayパターン）
        SetupButton();
        
        Debug.Log($"✅ WeaponEnhanceItemDisplay: Setup完了 - {weapon?.weaponName}");
    }

    private void UpdateDisplay()
    {
        if (weaponItem == null) return;

        // アイコン設定
        if (iconImage != null && weaponItem.icon != null)
        {
            iconImage.sprite = weaponItem.icon;
            Debug.Log($"アイコン設定完了: {weaponItem.weaponName}");
        }

        // 名前表示
        if (nameText != null)
        {
            nameText.text = weaponItem.weaponName;
            Debug.Log($"名前設定完了: {weaponItem.weaponName}");
        }

        // 説明文は固定のテキストを表示
        if (descriptionText != null)
        {
            descriptionText.text = weaponItem.description;
            Debug.Log($"説明文設定完了: {weaponItem.weaponName}");
        }

        // 効果テキスト設定（自動生成されるステータス）
        if (effectText != null)
        {
            effectText.text = GenerateEffectText(weaponItem);
            Debug.Log($"効果テキスト設定完了: {weaponItem.weaponName}");
        }
        
        // 強化レベル表示を更新
        UpdateEnhancementDisplay();
    }

    private void SetupButton()
    {
        // Buttonコンポーネントを再取得（Awakeで取得できなかった場合の対策）
        if (button == null)
        {
            button = GetComponent<Button>();
            Debug.Log($"Button再取得: {button != null}");
        }
        
        if (button == null) 
        {
            Debug.LogError($"WeaponEnhanceItemDisplay: buttonがnullです - GameObject: {gameObject.name}");
            return;
        }

        Debug.Log($"🔧 WeaponEnhanceItemDisplay: ボタン設定開始 - {weaponItem?.weaponName}");
        
        // ボタンの基本状態を強制設定
        button.enabled = true;
        button.interactable = true;
        
        // Image raycastTarget確認
        var image = GetComponent<Image>();
        if (image != null)
        {
            image.raycastTarget = true;
            Debug.Log($"Image raycastTarget設定: {image.raycastTarget}");
        }
        
        // 親のCanvasGroup確認
        var canvasGroup = GetComponentInParent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            Debug.Log($"親CanvasGroup設定: interactable={canvasGroup.interactable}, blocksRaycasts={canvasGroup.blocksRaycasts}");
        }
        
        // WeaponItemDisplayパターンを完全適用
        button.onClick.RemoveAllListeners(); // 既存のリスナーをクリア
        button.onClick.AddListener(() => {
            Debug.Log($"🎯🎯🎯 ラムダ式でクリック検出！ {weaponItem?.weaponName} 🎯🎯🎯");
            OnClickSelectWeapon();
        });
        
        // テスト用の直接クリックも追加
        button.onClick.AddListener(TestDirectClick);
        
        Debug.Log($"✅ WeaponEnhanceItemDisplay: ボタン設定完了 - {weaponItem?.weaponName}");
        Debug.Log($"  - Button enabled: {button.enabled}");
        Debug.Log($"  - Button interactable: {button.interactable}");
        Debug.Log($"  - GameObject active: {gameObject.activeInHierarchy}");
        Debug.Log($"  - リスナー数: {button.onClick.GetPersistentEventCount()}");
    }
    
    /// <summary>
    /// テスト用直接クリック検出
    /// </summary>
    private void TestDirectClick()
    {
        Debug.Log($"🔥🔥🔥 TestDirectClick発火！ {weaponItem?.weaponName} 🔥🔥🔥");
    }
    
    /// <summary>
    /// WeaponItemDisplayパターンのクリック処理
    /// </summary>
    private void OnClickSelectWeapon()
    {
        Debug.Log($"🎯🎯🎯 OnClickSelectWeapon: {weaponItem?.weaponName} がクリックされました！🎯🎯🎯");
        
        if (onWeaponSelected != null && weaponItem != null)
        {
            Debug.Log($"コールバック実行: {weaponItem.weaponName}");
            onWeaponSelected.Invoke(weaponItem);
        }
        else
        {
            Debug.LogError("onWeaponSelected または weaponItem が null です");
        }
    }

    /// <summary>
    /// キーボード選択時の詳細遷移用
    /// </summary>
    public void OnClickSelectWeaponByKeyboard()
    {
        Debug.Log($"[キーボード選択] OnClickSelectWeaponByKeyboard: {weaponItem?.weaponName}");
        OnClickSelectWeapon();
    }

    /// <summary>
    /// ハイライト表示切替
    /// </summary>
    public void SetHighlight(bool isHighlighted)
    {
        if (iconImage != null)
        {
            iconImage.color = isHighlighted ? Color.yellow : Color.white;
        }
        if (nameText != null)
        {
            nameText.color = isHighlighted ? Color.yellow : Color.white;
        }
        // 必要に応じて他のUI要素も色変更
    }

    /// <summary>
    /// 強化レベル表示を更新
    /// </summary>
    private void UpdateEnhancementDisplay()
    {
        Debug.Log($"[UpdateEnhancementDisplay] 開始: {weaponItem?.weaponName}");
        Debug.Log($"  - GameObject.activeInHierarchy: {gameObject.activeInHierarchy}");
        Debug.Log($"  - GameObject.activeSelf: {gameObject.activeSelf}");
        
        // WeaponEnhanceProcessorから強化レベルを取得
        var enhanceProcessor = FindObjectOfType<WeaponEnhanceProcessor>();
        int enhanceLevel = 0;
        
        if (enhanceProcessor != null && weaponItem != null)
        {
            enhanceLevel = enhanceProcessor.GetWeaponEnhanceLevel(weaponItem);
        }
        
        Debug.Log($"  - 強化レベル: {enhanceLevel}");
        Debug.Log($"  - enhancementLevelText != null: {enhancementLevelText != null}");
        
        // 強化レベルテキスト表示
        if (enhancementLevelText != null)
        {
            Debug.Log($"  - enhancementLevelText.gameObject.activeInHierarchy: {enhancementLevelText.gameObject.activeInHierarchy}");
            
            if (enhanceLevel > 0)
            {
                enhancementLevelText.text = $"+{enhanceLevel}";
                enhancementLevelText.color = new Color(0.412f, 0.690f, 1.0f, 1.0f); // 強化済みは#69B0FF
                enhancementLevelText.enabled = true; // テキストコンポーネントを有効化
                Debug.Log($"  - 強化レベルテキスト設定: +{enhanceLevel}");
            }
            else
            {
                enhancementLevelText.text = ""; // 空テキストに設定
                var transparentColor = new Color(0.412f, 0.690f, 1.0f, 0f); // #69B0FFで透明
                enhancementLevelText.color = transparentColor;
                Debug.Log($"  - 強化レベルテキスト非表示（透明化）");
                // enhancementLevelText.enabled = false; // コメントアウト：enabledを使わず透明度で制御
            }
        }
        
        // 強化インジケーター表示（オプション）
        if (enhancementIndicator != null)
        {
            // 安全性チェック：enhancementIndicatorが自分自身のGameObjectでないことを確認
            if (enhancementIndicator == gameObject)
            {
                Debug.LogError($"[重要] WeaponEnhanceItemDisplay: enhancementIndicatorが自分自身のGameObjectに設定されています！無限ループを防ぐためスキップします。武器: {weaponItem?.weaponName}");
                Debug.LogError("[解決方法] Prefabのインスペクターで enhancementIndicator を None または子オブジェクトに設定してください");
                // enhancementIndicatorの制御を完全にスキップ
            }
            else
            {
                bool shouldShow = enhanceLevel > 0;
                enhancementIndicator.SetActive(shouldShow);
                Debug.Log($"  - enhancementIndicator.SetActive: {shouldShow} (GameObject: {enhancementIndicator.name})");
            }
        }
        else
        {
            Debug.Log($"  - enhancementIndicator is null (設定されていません)");
        }
        
        Debug.Log($"[UpdateEnhancementDisplay] 完了: {weaponItem?.weaponName}");
        Debug.Log($"  - 最終GameObject.activeInHierarchy: {gameObject.activeInHierarchy}");
    }
    
    /// <summary>
    /// 強化表示を手動で更新（外部から呼び出し可能）
    /// </summary>
    public void RefreshEnhancementDisplay()
    {
        UpdateEnhancementDisplay();
        
        // 効果文も再生成
        if (effectText != null && weaponItem != null)
        {
            effectText.text = GenerateEffectText(weaponItem);
        }
    }
    
    /// <summary>
    /// 効果文を自動生成（ATK + 15(+6)形式）
    /// </summary>
    private string GenerateEffectText(WeaponItem weapon)
    {
        var effectParts = new System.Collections.Generic.List<string>();
        
        // WeaponEnhanceProcessorから強化レベルを取得
        var enhanceProcessor = FindObjectOfType<WeaponEnhanceProcessor>();
        int enhanceLevel = 0;
        
        if (enhanceProcessor != null && weapon != null)
        {
            enhanceLevel = enhanceProcessor.GetWeaponEnhanceLevel(weapon);
        }
        
        // 攻撃力（基礎値または強化値が0より大きい場合のみ表示）
        if (weapon.bonusAttack > 0 || (weapon.enhanceAttack > 0 && enhanceLevel > 0))
        {
            float baseValue = weapon.bonusAttack;
            float enhanceValue = weapon.enhanceAttack * enhanceLevel;
            string attackText = $"ATK +{baseValue:F0}";
            if (enhanceValue > 0)
                attackText += $"(+{enhanceValue:F0})";
            effectParts.Add(attackText);
        }
        
        // HP（基礎値または強化値が0より大きい場合のみ表示）
        if (weapon.bonusHP > 0 || (weapon.enhanceHP > 0 && enhanceLevel > 0))
        {
            float baseValue = weapon.bonusHP;
            float enhanceValue = weapon.enhanceHP * enhanceLevel;
            string hpText = $"HP +{baseValue:F0}";
            if (enhanceValue > 0)
                hpText += $"(+{enhanceValue:F0})";
            effectParts.Add(hpText);
        }
        
        // 防御力（基礎値または強化値が0より大きい場合のみ表示）
        if (weapon.bonusDefense > 0 || (weapon.enhanceDefense > 0 && enhanceLevel > 0))
        {
            float baseValue = weapon.bonusDefense;
            float enhanceValue = weapon.enhanceDefense * enhanceLevel;
            string defenseText = $"DEF +{baseValue:F0}";
            if (enhanceValue > 0)
                defenseText += $"(+{enhanceValue:F0})";
            effectParts.Add(defenseText);
        }
        
        // 魔法攻撃力（基礎値または強化値が0より大きい場合のみ表示）
        if (weapon.bonusMagicAttack > 0 || (weapon.enhanceMagicAttack > 0 && enhanceLevel > 0))
        {
            float baseValue = weapon.bonusMagicAttack;
            float enhanceValue = weapon.enhanceMagicAttack * enhanceLevel;
            string magicAttackText = $"MAT +{baseValue:F0}";
            if (enhanceValue > 0)
                magicAttackText += $"(+{enhanceValue:F0})";
            effectParts.Add(magicAttackText);
        }
        
        // 魔法防御力（基礎値または強化値が0より大きい場合のみ表示）
        if (weapon.bonusMagicDefense > 0 || (weapon.enhanceMagicDefense > 0 && enhanceLevel > 0))
        {
            float baseValue = weapon.bonusMagicDefense;
            float enhanceValue = weapon.enhanceMagicDefense * enhanceLevel;
            string magicDefenseText = $"MDF +{baseValue:F0}";
            if (enhanceValue > 0)
                magicDefenseText += $"(+{enhanceValue:F0})";
            effectParts.Add(magicDefenseText);
        }
        
        // 運（基礎値または強化値が0より大きい場合のみ表示）
        if (weapon.bonusFate > 0 || (weapon.enhanceFate > 0 && enhanceLevel > 0))
        {
            float baseValue = weapon.bonusFate;
            float enhanceValue = weapon.enhanceFate * enhanceLevel;
            string fateText = $"運 +{baseValue:F0}";
            if (enhanceValue > 0)
                fateText += $"(+{enhanceValue:F0})";
            effectParts.Add(fateText);
        }
        
        // 効果がない場合
        if (effectParts.Count == 0)
        {
            return "効果なし";
        }
        
        // 空白で結合（改行ではなく空白区切り）
        return string.Join(" ", effectParts);
    }

    /// <summary>
    /// デバッグ用：設定確認
    /// </summary>
    [ContextMenu("設定確認")]
    public void DebugSettings()
    {
        Debug.Log($"=== WeaponEnhanceItemDisplay 設定確認 ===\\n武器: {(weaponItem != null ? weaponItem.weaponName : "null")}\\nアイコン: {(iconImage != null ? "OK" : "null")}\\n名前テキスト: {(nameText != null ? "OK" : "null")}\\n効果テキスト: {(effectText != null ? "OK" : "null")}\\nボタン: {(button != null ? "OK" : "null")}");
    }
} 