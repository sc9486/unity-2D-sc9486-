﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [SerializeField]
    float _speed = 20.0f;

    public bool attacked = false;
    public bool sting = false;

    public Image nowHpbar;
    public float jumpPower = 60;

    bool inputRight = false;
    bool inputLeft = false;
    bool inputJump = false;

    Animator animator;
    Rigidbody2D rigid2D;
    Collider2D col2D;

    bool isplayerDead = false;

    public Status status;

    void Start()
    {
        transform.position = new Vector3(0,0,0);
        animator = GetComponent<Animator>();
        rigid2D = GetComponent<Rigidbody2D>();
        col2D = GetComponent<Collider2D>();

        // 플레이어 스테이터스 설정
        status = new Status();
        status = status.SetUnitStatus(UnitCode.player);

        SetAttackSpeed(status.atkSpeed);
        StartCoroutine(CheckplayerDeath());
    }

    void Update()
    {
        if (isplayerDead) return; // 조작 못하게 하기

        if (Input.GetKey(KeyCode.D))
        {
            inputRight = true;
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            inputLeft = true;
            transform.localScale = new Vector3(1, 1, 1);
        }

        if (Input.GetKey(KeyCode.Mouse0) &&
            !animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            animator.SetTrigger("attack");
            SFXManager.Instance.PlaySound(SFXManager.Instance.playerAttack);
        }

        if (Input.GetKey(KeyCode.Alpha1))
        {
            transform.localScale = new Vector3(1, 1, 1);
            transform.transform.Translate(new Vector3(-transform.localScale.x * 25f * Time.deltaTime, 0, 0));
            animator.SetBool("moving", true);
        }

        else if (Input.GetKey(KeyCode.Alpha2))
        {
            transform.localScale = new Vector3(-1, 1, 1);
            transform.transform.Translate(new Vector3(-transform.localScale.x * 25f * Time.deltaTime, 0, 0));
            animator.SetBool("moving", true);
        }
        else animator.SetBool("moving", false);

        if (Input.GetKeyDown(KeyCode.Space) && !animator.GetBool("jumping"))
        {
            inputJump = true;
        }

        nowHpbar.fillAmount = (float)status.nowHp / status.maxHp;

        RaycastHit2D raycastHit = Physics2D.BoxCast(col2D.bounds.center, col2D.bounds.size, 0f, Vector2.down, 0.02f, LayerMask.GetMask("Ground")); // 8 : Ground

        if (raycastHit.collider != null)
        {
            animator.SetBool("jumping", false);
        }
        else animator.SetBool("jumping", true);

    }

    void FixedUpdate()
    {
        if (inputRight)
        {
            inputRight = false;
            rigid2D.AddForce(Vector2.right * status.moveSpeed);
        }
        if (inputLeft)
        {
            inputLeft = false;
            rigid2D.AddForce(Vector2.left * status.moveSpeed);
        }
        if (rigid2D.velocity.x >= 1.5f) rigid2D.velocity = new Vector2(1.5f, rigid2D.velocity.y);
        else if (rigid2D.velocity.x <= -1.5f) rigid2D.velocity = new Vector2(-1.5f, rigid2D.velocity.y);

        if (inputJump)
        {
            inputJump = false;
            rigid2D.AddForce(Vector2.up * jumpPower);
        }

        if (rigid2D.velocity.y <= -15) rigid2D.velocity = new Vector2(rigid2D.velocity.x, -15);
    }

    void AttackTrue()
    {
        attacked = true;
    }
    void AttackFalse()
    {
        attacked = false;
    }
    void StingTrue()
    {
        sting = true;
    }
    void StingFalse()
    {
        sting = false;
    }
    public void SetAttackSpeed(float speed)
    {
        animator.SetFloat("attackSpeed", speed);
        status.atkSpeed = speed;
    }
    public float GetAttackSpeed()
    {
        return status.atkSpeed;
    }
    public void SetMoveSpeed(float speed)
    {
        status.moveSpeed = speed;
    }
    public float GetMoveSpeed()
    {
        return status.moveSpeed;
    }
    IEnumerator CheckplayerDeath()
    {
        while(true)
        {
            // 땅 밑으로 떨어졌다면
            if(transform.position.y < -8)
            {
                SceneManager.LoadScene("Main"); // Scene 재시작
            }

            // 체력이 0이하일 때
            if (status.nowHp <= 0)
            {
                isplayerDead = true;
                animator.SetTrigger("die");
                yield return new WaitForSeconds(2); // 2초 기다리기
                SceneManager.LoadScene("Main");
            }
            yield return new WaitForEndOfFrame(); // 매 프레임의 마지막 마다 실행
        }
    }
}
