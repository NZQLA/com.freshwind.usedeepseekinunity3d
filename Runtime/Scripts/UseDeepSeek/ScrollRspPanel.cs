using UnityEngine;
using UnityEngine.UI;


namespace Business
{
    /// <summary>
    /// Scroll the rsp panel on mouse scrolling
    /// </summary>
    public class ScrollRspPanel : MonoBehaviour
    {
        [SerializeField]
        protected ScrollRect rspPanel;

        public float scrollSpeed = 30;

        [SerializeField]
        protected string mouseScrollWheel = "Mouse ScrollWheel";


        public void Update()
        {
            ScrollOnMouseScrolling();
        }


        public void ScrollOnMouseScrolling()
        {
            var scroll = Input.GetAxis(mouseScrollWheel);

            var sv = scroll * Time.deltaTime * scrollSpeed;

            rspPanel.verticalNormalizedPosition = Mathf.Clamp01(rspPanel.verticalNormalizedPosition + sv);
        }

    }
}