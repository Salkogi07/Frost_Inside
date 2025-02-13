using UnityEngine;

public class MainCameraMove : MonoBehaviour
{
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private float offSetX = 0f;
    [SerializeField]
    private float offSetY = 0f;
    [SerializeField]
    private float offSetZ = -10f;

    Vector3 cameraPosition;

    private void Update()
    {
        cameraPosition.x = player.transform.position.x + offSetX;
        cameraPosition.y = player.transform.position.y + offSetY;
        cameraPosition.z = player.transform.position.z + offSetZ;

        transform.position = cameraPosition;
    }
}
