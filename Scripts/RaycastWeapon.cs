using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastWeapon : MonoBehaviour
{
    class Bullet
	{
        public float time;
        public Vector3 initialPosition;
        public Vector3 initialVelocity;
	}
    public bool isFiring = false;
    public int fireRate = 25;
    public float damage = 10f;
    public float bulletSpeed = 1000.0f;
    public float bulletDrop = 0.0f;
    public ParticleSystem[] muzzleFlash;
    public ParticleSystem hitEffect;
    public Transform raycastOrigin;
    public Transform raycastDestination;

    Ray ray;
    RaycastHit hitInfo;
    float accumulatedTime;
    List<Bullet> bullets = new List<Bullet>();
    float maxBulletLifeTime = 3.0f;

    Vector3 GetPosition (Bullet bullet)
	{
        Vector3 gravity = Vector3.down * bulletDrop;
        return (bullet.initialPosition) + (bullet.initialVelocity * bullet.time) + (0.5f * gravity * bullet.time * bullet.time);
	}

    Bullet CreateBullet (Vector3 position, Vector3 velocity)
	{
        Bullet bullet = new Bullet();
        bullet.initialPosition = position;
        bullet.initialVelocity = velocity;
        bullet.time = 0.0f;
        return bullet;
	}
    public void StartFiring()
	{
        isFiring = true;
        accumulatedTime = 0.0f;
        FireBullet();
    }
    public void StopFiring()
    {
        isFiring = false;
    }

    public void UpdateFiring(float deltaTime)
	{
        accumulatedTime += deltaTime;
        float fireInterval = 1.0f/fireRate;
        while (accumulatedTime >= 0.0f)
		{
            FireBullet();
            accumulatedTime -= fireInterval;
		}
	}

    public void UpdateBullets(float deltaTime)
	{
        SimulateBullets(deltaTime);
        DestroyBullets();

    }

    void SimulateBullets(float deltaTime)
    {
        bullets.ForEach(bullet =>
        {
            Vector3 p0 = GetPosition(bullet);
            bullet.time += deltaTime;
            Vector3 p1 = GetPosition(bullet);
            RaycastSegment(p0, p1, bullet);
        });
    }

    void DestroyBullets()
	{
        bullets.RemoveAll(bullet => bullet.time >= maxBulletLifeTime);
	}

    void RaycastSegment(Vector3 start, Vector3 end, Bullet bullet)
	{
        Vector3 direction = end - start;
        float distance = direction.magnitude;
        ray.origin = start;
        ray.direction = direction;
        if (Physics.Raycast(ray, out hitInfo, distance))
        {
           //Debug.DrawLine(ray.origin, hitInfo.point, Color.red, 1.0f);
           hitEffect.transform.position = hitInfo.point;
           hitEffect.transform.forward = hitInfo.normal;
           hitEffect.Emit(1);

            bullet.time = maxBulletLifeTime;
        }
    }
    private void FireBullet()
	{
        foreach (var particle in muzzleFlash)
        {
            particle.Emit(1);
        }
        Vector3 velocity = (raycastDestination.position - raycastOrigin.position).normalized * bulletSpeed;
        var bullet = CreateBullet(raycastOrigin.position, velocity);
        bullets.Add(bullet);
    }
}
