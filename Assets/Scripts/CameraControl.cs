using UnityEngine;

public class CameraControl : MonoBehaviour
{
    private GameObject player = null;
    public Vector3 offset;

    void Start()
    {
        this.player = GameObject.FindGameObjectWithTag("Player");
        this.offset = this.transform.position - this.player.transform.position;
    }

    void Update()
    {
        this.transform.position = new Vector3(player.transform.position.x + this.offset.x, this.transform.position.y, this.transform.position.z);
    }
}
