using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorControl : MonoBehaviour
{
    private GameObject main_camera = null;
    private Vector3 initial_position;

    public const float WIDTH = 10.0f * 4.0f;
    public const int MODEL_NUM = 3;

    // Start is called before the first frame update
    void Start()
    {
        this.main_camera = GameObject.FindGameObjectWithTag("MainCamera");
        this.initial_position = this.transform.position;
        // this.GetComponent<Renderer>().enabled = SceneControl.IS_DRAW_DEBUG_FLOOR_MODEL;
    }

    // Update is called once per frame
    void Update()
    {
#if false
        float total_width = FloorControl.WIDTH * FloorControl.MODEL_NUM;
        Vector3 floor_position = this.transform.position;
        Vector3 camera_position = this.main_camera.transform.position;

        if (floor_position.x + total_width/2.0f < camera_position.x)
        {
            floor_position.x += total_width;
            this.transform.position = floor_position;
        }

        if (floor_position.x - total_width/2.0f > camera_position.x)
        {
            floor_position.x -= total_width;
            this.transform.position = floor_position;
        }
#else
        float total_width = FloorControl.WIDTH * FloorControl.MODEL_NUM;
        Vector3 camera_position = this.main_camera.transform.position;

        float dist = camera_position.x - this.initial_position.x;

        int n = Mathf.RoundToInt(dist / total_width);

        Vector3 position = this.initial_position;

        position.x += n * total_width;

        this.transform.position = position;
#endif
    }
}
