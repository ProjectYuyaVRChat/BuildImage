using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PingPongMovement : UdonSharpBehaviour
{
    [Header("移動設定")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private bool useLocalPosition = true;
    
    [Header("動作設定")]
    [SerializeField] private bool autoStart = true;
    [SerializeField] private bool loop = true;
    
    private Vector3 currentStartPos;
    private Vector3 currentEndPos;
    private float currentTime = 0f;
    private bool isMoving = false;
    private bool movingForward = true;
    
    void Start()
    {
        // 初期位置を設定
        if (startPoint != null && endPoint != null)
        {
            if (useLocalPosition)
            {
                currentStartPos = startPoint.localPosition;
                currentEndPos = endPoint.localPosition;
            }
            else
            {
                currentStartPos = startPoint.position;
                currentEndPos = endPoint.position;
            }
        }
        else
        {
            // フォールバック: デフォルト位置を使用
            if (useLocalPosition)
            {
                currentStartPos = Vector3.zero;
                currentEndPos = Vector3.up * 2f;
            }
            else
            {
                currentStartPos = transform.position;
                currentEndPos = transform.position + Vector3.up * 2f;
            }
        }
        
        if (autoStart)
        {
            StartMovement();
        }
    }
    
    void Update()
    {
        if (!isMoving) return;
        
        // 時間を更新
        currentTime += Time.deltaTime * moveSpeed;
        
        // 往復移動の計算
        float pingPongValue = Mathf.PingPong(currentTime, 1.0f);
        
        // 位置を更新
        Vector3 newPosition = Vector3.Lerp(currentStartPos, currentEndPos, pingPongValue);
        
        if (useLocalPosition)
        {
            transform.localPosition = newPosition;
        }
        else
        {
            transform.position = newPosition;
        }
        
        // 方向を更新
        if (pingPongValue >= 0.99f)
        {
            movingForward = false;
        }
        else if (pingPongValue <= 0.01f)
        {
            movingForward = true;
        }
    }
    
    public void StartMovement()
    {
        isMoving = true;
    }
    
    public void StopMovement()
    {
        isMoving = false;
    }
    
    public void PauseMovement()
    {
        isMoving = false;
    }
    
    public void ResumeMovement()
    {
        isMoving = true;
    }
    
    public void SetSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }
    
    public void SetPositions(Vector3 newStartPos, Vector3 newEndPos)
    {
        if (useLocalPosition)
        {
            currentStartPos = newStartPos;
            currentEndPos = newEndPos;
        }
        else
        {
            currentStartPos = newStartPos;
            currentEndPos = newEndPos;
        }
    }
    
    public void SetStartPoint(Transform newStartPoint)
    {
        startPoint = newStartPoint;
        if (startPoint != null)
        {
            if (useLocalPosition)
            {
                currentStartPos = startPoint.localPosition;
            }
            else
            {
                currentStartPos = startPoint.position;
            }
        }
    }
    
    public void SetEndPoint(Transform newEndPoint)
    {
        endPoint = newEndPoint;
        if (endPoint != null)
        {
            if (useLocalPosition)
            {
                currentEndPos = endPoint.localPosition;
            }
            else
            {
                currentEndPos = endPoint.position;
            }
        }
    }
    
    public bool IsMoving()
    {
        return isMoving;
    }
    
    public bool IsMovingForward()
    {
        return movingForward;
    }
    
    public float GetCurrentProgress()
    {
        return Mathf.PingPong(currentTime, 1.0f);
    }
} 