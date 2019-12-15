#pragma warning disable 0649
using System.Collections;
using MarchingSquares;
using ScriptableAudio;
using TMPro;
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

        [SerializeField] private float textTime = 5f;
        [SerializeField] private TextMeshProUGUI dialogue;
        
        [SerializeField] private AudioEvent jumpAudioEvent;
        [SerializeField] private AudioEvent attackAudioEvent;
        [SerializeField] private AudioEvent mineAudioEvent;

        [SerializeField] private GameObject jumpParticle;
        
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
            controller.OnLandEvent.AddListener(OnLand);
            
            anim = GetComponentInChildren<Animator>();

            StartCoroutine(DisableText());
        }

        private IEnumerator DisableText()
        {
            yield return new WaitForSeconds(textTime);
            dialogue.gameObject.SetActive(false);
        }

        private void Update()
        {
            horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
            anim.SetFloat("horizontalMove", horizontalMove);

            if (Input.GetButtonDown("Jump") || Input.GetAxisRaw("Vertical") > 0.1f)
            {
                jump = true;
                anim.SetTrigger("doJump");
                jumpAudioEvent.Play(AudioManager.instance.effectSource);
                Instantiate(jumpParticle, controller.groundCheck.position, Quaternion.identity);
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
        
        private void Mine()
        {
            Vector2 position = origin.transform.position;
            Vector2 worldMousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = worldMousePosition - position;
            direction.Normalize();
            
            voxelMap.EditVoxels(position + direction * mineRange);
        }
        
        private void Attack()
        {
            Vector2 position = origin.transform.position;
            Vector2 worldMousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = worldMousePosition - position;
            direction.Normalize();
        }

        private void OnLand()
        {
            anim.SetTrigger("doLand");
            jumpAudioEvent.Play(AudioManager.instance.effectSource);
            Instantiate(jumpParticle, controller.groundCheck.position, Quaternion.identity);
        }
    }
}
