#pragma warning disable 0649
using UnityEngine;
using UnityEngine.Events;

namespace Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterController2D : MonoBehaviour
    {
        [SerializeField] private float jumpForce = 400f;
        [SerializeField, Range(0, 1)] private float crouchSpeed = 0.36f;
        [SerializeField, Range(0, .3f)] private float movementSmoothing = 0.05f;

        [SerializeField] private LayerMask whatIsGround;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Transform ceilingCheck;
        [SerializeField] private Collider2D crouchDisableCollider;

        private bool isGrounded;
        private const float groundedRadius = 0.2f;
        private const float ceilingRadius = 0.2f;
        private Vector3 velocity = Vector3.zero;
        
        [Header("Events"), Space] public UnityEvent OnLandEvent;
        [System.Serializable] public class BoolEvent : UnityEvent<bool> { }
        
        public BoolEvent OnCrouchEvent;
        private bool wasCrouching = false;
        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            if (OnLandEvent == null)
                OnLandEvent = new UnityEvent();

            if (OnCrouchEvent == null)
                OnCrouchEvent = new BoolEvent();
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
                }
            }
        }
        
        public void Move(float move, bool crouch, bool jump)
        {
            if (!crouch)
            {
                if (Physics2D.OverlapCircle(ceilingCheck.position, ceilingRadius, whatIsGround))
                {
                    crouch = true;
                }
            }

            if (crouch)
            {
                if (!wasCrouching)
                {
                    wasCrouching = true;
                    OnCrouchEvent.Invoke(true);
                }

                move *= crouchSpeed;

                if (crouchDisableCollider != null)
                    crouchDisableCollider.enabled = false;
            }
            else
            {
                if (crouchDisableCollider != null)
                    crouchDisableCollider.enabled = true;

                if (wasCrouching)
                {
                    wasCrouching = false;
                    OnCrouchEvent.Invoke(false);
                }
            }

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
