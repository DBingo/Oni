using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public SceneControl scene_control = null;
    public float run_speed = 5.0f;
    public const float RUN_SPEED_MAX = 20.0f;
    protected const float run_speed_add = 5.0f;
    protected const float run_speed_sub = 5.0f * 4.0f;
    protected const float MISS_GRAVITY = 9.8f * 2.0f;
    protected bool is_running = true;
    protected bool is_contact_floor = false;
    protected bool is_playable = true;
    protected AttackColliderControl attack_collider = null;
    protected float attack_timer = 0.0f;
    protected float attack_disable_timer = 0.0f;
    protected const float ATTACK_TIME = 0.3f;
    protected const float ATTACK_DISABLE_TIME = 1.0f;
    public enum STATE
    {
        NONE = -1,
        RUN = 0,
        STOP,
        MISS,
        NUM,
    }
    public STATE state = STATE.NONE;
    public STATE next_state = STATE.NONE;
    void Start()
    {
        this.attack_collider = GameObject.FindGameObjectWithTag("AttackCollider").GetComponent<AttackColliderControl>();
        this.attack_collider.player = this;


        // 开始
        this.run_speed = 0.0f;
        this.next_state = STATE.RUN;
    }

    void Update()
    {
        // 判断是否需要变换状态
        if (this.next_state == STATE.NONE)
        {
            switch (this.state)
            {
                case STATE.RUN:
                    {
                        if (!this.is_running && this.run_speed <= 0.0f)
                        {
                            // TODO 停止播放声音和跑步特效

                            this.next_state = STATE.STOP;
                        }
                    }
                    break;

                case STATE.MISS:
                    {
                        if (this.is_contact_floor)
                        {
                            // TODO 再次播放跑步特效

                            this.GetComponent<Rigidbody>().useGravity = true;
                            this.next_state = STATE.RUN;
                        }
                    }
                    break;
            }
        }

        // 状态变化切换前的一些逻辑
        if (this.next_state != STATE.NONE)
        {
            switch (this.next_state)
            {
                case STATE.STOP:
                    {
                        // TODO 播放停止动画
                    }
                    break;

                case STATE.MISS:
                    {
                        Vector3 velocity = this.GetComponent<Rigidbody>().velocity;

                        float jump_height = 1.0f;

                        velocity.x = -2.5f;
                        velocity.y = Mathf.Sqrt(MISS_GRAVITY * jump_height);
                        velocity.z = 0.0f;

                        this.GetComponent<Rigidbody>().velocity = velocity;
                        this.GetComponent<Rigidbody>().useGravity = false;

                        this.run_speed = 0.0f;

                        // TODO 播放碰撞后退动画
                        // TODO 播放碰撞后退声音
                        // TODO 停止奔跑粒子特效
                    }
                    break;
            }

            this.state = this.next_state;
            this.next_state = STATE.NONE;
        }

        // 根据当前状态进行对应逻辑
        switch (this.state)
        {
            case STATE.RUN:
                {
                    if (this.is_running)
                    {
                        this.run_speed += PlayerControl.run_speed_add * Time.deltaTime;
                    }
                    else
                    {
                        this.run_speed -= PlayerControl.run_speed_sub * Time.deltaTime;
                    }
                    this.run_speed = Mathf.Clamp(this.run_speed, 0.0f, PlayerControl.RUN_SPEED_MAX);

                    Vector3 new_velocity = this.GetComponent<Rigidbody>().velocity;

                    new_velocity.x = run_speed;

                    if (new_velocity.y > 0.0f)
                    {
                        new_velocity.y = 0.0f;
                    }

                    this.GetComponent<Rigidbody>().velocity = new_velocity;

                    // 攻击
                    this.attack_control();

#if UNITY_EDITOR
                    // ---------------------------------------------------- //
                    // 根据是否能进行攻击改变颜色（用于调试）
                    if (this.attack_disable_timer > 0.0f)
                    {
                        // 攻击中
                        if (this.attack_timer > 0.0f)
                        {
                            this.GetComponent<Renderer>().material.color = Color.red;
                        }
                        // 无法攻击
                        else
                        {
                            this.GetComponent<Renderer>().material.color = Color.gray;
                        }
                    }
                    else
                    {
                        this.GetComponent<Renderer>().material.color = Color.Lerp(Color.white, Color.blue, 0.5f);
                    }

                    // ---------------------------------------------------- //
                    // 通过“W”键向前方大幅移动（用于调试）
                    if (Input.GetKeyDown(KeyCode.W))
                    {
                        Vector3 position = this.transform.position;
                        position.x += 100.0f * FloorControl.WIDTH * FloorControl.MODEL_NUM;
                        this.transform.position = position;
                    }
#endif
                }
                break;
            case STATE.MISS:
                {
                    this.GetComponent<Rigidbody>().velocity += Vector3.down * MISS_GRAVITY * Time.deltaTime;
                }
                break;
        }

        this.is_contact_floor = false;
    }

    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.tag == "OniGroup")
        {
            if (this.attack_timer <= 0.0f && this.state != STATE.MISS)
            {
                this.next_state = STATE.MISS;

                // 和怪物碰撞的处理

                this.scene_control.OnPlayerMissed();
            }
        }

        if (other.gameObject.tag == "Floor")
        {
            if (other.relativeVelocity.y >= Physics.gravity.y * Time.deltaTime)
            {
                this.is_contact_floor = true;
            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        this.OnCollisionStay(other);
    }

    public bool IsStopped()
    {
        if (this.is_running || this.run_speed > 0.0f)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void SetPlayable(bool playable)
    {
        this.is_playable = playable;
    }

    private void attack_control()
    {
        if (!this.is_playable)
        {
            return;
        }

        if (this.attack_timer > 0.0f)
        {
            this.attack_timer -= Time.deltaTime;

            if (this.attack_timer <= 0.0f)
            {
                attack_collider.SetAttack(false);
            }
        }
        else
        {
            this.attack_disable_timer -= Time.deltaTime;
            if (this.attack_disable_timer <= 0.0f)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    attack_collider.SetAttack(true);

                    this.attack_timer = PlayerControl.ATTACK_TIME;
                    this.attack_disable_timer = PlayerControl.ATTACK_DISABLE_TIME;

                    // TODO 播放动作、特效、声音
                }
            }
        }

    }

    public float GetSpeedRate()
    {
        float player_speed_rate = Mathf.InverseLerp(0.0f, PlayerControl.RUN_SPEED_MAX, this.GetComponent<Rigidbody>().velocity.magnitude);
        return player_speed_rate;
    }

    public float GetAttackTimer()
    {
        return (PlayerControl.ATTACK_TIME - this.attack_timer);
    }

    public void StopRequest()
    {
        this.is_running = false;
    }
}
