using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(CharacterController2D))]
    public class InputController : MonoBehaviour
    {
        [SerializeField] private float runSpeed = 40f;

        private float horizontalMove = 0f;
        private bool jump = false;
        
        private float jumpStartTime;
        private const float jumpMinTime = 0.2f;
        
        private CharacterController2D controller;
        private Animator anim;

        private void Awake()
        {
            controller = GetComponent<CharacterController2D>();
            controller.OnLandEvent.AddListener(OnLand);

            anim = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
            anim.SetFloat("horizontalMove", horizontalMove);

            if (Input.GetButtonDown("Jump"))
            {
                jump = true;
                jumpStartTime = Time.time;
                
                anim.SetTrigger("doJump");
            }

            if (Input.GetButtonDown("Mine"))
                anim.SetTrigger("doMine");
            else if (Input.GetButtonUp("Mine"))
                anim.SetTrigger("stopMine");
            
            if (Input.GetButtonDown("Attack"))
                anim.SetTrigger("doAttack");
            else if (Input.GetButtonUp("Attack"))
                anim.SetTrigger("stopAttack");
        }

        private void FixedUpdate()
        {
            controller.Move(horizontalMove * Time.fixedDeltaTime, jump);
            jump = false;
        }

        private void OnLand()
        {
            if (Time.time - jumpStartTime < jumpMinTime)
                return;
            
            anim.SetTrigger("doLand");
        }
    }
}
