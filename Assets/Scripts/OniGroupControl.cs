using UnityEngine;

public class OniGroupControl : MonoBehaviour
{
    public PlayerControl player = null;

    public GameObject main_camera = null;

    public SceneControl scene_control = null;

    public GameObject[] OniPrefab;

    public OniControl[] onis;

    public static float collision_size = 2.0f;
    private int oni_num;
    static private int oni_num_max = 0;

    public float run_speed = SPEED_MIN;

    public enum TYPE
    {
        NONE = -1,
        NORMAL = 0,
        DECELERATE,
        LEAVE,
        NUM,
    };
    public TYPE type = TYPE.NORMAL;

    public struct Decelerate
    {
        public bool is_active;
        public float speed_base;
        public float timer;
    }

    public Decelerate decelerate;

    public static float SPEED_MIN = 2.0f;
    public static float SPEED_MAX = 10.0f;
    public static float LEAVE_SPEED = 10.0f;

    // Start is called before the first frame update
    void Start()
    {
        this.decelerate.is_active = false;
        this.decelerate.timer = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        this.speed_control();

        this.transform.rotation = Quaternion.identity;

        if (this.type == TYPE.LEAVE)
        {
            this.DestroyWhenInVisible();
        }
    }

    private void FixedUpdate()
    {
        Vector3 new_position = this.transform.position;
        new_position.x += this.run_speed * Time.deltaTime;
        this.transform.position = new_position;
    }

    private void speed_control()
    {
        switch (this.type)
        {
            case TYPE.DECELERATE:
                {
                    const float decelerate_start = 8.0f;
                    if (this.decelerate.is_active)
                    {
                        float rate;
                        const float time0 = 0.7f;
                        const float time1 = 0.4f;
                        const float time2 = 2.0f;

                        const float speed_max = 30.0f;
                        float speed_min = OniGroupControl.SPEED_MIN;

                        float time = this.decelerate.timer;

                        do
                        {
                            if (time < time0)
                            {
                                rate = Mathf.Clamp01(time / time0);
                                rate = (Mathf.Sin(Mathf.Lerp(-Mathf.PI / 2.0f, Mathf.PI / 2.0f, rate)) + 1.0f) / 2.0f;

                                this.run_speed = Mathf.Lerp(this.decelerate.speed_base, speed_max, rate);

                                //this.set_oni_motion_speed(2.0f);
                                break;
                            }

                            time -= time0;

                            if (time < time1)
                            {
                                rate = Mathf.Clamp01(time / time1);
                                rate = (Mathf.Sin(Mathf.Lerp(-Mathf.PI / 2.0f, Mathf.PI / 2.0f, rate)) + 1.0f) / 2.0f;

                                this.run_speed = Mathf.Lerp(speed_max, PlayerControl.RUN_SPEED_MAX, rate);
                                break;
                            }

                            time -= time1;

                            if (time < time2)
                            {
                                rate = Mathf.Clamp01(time / time2);
                                rate = (Mathf.Sin(Mathf.Lerp(-Mathf.PI / 2.0f, Mathf.PI / 2.0f, rate)) + 1.0f) / 2.0f;

                                this.run_speed = Mathf.Lerp(PlayerControl.RUN_SPEED_MAX, speed_min, rate);
                                //this.set_oni_motion_speed(1.0f);
                                break;
                            }

                            time -= time2;

                            this.run_speed = speed_min;

                        } while (false);

                        this.decelerate.timer += Time.deltaTime;

                    }
                    else
                    {
                        float distance = this.transform.position.x - this.player.transform.position.x;
                        if (distance < decelerate_start)
                        {
                            this.decelerate.is_active = true;
                            this.decelerate.speed_base = this.run_speed;
                            this.decelerate.timer = 0.0f;
                        }
                    }
                }
                break;

            case TYPE.LEAVE:
                {
                    this.run_speed = LEAVE_SPEED + this.player.run_speed;
                }
                break;
        }
    }

    private void DestroyWhenInVisible()
    {
        bool is_visible = false;
        foreach (var oni in this.onis)
        {
            if (oni.GetComponent<Renderer>().isVisible)
            {
                is_visible = true;
                break;
            }
        }

        if (!is_visible)
        {
            Destroy(this.gameObject);
        }
    }

    public void CreateOnis(int oni_num, Vector3 base_position)
    {
        this.oni_num = oni_num;
        oni_num_max = Mathf.Max(oni_num_max, oni_num);

        this.onis = new OniControl[this.oni_num];

        Vector3 position;

        for (int i = 0; i < this.oni_num; i++)
        {
            GameObject go = Instantiate(this.OniPrefab[i % this.OniPrefab.Length]) as GameObject;

            this.onis[i] = go.GetComponent<OniControl>();

            position = base_position;

            if (i > 0)
            {
                Vector3 splat_range;

                splat_range.x = OniControl.collision_size * (float)(oni_num - 1);
                splat_range.z = OniControl.collision_size / 2.0f * (float)(oni_num - 1);

                splat_range.x = Mathf.Min(splat_range.x, OniGroupControl.collision_size);
                splat_range.z = Mathf.Min(splat_range.z, OniGroupControl.collision_size / 2.0f);

                position.x += Random.Range(0.0f, splat_range.x);
                position.z += Random.Range(-splat_range.z, splat_range.z);
            }

            position.y = 0.5f;

            this.onis[i].transform.position = position;
            this.onis[i].transform.parent = this.transform;

            this.onis[i].main_camera = this.main_camera;
            this.onis[i].player = this.player;
        }
    }

    private static int count = 0;

    public void OnAttackedFromPlayer()
    {
        this.scene_control.AddDefeatNum(this.oni_num);


        Vector3 blowout;
        Vector3 blowout_up;
        Vector3 blowout_xz;

        float y_angle;
        float blowout_speed;
        float blowout_speed_base;

        float forward_back_angle;
        float base_radius;

        float y_angle_center;
        float y_angle_swing;

        float arc_length;

        // 默认击飞效果
        base_radius = 0.3f;
        blowout_speed_base = 10.0f;
        forward_back_angle = 40.0f;  // 往人物后方飞（画面左边，左手坐标系，大拇指朝屏幕，四指方向转40°）
        y_angle_center = 180.0f;     // 因为往后方飞，所以旋转了 180°
        y_angle_swing = 10.0f;


        switch (this.scene_control.evaluation)
        {
            default:
            case SceneControl.EVALUATION.OKAY:
                {
                    base_radius = 0.3f;
                    blowout_speed_base = 10.0f;
                    forward_back_angle = 40.0f;
                    y_angle_center = 180.0f;
                    y_angle_swing = 10.0f;
                }
                break;
            case SceneControl.EVALUATION.GOOD:
                {
                    base_radius = 0.3f;
                    blowout_speed_base = 10.0f;
                    forward_back_angle = 0.0f;
                    y_angle_center = 0.0f;
                    y_angle_swing = 60.0f;
                }
                break;

            case SceneControl.EVALUATION.GREAT:
                {
                    base_radius = 0.5f;
                    blowout_speed_base = 15.0f;
                    forward_back_angle = -20.0f;
                    y_angle_center = 0.0f;
                    y_angle_swing = 30.0f;
                }
                break;
        }





        forward_back_angle += Random.Range(-5.0f, 5.0f);

        // 根据怪物数量决定的弧长角度，不超过120°
        arc_length = (this.onis.Length - 1) * 30.0f;
        arc_length = Mathf.Min(arc_length, 120.0f);

        y_angle = y_angle_center;
        y_angle += -arc_length / 2.0f;  // 从一边开始排列
        y_angle += y_angle_swing;       // 左斩右斩变化

        // 这里不知道为什么要这么处理，感觉好像是个随机而已，(0到10)*3
        y_angle += ((OniGroupControl.count * 7) % 11) * 3.0f;

        foreach (OniControl oni in this.onis)
        {
            blowout_up = Vector3.up;
            blowout_xz = Vector3.right * base_radius;
            blowout_xz = Quaternion.AngleAxis(y_angle, Vector3.up) * blowout_xz;
            blowout = blowout_up + blowout_xz;
            blowout.Normalize();
            blowout = Quaternion.AngleAxis(forward_back_angle, Vector3.forward) * blowout;
            blowout_speed = blowout_speed_base * Random.Range(0.8f, 1.2f);
            blowout *= blowout_speed;

            Vector3 angular_velocity = Vector3.Cross(Vector3.up, blowout);
            angular_velocity.Normalize();

            // 这里是把旋转速度和击飞速度做了点相关性，但是为什么是 3.14*8/15？
            angular_velocity *= 3.14f * 8.0f * blowout_speed / 15.0f * Random.Range(0.5f, 1.5f);

            oni.AttackedFromPlayer(blowout, angular_velocity);

            Debug.DrawRay(this.transform.position, blowout * 2.0f, Color.white, 1000.0f);

            // 下一个击飞向量的旋转角，沿着弧长分布
            y_angle += arc_length / (this.onis.Length - 1);

        }

        // TODO 播放音效

        OniGroupControl.count++;

        Destroy(this.gameObject);
    }

    public void beginLeave()
    {
        this.GetComponent<Collider>().enabled = false;
        this.type = TYPE.LEAVE;
    }
}
