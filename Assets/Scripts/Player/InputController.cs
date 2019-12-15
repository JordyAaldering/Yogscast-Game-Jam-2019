#pragma warning disable 0649
using MarchingSquares;
using ScriptableAudio;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Player
{
    [RequireComponent(typeof(CharacterController2D))]
    public class InputController : MonoBehaviour
    {
        [SerializeField] private float runSpeed = 40f;
        [SerializeField] private float mineRange = 0.5f;
        [SerializeField] private Transform origin;

        [SerializeField] private AudioEvent attackAudioEvent;
        [SerializeField] private AudioEvent mineAudioEvent;
        
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

            if (Input.GetButtonDown("Jump") || Input.GetAxisRaw("Vertical") > 0.1f)
            {
                jump = true;
                anim.SetTrigger("doJump");
            }

            if (Input.GetButtonDown("Mine") && !EventSystem.current.IsPointerOverGameObject() &&
                !anim.GetCurrentAnimatorStateInfo(0).IsName("Dwarf Mine L") &&
                !anim.GetCurrentAnimatorStateInfo(0).IsName("Dwarf Mine R"))
            {
                Mine();
                anim.SetTrigger("doMine");
                mineAudioEvent.Play(AudioManager.instance.effectSource);
            }
            
            if (Input.GetButtonDown("Attack") &&
                !anim.GetCurrentAnimatorStateInfo(0).IsName("Dwarf Attack L") &&
                !anim.GetCurrentAnimatorStateInfo(0).IsName("Dwarf Attack R"))
            {
                Attack();
                anim.SetTrigger("doAttack");
                attackAudioEvent.Play(AudioManager.instance.effectSource);
            }

            if (Input.GetButtonDown("Chant"))
            {
                StartCoroutine(AudioManager.instance.StartChant());
            }
            else if (Input.GetButtonUp("Chant"))
            {
                StartCoroutine(AudioManager.instance.EndChant());
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
