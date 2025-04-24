using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public float groundDist;
    
    public LayerMask terrainLayer;
    public Rigidbody rb;
    public SpriteRenderer sr1; // Idle sprite (before chest opened)
    public SpriteRenderer sr2; // Walking sprite (before chest opened)
    public SpriteRenderer sr3; // Idle sprite (after chest opened)
    public SpriteRenderer sr4; // Walking sprite (after chest opened)
    public ChestInteraction chestInteraction; // Reference to ChestInteraction script

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        
        // Initialize sprite states
        sr1.enabled = true;
        sr2.enabled = false;
        if (sr3 != null) sr3.enabled = false;
        if (sr4 != null) sr4.enabled = false;
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

        // Determine if chest is opened
        bool isChestOpened = chestInteraction != null && chestInteraction.IsChestOpened();

        // Sprite management based on movement and chest state
        bool isMoving = x != 0 || y != 0;

        if (isChestOpened)
        {
            // Use sr3 and sr4 after chest is opened
            if (sr1 != null) sr1.enabled = false;
            if (sr2 != null) sr2.enabled = false;
            if (sr3 != null) sr3.enabled = !isMoving; // Show idle when not moving
            if (sr4 != null) sr4.enabled = isMoving;  // Show walking when moving
        }
        else
        {
            // Use sr1 and sr2 before chest is opened
            if (sr1 != null) sr1.enabled = !isMoving; // Show idle when not moving
            if (sr2 != null) sr2.enabled = isMoving;  // Show walking when moving
            if (sr3 != null) sr3.enabled = false;
            if (sr4 != null) sr4.enabled = false;
        }

        // Sprite flipping based on movement direction
        if (x != 0)
        {
            bool flip = x < 0;
            if (sr1 != null) sr1.flipX = flip;
            if (sr2 != null) sr2.flipX = flip;
            if (sr3 != null) sr3.flipX = flip;
            if (sr4 != null) sr4.flipX = flip;
        }
    }
}