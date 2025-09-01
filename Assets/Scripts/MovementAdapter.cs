using UnityEngine;

/// <summary>
/// 既存のCharacterMovementと他のシステムを連携させるためのアダプター
/// </summary>
[RequireComponent(typeof(CharacterMovement))]
public class MovementAdapter : MonoBehaviour
{
    private CharacterMovement characterMovement;

    private void Awake()
    {
        characterMovement = GetComponent<CharacterMovement>();
    }

    // 必要に応じて、ここにCharacterMovementを操作するためのパブリックメソッドを実装します。
    // 例えば、移動を一時的に無効にするなど。
    public void SetMovement(bool canMove)
    {
        if (characterMovement != null)
        {
            characterMovement.CanMove = canMove;
        }
    }
} 