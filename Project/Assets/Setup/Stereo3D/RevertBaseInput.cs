using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
namespace UnityEngine.EventSystems
{
    /// <summary>
    /// Interface to the Input system used by the BaseInputModule. With this it is possible to bypass the Input system with your own but still use the same InputModule. For example this can be used to feed fake input into the UI or interface with a different input system.
    /// </summary>
    public class RevertBaseInput : BaseInput
    {
        public FakeMouse mouse;
        /// <summary>
        /// Interface to Input.compositionCursorPos. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public override Vector2 compositionCursorPos
        {
            get
            {
                Vector2 newMousePos;
                if (mouse.isRevert)
                    newMousePos = new Vector2(Screen.width - Input.compositionCursorPos.x, Input.compositionCursorPos.y);
                else
                    newMousePos = Input.compositionCursorPos;
                //return Input.compositionCursorPos;
                return newMousePos;
            }
            set { Input.compositionCursorPos = value; }
        }
        /// <summary>
        /// Interface to Input.mousePosition. Can be overridden to provide custom input instead of using the Input class.
        /// </summary>
        public override Vector2 mousePosition
        {
            get
            {
                Vector2 newMousePos;
                if (mouse.isRevert)
                    newMousePos = new Vector2(Screen.width - Input.mousePosition.x, Input.mousePosition.y);
                else
                    newMousePos = Input.mousePosition;
                return newMousePos;
            }
        }

        protected override void Start()
        {
            base.Start();

            BaseInputModule module = this.GetComponent<BaseInputModule>();

            if (module != null)
            {

                var input = module.GetType().GetField("m_InputOverride", BindingFlags.NonPublic | BindingFlags.Instance);
                input.SetValue(module, this);
            }
        }
    }
}