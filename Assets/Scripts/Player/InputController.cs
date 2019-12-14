using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(CharacterController2D))]
    public class InputController : MonoBehaviour
    {
        [SerializeField] private float runSpeed = 40f;

        private float horizontalMove = 0f;
        private bool jump = false;
        
        private CharacterController2D controller;
        private Animator anim;

        private void Awake()
        {
            controller = GetComponent<CharacterController2D>();
            controller.OnLandEvent.AddListener(() => anim.SetTrigger("doLand"));

            anim = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
            anim.SetFloat("horizontalMove", horizontalMove);

            if (Input.GetButtonDown("Jump"))
            {
                jump = true;
                anim.SetTrigger("doJump");
            }

            if (Input.GetButtonDown("Mine"))
                anim.SetTrigger("doMine");
            
            if (Input.GetButtonDown("Attack"))
                anim.SetTrigger("doAttack");
        }

        private void FixedUpdate()
        {
            controller.Move(horizontalMove * Time.fixedDeltaTime, jump);
            jump = false;
        }
    }
}
