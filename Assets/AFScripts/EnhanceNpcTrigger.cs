using UnityEngine;
using UnityEngine.UI;

public class EnhanceNpcTrigger : MonoBehaviour
{
    public GameObject talkPanel; // Inspectorで会話UIをセット
    public Button closeButton;   // 閉じるボタン
    public GameObject player;    // プレイヤーのGameObject（Inspectorでセット）
    private bool playerInRange = false;

    void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseTalkPanel);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"🔥 OnTriggerEnter2D: {other.tag}");
        
        if (other.CompareTag("Player"))
        {
            Debug.Log("🎯 プレイヤーがNPCのエリアに入りました");
            
            // プレイヤーの移動を停止
            if (player != null)
            {
                Debug.Log("🛑 プレイヤーの移動を停止");
                var moveScript = player.GetComponent<CharacterMovement>();
                if (moveScript != null) moveScript.EnableMovement(false);
            }
            
            // 会話パネルを表示
            if (talkPanel != null)
            {
                Debug.Log("💬 会話パネルを表示します");
                talkPanel.SetActive(true);
                
                // パネルの状態を詳しく確認
                Debug.Log($"TalkPanel状態: activeInHierarchy={talkPanel.activeInHierarchy}, activeSelf={talkPanel.activeSelf}");
                
                // ボタンの状態も確認
                var enhanceButton = talkPanel.transform.Find("EnhanceOpenButton");
                if (enhanceButton != null)
                {
                    var buttonComp = enhanceButton.GetComponent<Button>();
                    Debug.Log($"EnhanceOpenButton状態: active={enhanceButton.gameObject.activeInHierarchy}, interactable={buttonComp?.interactable}");
                }
            }
            else
            {
                Debug.LogError("❌ talkPanel が null です！");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"🔥 OnTriggerExit2D: {other.tag}");
        
        if (other.CompareTag("Player"))
        {
            Debug.Log("🚶 プレイヤーがNPCのエリアから出ました");
            
            // 会話パネルを非表示
            if (talkPanel != null)
            {
                Debug.Log("❌ 会話パネルを非表示");
                talkPanel.SetActive(false);
            }
            
            // プレイヤーの移動を再開
            if (player != null)
            {
                Debug.Log("🏃 プレイヤーの移動を再開");
                var moveScript = player.GetComponent<CharacterMovement>();
                if (moveScript != null) moveScript.EnableMovement(true);
            }
        }
    }

    public void CloseTalkPanel()
    {
        talkPanel.SetActive(false);
        // プレイヤーの動きを再開
        if (player != null)
        {
            var moveScript = player.GetComponent<CharacterMovement>();
            if (moveScript != null) moveScript.EnableMovement(true);
        }
    }
}
