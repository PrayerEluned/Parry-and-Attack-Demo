using UnityEngine;

public class PersistentUI : MonoBehaviour
{
    private static PersistentUI instance;

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