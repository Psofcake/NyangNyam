using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngryBirdController : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D;
    // 최대 게이지 파워
    public float MaxPower = 0;
    private bool bSnapped = false;
    private bool bGragging = false;

    private Vector3 clickMousePosition = Vector2.zero;
    public float MaxGapSize = 0.0f;

    public float Speed = 0.25f;
    public LineRenderer _renderer;

    private Vector3 _lineInitPositoin;
    private Vector3 _lineSnappedPosition;

    public Camera _objectCamera;
    public GameObject _angryBirdSprite;
    
    // Start is called before the first frame update
    void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _lineInitPositoin = _renderer.GetPosition(1 );
    }

    void Start()
    {
        //GetComponent<MeshRenderer>().material.SetTexture("_MainTex", _angryBirdSprite.texture);
    }

    void Shot(Vector2 dir, float normalized)
    {
        // 방향을 계산해서 그쪽으로 날린다.
        _rigidbody2D.AddForce(dir * (normalized * MaxPower), ForceMode2D.Impulse);
        
        // 땡기기 모드 종료
        bSnapped = false;
    }

    // Update is called once per frame
    void Update()
    {
        // 마우스가 클릭 됬다면 ( 0번은 == 왼쪽 1번은 오른쪽 )
        if(Input.GetMouseButtonDown(0))
        {
            // 마우스 포지션을 월드 좌표로 변환한다.
            Vector3 worldPoint = _objectCamera.ScreenToWorldPoint(Input.mousePosition);
            // 변환된 좌표의 마우스 포지션과 클릭된 Angry버드가 일치한다면
            Collider2D collider = Physics2D.OverlapPoint(worldPoint);
            if (collider != null && collider.gameObject == gameObject)
            {
                // 현재 클릭했던 마우스 위치를 기억하고
                clickMousePosition = Input.mousePosition;
                _lineSnappedPosition = _objectCamera.ScreenToWorldPoint(Input.mousePosition);
                // 땡기기 모드에 들어간다
                bSnapped = true;
            }
            else
            {
                // 내가 클릭한 오브젝트가 앵그리버드 아닌데 클릭이 되었으면
                // 일단 그 위치를 저장
                clickMousePosition = Input.mousePosition;
                bGragging = true;
            }
        }

        // 앵그리버드 클릭 안했는데 드래그 중이면
        if (bGragging)
        {
            // 현재 나의 마우스 좌표를 3d로 변환한뒤
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // 나의 전 마우스 좌표도 3d로 변환한뒤
            Vector3 pos2 = Camera.main.ScreenToWorldPoint(clickMousePosition);

            // ray로 만들어주고
            Ray2D ray = new Ray2D(pos, Vector2.zero);
            Ray2D ray2 = new Ray2D(pos2, Vector2.zero);
            
            // ===둘다 광선을 쏜다.===
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
            RaycastHit2D hit2 = Physics2D.Raycast(ray2.origin, ray2.direction, Mathf.Infinity); Debug.Log(hit2.point);
            // ===둘다 광선을 쏜다.===

            // 이전좌표와 차이가 나는만큼
            Vector3 gap = hit2.point - hit.point;
            
            // 그 차이를 더해주고
            Camera.main.transform.position += gap;
            
            // 이전 좌표를 갱신한다.
            clickMousePosition = Input.mousePosition;
            
            if (Input.GetMouseButtonUp(0))
            {
                bGragging = false;
            }
        }
        
        // 땡기기 모드일때
        if (bSnapped)
        {
            // 처음 마우스 찍었단 좌표에서 현재 마우스 좌표를 빼면 앵그리버드가 날아가야 할 방향이 뜬다.
            Vector3 gap = clickMousePosition - Input.mousePosition;
            
            // 그 길이가 마우스 최대 허용범위를 넘어서면
            float currentGap = gap.magnitude;
            if (currentGap >= MaxGapSize)
            {
                // 마우스 최대 허용 범위로 바뀐다.
                currentGap = MaxGapSize;
            }

            // gap에 방향이 있다면 -> 옆으로 처다보니까 right에다가 dir을 넣어준다.
            Vector2 dir = gap.normalized;
            if (dir != Vector2.zero)
                transform.right = dir;

            // 로컬 좌표로 lineRenderer는 계산하기 때문에 로컬좌표로 바꿔준다.
            Vector3 lineSnapped = Camera.main.ScreenToWorldPoint(Input.mousePosition) - _renderer.transform.position;
            lineSnapped.z = 0;

            if (lineSnapped.magnitude >= 1)
                lineSnapped = lineSnapped.normalized;
            
            _renderer.SetPosition(1,  lineSnapped);

            // 땡기기 모드였다가 마우스를 땠을때
            if (Input.GetMouseButtonUp(0))
            {
                // 앵그리 버드를 날린다.
                Shot(dir, currentGap / MaxGapSize);
                
                // 슛을 했으면 새총 제자리
                _renderer.SetPosition(1,  _lineInitPositoin);
            }
        }
    }
}
