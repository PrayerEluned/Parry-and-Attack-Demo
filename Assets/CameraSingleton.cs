using UnityEngine;

public class CameraSingleton : MonoBehaviour
{
    private static CameraSingleton instance;

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