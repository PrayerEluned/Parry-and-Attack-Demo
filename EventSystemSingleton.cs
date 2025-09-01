using UnityEngine;

public class EventSystemSingleton : MonoBehaviour
{
    private static EventSystemSingleton instance;

    void Awake()
    {
        // シングルトンパターンは不要になったのでコメントアウト
        // if (instance == null)
        // {
        //     instance = this;
        // }
        // else
        // {
        //     Destroy(gameObject);
        // }
    }
} 