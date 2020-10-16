using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OniControl : MonoBehaviour
{
    public PlayerControl player = null;

    public GameObject main_camera = null;

    private Vector3 initial_position;
    
    public const float collision_size = 1.0f;

    private bool is_alive = true;

    enum STATE
    {
        NONE = -1,
        RUN = 0,
        DEFEATED,
        NUM
    }

    private STATE state = STATE.NONE;
    private STATE next_state = STATE.NONE;

    private float step_time = 0.0f;

    private Vector3 blowout_vector = Vector3.zero;
    private Vector3 blowout_angular_velocity = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        this.initial_position = this.transform.position;

        this.transform.rotation = Quaternion.AngleAxis(180.0f, Vector3.up);

        // this.GetComponent<Collider>().enabled = false;
        this.GetComponent<Rigidbody>().maxAngularVelocity = float.PositiveInfinity;
    }

    // Update is called once per frame
    void Update()
    {
        this.step_time += Time.deltaTime;

        if(this.state == STATE.NONE)
        {
            this.next_state = STATE.RUN;
        }

        if(next_state != STATE.NONE)
        {
            switch (this.next_state)
            {
                case STATE.DEFEATED:
                {
                    this.GetComponent<Rigidbody>().velocity = this.blowout_vector;
                    this.GetComponent<Rigidbody>().angularVelocity = this.blowout_angular_velocity;

                    this.transform.parent = this.main_camera.transform;
                    this.is_alive = false;
                }
                break;
            }

            this.state = this.next_state;
            this.next_state = STATE.NONE;
            this.step_time = 0.0f;
        }

        Vector3 new_position = this.transform.position;
        float low_limit = this.initial_position.y;

        switch (this.state)
        {
            case STATE.RUN:
            {
                if (new_position.y < low_limit)
                {
                    new_position.y = low_limit;
                }
            }
            break;
            case STATE.DEFEATED:
            {
                // 死后的短时间内可能会陷入地面中，速度朝上（＝死后的瞬间）时，让其不落入地面中
				if(new_position.y < low_limit) {
	
					if(this.GetComponent<Rigidbody>().velocity.y > 0.0f) {
	
						new_position.y = low_limit;
					}
				}

                if(this.transform.parent != null)
                {
                    this.GetComponent<Rigidbody>().velocity += -3.0f * Vector3.right * Time.deltaTime;
                }
            }
            break;
        }

        this.transform.position = new_position;

        if(!this.GetComponent<Renderer>().isVisible && !this.is_alive)
        {
            Destroy(this.gameObject);
        }
    }

    public void AttackedFromPlayer(Vector3 blowout, Vector3 angular_velocity)
    {
        this.blowout_vector = blowout;
        this.blowout_angular_velocity = angular_velocity;

        this.transform.parent = null;

        this.next_state = STATE.DEFEATED;
    }
}
