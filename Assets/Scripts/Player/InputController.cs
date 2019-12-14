#pragma warning disable 0649
using MarchingSquares;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(CharacterController2D))]
    public class InputController : MonoBehaviour
    {
        [SerializeField] private float runSpeed = 40f;
        [SerializeField] private float mineRange = 0.5f;
        [SerializeField] private Transform origin;

        private float horizontalMove = 0f;
        private bool jump = false;

        private Camera cam;
        private VoxelMap voxelMap;
        
        private CharacterController2D controller;
        private Animator anim;

        private void Awake()
        {
            cam = Camera.main;
            voxelMap = FindObjectOfType<VoxelMap>();
            
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
            {
                Mine();
                anim.SetTrigger("doMine");
            }
            
            if (Input.GetButtonDown("Attack"))
            {
                Attack();
                anim.SetTrigger("doAttack");
            }
        }

        private void FixedUpdate()
        {
            controller.Move(horizontalMove * Time.fixedDeltaTime, jump);
            jump = false;
        }
        
        public void Mine()
        {
            Vector2 position = origin.transform.position;
            Vector2 worldMousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = worldMousePosition - position;
            direction.Normalize();
            
            voxelMap.EditVoxels(position + direction * mineRange);
        }
        
        public void Attack()
        {
            Vector2 position = origin.transform.position;
            Vector2 worldMousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = worldMousePosition - position;
            direction.Normalize();
        }
    }
}
