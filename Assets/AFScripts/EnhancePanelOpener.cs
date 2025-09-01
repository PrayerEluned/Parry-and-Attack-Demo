using UnityEngine;
using UnityEngine.UI;

public class EnhancePanelOpener : MonoBehaviour
{
    [Header("パネル参照")]
    public GameObject talkPanel;      // 会話パネル
    public GameObject enhancePanel;   // 武器選択パネル
    
    void Start()
    {
        Debug.Log("EnhancePanelOpener: Start()開始");
        
        // パネル参照の確認
        Debug.Log($"talkPanel参照: {talkPanel != null}");
        Debug.Log($"enhancePanel参照: {enhancePanel != null}");
    }
    
    /// <summary>
    /// 会話パネルから武器強化パネルを開く
    /// </summary>
    public void OpenEnhancePanel()
    {
        Debug.Log("🔥🔥🔥 EnhancePanelOpener: OpenEnhancePanel ボタンがクリックされました！🔥🔥🔥");
        Debug.Log($"talkPanel: {talkPanel != null} (isActive: {talkPanel?.activeInHierarchy})");
        Debug.Log($"enhancePanel: {enhancePanel != null} (isActive: {enhancePanel?.activeInHierarchy})");
        
        if (talkPanel != null) 
        {
            Debug.Log("talkPanelを閉じます");
            talkPanel.SetActive(false);      // 会話パネルを閉じる
        }
            
        if (enhancePanel != null) 
        {
            Debug.Log("enhancePanelを開きます");
            enhancePanel.SetActive(true);    // 強化パネルを開く
            Debug.Log($"enhancePanel開いた後の状態: {enhancePanel.activeInHierarchy}");
        }
        else
        {
            Debug.LogError("enhancePanelがnullです！");
        }
    }
    
    /// <summary>
    /// 武器強化パネルを閉じて、プレイヤーの動きを再開
    /// </summary>
    public void CloseEnhancePanel()
    {
        Debug.Log("🚪 CloseEnhancePanel: 強化パネルを閉じます");
        
        if (enhancePanel != null) 
        {
            Debug.Log("enhancePanelを閉じます");
            enhancePanel.SetActive(false);   // 強化パネルを閉じる
        }
            
        // プレイヤーの動きを再開
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var moveScript = player.GetComponent<CharacterMovement>();
            if (moveScript != null) 
            {
                Debug.Log("プレイヤーの動きを再開します");
                moveScript.EnableMovement(true);
            }
        }
    }
}