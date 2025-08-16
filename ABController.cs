using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class ABController : MonoBehaviour
{
    //새로작성-리스폰
    public Action AngryBirdNotification;
    //새로작성-궤적
    [FormerlySerializedAs("timestep")] public float timeStep = 0.1f;
    public int step = 30;
    public GameObject TracePrefab;
    private List<GameObject> tracedObjects = new List<GameObject>();
    
    public PowerGauge _powerGauge;
    //개선할부분★public보다는 [SerializeField] private 변수로 선언하여 인스펙터에서 조절가능하도록 변경
    public float MoveSpeed = 10;
    public float JumpPower = 6;
    public float MaxPower = 15;
    public float MaxGapSize = 100;

    //Camera
    private int _camIndex = 2;
    public Camera cam2d;
    public Camera cam3d;
    public Camera camCharacter;
    //Animation
    private int _animIndex = 0;
    public AnimationClip[] _animationClips;
    //Sound
    private SoundController Sound;
    
    private Rigidbody _rigidbody;
    private Animator _animator;
    private Collider _collider;

    //개선할부분★ 여러개의 bool변수 보다는 enum으로 상태를 관리하자.
    private bool isRunning = false;
    private bool isLoaded = false;
    private bool isDragging = false;
    private bool isLaunched = false;
    private bool isNearLaunchPad = false;

    private Vector3 clickMousePosition = Vector2.zero;

    private void Awake()
    {
        //SoundController 스크립트를 가진 게임오브젝트 찾아오기
        Sound =  GameObject.FindObjectOfType<SoundController>();
        if(!Sound) Debug.Log("Not Found SoundManager");
        
        _rigidbody = GetComponent<Rigidbody>();
        if (!_rigidbody)
            Debug.Log("Not Found Rigidbody");
        
        _animator = GetComponent<Animator>();
        if (!_animator)
            Debug.Log("Not Found Animator");
        
    }

    void Start()
    {
        ChangeCamera(0);
        //ERROR
        _powerGauge.SetGaugePercent(1);
    }

    void ChangeCamera(int index)
    {
        if (index == _camIndex) return;
        if (index < 3)
            _camIndex = index;
        else
            _camIndex = (_camIndex + 1) % 3;
            
        //개선할부분★개별 카메라를 연결하기보다는 리스트를 사용해 카메라를 관리하고 인덱스 기반으로 처리하도록 차후에 변경하자.
        switch (_camIndex)
        {
            case 0:
                cam2d.enabled = false;
                cam3d.enabled = false;
                camCharacter.enabled = true;
                camCharacter.transform.GetChild(0).gameObject.SetActive(false);
                _powerGauge.transform.GetChild(0).gameObject.SetActive(false);
                _powerGauge.transform.GetChild(1).gameObject.SetActive(false);
                Debug.Log("Cam - Character");
                break;
            case 1:
                cam2d.enabled = true;
                cam3d.enabled = true;
                camCharacter.enabled = false;
                cam3d.transform.GetChild(0).gameObject.SetActive(true);
                _powerGauge.transform.GetChild(0).gameObject.SetActive(true);
                _powerGauge.transform.GetChild(1).gameObject.SetActive(true);
                Debug.Log("Cam - Main");
                break;
            case 2:
                cam2d.enabled = true;
                cam3d.enabled = false;
                camCharacter.enabled = true;
                camCharacter.transform.GetChild(0).gameObject.SetActive(true);
                _powerGauge.transform.GetChild(0).gameObject.SetActive(false);
                _powerGauge.transform.GetChild(1).gameObject.SetActive(false);
                Debug.Log("Cam - Character+Minimap");
                break;
        }
    }
    
    void PlayAnim(string anim)
    {
        int index = 0;
        
        switch (anim)
        {
            case "Idle":
                index = 0;
                break;
            case "Move":
                if (!isRunning)
                    index = 1;
                else
                    index = 2;
                break;
            case "Jump":
                index = 3;
                Sound.PlayJumping();
                break;
            case "Greeting":
                index = 4;
                break;
        }
        
        if (index == _animIndex) return; //현재 실행중인 애니메이션이면 return
        else _animIndex = index; //아니면 인덱스 저장
        
        //개선할부분★ToString().Substring보다는 AnimationClip.name을 사용해서 애니메이션 이름 추출하기
        anim = _animationClips[_animIndex].ToString().Substring(0,_animationClips[_animIndex].ToString().IndexOf(" "));
        
        _animator.CrossFade(anim,0.1f); //해당 애니메이션 실행
    }
    
    void Shoot(Vector3 direction, float normalized)
    {
        ChangeCamera(1);
        PlayAnim("Jump");
        //Debug.Log("Direction:"+direction+" / Power:"+normalized*MaxPower);
        _rigidbody.AddForce(direction * (normalized*MaxPower), ForceMode.Impulse);
        isLaunched = true;
        
        //ERROR   
        StartCoroutine(AngryBirdSpeedObserver());
        
        //UnassignedReferenceException: The variable cam2d of ABController has not been assigned.
        //    You probably need to assign the cam2d variable of the ABController script in the inspector.
    }
    // Update is called once per frame
    void Update()   //개선할부분★ 너무 커서 읽기 어려움.. 각 기능별로 메서드 분리 필요(이동,점프,발사,카메라 조작 등)
    {
        //발사시 FreezeRotationX 해제 (데굴데굴..)
        if (isLaunched)
        {
            _rigidbody.constraints &= ~RigidbodyConstraints.FreezeRotationX;
        }
        else
        {
            _rigidbody.constraints |= RigidbodyConstraints.FreezeRotationX;
        }
        //CHECK
        camCharacter.transform.position = transform.position+new Vector3(-2, 1, 0);
        cam3d.transform.position = transform.position+new Vector3(6,4,-10);
        if (isLaunched)
        {
            float velocity = GetComponent<Rigidbody>().velocity.sqrMagnitude;
            if (velocity > 0 && velocity < 10f)
            {
                //Debug.Log("velocity : " + velocity);
                ChangeCamera(2);
            }
        }

        if (Input.GetKeyDown(KeyCode.C)) //카메라전환
        {
            ChangeCamera(10);
        }

        if (isLoaded)
        {
            _rigidbody.isKinematic = true;
            //발사대 탑승시 발사 위치에 포지셔닝
            transform.position = _collider.transform.GetChild(0).transform.GetChild(1).transform.GetChild(0).position;
            transform.rotation = _collider.transform.GetChild(0).transform.GetChild(1).transform.GetChild(0).rotation;
            PlayAnim("Idle");
        }
        else //Loaded 아닐때만 Move+Jump 가능
        {
            //Move
            if (Input.GetKey(KeyCode.LeftShift))
            {
                isRunning = true;
                MoveSpeed = 20;
            }
            else
            {
                isRunning = false;
                MoveSpeed = 10;
            }
            
            if (Input.GetKey(KeyCode.A))
            {
                transform.rotation = Quaternion.Euler(0,270,0);
                _rigidbody?.MovePosition(Vector3.MoveTowards(transform.position, transform.forward * 100,
                    MoveSpeed * Time.deltaTime));
                
                PlayAnim("Move");
            }

            if (Input.GetKey(KeyCode.D))
            {
                transform.rotation = Quaternion.Euler(0,90,0);
                _rigidbody?.MovePosition(Vector3.MoveTowards(transform.position, transform.forward * 100,
                    MoveSpeed * Time.deltaTime));
                
                PlayAnim("Move");
            }

            if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))
            {
                PlayAnim("Idle");
            }

            //Jump
            if (Input.GetKeyDown(KeyCode.W))
            {
                PlayAnim("Jump");
                _rigidbody?.AddForce(Vector3.up * JumpPower, ForceMode.Impulse);
                
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                PlayAnim("Greeting");
            }

        }

        if (Input.GetKeyDown(KeyCode.Space)&&isLaunched) //재장전, 제자리에 배치
        {
            ChangeCamera(1);
            isLoaded = true;
            isLaunched = false;
            
            Debug.Log("Bomb is Reloaded.");
        }
        //Load & Unload
        if (Input.GetKeyDown(KeyCode.S)&&isNearLaunchPad)
        {
            isLoaded = !isLoaded;
            isLaunched = false;
            
            if (isLoaded)
            {
                ChangeCamera(1);
                Debug.Log("Bomb is Loaded.");
            }
            else
            {
                //발사대에서 내리기
                ChangeCamera(0);
                transform.position = transform.position + new Vector3(0, 1, 0);
                transform.rotation = Quaternion.Euler(0, 90, 0);
                Debug.Log("Bomb is Unloaded."); }
        }
        
        //ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡAngryBirdㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
        // 마우스가 클릭 됐다면 ( 0번은 == 왼쪽 1번은 오른쪽 )
        if(Input.GetMouseButtonDown(0) && isLoaded )
        {
            isDragging = true;
            clickMousePosition = Input.mousePosition;
            
            // cam.ScreenToWorldPoint :: 마우스 포지션을 월드 좌표로 변환한다.
            // 참고 링크 : https://docs.unity3d.com/ScriptReference/Camera.ScreenToWorldPoint.html
            //Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        
        // 드래그 중이면
        if (isDragging)
        {
            Vector3 gap = clickMousePosition - Input.mousePosition;
            
            // 그 길이가 마우스 최대 허용범위를 넘어서면
            float currentGap = gap.magnitude;
            if (currentGap >= MaxGapSize)
            {
                // 마우스 최대 허용 범위로 바뀐다.
                currentGap = MaxGapSize;
            }

            float gaugePercent = currentGap / MaxGapSize;
            
            //ERROR
            _powerGauge.SetGaugePercent(gaugePercent);
            
            //gap에 방향이 있다면
            Vector3 dir = gap.normalized;
            if (dir != Vector3.zero)
            {
                //마우스 방향에 따라 발사대방향 회전
                _collider.transform.GetChild(0).transform.GetChild(1).transform.right = -dir;
            }
            
            // ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ궤적그리기ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
            //개선할부분★ 오브젝트 풀링으로 궤적그리기 점선의 개수를 제한하고 성능 낭비를 줄이자
            if (tracedObjects.Count <= 0)
            {
                for (int i = 0; i < step; ++i)
                {
                    float time = timeStep * i;
                    tracedObjects.Add(Instantiate(TracePrefab));
                }

                //라인렌더러 사용시 : _renderer.positionCount = step;
            }
            else
            {
                Vector3 impluseVelocity = dir * (currentGap / MaxGapSize) * MaxPower;
                
                Vector3 CurrentPosition = _rigidbody.position;
                Vector3 CurrentVelocity = _rigidbody.velocity + impluseVelocity;
                Vector3 currentAccelation = Physics.gravity;
                
                for (int i = 0; i < step; ++i)
                {
                    float time = timeStep * i;
                    // 중력가속도
                    Vector3 gravityFactor = currentAccelation * (0.5f * time * time);
                    
                    // 현재 포지션 + 속도 * 시간;
                    // 속도 * 시간 = 거리 벡터가 나오고0
                    Vector3 future = CurrentPosition + CurrentVelocity * time + gravityFactor ;
                    tracedObjects[i].transform.position = future;
                    //라인렌더러 _renderer.SetPosition(i, future);
                    //tracedObjects.Add(Instantiate(TracePrefab,future,Quaternion.identity));
                }
            }

            // ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ궤적그리기 endㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
            
            
            //마우스를 떼면 머리방향으로 날아가게 함
            if (Input.GetMouseButtonUp(0))
            {
                _rigidbody.isKinematic = false;
                isLoaded = false;
                Shoot(transform.up, currentGap/MaxGapSize);
                isDragging = false;
                
                //궤적그리기
                foreach (var tracedObject in tracedObjects)
                {
                    Destroy(tracedObject);
                }
                tracedObjects.Clear();
            }
        }
        //ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡAngryBirdEndㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
    }

    
    //발사대에 닿아있는지 체크
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LaunchPad"))
        {
            isNearLaunchPad = true;
            _collider = other;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("LaunchPad"))
        {
            isNearLaunchPad = false;
        }
    }

    // 새로작성-리스폰
    public IEnumerator AngryBirdSpeedObserver()
    {
        yield return new WaitForSeconds(1); //가속도가 붙어야하므로 1초기다림
        
        //오브젝트는 velocity제곱근이 1f보다 빠를때까지만 생존
        while (GetComponent<Rigidbody>().velocity.sqrMagnitude > 0.01f)
        {
            yield return null;
        }
        
        AngryBirdNotification?.Invoke();
    }
}
