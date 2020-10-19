using UnityEngine;
using System.Collections;

public class OniEmitterControl : MonoBehaviour
{
    public GameObject[] oni_prefab;

    public int oni_num = 2;

    private GameObject last_created_oni = null;
    private const float collision_radius = 1.0f;

    public bool is_enable_hit_sound = true;

    void Start()
    {

    }
    
    void Update()
    {
        do
        {
            if(this.oni_num <= 0)
            {
                break;
            }

            // 上一个生成的离开一定距离后再生成下一个，以免在生成点互相碰撞
            if(this.last_created_oni != null)
            {
                if(Vector3.Distance(this.transform.position, last_created_oni.transform.position) <= OniEmitterControl.collision_radius * 2.0f)
                {
                    break;
                }
            }

            Vector3 initial_position = this.transform.position;

            initial_position.y += Random.Range(-0.5f, 0.5f);
            initial_position.z += Random.Range(-0.5f, 0.5f);

            Quaternion initial_rotation;

            initial_rotation = Quaternion.identity;
            initial_rotation *= Quaternion.AngleAxis(this.oni_num * 50, Vector3.forward);
            initial_rotation *= Quaternion.AngleAxis(this.oni_num * 30, Vector3.right);

            GameObject oni = Instantiate(this.oni_prefab[this.oni_num%2], initial_position, initial_rotation) as GameObject;

            oni.GetComponent<Rigidbody>().velocity = Vector3.down * 1.0f;
            oni.GetComponent<Rigidbody>().angularVelocity = initial_rotation * Vector3.forward * 5.0f * (this.oni_num % 3);
            oni.GetComponent<OniStillBodyControl>().emitter_control = this;

            this.last_created_oni = oni;

            this.oni_num--;

        } while (false);
    }

    public void PlayHitSound()
    {

    }
}
