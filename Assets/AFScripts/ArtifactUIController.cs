using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class ArtifactUIController : MonoBehaviour
{
    [Header("UIプレハブと表示先")]
    [SerializeField] private GameObject artifactItemPrefab;    // 1つ分の表示プレハブ
    [SerializeField] private Transform artifactListParent;     // ScrollViewのContent

    [Header("参照するインベントリ")]
    [SerializeField] public ArtifactInventory artifactInventory;

    private void OnEnable()
    {
        artifactInventory = ArtifactInventory.Instance;
        Debug.Log($"[ArtifactUIController] OnEnable: artifactInventory={artifactInventory?.GetInstanceID()} (null? {artifactInventory == null})");
        if (artifactItemPrefab == null)
        {
            artifactItemPrefab = Resources.Load<GameObject>("ArtifactItemDisplay");
            Debug.LogWarning("ArtifactUIController: artifactItemPrefabがInspector未設定のためResourcesから取得: " + (artifactItemPrefab != null));
        }
        if (artifactListParent == null)
        {
            artifactListParent = transform;
            Debug.LogWarning("ArtifactUIController: artifactListParentがInspector未設定のため自身のTransformを使用");
        }
        if (artifactItemPrefab == null || artifactListParent == null || artifactInventory == null)
        {
            Debug.LogWarning("ArtifactUIController: 必須参照が揃っていません。UI更新をスキップします。");
            return;
        }
        Debug.Log($"[ArtifactUIController] OnEnable: OnInventoryChangedイベント購読");
        artifactInventory.OnInventoryChanged += RefreshArtifactList;
        RefreshArtifactList();
    }

    private void OnDisable()
    {
        if (artifactInventory != null)
        {
            Debug.Log($"[ArtifactUIController] OnDisable: OnInventoryChangedイベント解除");
            artifactInventory.OnInventoryChanged -= RefreshArtifactList;
        }
    }

    public void RefreshArtifactList()
    {
        Debug.Log($"[ArtifactUIController] RefreshArtifactList: artifactInventory={artifactInventory?.GetInstanceID()} (null? {artifactInventory == null})");
        if (artifactItemPrefab == null)
        {
            Debug.LogError("artifactItemPrefabがInspectorに設定されていません！");
            return;
        }
        
        // 既存の子オブジェクトを安全に削除
        while (artifactListParent.childCount > 0)
        {
            Transform child = artifactListParent.GetChild(0);
            if (child != null && child.gameObject != null)
            {
                child.SetParent(null);
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
        
        if (artifactInventory == null)
        {
            Debug.LogError("artifactInventoryがnullです。UI更新できません。");
            return;
        }
        // AF（ArtifactItem）を表示
        var afList = artifactInventory.GetAllOwnedArtifacts();
        Debug.Log($"[ArtifactUIController] UI更新: 所持AF数={afList.Count}");
        foreach (var af in afList)
        {
            Debug.Log($"[ArtifactUIController] AF: {af.artifactName}");
            GameObject go = Instantiate(artifactItemPrefab, artifactListParent);
            Image icon = go.transform.Find("IconFrame/Icon")?.GetComponent<Image>();
            TextMeshProUGUI nameText = go.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI descriptionText = go.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI countText = go.transform.Find("CountText")?.GetComponent<TextMeshProUGUI>();
            if (icon == null || nameText == null || descriptionText == null || countText == null)
            {
                Debug.LogError("ArtifactDisplayのUI部品が不足しています");
                continue;
            }
            icon.sprite = af.icon;
            nameText.text = af.artifactName;
            descriptionText.text = af.description;
            int count = artifactInventory.GetArtifactCount(af);
            countText.text = $"{count} / {af.maxStackCount}";
        }
    }
}
