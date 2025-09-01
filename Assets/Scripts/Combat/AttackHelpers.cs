using UnityEngine;

/// <summary>
/// SPD から攻撃インターバルを計算する共通関数
/// </summary>
public static class AttackHelpers
{
    private const int SPD_STEP = 16;      // 16 SPD ごとに
    private const float REDUCTION_PER = 0.01f; // 1 % 短縮
    private const float MIN_INTERVAL = 0.2f;   // 下限 0.2 秒

    public static float CalcInterval(float baseInterval, float totalSPD)
    {
        int steps = Mathf.FloorToInt(totalSPD / SPD_STEP);
        float interval = baseInterval * (1f - REDUCTION_PER * steps);
        return Mathf.Max(MIN_INTERVAL, interval);
    }
}
