using UnityEngine;

public class GameBootstrapper : MonoBehaviour
{
    public GameObject artifactInventoryPrefab;
    public GameObject playerPrefab;
    public GameObject cameraPrefab;
    public GameObject eventSystemPrefab;
    public GameObject uiRootPrefab;
    public GameObject areaManagerPrefab; // エリアマネージャーのプレハブ

    void Awake()
    {
        // Unity 6でのフリーズ対策：GameBootstrapper機能を完全に無効化
        Debug.Log("GameBootstrapper: Unity 6フリーズ対策により、自動初期化は無効化されています");
        return;
        
        // フリーズ対策：FindObjectOfType完全削除、Singletonパターンを活用
        // Debug.Log("GameBootstrapper: 起動開始 - フリーズ対策版");

        // 1. ArtifactInventory（Singletonチェック）
        if (ArtifactInventory.Instance == null && artifactInventoryPrefab != null)
        {
            var artifactInventoryInstance = Instantiate(artifactInventoryPrefab);
            artifactInventoryInstance.SetActive(true);
            // Debug.Log("GameBootstrapper: ArtifactInventoryをインスタンス化してアクティブ化しました");
        }
        else if (ArtifactInventory.Instance != null)
        {
            // Debug.Log("GameBootstrapper: ArtifactInventoryは既に存在します");
        }

        // 2. UIRoot（コンポーネント名で直接チェック）
        if (GameObject.Find("PersistentUI") == null && uiRootPrefab != null)
        {
            var uiRootInstance = Instantiate(uiRootPrefab);
            uiRootInstance.SetActive(true);
            // Debug.Log("GameBootstrapper: UIRootをインスタンス化してアクティブ化しました");
        }
        else
        {
            // Debug.Log("GameBootstrapper: UIRootは既に存在します");
        }

        // 3. Player（Singletonチェック）
        if (PlayerStats.Instance == null && playerPrefab != null)
        {
            var playerInstance = Instantiate(playerPrefab);
            playerInstance.SetActive(true);
            // Debug.Log("GameBootstrapper: Playerをインスタンス化してアクティブ化しました");
        }
        else if (PlayerStats.Instance != null)
        {
            // Debug.Log("GameBootstrapper: Playerは既に存在します");
        }

        // 4. Camera（オブジェクト名で直接チェック）
        if (GameObject.Find("CameraSingleton") == null && cameraPrefab != null)
        {
            var cameraInstance = Instantiate(cameraPrefab);
            cameraInstance.SetActive(true);
            // Debug.Log("GameBootstrapper: Cameraをインスタンス化してアクティブ化しました");
        }
        else
        {
            // Debug.Log("GameBootstrapper: Cameraは既に存在します");
        }

        // 5. AreaManager（オブジェクト名で直接チェック）- エリア機能は無効化
        // if (GameObject.Find("AreaManager") == null && areaManagerPrefab != null)
        // {
        //     var areaManagerInstance = Instantiate(areaManagerPrefab);
        //     areaManagerInstance.SetActive(true);
        //     Debug.Log("GameBootstrapper: AreaManagerをインスタンス化してアクティブ化しました");
        // }
        // else
        // {
        //     Debug.Log("GameBootstrapper: AreaManagerは既に存在します");
        // }
        Debug.Log("GameBootstrapper: AreaManager機能は無効化されています");

        // 6. EventSystem（オブジェクト名で直接チェック）
        if (GameObject.Find("EventSystemSingleton") == null && eventSystemPrefab != null)
        {
            var eventSystemInstance = Instantiate(eventSystemPrefab);
            eventSystemInstance.SetActive(true);
            // Debug.Log("GameBootstrapper: EventSystemをインスタンス化してアクティブ化しました");
        }
        else
        {
            // Debug.Log("GameBootstrapper: EventSystemは既に存在します");
        }

        // Debug.Log("GameBootstrapper: 初期化完了");
    }
} 