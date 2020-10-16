using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackColliderControl : MonoBehaviour
{
    public PlayerControl player = null;

    private bool is_attacking = false;


    // Start is called before the first frame update
    void Start()
    {
        this.SetAttack(false);
    }

    // Update is called once per frame
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
