using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private int powerBullet;
    private float speedBullet;
    private float splashRadiusBullet;

    private Enemy targetEnemy;
    private Tower targetTower;

    // FixedUpdate adalah update yang lebih konsisten jeda pemanggilannya. cocok digunakan jika karakter memiliki Physic (Rigidbody, dll)
    private void FixedUpdate ()
    {
        if (LevelManager.Instance.IsOver)
        {
            return;
        }

        if (targetEnemy != null)
        {
            if (!targetEnemy.gameObject.activeSelf)
            {
                gameObject.SetActive (false);
                targetEnemy = null;
                return;
            }

            transform.position = Vector3.MoveTowards (transform.position, targetEnemy.transform.position, speedBullet * Time.fixedDeltaTime);

            Vector3 direction = targetEnemy.transform.position - transform.position;
            float targetAngle = Mathf.Atan2 (direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler (new Vector3 (0f, 0f, targetAngle - 90f));
        }
    }

    private void OnTriggerEnter2D (Collider2D collision)
    {
        if (targetEnemy == null)
        {
            return;
        }

        if (collision.gameObject.Equals (targetEnemy.gameObject))
        {
            gameObject.SetActive (false);

            // Bullet splash area
            if (splashRadiusBullet > 0f)
            {
                LevelManager.Instance.ExplodeAt (transform.position, splashRadiusBullet, powerBullet);
            }
            // Bullet single target
            else
            {
                targetEnemy.ReduceEnemyHealth (powerBullet);
            }

            targetEnemy = null;
        }
    }

    public void SetProperties (int bulletPower, float bulletSpeed, float bulletSplashRadius)
    {
        powerBullet = bulletPower;
        speedBullet = bulletSpeed;
        splashRadiusBullet = bulletSplashRadius;
    }

    public void SetTargetEnemy (Enemy enemy)
    {
        targetEnemy = enemy;
    }

    public void SetTargetTower (Tower tower)
    {
        targetTower = tower;
    }
}