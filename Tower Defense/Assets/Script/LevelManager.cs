using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    // Fungsi untuk Singleton
    private static LevelManager _instance = null;
    public static LevelManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LevelManager> ();
            }

            return _instance;
        }
    }

    [SerializeField] private int _maxLives;
    [SerializeField] private int _totalEnemy;

    [SerializeField] private GameObject panel;
    [SerializeField] private Text statusInfo;
    [SerializeField] private Text _livesInfo;
    [SerializeField] private Text totalEnemyInfo;

    [SerializeField] private Tower[] towerPrefabs;
    [SerializeField] private Enemy[] enemyPrefabs;
    [SerializeField] private Transform towerUIParent;
    [SerializeField] private GameObject towerUIPrefab;

    [SerializeField] private Transform[] enemyPaths;
    [SerializeField] private float spawnDelay = 5f;

    private List<Tower> spawnedTowers = new List<Tower> ();
    private List<Enemy> spawnedEnemies = new List<Enemy> ();
    private List<Bullet> spawnedBullets = new List<Bullet> ();

    private int _currentLives;
    private int _enemyCounter;
    private float runningSpawnDelay;
    

    public bool IsOver { get; private set; }

    private void Start ()
    {
        SetCurrentLives (_maxLives);
        SetTotalEnemy (_totalEnemy);

        InstantiateAllTowerUI ();
    }

    private void Update ()
    {
        // Jika menekan tombol R, fungsi restart akan terpanggil
        if (Input.GetKeyDown (KeyCode.R))
        {
            SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
        }

        if (IsOver)
        {
            return;
        }


        // Ini adalah Counter untuk spawn enemy dalam jeda waktu yang ditentukan
        // Time.unscaledDeltaTime adalah deltaTime yang independent, tidak terpengaruh oleh apapun kecuali game object itu sendiri,
        // jadi bisa digunakan sebagai penghitung waktu
        runningSpawnDelay -= Time.unscaledDeltaTime;
        if (runningSpawnDelay <= 0f)
        {
            SpawnEnemy ();
            runningSpawnDelay = spawnDelay;
        }

        foreach (Tower tower in spawnedTowers)
        {
            tower.CheckNearestEnemy (spawnedEnemies);
            tower.SeekTarget ();
            tower.ShootTarget ();
        }

        foreach (Enemy enemy in spawnedEnemies)
        {
            if (!enemy.gameObject.activeSelf)
            {
                continue;
            }

            // Kenapa nilainya 0.1? Karena untuk lebih mentoleransi perbedaan posisi,
            // akan terlalu sulit jika perbedaan posisinya harus 0 atau sama persis
            if (Vector2.Distance (enemy.transform.position, enemy.TargetPosition) < 0.1f)
            {
                enemy.SetCurrentPathIndex (enemy.CurrentPathIndex + 1);
                if (enemy.CurrentPathIndex < enemyPaths.Length)
                {
                    enemy.SetTargetPosition (enemyPaths[enemy.CurrentPathIndex].position);
                }
                else
                {
                    ReduceLives (1);
                    enemy.gameObject.SetActive (false);
                }
            }
            else
            {
                enemy.MoveToTarget ();
            }
        }
    }

    private void InstantiateAllTowerUI ()
    {
        foreach (Tower tower in towerPrefabs)
        {
            GameObject newTowerUIObj = Instantiate (towerUIPrefab.gameObject, towerUIParent);
            TowerUI newTowerUI = newTowerUIObj.GetComponent<TowerUI> ();

            newTowerUI.SetTowerPrefab (tower);
            newTowerUI.transform.name = tower.name;
        }
    }

    public void RegisterSpawnedTower (Tower tower)
    {
        spawnedTowers.Add (tower);
    }

    private void SpawnEnemy ()
    {
        SetTotalEnemy (--_enemyCounter);
        if (_enemyCounter < 0)
        {
            bool isAllEnemyDestroyed = spawnedEnemies.Find (e => e.gameObject.activeSelf) == null;
            if (isAllEnemyDestroyed)
            {
                SetGameOver (true);
            }

            return;
        }

        int randomIndex = Random.Range (0, enemyPrefabs.Length);
        string enemyIndexString = (randomIndex + 1).ToString ();

        GameObject newEnemyObj = spawnedEnemies.Find (
            e => !e.gameObject.activeSelf && e.name.Contains (enemyIndexString))?.gameObject;

        if (newEnemyObj == null)
        {
            newEnemyObj = Instantiate (enemyPrefabs[randomIndex].gameObject);
        }

        Enemy newEnemy = newEnemyObj.GetComponent<Enemy> ();
        if (!spawnedEnemies.Contains (newEnemy))
        {
            spawnedEnemies.Add (newEnemy);
        }

        newEnemy.transform.position = enemyPaths[0].position;
        newEnemy.SetTargetPosition (enemyPaths[1].position);
        newEnemy.SetCurrentPathIndex (1);
        newEnemy.gameObject.SetActive (true);
    }

    public Bullet GetBulletFromPool (Bullet prefab)
    {
        GameObject newBulletObj = spawnedBullets.Find (
            b => !b.gameObject.activeSelf && b.name.Contains (prefab.name)
        )?.gameObject;

        if (newBulletObj == null)
        {
            newBulletObj = Instantiate (prefab.gameObject);
        }

        Bullet newBullet = newBulletObj.GetComponent<Bullet> ();
        if (!spawnedBullets.Contains (newBullet))
        {
            spawnedBullets.Add (newBullet);
        }

        return newBullet;
    }

    public void ExplodeAt (Vector2 point, float radius, int damage)
    {
        foreach (Enemy enemy in spawnedEnemies)
        {
            if (enemy.gameObject.activeSelf)
            {
                if (Vector2.Distance (enemy.transform.position, point) <= radius)
                {
                    enemy.ReduceEnemyHealth (damage);
                }
            }
        }
    }

    public void ReduceLives (int value)
    {
        SetCurrentLives (_currentLives - value);
        if (_currentLives <= 0)
        {
            SetGameOver (false);
        }
    }

    public void SetCurrentLives (int currentLives)
    {
        // Mathf.Max fungsi nya adalah mengambil angka terbesar
        // sehingga _currentLives di sini tidak akan lebih kecil dari 0
        _currentLives = Mathf.Max (currentLives, 0);
        _livesInfo.text = $"Lives: {_currentLives}";
    }

    public void SetTotalEnemy (int totalEnemy)
    {
        _enemyCounter = totalEnemy;
        totalEnemyInfo.text = $"Total Enemy: {Mathf.Max (_enemyCounter, 0)}";
    }

    public void SetGameOver (bool isWin)
    {
        IsOver = true;
        statusInfo.text = isWin ? "You Win!" : "You Lose!";
        panel.gameObject.SetActive (true);
    }

    // Untuk menampilkan garis penghubung dalam window Scene
    // tanpa harus di-Play terlebih dahulu
    private void OnDrawGizmos ()
    {
        for (int i = 0; i < enemyPaths.Length - 1; i++)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine (enemyPaths[i].position, enemyPaths[i + 1].position);
        }
    }
}