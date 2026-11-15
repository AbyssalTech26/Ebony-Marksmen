using UnityEngine;

public class PickUp : MonoBehaviour
{
    public Material highlightMaterial;
    private Material[] originMaterials; //where original materials of gun will be stored 
    private MeshRenderer[] meshRenderers;

    public GameObject weaponPrefab; // Reference to the gun prefab to instantiate
    public float lookRange = 3f;

    private bool isLookedAt = false;
    private Camera playerCam;
    private PlayerShooting player;

    void Start()
    {
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        originMaterials = new Material[meshRenderers.Length];
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            originMaterials[i] = meshRenderers[i].material; //How all materials will be looked through and set 
        }

        player = FindAnyObjectByType<PlayerShooting>();//FindObjectOfType
        playerCam = player.GetComponentInChildren<Camera>();
    }


    void Update()
    {
        Ray ray = new Ray(playerCam.transform.position, playerCam.transform.forward); //Ray is shot from player pov 
        if (Physics.Raycast(ray, out RaycastHit hit, lookRange))
        {
            if (hit.collider.GetComponentInParent<PickUp>() == this)
            {
                if (!isLookedAt)
                    SetLookedAt(true);

                return;
            }
        }

        if (isLookedAt)
            SetLookedAt(false);
    }

    void SetLookedAt(bool lookedAt)
    {
        isLookedAt = lookedAt;

        if (lookedAt)
        {
            foreach (MeshRenderer mr in meshRenderers)
            {
                mr.material = highlightMaterial;
            } //When looking at smt, it will loop thru all meshRenderes and set mateiral to be the highlightMaterial(blue)
        }
        else
        {
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].material = originMaterials[i];
            } //set back to old materials 
        }
    }

    public void OnPickUp()
    {
        if (!isLookedAt) return;

        player.OnDrop(); //Check if player has gun, If u have one, drop it before pickign up new one 

        GameObject newWeapon = Instantiate(weaponPrefab, player.gunHolder);
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;

        player.gun = newWeapon.GetComponent<Gun>();

        Destroy(gameObject);
    }
}
