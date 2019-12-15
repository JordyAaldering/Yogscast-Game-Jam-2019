#pragma warning disable 0649
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider effectSlider;

        private void Awake()
        {
            musicSlider.value = AudioManager.instance.musicSource.volume;
            effectSlider.value = AudioManager.instance.effectSource.volume;
        }

        public void StartGame()
        {
            SceneManager.LoadScene(1);
        }

        public void Exit()
        {
            Application.Quit();
        }

        public void SetMusicVolume(float value)
        {
            AudioManager.instance.musicSource.volume = value;
            AudioManager.instance.chantSource.volume = value;
        }
        
        public void SetEffectVolume(float value)
        {
            AudioManager.instance.effectSource.volume = value;
        }
    }
}
