using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerSystem
{
    /// <summary>
    /// 攻撃ボタンの制御を行うコンポーネント
    /// 0.2秒間のクールダウン機能を提供
    /// </summary>
    public class AttackController : MonoBehaviour
    {
        [Header("Attack Settings")]
        [SerializeField] private float attackCooldown = 0.2f; // 攻撃クールダウン時間（秒）
        [SerializeField] private string attackButtonName = "Fire1"; // 攻撃ボタンの名前
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        
        // プライベート変数
        private float lastAttackTime; // 最後に攻撃した時間
        private bool canAttack = true; // 攻撃可能かどうか
        
        // イベント
        public System.Action OnAttackPerformed; // 攻撃実行時のイベント
        public System.Action OnAttackCooldownStart; // クールダウン開始時のイベント
        public System.Action OnAttackCooldownEnd; // クールダウン終了時のイベント
        
        private void Update()
        {
            // 攻撃ボタンの入力をチェック
            CheckAttackInput();
            
            // クールダウンの更新
            UpdateCooldown();
        }
        
        /// <summary>
        /// 攻撃ボタンの入力をチェック
        /// </summary>
        private void CheckAttackInput()
        {
            // 攻撃ボタンが押されたかチェック
            if (Input.GetButtonDown(attackButtonName) && canAttack)
            {
                PerformAttack();
            }
        }
        
        /// <summary>
        /// 攻撃を実行
        /// </summary>
        private void PerformAttack()
        {
            // 攻撃実行
            lastAttackTime = Time.time;
            canAttack = false;
            
            // 攻撃実行イベントを発火
            OnAttackPerformed?.Invoke();
            
            // クールダウン開始イベントを発火
            OnAttackCooldownStart?.Invoke();
            
            if (showDebugInfo)
            {
                Debug.Log($"AttackController: 攻撃実行 - クールダウン開始 ({attackCooldown}秒)");
            }
        }
        
        /// <summary>
        /// クールダウンの更新
        /// </summary>
        private void UpdateCooldown()
        {
            if (!canAttack)
            {
                // クールダウン時間が経過したかチェック
                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    canAttack = true;
                    
                    // クールダウン終了イベントを発火
                    OnAttackCooldownEnd?.Invoke();
                    
                    if (showDebugInfo)
                    {
                        Debug.Log("AttackController: クールダウン終了 - 攻撃可能");
                    }
                }
            }
        }
        
        /// <summary>
        /// 攻撃可能かどうかを取得
        /// </summary>
        public bool CanAttack()
        {
            return canAttack;
        }
        
        /// <summary>
        /// 残りクールダウン時間を取得
        /// </summary>
        public float GetRemainingCooldown()
        {
            if (canAttack)
            {
                return 0f;
            }
            
            float elapsed = Time.time - lastAttackTime;
            return Mathf.Max(0f, attackCooldown - elapsed);
        }
        
        /// <summary>
        /// クールダウン時間を設定
        /// </summary>
        public void SetCooldown(float cooldown)
        {
            attackCooldown = Mathf.Max(0f, cooldown);
        }
        
        /// <summary>
        /// 攻撃ボタン名を設定
        /// </summary>
        public void SetAttackButton(string buttonName)
        {
            attackButtonName = buttonName;
        }
        
        /// <summary>
        /// クールダウンをリセット（強制的に攻撃可能にする）
        /// </summary>
        public void ResetCooldown()
        {
            canAttack = true;
            lastAttackTime = 0f;
            
            if (showDebugInfo)
            {
                Debug.Log("AttackController: クールダウンリセット");
            }
        }
        
        /// <summary>
        /// デバッグ情報を表示
        /// </summary>
        private void OnGUI()
        {
            if (showDebugInfo)
            {
                GUI.Label(new Rect(10, 10, 300, 20), $"攻撃可能: {canAttack}");
                GUI.Label(new Rect(10, 30, 300, 20), $"残りクールダウン: {GetRemainingCooldown():F2}秒");
            }
        }
    }
}