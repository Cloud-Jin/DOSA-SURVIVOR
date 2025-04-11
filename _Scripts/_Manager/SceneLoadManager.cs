using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace ProjectM
{
    public class SceneLoadManager : Singleton<SceneLoadManager>
    {
        private string loadSceneName;

        private float progress;
        private float minTime = 2f; // 최소 시간
        
        protected override void Init()
        {
        }

        public void LoadScene(string sceneName)
        {
            Time.timeScale = 1f;
            SceneManager.sceneLoaded += LoadSceneEnd;
            loadSceneName = sceneName;
            SceneManager.LoadScene("_Scenes/LoadingScene");
            StartCoroutine(Load());
        }

        private IEnumerator Load()
        {
            
            {
                yield return null;
                AsyncOperation op = SceneManager.LoadSceneAsync(loadSceneName);
                op.allowSceneActivation = false;
                
                float timer = 0.0f;
                
                
                
                while (!op.isDone)
                {
                    yield return null;
                    timer += Time.deltaTime;
                    if (op.progress < 0.9f)
                    {
                     
                        progress = Mathf.Lerp(progress, op.progress, timer);
                        if (progress >= op.progress)
                        {
                            // timer = 0f;
                        }
                    }
                    else
                    {
                        progress = Mathf.Lerp(progress, 1f, timer);
                        if (progress >= 1.0f && timer > minTime)
                        {
                            op.allowSceneActivation = true;
                        }
                    }
                }
            }
        }

        private void LoadSceneEnd(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.name == loadSceneName)
            {
                SceneManager.sceneLoaded -= LoadSceneEnd;
            }
        }
    }
}