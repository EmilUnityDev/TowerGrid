using System;

namespace Game.Scripts.GameRoot.Services.Scenes
{
    public interface ISceneService
    {
        void LoadScene(string sceneName);
        void LoadScene(string sceneName, Action onLoadStart, Action onComplete);
        bool IsCurrentScene(string sceneName);
    }
}