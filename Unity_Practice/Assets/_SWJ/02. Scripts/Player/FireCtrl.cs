﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
//총알 발사와 재장전 오디오 클립을 저장할 구조체
[System.Serializable]
public struct PlayerSfx
{
    public AudioClip[] fire;
    public AudioClip[] reload;
}
public class FireCtrl : MonoBehaviour
{
    //무기 타입
    public enum WeaponType
    {
        RIFLE=0,
        SHOTGUN
    }

    //주인공이 현재 들고 있는 무기를 저장할 변수
    public WeaponType currWeapon = WeaponType.RIFLE;

    //총알 프리팹
    public GameObject bullet;
    //탄피 추출 파티클
    public ParticleSystem cartridge;
    //총구 화염 파티클
    private ParticleSystem muzzleFlash;
    //AudioSource 컴포넌트를 저장할 변수
    private AudioSource _audio;
    

    //총알 발사 좌표
    public Transform firePos;
    //오디오 클립을 저장할 변수
    public PlayerSfx playerSfx;
    //Shake 클래스를 저장할 변수
    private Shake shake;

    //탄창 이미지 ImageUI
    public Image magazineImg;
    //남은 총알 수 TextUI
    public Text magazineText;

    //최대 총알 수
    public int maxBullet = 10;
    //남은 총알 수 
    public int remainiingBullet = 10;

    //재장전 시간
    public float reloadTime = 2.0f;
    //재장전 여부를 판단할 변수
    private bool isReloading = false;

    //변경할 무기 이미지
    public Sprite[] weaponIcons;
    //교체할 무기 이미지 UI
    public Image weaponImage;

    // Start is called before the first frame update
    void Start()
    {
        //FirePos 하위에 있는 컴포넌트 추출
        muzzleFlash = firePos.GetComponentInChildren<ParticleSystem>();
        //AudioSource 컴포넌트 추출
        _audio = GetComponent<AudioSource>();
        //Shake 스크립트를 추출
        shake = GameObject.Find("CameraRig").GetComponent<Shake>();
    }

    // Update is called once per frame
    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        //마우스 왼쪽 버튼을 클릭했을 때 Fire 함수 호출
        if(!isReloading && Input.GetMouseButtonDown(0))
        {
            //총알 수를 하나 감소
            --remainiingBullet;
            Fire();

            //남은 총알이 없을 경우 재장전 코루틴 호출
            if(remainiingBullet==0)
            {
                StartCoroutine(Reloading());
            }
        }
    }

    void Fire()
    {
        //셰이크 효과 호출
        StartCoroutine(shake.ShakeCamera());
        //Bullet 프리팹을 동적으로 생성
        //Instantiate(bullet, firePos.position, firePos.rotation);
        var _bullet = GameManager.instance.GetBullet();
        if(_bullet !=null)
        {
            _bullet.transform.position = firePos.position;
            _bullet.transform.rotation = firePos.rotation;
            _bullet.SetActive(true);
        }
        //파티클 실행
        cartridge.Play();
        //총구 화염 파티클 실행
        muzzleFlash.Play();
        //사운드 발생
        FireSfx();
        //재장전 이미지의 fillAmount 속성값 지정
        magazineImg.fillAmount = (float)remainiingBullet / (float)maxBullet;
        //남은 총알 수 갱신
        UpdateBulletText();
    }
    void FireSfx()
    {
        //현재 들고 있는 무기의 오디오 클립을 가져옴
        var _sfx = playerSfx.fire[(int)currWeapon];
        //사운드 발생
        _audio.PlayOneShot(_sfx, 1.0f);
    }
    IEnumerator Reloading()
    {
        isReloading = true;
        _audio.PlayOneShot(playerSfx.reload[(int)currWeapon], 1.0f);

        //재장전 오디오 길이 + 0.3초 동안 대기
        yield return new WaitForSeconds(playerSfx.reload[(int)currWeapon].length + 0.3f);

        //각종 변숫값의 초기화
        isReloading = false;
        magazineImg.fillAmount = 1.0f;
        remainiingBullet = maxBullet;
        //남은 총알 수 갱신
        UpdateBulletText();

    }

    private void UpdateBulletText()
    {
        //(남은 총알 수 / 최대 총알 수) 표시
        magazineText.text = string.Format("<color=#ff0000>{0}</color>/{1}", remainiingBullet, maxBullet);
    }

    public void OnChangeWeapon()
    {
        currWeapon = (WeaponType)((int)++currWeapon % 2);
        weaponImage.sprite = weaponIcons[(int)currWeapon];
    }

}
