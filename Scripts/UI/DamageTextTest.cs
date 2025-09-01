using UnityEngine;

public class DamageTextTest : MonoBehaviour
{
    [Header("Test Settings")]
    public KeyCode testKey = KeyCode.Space;
    public int testDamage = 50;
    public bool isCritical = false;
    public bool isHeal = false;
    
    private DamageTextComponent damageTextComponent;
    
    private void Start()
    {
        damageTextComponent = GetComponent<DamageTextComponent>();
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(testKey))
        {
            if (damageTextComponent != null)
            {
                if (isHeal)
                {
                    damageTextComponent.OnHeal(testDamage);
                }
                else
                {
                    damageTextComponent.OnTakeDamage(testDamage, isCritical);
                }
            }
            else
            {
                // 直接マネージャーを使用
                Vector3 position = transform.position + Vector3.up * 0.5f;
                DamageTextManager.ShowDamageAt(position, testDamage, isCritical, isHeal);
            }
        }
    }
    
    [ContextMenu("Test Damage")]
    public void TestDamage()
    {
        Vector3 position = transform.position + Vector3.up * 0.5f;
        DamageTextManager.ShowDamageAt(position, testDamage, isCritical, isHeal);
    }
} 