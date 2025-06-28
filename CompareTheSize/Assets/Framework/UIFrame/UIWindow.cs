using UnityEngine;
using UnityEngine.UI;

namespace Framework.UIFrame
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public abstract class UIWindow:MonoBehaviour
    {
        public abstract void OnShow(object param);
        public abstract void OnHide();
        
    }
}