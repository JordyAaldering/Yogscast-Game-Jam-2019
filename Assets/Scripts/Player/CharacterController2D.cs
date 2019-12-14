#pragma warning disable 0649
using UnityEngine;
using UnityEngine.Events;

namespace Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterController2D : MonoBehaviour
    {
        [SerializeField] private float jumpForce = 400f;
        [SerializeField, Range(0f, 0.3f)] private float movementSmoothing = 0.05f;

        [SerializeField] private LayerMask whatIsGround;
        [SerializeField] private Transform groundCheck;

        private bool isGrounded;
        private const float groundedRadius = 0.1f;
        private Vector3 velocity = Vector3.zero;
        
        [Space] public UnityEvent OnLandEvent;
        
        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            if (OnLandEvent == null)
                OnLandEvent = new UnityEvent();
        }

        private void FixedUpdate()
        {
            bool wasGrounded = isGrounded;
            isGrounded = false;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundedRadius, whatIsGround);
            foreach (Collider2D col in colliders)
            {
                if (col.gameObject != gameObject)
                {
                    isGrounded = true;
                    if (!wasGrounded)
                        OnLandEvent.Invoke();
                    break;
                }
            }
        }
        
        public void Move(float move, bool jump)
        {
            Vector3 vel = rb.velocity;
            Vector3 targetVelocity = new Vector2(move * 10f, vel.y);
            rb.velocity = Vector3.SmoothDamp(vel, targetVelocity, ref velocity, movementSmoothing);

            if (isGrounded && jump)
            {
                isGrounded = false;
                rb.AddForce(new Vector2(0f, jumpForce));
            }
        }
    }
}
