using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public float groundDist;
    
    public LayerMask terrainLayer;
    public Rigidbody rb;
    public SpriteRenderer sr1; // Idle sprite
    public SpriteRenderer sr2; // Walking sprite

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        
        // Initialize sprite states
        sr1.enabled = true;
        sr2.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Ground detection and adjustment
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity, terrainLayer))
        {
            if (hit.collider != null)
            {
                Vector3 movePos = transform.position;
                movePos.y = hit.point.y + groundDist;
                transform.position = movePos;
            }
        }

        // Movement input
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        Vector3 moveDir = new Vector3(x, 0, y);
        rb.linearVelocity = moveDir * speed;

        // Sprite management based on movement
        bool isMoving = x != 0 || y != 0;
        
        sr1.enabled = !isMoving; // Show idle when not moving
        sr2.enabled = isMoving;  // Show walking when moving

        // Sprite flipping based on movement direction
        if (x != 0)
        {
            bool flip = x < 0;
            sr1.flipX = flip;
            sr2.flipX = flip;
        }
    }
}