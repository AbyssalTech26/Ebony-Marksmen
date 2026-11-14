using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public Gun gun; //Reference to the gun script
    private bool isHoldingShoot = false; //Flag when player is holding shoot button

    //Void methods refer to input actions in the PlayerInput action editor. Done in order.
    void OnShoot()
    {
        isHoldingShoot = true;
    }

    void OnShootRelease()
    {
        isHoldingShoot = false;
    }

    void OnReload()
    {
        if (gun != null) // if player has a gun, call the TryReload method in the Gun script
        {
            gun.TryReload();
        }
    }

    void Update()
    {
        if (isHoldingShoot && gun != null)
        {
            gun.Shoot();
        }
    }
}
