using UnityEngine;

public class LevelControl
{
    public SceneControl scene_control = null;
    public GameObject OniGroupPrefab = null;
    public PlayerControl player = null;
    private float oni_generate_line;
    private float appear_margin = 15.0f;
    private int oni_appear_num = 1;
    private int no_miss_count = 0;
    public enum GROUP_TYPE
    {
        NONE = -1,
        SLOW = 0,
        DECELERATE,
        PASSING,
        RAPID,
        NORMAL,
        NUM,
    }
    public GROUP_TYPE group_type = GROUP_TYPE.NORMAL;
    public GROUP_TYPE group_type_next = GROUP_TYPE.NORMAL;
    private bool can_dispatch = false;
    public bool is_random = true;
    private float next_line = 50.0f;
    private float next_speed = OniGroupControl.SPEED_MIN * 5.0f;
    private int normal_count = 5;
    private int event_count = 1;
    private GROUP_TYPE event_type = GROUP_TYPE.NONE;
    public const float INTERVAL_MIN = 20.0f;
    public const float INTERVAL_MAX = 50.0f;
    public void create()
    {
        this.oni_generate_line = this.player.transform.position.x - 1.0f;
    }

    public void OnPlayerMissed()
    {
        this.oni_appear_num = 1;
        this.no_miss_count = 0;
    }

    public void oniAppearControl()
    {
#if true
        for (int i = 0; i < 4; i++)
        {
            if (Input.GetKeyDown((KeyCode)(KeyCode.Alpha1 + i)))
            {
                this.group_type_next = (GROUP_TYPE)i;
                this.is_random = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            this.is_random = !this.is_random;
        }
#endif

        if (!this.can_dispatch)
        {
            if (this.is_special_group())
            {
                if (GameObject.FindGameObjectsWithTag("OniGroup").Length == 0)
                {
                    this.can_dispatch = true;
                }
            }
            else
            {
                this.can_dispatch = true;
            }

            if (this.can_dispatch)
            {
                if (this.group_type_next == GROUP_TYPE.NORMAL)
                {
                    this.oni_generate_line = this.player.transform.position.x + this.next_line;
                }
                else if (this.group_type_next == GROUP_TYPE.SLOW)
                {
                    this.oni_generate_line = this.player.transform.position.x + 50.0f;
                }
                else
                {
                    this.oni_generate_line = this.player.transform.position.x + 10.0f;
                }
            }
        }

        do
        {
            if (this.scene_control.oni_group_num >= this.scene_control.oni_group_appear_max)
            {
                break;
            }
            if (!this.can_dispatch)
            {
                break;
            }
            if (this.player.transform.position.x <= this.oni_generate_line)
            {
                break;
            }

            this.group_type = this.group_type_next;

            switch (this.group_type)
            {
                case GROUP_TYPE.SLOW:
                    this.dispatch_slow();
                    break;
                case GROUP_TYPE.DECELERATE:
                    this.dispatch_decelerate();
                    break;
                case GROUP_TYPE.PASSING:
                    this.dispatch_passing();
                    break;
                case GROUP_TYPE.RAPID:
                    this.dispatch_rapid();
                    break;
                case GROUP_TYPE.NORMAL:
                    this.dispatch_normal(this.next_speed);
                    break;
            }

            this.oni_appear_num++;
            this.oni_appear_num = Mathf.Min(this.oni_appear_num, SceneControl.ONI_APPEAR_NUM_MAX);

            this.can_dispatch = false;
            this.no_miss_count++;

            this.scene_control.oni_group_num++;

            if (this.is_random)
            {
                this.select_next_group_type();
            }

        } while (false);
    }

    public bool is_special_group()
    {
        if (
            this.group_type == GROUP_TYPE.PASSING ||
            this.group_type_next == GROUP_TYPE.PASSING ||
            this.group_type == GROUP_TYPE.DECELERATE ||
            this.group_type_next == GROUP_TYPE.DECELERATE ||
            this.group_type == GROUP_TYPE.SLOW ||
            this.group_type_next == GROUP_TYPE.SLOW
        )
        {
            return true;
        }

        return false;
    }

    public void select_next_group_type()
    {
        if (this.event_type != GROUP_TYPE.NONE)
        {
            this.event_count--;
            if (this.event_count <= 0)
            {
                this.event_type = GROUP_TYPE.NONE;
                this.normal_count = Random.Range(3, 7);
            }
        }
        else
        {
            this.normal_count--;

            if (this.normal_count <= 0)
            {
                this.event_type = (GROUP_TYPE)Random.Range(0, 4);
                switch (this.event_type)
                {
                    default:
                    case GROUP_TYPE.DECELERATE:
                    case GROUP_TYPE.PASSING:
                    case GROUP_TYPE.SLOW:
                        this.event_count = 1;
                        break;
                    case GROUP_TYPE.RAPID:
                        this.event_count = Random.Range(2, 4);
                        break;
                }
            }
        }

        if (this.event_type == GROUP_TYPE.NONE)
        {
            float rate;
            rate = (float)this.no_miss_count / 10.0f;
            rate = Mathf.Clamp01(rate);

            // Oni 的 speed 越大，相对 player 的速度是较慢的，所以从 SPEED_MAX 开始插值
            this.next_speed = Mathf.Lerp(OniGroupControl.SPEED_MAX, OniGroupControl.SPEED_MIN, rate);
            this.next_line = Mathf.Lerp(LevelControl.INTERVAL_MAX, LevelControl.INTERVAL_MIN, rate);

            this.group_type_next = GROUP_TYPE.NORMAL;
        }
        else
        {
            this.group_type_next = this.event_type;
        }
    }

    public void dispatch_normal(float speed)
    {
        Vector3 appear_position = this.player.transform.position;
        appear_position.x += appear_margin;
        this.create_oni_group(appear_position, speed, OniGroupControl.TYPE.NONE);
    }

    public void dispatch_slow()
    {
        Vector3 appear_position = this.player.transform.position;
        appear_position.x += appear_margin;

        float rate;
        rate = (float)this.no_miss_count / 10.0f;
        rate = Mathf.Clamp01(rate);

        this.create_oni_group(appear_position, OniGroupControl.SPEED_MIN * rate, OniGroupControl.TYPE.NORMAL);
    }

    public void dispatch_rapid()
    {
        Vector3 appear_position = this.player.transform.position;
        appear_position.x += appear_margin;

        this.create_oni_group(appear_position, this.next_speed, OniGroupControl.TYPE.NORMAL);
    }

    public void dispatch_decelerate()
    {
        Vector3 appear_position = this.player.transform.position;
        appear_position.x += appear_margin;

        this.create_oni_group(appear_position, 9.0f, OniGroupControl.TYPE.DECELERATE);
    }

    public void dispatch_passing()
    {
        float speed_low = 2.0f;    // 往右方向的低速，相对 Player 则是高速接近，是超越组
        float speed_rate = 2.0f;
        float speed_high = (speed_low - this.player.GetComponent<Rigidbody>().velocity.x) / speed_rate + this.player.GetComponent<Rigidbody>().velocity.x;

        float passing_point = 0.5f;

        Vector3 appear_position = this.player.transform.position;

        appear_position.x = this.player.transform.position.x + appear_margin;

        this.create_oni_group(appear_position, speed_high, OniGroupControl.TYPE.NORMAL);

        appear_position.x = this.player.transform.position.x + appear_margin * Mathf.Lerp(speed_rate, 1.0f, passing_point);

        this.create_oni_group(appear_position, speed_low, OniGroupControl.TYPE.NORMAL);
    }

    private void create_oni_group(Vector3 appear_position, float speed, OniGroupControl.TYPE type)
    {
        Vector3 position = appear_position;

        GameObject go = GameObject.Instantiate(this.OniGroupPrefab) as GameObject;
        OniGroupControl new_group = go.GetComponent<OniGroupControl>();

        position.y = OniGroupControl.collision_size / 2.0f;
        position.z = 0.0f;

        new_group.transform.position = position;

        new_group.main_camera = GameObject.FindGameObjectWithTag("MainCamera");
        new_group.player = this.player;
        new_group.run_speed = speed;
        new_group.type = type;

        Vector3 base_position = position;
        int oni_num = this.oni_appear_num;

        base_position.x -= (OniGroupControl.collision_size / 2.0f - OniControl.collision_size / 2.0f);
        base_position.y = OniControl.collision_size / 2.0f;

        new_group.CreateOnis(oni_num, base_position);
    }
}
