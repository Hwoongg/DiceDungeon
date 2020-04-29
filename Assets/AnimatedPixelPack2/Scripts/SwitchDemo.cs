using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BlankSourceCode.AnimatedPixelPack2
{
    public class SwitchDemo : MonoBehaviour
    {
        /// <summary>
        /// Open the next demo scene
        /// </summary>
        public void NextDemo()
        {
            int current = SceneManager.GetActiveScene().buildIndex;
            current++;
            if (current >= SceneManager.sceneCountInBuildSettings)
            {
                current = 0;
            }
            SceneManager.LoadScene(current);
        }
    }
}
