using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class ABController : MonoBehaviour
{
    public Action AngryBirdNotification;
    
    [Header("References")] // 인스펙터에서 직접 바인딩
    [SerializeField] private PowerGauge powerGauge;
    [SerializeField] private Camera cam2D;
    [SerializeField] private Camera cam3D;
    [SerializeField] private Camera camCharacter;
    [SerializeField] private GameObject tracePrefab;
    [SerializeField] private AnimationClip[] animationClips;
    [SerializeField] private SoundController soundController;  //개선할부분★ 차후에 싱글톤으로 변경해보자

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 10f;
    [SerializeField] private float runSpeed = 20f;
    [SerializeField] private float jumpPower = 6f;
    [SerializeField] private float maxPower = 15f;
    [SerializeField] private float maxGapSize = 100f;

    [Header("Trajectory Settings")]
    [SerializeField] private float timeStep = 0.1f;
    [SerializeField] private int steps = 30;

    // 유니티 컴포넌트
    private Rigidbody rb;
    private Animator animator;
    private Collider launchPadCollider;

    // 캐릭터의 상태를 enum으로 관리
    private enum PlayerState { Idle, Moving, Jumping, Launched, Loaded }
    private PlayerState currentState = PlayerState.Idle;

    private int currentAnimIndex = 2;
    private int currentCamIndex = -0;
    private bool isRunning = false;
    private bool isDragging = false;
    private bool isNearLaunchPad = false;

    private Vector3 clickMousePosition;
    private List<GameObject> tracedObjects = new List<GameObject>();
    
    private void Awake()
    {
        if(!soundController) Debug.Log("Not Found SoundManager");
        
        rb = GetComponent<Rigidbody>();
        if (!rb)
            Debug.Log("Not Found Rigidbody");
        
        animator = GetComponent<Animator>();
        if (!animator)
            Debug.Log("Not Found Animator");
        
    }

    void Start()
    {
        ChangeCamera(0);
        //ERROR
        powerGauge.SetGaugePercent(1);
    }
    
    private void UpdateCameras()
    {
        camCharacter.transform.position = transform.position + new Vector3(-2, 1, 0);
        cam3D.transform.position = transform.position + new Vector3(6, 4, -10);
        
        if (currentState==PlayerState.Launched)
        {
            float velocity = GetComponent<Rigidbody>().velocity.sqrMagnitude;
            if (velocity > 0 && velocity < 10f)
            {
                //Debug.Log("velocity : " + velocity);
                ChangeCamera(2);
            }
        }
    }
    void ChangeCamera(int index)
    {
        currentCamIndex = index;
        
        cam2D.enabled = index == 1 || index == 2;
        cam3D.enabled = index == 1;
        camCharacter.enabled = index == 0 || index == 2;

        camCharacter.transform.GetChild(0).gameObject.SetActive(index == 1 || index == 2);

        powerGauge.transform.GetChild(0).gameObject.SetActive(index == 1);
        powerGauge.transform.GetChild(1).gameObject.SetActive(index == 1);

        Debug.Log($"Camera Changed: {index}");
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
                soundController.PlayJumping();
                break;
            case "Greeting":
                index = 4;
                break;
        }
        
        if (index == currentAnimIndex) return; //현재 실행중인 애니메이션이면 return
        else currentAnimIndex = index; //아니면 인덱스 저장
        
        //개선할부분★ToString().Substring보다는 AnimationClip.name을 사용해서 애니메이션 이름 추출하기
        anim = animationClips[currentAnimIndex].ToString().Substring(0,animationClips[currentAnimIndex].ToString().IndexOf(" "));
        
        animator.CrossFade(anim,0.1f); //해당 애니메이션 실행
    }
    
    void Shoot(Vector3 direction, float normalized)
    {
        ChangeCamera(1);
        PlayAnim("Jump");
        //Debug.Log("Direction:"+direction+" / Power:"+normalized*MaxPower);
        rb.AddForce(direction * (normalized*maxPower), ForceMode.Impulse);
        
        //ERROR   
        StartCoroutine(AngryBirdSpeedObserver());
        
        //UnassignedReferenceException: The variable cam2d of ABController has not been assigned.
        //    You probably need to assign the cam2d variable of the ABController script in the inspector.
    }
    // Update is called once per frame
    void Update()   //개선할부분★ 너무 커서 읽기 어려움.. 각 기능별로 메서드 분리 필요(이동,점프,발사,카메라 조작 등)
    {
        UpdateConstraints();
        UpdateCameras();
    
        HandleInput();
    }
    private void UpdateConstraints()
    {
        if (currentState == PlayerState.Launched) //발사 완료 시 FreezeRotationX 해제
            rb.constraints &= ~RigidbodyConstraints.FreezeRotationX;
        else
            rb.constraints |= RigidbodyConstraints.FreezeRotationX;
    }
    private void HandleInput()
    {
        switch (currentState)
        {
            case PlayerState.Idle:
            case PlayerState.Moving:
                HandleMovement();
                break;
            case PlayerState.Loaded:
                HandleLoadedState();
                break;
            case PlayerState.Launched:
                HandleReload();
                break;
        }

        if (Input.GetKeyDown(KeyCode.C))
            ChangeCamera((currentCamIndex + 1) % 3);
    }
    private void HandleMovement()
    {
        isRunning = Input.GetKey(KeyCode.LeftShift);
        float moveSpeed = isRunning ? runSpeed : walkSpeed;

        if (Input.GetKey(KeyCode.A))
        {
            transform.rotation = Quaternion.Euler(0, 270, 0);
            rb.MovePosition(transform.position + transform.forward * moveSpeed * Time.deltaTime);
            PlayAnim("Move");
            currentState = PlayerState.Moving;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.rotation = Quaternion.Euler(0, 90, 0);
            rb.MovePosition(transform.position + transform.forward * moveSpeed * Time.deltaTime);
            PlayAnim("Move");
            currentState = PlayerState.Moving;
        }

        if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))
        {
            PlayAnim("Idle");
            currentState = PlayerState.Idle;
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            PlayAnim("Jump");
            rb?.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            currentState = PlayerState.Jumping;
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            PlayAnim("Greeting");
        }

        if (Input.GetKeyDown(KeyCode.S) && isNearLaunchPad)
        {
            Load();
        }
    }
    private void Load()
    {
        currentState = PlayerState.Loaded;
        rb.isKinematic = true;

        Transform launchTransform = launchPadCollider.transform.GetChild(0).GetChild(1).GetChild(0);
        transform.position = launchTransform.position;
        transform.rotation = launchTransform.rotation;

        PlayAnim("Idle");
        ChangeCamera(1);
        Debug.Log("Bomb is Loaded.");
    }
    private void HandleLoadedState()
    {
        if (Input.GetMouseButtonDown(0)) //마우스 왼쪽 버튼이 눌리면 드래깅으로 체크하기
        {
            isDragging = true;
            clickMousePosition = Input.mousePosition; //클릭된위치 저장
        }

        if (isDragging)
        {
            HandleDragAndTrajectory();
        }

        if (Input.GetKeyDown(KeyCode.S) && isNearLaunchPad)
            Unload();
    }
    private void Unload()
    {
        currentState = PlayerState.Idle;
        transform.position += Vector3.up;
        transform.rotation = Quaternion.Euler(0, 90, 0);
        ChangeCamera(0);
        Debug.Log("Bomb is Unloaded.");
    }
    
    private void HandleReload() //발사대위치로 재장전하기
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Load();
            Debug.Log("Bomb is Reloaded.");
        }
    }
    private void HandleDragAndTrajectory()
    {
        //gap=(마우스가클릭된위치-이동한뒤지금위치)
        Vector3 gap = clickMousePosition - Input.mousePosition;
        
        //gap만큼의 거리, maxGapSize 중 더 작은 숫자로 반환. 즉, 최대범위를 제한.
        float currentGap = Mathf.Min(gap.magnitude, maxGapSize);
        
        float gaugePercent = currentGap / maxGapSize;   // 파워게이지가 얼만큼 채워졌는지(0~1)
        powerGauge.SetGaugePercent(gaugePercent);   //비율만큼 게이지바에 적용

        Vector3 dir = gap.normalized;
        if (dir != Vector3.zero)
        {
            //마우스 방향에 따라 발사대방향 회전
            launchPadCollider.transform.GetChild(0).GetChild(1).right = -dir;
        }
        //날아갈 예상 궤적 그리기
        DrawTrajectory(dir, gaugePercent);
        //마우스를 떼면 날아가기.
        if (Input.GetMouseButtonUp(0))
        {
            rb.isKinematic = false;
            currentState = PlayerState.Launched;
            isDragging = false;
            Shoot(dir, gaugePercent);
            ClearTrajectory();  //궤적 지우기
        }
    }
    private void DrawTrajectory(Vector3 dir, float powerRatio)
    {
        if (tracedObjects.Count == 0) //빈 배열이므로 궤적그리기에 사용할 동그라미 인스턴스들 넣어주기
        {
            for (int i = 0; i < steps; ++i)
                tracedObjects.Add(Instantiate(tracePrefab));
        }

        Vector3 impulseVelocity = dir * powerRatio * maxPower;
        Vector3 position = rb.position;
        Vector3 accelation = Physics.gravity;

        for (int i = 0; i < steps; ++i)
        {
            float time = timeStep * i;

            Vector3 gravityFactor = accelation * 0.5f * time * time; //중력가속도
            Vector3 future = position + (rb.velocity+impulseVelocity) * time + gravityFactor; //velocity*time = 거리벡터
            tracedObjects[i].transform.position = future;
        }
    }
    private void ClearTrajectory()
    {
        foreach (var tracedObject in tracedObjects)
            Destroy(tracedObject);
        tracedObjects.Clear();
    }
    
    //발사대에 닿아있는지 체크
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LaunchPad"))
        {
            isNearLaunchPad = true;
            launchPadCollider = other;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("LaunchPad"))
        {
            isNearLaunchPad = false;
        }
    }
    
    public IEnumerator AngryBirdSpeedObserver()
    {
        yield return new WaitForSeconds(1); //가속도가 붙어야하므로 1초기다림
        
        //오브젝트는 velocity제곱근이 1f보다 빠를때까지만 생존
        while (rb.velocity.sqrMagnitude > 0.01f)
        {
            yield return null;
        }
        
        AngryBirdNotification?.Invoke();
    }
}
