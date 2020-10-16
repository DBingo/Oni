using UnityEngine;

public class AttackColliderControl : MonoBehaviour
{
    public PlayerControl player = null;
    private bool is_attacking = false;

    void Start()
    {
        this.SetAttack(false);
    }

    void Update()
    {

    }

    private void OnTriggerStay(Collider other)
    {
        if (this.is_attacking && other.tag == "OniGroup")
        {
            // OniGroupControl 触发被攻击效果
            OniGroupControl oni_group = other.GetComponent<OniGroupControl>();
            oni_group.OnAttackedFromPlayer();

            // TODO player 重置普攻，播放命中特效和音效
        }
    }

    public void SetAttack(bool attack)
    {
        this.is_attacking = attack;
    }
}
