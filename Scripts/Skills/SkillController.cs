using UnityEngine;
using System.Collections;
using AudioSystem;

public class SkillController : MonoBehaviour
{
    [Header("Skill Slots (SO)")]
    public SkillData[] slots = new SkillData[3];
    [Header("References")]
    [SerializeField] private BasicAttackController basicAttackController;
    private AudioSystem.AudioManager audioManager;
    [SerializeField] private Transform skillSpawnOrigin;

    private float[] cooldownTimers = new float[3];
    private bool isCasting = false;
    private ISkillBehaviour currentSkill;

    private void Awake()
    {
        if (!basicAttackController) basicAttackController = GetComponent<BasicAttackController>();
        audioManager = AudioSystem.AudioManager.Instance;
        if (!skillSpawnOrigin) skillSpawnOrigin = transform;
        
        // 初期化結果のログ出力
        if (basicAttackController == null)
        {
            Debug.LogWarning("SkillController: basicAttackControllerが見つかりませんでした");
        }
        else
        {
            Debug.Log("SkillController: basicAttackControllerを正常に取得しました");
        }
    }

    private void Update()
    {
        // Debug.Log("SkillController: スキルシステム解禁テスト - Update処理を開始");
        
        try
        {
            // 軽量なUpdate処理のみ実行
            UpdateSkillCooldowns();
            HandleSkillInput();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SkillController: Update エラー - {e.Message}");
        }
    }
    
    private void UpdateSkillCooldowns()
    {
        // スキルクールダウンの更新
        for (int i = 0; i < cooldownTimers.Length; i++)
        {
            if (cooldownTimers[i] > 0)
            {
                cooldownTimers[i] -= Time.deltaTime;
            }
        }
    }
    
    private void HandleSkillInput()
    {
        // 基本的なスキル入力処理
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TryUseSkill(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TryUseSkill(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TryUseSkill(2);
        }
    }

    public bool TryUseSkill(int slotIndex)
    {
        // Debug.Log($"TryUseSkill called: slotIndex={slotIndex}");
        if (isCasting || slotIndex < 0 || slotIndex >= slots.Length) return false;
        var data = slots[slotIndex];
        if (cooldownTimers[slotIndex] > 0f) return false;
        // Debug.Log($"Skill {data.skillName} prefab={data.skillPrefab}");
        if (data.skillPrefab == null)
        {
            // Debug.Log("NoneSkill: 発動処理なし");
            return true;
        }
        var skillObj = Instantiate(data.skillPrefab, skillSpawnOrigin.position, skillSpawnOrigin.rotation, skillSpawnOrigin);
        var skill = skillObj.GetComponent<ISkillBehaviour>();
        if (skill == null)
        {
            // Debug.Log("NoneSkill: 発動処理なし(ISkillBehaviour未実装)" );
            Destroy(skillObj);
            return true;
        }
        // Debug.Log($"Skill Init called for {data.skillName}");
        skill.Init(gameObject, data);
        StartCoroutine(CastSkillRoutine(slotIndex, skill, data, skillObj));
        return true;
    }

    private IEnumerator CastSkillRoutine(int slotIndex, ISkillBehaviour skill, SkillData data, GameObject skillObj)
    {
        isCasting = true;
        
        // basicAttackControllerのnullチェック
        if (basicAttackController != null)
        {
            basicAttackController.SetEnabled(false);
        }
        else
        {
            Debug.LogWarning("SkillController: basicAttackControllerがnullです");
        }
        
        cooldownTimers[slotIndex] = data.cooldown;
        if (data.castSE && audioManager) audioManager.PlaySE(data.castSE);
        float duration = skill.Duration;
        yield return new WaitForSeconds(duration);
        if (skillObj) Destroy(skillObj);
        isCasting = false;
        
        // basicAttackControllerのnullチェック
        if (basicAttackController != null)
        {
            basicAttackController.SetEnabled(true);
        }
    }

    public float GetCooldownRatio(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length || slots[slotIndex] == null) return 0f;
        return Mathf.Clamp01(cooldownTimers[slotIndex] / slots[slotIndex].cooldown);
    }

    public float GetRemainCooldown(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return 0f;
        return Mathf.Max(0f, cooldownTimers[slotIndex]);
    }

    public void UseSkill0() { TryUseSkill(0); }
    public void UseSkill1() { TryUseSkill(1); }
    public void UseSkill2() { TryUseSkill(2); }

    public void SetSkill(int slotIndex, SkillData skill)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return;
        slots[slotIndex] = skill;
    }
} 