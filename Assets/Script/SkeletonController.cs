﻿﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

enum skeletonStatus
{
    IDLE = 0, // 待機
    WALK = 1, // 遊走
    ATTACK = 2, // 當進入視線範圍時 怒吼 | 當進入攻擊範圍時 追擊+攻擊
    RESET = 3, // 血量回復 + 走回起始點
    Death = 4 // 死亡
}

public class SkeletonController : MonoBehaviour
{
    skeletonStatus skeletonStatus; // 怪物狀態
    public GameObject player; // 主角
    public float speed; // 移動速度
    public Vector3 initTransform;
    Vector3 randomPosition;
    // 是否攻擊
    bool attack = false;
    bool idle = false;
    bool walk = false;
    Animator anim;// 動畫控制
    NavMeshAgent navMeshAgent;// 導航控制

    //血量
    public GameObject healthBar;
    public float health = 100;
    public float healthmax = 100;
    
    public GameObject blood_FX;
    public GameObject canvas;
    int status = 0;
    float dist;//計算怪物與腳色距離
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
        initTransform = new Vector3(this.transform.position.x,this.transform.position.y,this.transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        // 偵測血量
        CheckHealthBar();
        dist = Vector3.Distance(player.transform.position,transform.position);
        if(dist<35 && skeletonStatus != skeletonStatus.ATTACK)
        //&&= and, ||= or
        {
            IdleEvent();
            anim.SetTrigger("Skill");
            skeletonStatus = skeletonStatus.ATTACK;
        }

        switch (skeletonStatus)
        {
            // 待機事件
            case skeletonStatus.IDLE:
                IdleEvent();
                break;
            // 遊走事件
            case skeletonStatus.WALK:
                WalkEvet();
                break;
            // 攻擊事件
            case skeletonStatus.ATTACK:
                AttackEvent();
                break;
            // 初始化
            case skeletonStatus.RESET:
                RestEvent();
                break;
        }
    } 
    void IdleEvent()
    {
        //站立不動
        //左右看
        //進入遊走狀態
        if(idle)
            return;

        int RandomNumber = Random.Range(1,4);

        if(RandomNumber >= 3 )
        {
            skeletonStatus = skeletonStatus.WALK;
        }
        else if (RandomNumber == 2)
        {
            anim.SetBool ("Idle",true);
            idle = true;
        }
    
    }
    void IdleEnd()
    {
        anim.SetBool ("Idle",false);
        idle = false;
    }
    void WalkEvet()
    {
        
        if (walk)
        {
            float randomDist = Vector3.Distance(randomPosition, transform.position);
            if (randomDist < 3)
            {
                walk = false; anim.SetBool("Walk", false);
                skeletonStatus = skeletonStatus.IDLE;
            }
            return;
        }
        float x =initTransform.x + Random.Range(-10.0f,10.0f);
        float y =transform.position.y;
        float z =initTransform.z + Random.Range(-10.0f,10.0f);
        randomPosition = new Vector3(x,y,z);

        navMeshAgent.SetDestination(randomPosition);
        anim.SetBool("Walk", true);
        walk = true;
    }
    void AttackEvent()
    {
        //如果怪物距離角色 25 之內開始追擊
        //怪物距離腳色 35之後進入待機模式
        //怪物距離角色小於 35 開始進入攻擊狀態
        if(attack)
            return;
        if(dist > 35)
        {
            skeletonStatus = skeletonStatus.RESET;//玩家距離太遠,怪物走回初始位置並回復血量
        }
        //如果距離大於 XX 不追
        else if (dist > 25)
        {
            anim.SetBool("Run", false);
        } 
        // 如果距離小於 XX 追擊
        else if (dist < 25 && dist > 3)
        {
            anim.SetBool("Run", true);
            navMeshAgent.SetDestination(player.transform.position);
        }
        else
        {
            anim.SetBool ("Attack", true);
            attack = true;
        }
    }
    void AttackEnd()
    {

        anim.SetBool("Attack", false);
        attack = false;
    }
    void RestEvent()
    {
        float initDist = Vector3.Distance(initTransform, transform.position);//計算怪物與原點位置
        if(health <=100)
        {
            health += 1;//補滿怪物血量
        }
        if (initDist <3)
        {
            anim.SetBool("Walk", false);
            skeletonStatus = skeletonStatus.IDLE;
        }
        else
        {
            anim.SetBool("Walk", true);
            navMeshAgent.SetDestination(initTransform);
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        if(other.tag == "Attack")
        {
            // 觸發受傷事件
            Damage();
        }

        if(other.tag == "Untagged")
        {
            canvas.SetActive(true);
        }
    }  

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "Untagged")
        {
            canvas.SetActive(false);
        }
    }
    
    //受傷事件
    void Damage()
    {
        // 如果狀態違背攻擊中 則不往下做
        // if (status == 2)
        //     return;
        //產生怪物受傷特效及音效
        // todo 隨機特效及位置
        Instantiate(blood_FX, new Vector3(transform.position.x,transform.position.y+2f,transform.position.z), transform.rotation);

        health -= 40;
        if (health < 0 && status != 3)
        {
            status = 3;
            anim.SetTrigger("Dead");
            Debug.Log("Destroy");
            Invoke("Destroy", 3);
            return;
        }

        anim.SetTrigger("Damage");
    }
    void CheckHealthBar()
    {
        healthBar.GetComponent<Transform>().localPosition = new Vector3(  3 - ((3 / healthmax)* health), 0f, 0f);
    }
    void DamageEnd()
    {   
        anim.SetBool("Attack",false);
        anim.SetBool("Run", false);
        Invoke("ResetStatus",1);
    }

    void Destroy()
    {
        Destroy(this.gameObject);
    }
    //reset
    
    void ResetStatus()
    {
        status = 1;
    }
}
