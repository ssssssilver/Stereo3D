using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FakeMouse : MonoBehaviour
{
    public Texture2D t;
    private RectTransform root;
    private RectTransform mouseRect;
    public GraphicRaycaster graphicRaycaster;
    void Start()
    {
        root = transform.parent.GetComponent<RectTransform>();
        mouseRect = GetComponent<RectTransform>();
    }
    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {
        Cursor.SetCursor(t, Vector2.zero, CursorMode.Auto);
    }

    /// <summary>
    /// This function is called when the behaviour becomes disabled or inactive.
    /// </summary>
    void OnDisable()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
    List<RaycastResult> resultAppendList = new List<RaycastResult>();
    // Update is called once per frame
    void Update()
    {
        ScreenPointToUIPoint();
        // if (Input.GetMouseButtonDown(0))
        // {

        //     StartCoroutine(SimalateMouse());
        // }
        // if (Input.GetMouseButton(0))
        // {
        //     Vector2 screen = new Vector2(Screen.width - Input.mousePosition.x, Input.mousePosition.y);

        //     // ExecuteEvents.Execute(gameObject,);
        //     PointerEventData data = new PointerEventData(EventSystem.current);
        //     data.position = screen;
        //     resultAppendList.Clear();
        //     graphicRaycaster.Raycast(data, resultAppendList);
        //     if (resultAppendList.Count != 0)
        //         foreach (RaycastResult i in resultAppendList)
        //         {
        //             //Debug.Log(i.gameObject);
        //             if (i.gameObject.transform.parent.GetComponent<Slider>() != null)
        //             {
        //                 ExecuteEvents.Execute(i.gameObject.transform.parent.gameObject, data, ExecuteEvents.dragHandler);
        //             }

        //         }
        // }
    }

    IEnumerator SimalateMouse()
    {
        yield return 0;
        Vector2 screen = new Vector2(Screen.width - Input.mousePosition.x, Input.mousePosition.y);

        // ExecuteEvents.Execute(gameObject,);
        PointerEventData data = new PointerEventData(EventSystem.current);
        data.position = screen;
        resultAppendList.Clear();
        graphicRaycaster.Raycast(data, resultAppendList);
        if (resultAppendList.Count != 0)
            foreach (RaycastResult i in resultAppendList)
            {
                //Debug.Log(i.gameObject);
                if (i.gameObject.transform.parent.GetComponent<Toggle>() != null)
                {
                    ExecuteEvents.Execute(i.gameObject.transform.parent.gameObject, data, ExecuteEvents.beginDragHandler);
                    ExecuteEvents.Execute(i.gameObject.transform.parent.gameObject, data, ExecuteEvents.pointerEnterHandler);
                    ExecuteEvents.Execute(i.gameObject.transform.parent.gameObject, data, ExecuteEvents.submitHandler);
                    yield return new WaitForSeconds(.1f);
                    //ExecuteEvents.Execute(i.gameObject, data, ExecuteEvents.pointerUpHandler);
                    ExecuteEvents.Execute(i.gameObject.transform.parent.gameObject, data, ExecuteEvents.pointerExitHandler);
                }

            }
    }

    public Vector2 screenPos;
    public bool isRevert;

    // 屏幕坐标转换为 UGUI 坐标
    public void ScreenPointToUIPoint()
    {
        Vector3 globalMousePos;
        Vector3 screen;
        if (isRevert)
            screen = new Vector3(Screen.width - Input.mousePosition.x, Input.mousePosition.y, 0);
        else
            screen = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);

        RectTransformUtility.ScreenPointToWorldPointInRectangle(mouseRect, screen, null, out globalMousePos);

        mouseRect.position = globalMousePos;
    }
}
