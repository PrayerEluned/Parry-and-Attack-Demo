using UnityEngine;
using UnityEngine.UI;

public class CurrentSkillSlotUI : MonoBehaviour
{
    [HideInInspector] public int slotIndex;
    [SerializeField] private Image frameImage;
    [SerializeField] private Image iconImage;
    public System.Action<int> OnCurrentSlotClicked;

    private Button button;

    private void Awake()
    {
        Debug.Log("CurrentSkillSlotUI: スキルシステム解禁 - 初期化を開始");
        
        try
        {
            // 基本的なコンポーネント取得
            button = GetComponent<Button>();
            if (button)
            {
                button.onClick.AddListener(OnClick);
                Debug.Log("CurrentSkillSlotUI: ボタンクリックイベント設定完了");
            }
            
            Debug.Log("CurrentSkillSlotUI: Awake完了");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CurrentSkillSlotUI: Awakeエラー - {e.Message}");
        }
    }

    private void Start()
    {
        Debug.Log("CurrentSkillSlotUI: スキルシステム解禁 - Start初期化を開始");
        
        try
        {
            // 基本的なスキルスロットUI初期化
            InitializeSkillSlotUI();
            Debug.Log("CurrentSkillSlotUI: Start初期化完了");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CurrentSkillSlotUI: Start初期化エラー - {e.Message}");
        }
    }
    
    private void InitializeSkillSlotUI()
    {
        // スキルスロットUIの基本初期化
        Debug.Log("CurrentSkillSlotUI: スキルスロットUI初期化");
        
        // 基本的な初期化処理
        if (frameImage != null)
        {
            Debug.Log("CurrentSkillSlotUI: フレームImage確認完了");
        }
        
        if (iconImage != null)
        {
            Debug.Log("CurrentSkillSlotUI: アイコンImage確認完了");
        }
    }

    public void Setup(int index, System.Action<int> onClick)
    {
        slotIndex = index;
        OnCurrentSlotClicked = onClick;
        SetSelected(false);
        SetSkillIcon(null);
        
        Debug.Log($"CurrentSkillSlotUI: Setup完了 - slotIndex={index}");
    }

    private void OnClick()
    {
        Debug.Log($"CurrentSkillSlotUI: クリック - slotIndex={slotIndex}");
        OnCurrentSlotClicked?.Invoke(slotIndex);
    }

    public void SetSelected(bool isSelected)
    {
        if (frameImage != null)
        {
            frameImage.color = isSelected ? Color.yellow : Color.white;
            Debug.Log($"CurrentSkillSlotUI: 選択状態変更 - isSelected={isSelected}");
        }
    }

    public void SetSkillIcon(Sprite sprite)
    {
        if (iconImage == null) return;
        
        iconImage.sprite = sprite;
        iconImage.color = sprite ? Color.white : new Color(1f, 1f, 1f, 0f);
        
        Debug.Log($"CurrentSkillSlotUI: アイコン設定 - sprite={sprite?.name}");
    }
} 