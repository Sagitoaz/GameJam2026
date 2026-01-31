using UnityEngine;

public class MovingObject : MonoBehaviour
{
    [SerializeField] private Transform transform1;
    [SerializeField] private Transform transform2;

    [SerializeField] private float speed;


    [SerializeField]  private Vector2 targetPos;

    [SerializeField] private  Rigidbody2D rb;

    void Awake()
    {
        rb  = GetComponent<Rigidbody2D>();
    }
    void Start()
    {
        targetPos = transform1.position;
    }

    void FixedUpdate()
    {
        
        if((targetPos- (Vector2) transform.position).sqrMagnitude < 0.1f){
            if((targetPos- (Vector2) transform1.position).sqrMagnitude < 0.1f)
            {
                targetPos = transform2.position;
            }
            else
            {
                targetPos = transform1.position;
            }
        }
        else
        {
            rb.MovePosition(Vector2.MoveTowards(rb.position, targetPos, speed * Time.fixedDeltaTime));
        }
    }




}
