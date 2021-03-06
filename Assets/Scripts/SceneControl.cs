﻿using UnityEngine;

public class SceneControl : MonoBehaviour
{
    // 生成的怪物组
    public GameObject OniGroupPrefab = null;
    // 结束画面的怪物小山
    public GameObject[] OniYamaPrefab;
    public GameObject OniEmitterPrefab = null;
    // -----------------
    public PlayerControl player = null;
    public GameObject main_camera = null;
    public LevelControl level_control = null;
    public ResultControl result_control = null;
    public ScoreControl score_control = null;
    public OniEmitterControl oni_emitter = null;

    private GUIControl gui_control = null;

    // 淡入淡出控制
    private FadeControl fader = null;

    // 计算分数使用 ------------------------------------------------------------------- //
    public int oni_group_num = 0;
    public int oni_group_appear_max = 10;
    public static int oni_group_penalty = 1;
    public int oni_group_complite = 0;
    public int oni_group_defeat_num = 0;
    public int oni_group_miss_num = 0;

    // 
    public float eval_rate_okay = 1.0f;
    public float eval_rate_good = 2.0f;
    public float eval_rate_great = 4.0f;
    // 击飞时机评价
    public float attack_time = 0.0f;
    public enum EVALUATION
    {
        NONE = -1,
        OKAY = 0,
        GOOD,
        GREAT,
        MISS,
        NUM
    }
    public EVALUATION evaluation = EVALUATION.NONE;
    public const float ATTACK_TIME_GREAT = 0.05f;
    public const float ATTACK_TIME_GOOD = 0.10f;
    // 状态管理 ------------------------------------------------------------------- //
    public enum STATE
    {
        NONE = -1,
        START,  // 开始按钮消失的阶段
        GAME,   // 正式游戏阶段，怪物生成中
        ONI_VANISH_WAIT,
        LAST_RUN,   // 最后再移动一段时间
        PLAYER_STOP_WAIT,  // 制动开始，创建出小山在前面等待角色
        GOAL,
        ONI_FALL_WAIT,
        RESULT_DEFEAT,
        RESULT_EVALUATION,
        RESULT_TOTAL,
        GAME_OVER,
        GOTO_TITLE,
        NUM,
    }
    public STATE state = STATE.NONE;
    public STATE next_state = STATE.NONE;
    public float state_timer = 0.0f;
    public float state_timer_prev = 0.0f;

    // 一些设定常量 ------------------------------------------------------------------- //
    public const int ONI_APPEAR_NUM_MAX = 10; // 生成的每组最大怪物数量
    private const float START_TIME = 2.0f;    // 开始文字渐隐时间
    private const float GOAL_STOP_DISTANCE = 3.0f;  // 结束时，怪物小山与玩家停下位置的距离

    // 游戏整体的结果 ------------------------------------------------------------------ //
    public struct Result
    {
        public int oni_defeat_num;          // 斩杀的怪物数量（总和）
        public int[] eval_count;            // 各个评价的次数
        public int rank;                    // 游戏整体的结果
        public float score;                 // 当前得分
        public float score_max;             // 理论可以获得的最大分数（all great)
    };

    public Result result;

    private int backup_oni_defeat_num = -1;

    // -----------------------------------------------------------------------------------

    void Start()
    {
        // 获取角色相机对象
        this.player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
        this.player.scene_control = this;
        this.main_camera = GameObject.FindGameObjectWithTag("MainCamera");
        // 初始化生成器
        this.level_control = new LevelControl();
        this.level_control.scene_control = this;
        this.level_control.player = this.player;
        this.level_control.OniGroupPrefab = this.OniGroupPrefab;
        this.level_control.create();
        // 初始化结果计算器
        this.result_control = new ResultControl();

        // UI控制组件
        this.gui_control = GUIControl.get();
        this.score_control = this.gui_control.score_control;

        // 清空游戏结果
        this.result.oni_defeat_num = 0;
        this.result.eval_count = new int[(int)EVALUATION.NUM];
        this.result.rank = 0;
        this.result.score = 0;
        this.result.score_max = 0;
        for (int i = 0; i < this.result.eval_count.Length; i++)
        {
            this.result.eval_count[i] = 0;
        }

        this.fader = FadeControl.get();
        this.fader.fade(3.0f, new Color(0.0f, 0.0f, 0.0f, 1.0f), new Color(0.0f, 0.0f, 0.0f, 0.0f));

        // 开始游戏
        this.next_state = STATE.START;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Break();
        }

        this.state_timer_prev = this.state_timer;
        this.state_timer += Time.deltaTime;

        // 根据当前状态和条件，判断是否迁移到下一个状态
        switch (this.state)
        {
            case STATE.START:
                {
                    // 片头阶段，过了 START_TIME 之后进入游戏阶段
                    if (this.state_timer > SceneControl.START_TIME)
                    {
                        this.gui_control.setVisibleStart(false);
                        this.next_state = STATE.GAME;
                    }
                }
                break;

            case STATE.GAME:
                {
                    // 游戏进行到生成的怪物超过数量之后，停止生成，准备结束游戏
                    if (this.oni_group_complite >= this.oni_group_appear_max)
                    {
                        next_state = STATE.ONI_VANISH_WAIT;
                    }

                    // 在结束之前的最后一段，隐藏分数，在人物停止的时候再显示出来递增到最后
                    if (this.oni_group_complite >= this.oni_group_appear_max - 10 && this.backup_oni_defeat_num == -1)
                    {
                        this.backup_oni_defeat_num = this.result.oni_defeat_num;
                    }
                }
                break;

            case STATE.ONI_VANISH_WAIT:
                {
                    // 等所有怪物都消失，且速度超过一半时（才）进入 LAST_RUN
                    do
                    {
                        if (GameObject.FindGameObjectsWithTag("OniGroup").Length > 0)
                        {
                            break;
                        }
                        if (this.player.GetSpeedRate() < 0.5f)
                        {
                            break;
                        }

                        next_state = STATE.LAST_RUN;
                    } while (false);
                }
                break;

            case STATE.LAST_RUN:
                {
                    // LAST_RUN 阶段持续 2.0 秒后进入刹车阶段
                    if (this.state_timer > 2.0f)
                    {
                        next_state = STATE.PLAYER_STOP_WAIT;
                    }
                }
                break;

            case STATE.PLAYER_STOP_WAIT:
                {
                    // 刹车持续到角色停止后，进入 GOAL 阶段
                    if (this.player.IsStopped()) { 
                        next_state = STATE.GOAL;
                    }
                }
                break;

            case STATE.GOAL:
                {
                    // 掉落的怪物全部生成后，进入 RANK 展示前的过渡态 ONI_FALL_WAIT
                    if (this.oni_emitter.oni_num == 0)
                    {
                        this.next_state = STATE.ONI_FALL_WAIT;
                    }
                }
                break;

            case STATE.ONI_FALL_WAIT:
                {
                    // 分数不再变化，且超过 1.5 秒后，进入 RANK 展示阶段
                    if (!this.score_control.isActive() && this.state_timer > 1.5f)
                    {
                        this.next_state = STATE.RESULT_DEFEAT;
                    }
                }
                break;

            case STATE.RESULT_DEFEAT:
                {
                    // 播放"咚咚"音效 （感觉与下面其他播放音效的代码一起，都可以放最后的 switch 处理，这个 switch 里面只关心和判断是否变化状态，放这里可以少写case的判断）
                    // 这个判断方法使得只会在本状态时间超过 0.4 秒的【那一帧】播放一次，防止了多次播放
                    if (this.state_timer >= 0.4f && this.state_timer_prev < 0.4f)
                    {
                        // TODO 播放音效
                    }

                    // 超过 0.5 秒后展示下一个
                    if(this.state_timer > 0.5f)
                    {
                        this.next_state = STATE.RESULT_EVALUATION;
                    }
                }
                break;

            case STATE.RESULT_EVALUATION:
                {
                    if (this.state_timer >= 0.4f && this.state_timer_prev < 0.4f)
                    {
                        // TODO 播放音效
                    }

                    // 超过 2.0 秒后展示总得分
                    if (this.state_timer > 2.0f)
                    {
                        this.next_state = STATE.RESULT_TOTAL;
                    }
                }
                break;

            case STATE.RESULT_TOTAL:
                {
                    if (this.state_timer >= 0.4f && this.state_timer_prev < 0.4f)
                    {
                        // TODO 播放音效
                    }

                    // 超过 2.0 秒后进入结束画面 
                    if (this.state_timer > 2.0f)
                    {
                        this.next_state = STATE.GAME_OVER;
                    }
                }
                break;

            case STATE.GAME_OVER:
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        this.fader.fade(1.0f, new Color(0.0f, 0.0f, 0.0f, 0.0f), new Color(0.0f, 0.0f, 0.0f, 1.0f));
                        this.next_state = STATE.GOTO_TITLE;
                    }
                }
                break;

            case STATE.GOTO_TITLE:
                {
                    if (!this.fader.isActive())
                    {
                        UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
                    }
                }
                break;
        }

        // 状态变化时，下一个状态开始前的初始化处理，然后改变状态
        // 只会触发一次的逻辑放在这里，不能放在下面的 switch 里
        if (this.next_state != STATE.NONE)
        {
            switch (this.next_state)
            {
                case STATE.START:
                    {
                        // 显示 "开始" 文字
                        this.gui_control.setVisibleStart(true);
                    }
                    break;
                case STATE.PLAYER_STOP_WAIT:
                    {
                        // 使 player 开始减速
                        this.player.StopRequest();

                        // 根据结果的 rank 生成怪物山,放在前方，自然进入画面
                        //if(this.result_control.getTotalRank() > 0)
                        //{
                            GameObject oni_yama = Instantiate(this.OniYamaPrefab[2]) as GameObject;

                            Vector3 oni_yama_position = this.player.transform.position;

                            oni_yama_position.x += this.player.CalcDistanceToStop();
                            oni_yama_position.x += SceneControl.GOAL_STOP_DISTANCE;

                            oni_yama_position.y = 0.5f;

                            oni_yama.transform.position = oni_yama_position;
                        //}
                    }
                    break;

                case STATE.GOAL:
                    {
                        // 隐藏掉的分数重新显示并且开始递增
                        this.gui_control.score_control.setNumForce(this.backup_oni_defeat_num);
                        this.gui_control.score_control.setNum(this.result.oni_defeat_num);

                        // 生成使“怪物从上方掉下来”的发射器
                        GameObject go = Instantiate(this.OniEmitterPrefab) as GameObject;

                        this.oni_emitter = go.GetComponent<OniEmitterControl>();

                        Vector3 emitter_position = oni_emitter.transform.position;

                        emitter_position.x = this.player.transform.position.x;
                        emitter_position.x += this.player.CalcDistanceToStop();
                        emitter_position.x += SceneControl.GOAL_STOP_DISTANCE;

                        emitter_position.y = 10;
                        this.oni_emitter.transform.position = emitter_position;

                        // 根据 rank 确定掉落数量
                        int oni_num = 0;
                        switch (this.result_control.getTotalRank())
                        {
                            case 0: oni_num = Mathf.Min(this.result.oni_defeat_num, 10); break;
                            case 1: oni_num = 6; break;
                            case 2: oni_num = 10; break;
                            case 3: oni_num = 20; break;
                        }

                        oni_num = 10; // 测试用

                        this.oni_emitter.oni_num = oni_num;
                    }
                    break;

                // 显示最终 Rank 的三个状态，因为只要触发一次，所以放在这里
                case STATE.RESULT_DEFEAT:
                    {
                        this.gui_control.startDispDefeatRank();
                    }
                    break;
                case STATE.RESULT_EVALUATION:
                    {
                        this.gui_control.startDispEvaluationRank();
                    }
                    break;
                case STATE.RESULT_TOTAL:
                    {
                        this.gui_control.hideDefeatRank();
                        this.gui_control.hideEvaluationRank();
                        this.gui_control.startDispTotalRank();
                    }
                    break;

                case STATE.GAME_OVER:
                    {
                        this.gui_control.setVisibleReturn(true);
                    }
                    break;


            }

            this.state = this.next_state;
            this.next_state = STATE.NONE;
            this.state_timer = 0.0f;
            this.state_timer_prev = -1.0f;
        }

        // 各个状态的逻辑
        switch (this.state)
        {
            case STATE.GAME:
                {
                    this.level_control.oniAppearControl();
                }
                break;
        }
    }

    public void OnPlayerMissed()
    {
        this.oni_group_miss_num++;
        this.oni_group_complite++;
        this.oni_group_appear_max -= oni_group_penalty;
        this.evaluation = EVALUATION.MISS;

        this.level_control.OnPlayerMissed();

        this.result.eval_count[(int)this.evaluation]++;

        GameObject[] oni_groups = GameObject.FindGameObjectsWithTag("OniGroup");

        foreach (var oni_group in oni_groups)
        {
            this.oni_group_num--;
            oni_group.GetComponent<OniGroupControl>().beginLeave();
        }
    }

    public void AddDefeatNum(int num)
    {
        this.oni_group_defeat_num++;
        this.oni_group_complite++;
        this.result.oni_defeat_num += num;

        this.attack_time = this.player.GetComponent<PlayerControl>().GetAttackTimer();

        if (this.evaluation == EVALUATION.MISS)
        {
            this.evaluation = EVALUATION.OKAY;
        }
        else
        {
            if (this.attack_time < ATTACK_TIME_GREAT)
            {
                this.evaluation = EVALUATION.GREAT;
            }
            else if (this.attack_time < ATTACK_TIME_GOOD)
            {
                this.evaluation = EVALUATION.GOOD;
            }
            else
            {
                this.evaluation = EVALUATION.OKAY;
            }
        }

        this.result.eval_count[(int)this.evaluation] += num;

        float[] score_list = { this.eval_rate_okay, this.eval_rate_good, this.eval_rate_great, 0 };
        this.result.score_max += num * this.eval_rate_great; // 理论可以获得的最大分数
        this.result.score += num * score_list[(int)this.evaluation]; // 实际获得的分数

        // Rank 的计算
        this.result_control.addOniDefeatScore(num);
        this.result_control.addEvaluationScore(this.evaluation);
    }

    // ================================================================ //
    // 单例模式
    protected static SceneControl instance = null;

    public static SceneControl get()
    {
        if (SceneControl.instance == null)
        {
            GameObject go = GameObject.Find("SceneControl");

            if (go != null)
            {
                SceneControl.instance = go.GetComponent<SceneControl>();
            }
            else
            {
                Debug.LogError("Can't find game object \"SceneControl\".");
            }
        }

        return (SceneControl.instance);
    }
}
