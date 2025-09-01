using UnityEngine;
public static class DamageCalculator
{
    public static int CalculatePhysicalDamage(float atk, float def)
    {
        float baseDmg = (atk / 2f) - (def / 4f);
        float rand = Random.Range(0.9f, 1.1f);
        return Mathf.Max(Mathf.RoundToInt(baseDmg * rand), 1);
    }
}
