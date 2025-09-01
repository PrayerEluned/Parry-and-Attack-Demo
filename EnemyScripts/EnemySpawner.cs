using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class EnemySpawner : MonoBehaviour
{
    [Header("基本設定")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private int maxEnemies = 5;
    [SerializeField] private float spawnInterval = 5f;

    [Header("出現範囲")]
    [Tooltip("敵が出現する範囲を示すタイルマップ")]
    [SerializeField] private Tilemap spawnAreaTilemap;

    private GameObject player;
    private int currentEnemies = 0;
    private List<Vector3Int> spawnableTiles;

    private void Start()
    {
        try
        {
            InitializeSpawner();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"EnemySpawner: 初期化エラー - {e.Message}");
        }
    }
    
    private void InitializeSpawner()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("EnemySpawner: プレイヤーが見つかりません！", this);
                return;
            }
        }
        
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("EnemySpawner: 敵プレハブが設定されていません！", this);
            return;
        }

        CacheSpawnableTiles();
        StartSpawning();
    }

    private void CacheSpawnableTiles()
    {
        if (spawnAreaTilemap == null)
        {
            Debug.LogError("EnemySpawner: 出現範囲のTilemapがインスペクターで設定されていません！", this);
            return;
        }

        spawnableTiles = new List<Vector3Int>();
        foreach (var pos in spawnAreaTilemap.cellBounds.allPositionsWithin)
        {
            if (spawnAreaTilemap.HasTile(pos))
            {
                spawnableTiles.Add(pos);
            }
        }
        
        if (spawnableTiles.Count == 0)
        {
            Debug.LogWarning("EnemySpawner: 出現範囲のTilemapにタイルが1つもありません。", this);
        }
        else
        {
            Debug.Log($"EnemySpawner: {spawnableTiles.Count}個の出現可能タイルをキャッシュしました。");
        }
    }
    
    private void StartSpawning()
    {
        // 基本的なスポーン処理を開始
        if (spawnInterval > 0)
        {
            InvokeRepeating(nameof(SpawnEnemy), 1f, spawnInterval); // 1秒後に開始
            Debug.Log($"EnemySpawner: スポーン処理開始 - 間隔: {spawnInterval}秒");
        }
        else
        {
            Debug.LogError("EnemySpawner: spawnIntervalが0以下です！");
        }
    }
    
    private void SpawnEnemy()
    {
        try
        {
            // 最大数チェック
            if (currentEnemies >= maxEnemies)
            {
                return;
            }
            
            // 軽量な敵スポーン処理
            if (enemyPrefabs != null && enemyPrefabs.Length > 0 && player != null)
            {
                // ランダムな敵を選択
                int randomIndex = Random.Range(0, enemyPrefabs.Length);
                GameObject enemyPrefab = enemyPrefabs[randomIndex];
                
                if (enemyPrefab == null)
                {
                    Debug.LogError($"EnemySpawner: 選択された敵プレハブ[{randomIndex}]がnullです！");
                    return;
                }
                
                // プレイヤーから一定距離離れた場所にスポーン
                Vector3 spawnPosition = GetRandomSpawnPosition();
                
                // 敵を生成
                GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
                currentEnemies++;
                
                // 敵の初期化
                var enemyHealth = enemy.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    // EnemyStatsを取得（EnemyHealthのdefaultStatsから取得）
                    var enemyStats = enemyHealth.DefaultStats;
                    if (enemyStats != null)
                    {
                        enemyHealth.Initialize(enemyStats, this);
                    }
                    else
                    {
                        Debug.LogWarning("EnemySpawner: EnemyHealthのdefaultStatsが設定されていません");
                    }
                }
                else
                {
                    Debug.LogWarning("EnemySpawner: EnemyHealthコンポーネントが見つかりません");
                }
            }
            else
            {
                Debug.LogError("EnemySpawner: 必要な参照が不足しています！");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"EnemySpawner: スポーンエラー - {e.Message}");
        }
    }
    
    private Vector3 GetRandomSpawnPosition()
    {
        if (spawnableTiles == null || spawnableTiles.Count == 0)
        {
            Debug.LogError("EnemySpawner: 出現可能なタイルがありません！スポナーの位置を代わりに使用します。", this);
            return transform.position;
        }

        Vector3Int randomCellPosition = spawnableTiles[Random.Range(0, spawnableTiles.Count)];
        Vector3 spawnPosition = spawnAreaTilemap.GetCellCenterWorld(randomCellPosition);
        
        return spawnPosition;
    }
    
    public void NotifyEnemyDied()
    {
        currentEnemies--;
    }

    private void Update()
    {
        // テスト用：Tキーで手動スポーン
        if (Input.GetKeyDown(KeyCode.T))
        {
            SpawnEnemy();
        }
    }
    
    private void OnDrawGizmos()
    {
        // プレイヤーがいる場合のみ描画
        if (player == null) return;
        
        Vector3 playerPos = player.transform.position;
        
        // プレイヤー位置を緑で表示
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(playerPos, 0.5f);
        
        // 最小スポーン距離を黄色で表示
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(playerPos, 0.5f);
        
        // 最大スポーン距離を赤で表示
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(playerPos, 0.5f);
        
        // 現在生きている敵の位置を青で表示
        EnemyHealth[] enemies = Object.FindObjectsOfType<EnemyHealth>();
        Gizmos.color = Color.blue;
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                Gizmos.DrawWireSphere(enemy.transform.position, 0.3f);
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // ギズモの色を緑に設定
        Gizmos.color = Color.green;

        // spawnAreaMinとspawnAreaMaxから中心とサイズを計算
        Vector3 center = new Vector3(
            (spawnAreaTilemap.cellBounds.min.x + spawnAreaTilemap.cellBounds.max.x) / 2,
            (spawnAreaTilemap.cellBounds.min.y + spawnAreaTilemap.cellBounds.max.y) / 2,
            transform.position.z
        );

        Vector3 size = new Vector3(
            spawnAreaTilemap.cellBounds.max.x - spawnAreaTilemap.cellBounds.min.x,
            spawnAreaTilemap.cellBounds.max.y - spawnAreaTilemap.cellBounds.min.y,
            0
        );

        // 計算した中心とサイズでワイヤーフレームのキューブを描画
        Gizmos.DrawWireCube(center, size);
    }
}
