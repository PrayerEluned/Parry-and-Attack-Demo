using UnityEngine;

public class PlayerSingleton : MonoBehaviour
{
    private static PlayerSingleton instance;

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