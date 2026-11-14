using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public float reloadTime = 1f;
    public float fireRate = 0.15f; //Time between shots 
    public int magSize = 20;

    public GameObject bullet; //the bullet prefab that will spawn
    public Transform bulletSpawnPoint;

    public GameObject weaponFlash; //the flash that will spawn when shooting

    public float recoilDistance = 0.1f; //Distance the gun moves back when shooting
    public float recoilSpeed = 10f; //How fast recoil happens 

    private int currentAmmo;
    private bool isReloading = false;
    private float nextTimeToFire = 0f;

    private Quaternion initialRotation;
    private Vector3 initialPosition;
    private Vector3 reloadRoatationOffset = new Vector3(66, 0, 0); //Rotationg the gun when reloading

    void Start()
    {
        currentAmmo = magSize;
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    public void Shoot()
    {
        if (isReloading) return; //cantr shoot while reloading
        if (Time.time < nextTimeToFire) return;

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        nextTimeToFire = Time.time + fireRate;
        currentAmmo--;

        Instantiate(bullet, bulletSpawnPoint.position, bulletSpawnPoint.rotation); //Creation of a bullet object at the spawn point with the same rotation as the gun
        Instantiate(weaponFlash, bulletSpawnPoint.position, bulletSpawnPoint.rotation); //Creation of a weapon flash object at the spawn point with the same rotation as the gun

        StopCoroutine(nameof(Recoil));
        StartCoroutine(nameof(Recoil));

    }

    IEnumerator Reload()
    {
        isReloading = true;

        Quaternion targetRotation = Quaternion.Euler(initialRotation.eulerAngles + reloadRoatationOffset);
        float halfReload = reloadTime / 2f;
        float t = 0f;

        while (t < halfReload) //Rotate the gun to the target rotation over half of the reload time
        {
            t += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(initialRotation, targetRotation, t / halfReload);
            yield return null; //wait 1 frame before continuing the loop
        }

        t = 0f;

        while (t < halfReload)
        {
            t += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(targetRotation, initialRotation, t / halfReload);
            yield return null;
        }

        currentAmmo = magSize; //sets the current ammo to the max amount after reloading
        isReloading = false;
    }

    public void TryReload()
    {
        if (isReloading) return;
        if (currentAmmo == magSize) return;

        StartCoroutine(Reload());
    }

    private IEnumerator Recoil()
    {
        Vector3 recoilTarget = initialPosition + new Vector3(recoilDistance, 0, 0);
        float t = 0f;

        while (t < 1f) 
        {
            t += Time.deltaTime * recoilSpeed;
            transform.localPosition = Vector3.Lerp(initialPosition, recoilTarget, t);
            yield return null; 
        }

        t= 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * recoilSpeed;
            transform.localPosition = Vector3.Lerp(recoilTarget, initialPosition, t);
            yield return null;
        }

        transform.localPosition = initialPosition; //Ensures the gun is back to its original position after recoil
    }

}
