using UnityEngine;

public class AttackGaugeFollow : MonoBehaviour
{
    [SerializeField] private Transform target; // Playerをアサイン
    [SerializeField] private Vector3 offset = new Vector3(0, -0.8f, 0); // 真下に表示

    void LateUpdate()
    {
        if (target)
            transform.position = target.position + offset;
    }
} 