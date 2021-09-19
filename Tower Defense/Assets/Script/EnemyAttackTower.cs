using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackTower : MonoBehaviour
{
    // Komponen Enemy
    [SerializeField] private SpriteRenderer enemyObject;

    // Enemy Properties
    [SerializeField] private int shootPower = 1;
    [SerializeField] private float shootDistance = 1f;
    [SerializeField] private float shootDelay = 5f;
    [SerializeField] private float bulletSpeed = 1f;
    [SerializeField] private float bulletSplashRadius = 0f;
    [SerializeField] private Bullet bulletPrefab;

    private float runningShootDelay;
    private Tower targetTower;
    private Quaternion targetRotation;
    public Vector2? PlacePosition { get; private set; }

    // Fungsi cek musuh terdekat
    public void CheckNearestTower (List<Tower> towers)
    {
        if (targetTower != null)
        {
            if (!targetTower.gameObject.activeSelf || Vector3.Distance (transform.position, targetTower.transform.position) > shootDistance)
            {
                targetTower = null;
            }
            else
            {
                return;
            }
        }

        float nearDistance = Mathf.Infinity;
        Tower nearTower = null;

        foreach (Tower tower in towers)
        {
            float distance = Vector3.Distance (transform.position, tower.transform.position);
            if (distance > shootDistance)
            {
                continue;
            }

            if (distance < nearDistance)
            {
                nearDistance = distance;
                nearTower = tower;
            }
        }
        targetTower = nearTower;
    }

    // Fungsi Menembak musuh yang telah disimpan sebagai target
    public void ShootTargetTower ()
    {
        if (targetTower == null)
        {
            return;
        }

        runningShootDelay -= Time.unscaledDeltaTime;
        if (runningShootDelay <= 0f)
        {
            bool headHasAimed = Mathf.Abs (enemyObject.transform.rotation.eulerAngles.z - targetRotation.eulerAngles.z) < 10f;
            if (!headHasAimed)
            {
                return;
            }

            Bullet bullet = LevelManager.Instance.GetBulletFromPool (bulletPrefab);
            bullet.transform.position = transform.position;
            bullet.SetProperties (shootPower, bulletSpeed, bulletSplashRadius);
            bullet.SetTargetTower (targetTower);
            bullet.gameObject.SetActive (true);

            runningShootDelay = shootDelay;
        }
    }

    // Fungsi untuk membuat enemy selalu melihat ke arah musuh
    public void SeekTargetTower ()
    {
        if (targetTower == null)
        {
            return;
        }

        Vector3 direction = targetTower.transform.position - transform.position;
        float targetAngle = Mathf.Atan2 (direction.y, direction.x) * Mathf.Rad2Deg;
        targetRotation = Quaternion.Euler (new Vector3 (0f, 0f, targetAngle - 90f));
        enemyObject.transform.rotation = Quaternion.RotateTowards (enemyObject.transform.rotation, targetRotation, Time.deltaTime * 180f);
    }



    public void SetPlacePosition(Vector2? newPosition)
    {
        PlacePosition = newPosition;
    }

    public void LockPlacement ()
    {
        transform.position = (Vector2) PlacePosition;
    }

    // Mengubah order in layer pada tower yang sedang di drag
    public void ToggleOrderInLayer (bool toFront)
    {
        int orderInLayer = toFront ? 2 : 0;
        enemyObject.sortingOrder = orderInLayer;
    }


    // Fungsi untuk mengambil sprite pada Tower Head
    public Sprite GetEnemyIcon ()
    {
        return enemyObject.sprite;
    }
}
