using System;
using System.Collections;
using System.Collections.Generic;
using Game.Scripts.GameRoot.Services.Analytics;
using Game.Scripts.GameRoot.Services.Popups;
using UnityEngine.SceneManagement;

namespace Game.Scripts.GameRoot.Services.Scenes
{
    public class SceneService : ISceneService
    {
        private readonly IAnalyticEventService _analyticEventService;
        private readonly UIRootView _uiRoot;

        public SceneService(IAnalyticEventService analyticEventService, UIRootView uiRoot)
        {
            _analyticEventService = analyticEventService;
            _uiRoot = uiRoot;
        }

        public void LoadScene(string sceneName)
        {
            LogSceneChange(sceneName);
            SceneManager.LoadScene(sceneName);
        }

        public void LoadScene(string sceneName, Action onLoadStart, Action onComplete)
        {
            _uiRoot.StartCoroutine(LoadSceneAsync(sceneName, onLoadStart, onComplete));
        }

        public bool IsCurrentScene(string sceneName)
        {
            return SceneManager.GetActiveScene().name == sceneName;
        }
        
        private void LogSceneChange(string toScene)
        {
            var ev = new Dictionary<string, string>
            {
                { "from", SceneManager.GetActiveScene().name },
                { "to", toScene }
            };
            
            _analyticEventService.SendEvent(AnalyticEventType.SceneSwitched, ev);
        }
        
        private IEnumerator LoadSceneAsync(string sceneName, Action onLoadStart, Action onComplete)
        {
            onLoadStart?.Invoke();
            
            LogSceneChange(sceneName);
            
            var asyncOp = SceneManager.LoadSceneAsync(sceneName);
            
            while (!asyncOp.isDone)
            {
                yield return null;
            }
            
            onComplete?.Invoke();
        }
    }
}