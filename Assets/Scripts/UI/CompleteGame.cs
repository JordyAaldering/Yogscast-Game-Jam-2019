using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class CompleteGame : MonoBehaviour
    {
        public void Complete()
        {
            SceneManager.LoadScene(2);
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}
