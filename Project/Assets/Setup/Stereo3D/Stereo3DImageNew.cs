using UnityEngine;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Rendering.Universal;
//using UnityEngine.Rendering.HighDefinition;
#if USE_URP
using UnityEngine.Rendering.URP;
#endif
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif
using UnityEngine.UI;
using System;
using Aspose.Pdf.Structure;


public class Stereo3DImageNew : MonoBehaviour
{

    [Header("Settings")]
    public bool S3DEnabled = true; //mono or Stereo3D enabled
    public bool isRevert = false;

    public float hFOV = 90; //horizontal Field Of View
    public float IPA = 0;
    public float IPD;
    public bool GuiVisible = true; //GUI window visible or not on the start
    public KeyCode GuiKey = KeyCode.Tab; //GUI window show/hide Key
    public StereoType stereoType = StereoType.Col; //Type of Interleaved Stereo3D output method
    public float distance;
    public float height;
    public Transform settingCanvas;
    private Transform setting;
    [Header("RenderImage")]
    private Image RenderImage;
    public Material colMaterial;
    public Material rowMaterial;
    public Material sideBySideMaterial;
    public Material topBottomMaterial;
    private Material imageMaterial;

    private Toggle Active3DToggle;

    private Toggle ActiveRevert;
    private Toggle horizontalToggle;
    private Toggle verticalToggle;
    private Toggle sideBySideToggle;
    private Toggle topBottomToggle;

    private Slider IPDSlider;
    private Slider IPASlider;
    private Slider FOVSlider;

    private Text IPDText;
    private Text IPAText;
    private Text FOVText;
    private Text distanceText;
    private Text verticalOffsetText;

    private GameObject mouseImg;

    //float disConstant = 0;

    Camera cam;
    Camera leftCam;
    Camera rightCam;
    RenderTexture leftCamRT;
    RenderTexture rightCamRT;
    int cullingMask;
    float nearClip;


    bool lastS3DEnabled;
    bool lastRevert;
    float lastIPD;
    float lastHFOV;
    float lastIPA;
    StereoType lastStereo;
    float lastDistance;
    float lastHeight;

    private float moveOffset = .01f;



#if UNITY_POST_PROCESSING_STACK_V2
    PostProcessLayer PPLayer;
    bool PPLayerStatus;
#endif

    IEnumerator Delay(float time, Action action)
    {
        yield return new WaitForSeconds(time);
        action();
    }
    void Init()
    {
        GetCurrentPipeline();
        Cursor.visible = false;
        RenderImage = settingCanvas.Find("Image").GetComponent<Image>();
        imageMaterial = RenderImage.material;
        setting = settingCanvas.Find("Setting");
        Active3DToggle = setting.Find("Layout/lay1/Active3D").GetComponent<Toggle>();
        ActiveRevert = setting.Find("Layout/lay1/ActiveRevert").GetComponent<Toggle>();
        verticalToggle = setting.Find("Layout/lay2/Vertical").GetComponent<Toggle>();
        horizontalToggle = setting.Find("Layout/lay2/Horizontal").GetComponent<Toggle>();
        sideBySideToggle = setting.Find("Layout/lay2/SideBySide").GetComponent<Toggle>();
        topBottomToggle = setting.Find("Layout/lay2/TopBottom").GetComponent<Toggle>();
        IPDSlider = setting.Find("Layout/lay3/IPD").GetComponent<Slider>();
        IPDText = setting.Find("Layout/lay3/Text").GetComponent<Text>();
        IPASlider = setting.Find("Layout/lay4/IPA").GetComponent<Slider>();
        IPAText = setting.Find("Layout/lay4/Text").GetComponent<Text>();
        FOVSlider = setting.Find("Layout/lay5/FOV").GetComponent<Slider>();
        FOVText = setting.Find("Layout/lay5/Text").GetComponent<Text>();
        mouseImg = settingCanvas.Find("Setting/Mouse").gameObject;
        distanceText = setting.Find("Layout/lay6/Distance").GetComponent<Text>();
        distanceText.text = Vector3.Distance(transform.position, new Vector3(0, transform.position.y, 0)).ToString();
        verticalOffsetText = setting.Find("Layout/lay6/Vertical").GetComponent<Text>();
        verticalOffsetText.text = transform.position.y.ToString();
        //disConstant = Vector3.Distance(transform.position, aim.position);
        cam = GetComponent<Camera>();

#if UNITY_POST_PROCESSING_STACK_V2
        if (GetComponent<PostProcessLayer>())
        {
            PPLayer = GetComponent<PostProcessLayer>();
            PPLayerStatus = PPLayer.enabled;

            if (PPLayerStatus)
                setMatrixDirectly = false;

        }
#endif

        cullingMask = cam.cullingMask;
        nearClip = cam.nearClipPlane;
        leftCam = new GameObject("leftCam").AddComponent<Camera>();
        rightCam = new GameObject("rightCam").AddComponent<Camera>();
        leftCam.tag = "MainCamera";
        rightCam.tag = "MainCamera";


        leftCam.CopyFrom(cam);
        rightCam.CopyFrom(cam);
        leftCam.usePhysicalProperties = rightCam.usePhysicalProperties = cam.usePhysicalProperties;

        leftCam.targetDisplay = 0;
        rightCam.targetDisplay = 1;
        // ����Ƿ��ж���һ����ʾ������
        if (Display.displays.Length > 1)
        {
            for (int i = 0; i < Display.displays.Length; i++)
            {
                Display.displays[i].Activate(); // ����ÿ����ʾ��
            }
        }

        //#if USE_URP
        var leftCamData = leftCam.GetUniversalAdditionalCameraData();
        if (leftCamData != null)
        {
            leftCamData.renderPostProcessing = true;
            leftCamData.antialiasing = cam.GetUniversalAdditionalCameraData().antialiasing;
            leftCamData.antialiasingQuality = cam.GetUniversalAdditionalCameraData().antialiasingQuality;
        }

        var rightCamData = rightCam.GetUniversalAdditionalCameraData();
        if (rightCamData != null)
        {
            rightCamData.renderPostProcessing = true;
            rightCamData.antialiasing = cam.GetUniversalAdditionalCameraData().antialiasing;
            rightCamData.antialiasingQuality = cam.GetUniversalAdditionalCameraData().antialiasingQuality;
        }

        // #if USE_HDRP
        // var leftCameraData = leftCam.gameObject.AddComponent<HDAdditionalCameraData>();
        // //Debug.Log(leftCameraData);
        // if (leftCameraData != null)
        // {
        //     leftCameraData.antialiasing = cam.GetComponent<HDAdditionalCameraData>().antialiasing;
        //     leftCameraData.allowDynamicResolution = true;
        //     leftCameraData.allowDeepLearningSuperSampling = true;
        // }
        // var rightCameraData = rightCam.gameObject.AddComponent<HDAdditionalCameraData>();
        // if (rightCameraData != null)
        // {
        //     rightCameraData.antialiasing = cam.GetComponent<HDAdditionalCameraData>().antialiasing;
        //     rightCameraData.allowDynamicResolution = true;
        //     rightCameraData.allowDeepLearningSuperSampling = true;
        // }

        // #endif


        //#endif
        leftCam.depth = rightCam.depth = cam.depth;
        leftCam.transform.parent = rightCam.transform.parent = transform;

#if UNITY_POST_PROCESSING_STACK_V2
        if (PPLayerStatus)
        {
            PostProcessResources processResources = Resources.Load<PostProcessResources>("PostProcessResources");
            PostProcessLayer left = leftCam.gameObject.AddComponent<PostProcessLayer>();
            left.volumeLayer = 1 << 1;
            left.volumeTrigger = left.transform;
            left.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
            PostProcessLayer right = rightCam.gameObject.AddComponent<PostProcessLayer>();
            left.Init(processResources);
            right.volumeLayer = 1 << 1;
            right.volumeTrigger = right.transform;
            right.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
            right.Init(processResources);
        }
#endif

        lastS3DEnabled = S3DEnabled;
        lastRevert = isRevert;
        lastStereo = stereoType;

        lastHFOV = hFOV;
        lastIPA = IPA;
        lastIPD = IPD;
        //lastHeight = height;
        //lastDistance = distance;

        //transform.position = new Vector3(transform.position.x, lastHeight, lastDistance);
        setting.gameObject.SetActive(GuiVisible);

        if (GuiVisible)
        {
            if (lastRevert)
            {
                //mouseImg.gameObject.SetActive(true);
                mouseImg.GetComponent<FakeMouse>().isRevert = true;
                Cursor.SetCursor(mouseImg.GetComponent<FakeMouse>().t, Vector2.zero, CursorMode.Auto);
            }
            else
            {
                //mouseImg.gameObject.SetActive(false);
                mouseImg.GetComponent<FakeMouse>().isRevert = false;
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }
        else
        {
            //mouseImg.gameObject.SetActive(false);
            mouseImg.GetComponent<FakeMouse>().isRevert = false;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            Cursor.visible = false;
        }
        IPDText.text = lastIPD.ToString();
        IPAText.text = lastIPA.ToString();
        FOVText.text = lastHFOV.ToString();

        Active3DToggle.isOn = lastS3DEnabled == true;
        RenderImage.gameObject.SetActive(lastS3DEnabled);

        Active3DToggle.onValueChanged.AddListener((isActive) =>
        {
            S3DEnabled = isActive;
            RenderImage.gameObject.SetActive(S3DEnabled);
        });
        verticalToggle.isOn = lastStereo == StereoType.Col;
        verticalToggle.onValueChanged.AddListener((isActive) =>
        {
            if (!S3DEnabled) return;
            if (isActive)
            {
                IPDSlider.value = 0;
                IPASlider.value = 0;

                stereoType = StereoType.Col;

                InitStereoSetup();
            }
        });
        horizontalToggle.isOn = lastStereo == StereoType.Row;
        horizontalToggle.onValueChanged.AddListener((isActive) =>
        {
            if (!S3DEnabled) return;
            if (isActive)
            {
                IPDSlider.value = 0;
                IPASlider.value = 0;

                stereoType = StereoType.Row;

                InitStereoSetup();
            }
        });
        sideBySideToggle.isOn = lastStereo == StereoType.SideBySide;
        sideBySideToggle.onValueChanged.AddListener((isActive) =>
        {
            if (!S3DEnabled) return;
            if (isActive)
            {
                IPDSlider.value = 0;
                IPASlider.value = 0;

                stereoType = StereoType.SideBySide;

                InitStereoSetup();
            }
        });
        topBottomToggle.isOn = lastStereo == StereoType.TopBottom;
        topBottomToggle.onValueChanged.AddListener((isActive) =>
        {
            if (!S3DEnabled) return;
            if (isActive)
            {
                IPDSlider.value = 0;
                IPASlider.value = 0;

                stereoType = StereoType.TopBottom;

                InitStereoSetup();
            }
        });


        setting.localScale = new Vector3(lastRevert ? -1 : 1, 1, 1);
        RenderImage.transform.localScale = new Vector3(lastRevert ? -1 : 1, 1, 1);
        ActiveRevert.isOn = lastRevert == true;
        //mouseImg.SetActive(lastRevert);
        mouseImg.GetComponent<FakeMouse>().isRevert = lastRevert;
        ActiveRevert.onValueChanged.AddListener((bool isActive) =>
        {
            setting.localScale = new Vector3(isActive ? -1 : 1, 1, 1);
            RenderImage.transform.localScale = new Vector3(isActive ? -1 : 1, 1, 1);
            lastRevert = isRevert = isActive;
            mouseImg.GetComponent<FakeMouse>().isRevert = isActive;

        });

        IPDSlider.value = lastIPD;
        IPDSlider.onValueChanged.AddListener((v) =>
        {
            IPD = v;
            IPDText.text = v.ToString();
        });
        IPASlider.value = lastIPA;
        IPASlider.onValueChanged.AddListener((v) =>
        {
            IPA = v;
            IPAText.text = string.Format("{0:F2}", v);
        });
        FOVSlider.value = lastHFOV;
        leftCam.fieldOfView = rightCam.fieldOfView = cam.fieldOfView = lastHFOV;
        FOVSlider.onValueChanged.AddListener((v) =>
        {
            Debug.Log(v);
            hFOV = v;
            FOVText.text = v.ToString();
            leftCam.fieldOfView = rightCam.fieldOfView = cam.fieldOfView = hFOV;
        });

        InitStereoSetup();
    }
    private bool firstIn = true;
    private bool initComplete = false;
    void OnEnable()
    {
        if (firstIn)
        {
            firstIn = false;
            StartCoroutine(Delay(1, () =>
            {
                Init();
            }));
        }
        else
        {
            Init();
        }
    }

    void Update()
    {

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.position += Vector3.forward * moveOffset;
            distanceText.text = Vector3.Distance(transform.position, new Vector3(0, transform.position.y, 0)).ToString();
            lastHeight = height = transform.position.z;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.position -= Vector3.forward * moveOffset;
            distanceText.text = Vector3.Distance(transform.position, new Vector3(0, transform.position.y, 0)).ToString();

            lastDistance = distance = transform.position.z;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.position += Vector3.up * moveOffset;
            verticalOffsetText.text = transform.position.y.ToString();
            lastHeight = height = transform.position.y;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.position -= Vector3.up * moveOffset;
            verticalOffsetText.text = transform.position.y.ToString();
            lastHeight = height = transform.position.y;
        }

        if (Input.GetKeyDown(GuiKey))
        {
            GuiVisible = !GuiVisible;
            setting.gameObject.SetActive(GuiVisible);
            if (GuiVisible)
            {
                if (lastRevert)
                {
                    mouseImg.GetComponent<FakeMouse>().isRevert = true;

                }
                else
                {
                    mouseImg.GetComponent<FakeMouse>().isRevert = false;
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                }
            }
            else
            {
                mouseImg.GetComponent<FakeMouse>().isRevert = false;
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                Cursor.visible = false;
            }
        }
        if (cam == null || leftCam == null || rightCam == null)
        {
            return;
        }

        //check variable changes after Keys pressed
        if (lastS3DEnabled != S3DEnabled)
        {
            lastS3DEnabled = S3DEnabled;
            InitStereoSetup();
        }



        if (lastIPD != IPD)
        {
            lastIPD = IPD;
            CamPositionSet();
        }

        if (lastIPA != IPA)
        {

            lastIPA = IPA;
            CamRotationSet();
        }


        if (lastStereo != stereoType)
        {

            lastStereo = stereoType;
            InitStereoSetup();
        }


        if (lastHFOV != hFOV)
        {

            lastHFOV = hFOV;
            leftCam.fieldOfView = rightCam.fieldOfView = cam.fieldOfView = lastHFOV;
        }
    }
    private float leftAspect;
    private float rightAspect;
    void InitStereoSetup()
    {
        ReleaseRT();
        if (S3DEnabled)
        {

            //cam.cullingMask = 0;
            //cam.enabled = false;
            leftCam.enabled = true;
            rightCam.enabled = true;

            if (leftCamRT == null)
            {
                // if (stereoType == StereoType.SideBySide)
                // {
                //     leftCamRT = new RenderTexture(Screen.width / 2, Screen.height, 16, RenderTextureFormat.DefaultHDR);
                // }
                // else if (stereoType == StereoType.TopBottom)
                // {
                //     leftCamRT = new RenderTexture(Screen.width, Screen.height / 2, 16, RenderTextureFormat.DefaultHDR);
                // }
                // else
                {
                    leftCamRT = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.DefaultHDR);
                }
                leftCamRT.Create();
            }

            if (rightCamRT == null)
            {
                // if (stereoType == StereoType.SideBySide)
                // {
                //     rightCamRT = new RenderTexture(Screen.width / 2, Screen.height, 16, RenderTextureFormat.DefaultHDR);
                // }
                // else if (stereoType == StereoType.TopBottom)
                // {
                //     rightCamRT = new RenderTexture(Screen.width, Screen.height / 2, 16, RenderTextureFormat.DefaultHDR);
                // }
                // else
                {
                    rightCamRT = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.DefaultHDR);
                }
                rightCamRT.Create();
            }

            leftCam.targetDisplay = 0;
            rightCam.targetDisplay = 0;
            //float distance = Vector3.Distance(transform.position, aim.position);
            float percent = distance / 1;
            percent = Mathf.Log10(percent * 10 + 1);
            leftCam.transform.localPosition = new Vector3((-IPD / 2) * 0.01f, 0, 0);
            leftCam.transform.localEulerAngles = new Vector3(0, -IPA * percent, 0);

            rightCam.transform.localPosition = new Vector3((IPD / 2) * 0.01f, 0, 0);
            rightCam.transform.localEulerAngles = new Vector3(0, IPA * percent, 0);
            if (stereoType == StereoType.Col)
            {
                RenderImage.material = colMaterial;
                leftCam.rect = rightCam.rect = new Rect(0, 0, 0.5f, 1f);
                leftCam.ResetAspect();
                rightCam.ResetAspect();
                leftCam.aspect *= 2f;
                rightCam.aspect *= 2f;
            }
            else if (stereoType == StereoType.Row)
            {
                RenderImage.material = rowMaterial;
                leftCam.rect = rightCam.rect = new Rect(0, 0, 1f, 0.5f);
                leftCam.ResetAspect();
                rightCam.ResetAspect();
                leftCam.aspect *= 0.5f;
                rightCam.aspect *= 0.5f;
            }
            else if (stereoType == StereoType.SideBySide)
            {
                RenderImage.material = sideBySideMaterial;
                if (isRevert)
                {
                    leftCam.rect = new Rect(0, 0, 0.5f, 1);
                    rightCam.rect = new Rect(0.5f, 0, 0.5f, 1);
                }
                else
                {
                    leftCam.rect = new Rect(0.5f, 0, 0.5f, 1);
                    rightCam.rect = new Rect(0f, 0, 0.5f, 1);
                }
                // leftCam.rect = new Rect(0, 0, 1f, 1);
                // rightCam.rect = new Rect(0f, 0, 1f, 1);
                leftCam.ResetAspect();
                rightCam.ResetAspect();
                leftCam.aspect *= 2f;
                rightCam.aspect *= 2f;
            }
            else if (stereoType == StereoType.TopBottom)
            {
                RenderImage.material = topBottomMaterial;

                if (isRevert)
                {
                    leftCam.rect = new Rect(0, 0, 1f, 0.5f);
                    rightCam.rect = new Rect(0f, 0.5f, 1f, 0.5f);
                }
                else
                {
                    leftCam.rect = new Rect(0f, 0.5f, 1f, 0.5f);
                    rightCam.rect = new Rect(0, 0, 1f, 0.5f);
                }
                // leftCam.rect = new Rect(0, 0, 1f, 1);
                // rightCam.rect = new Rect(0f, 0, 1f, 1);
                leftCam.ResetAspect();
                rightCam.ResetAspect();
                leftCam.aspect /= 2f;
                rightCam.aspect /= 2f;
            }

            leftCam.cullingMask = cullingMask;
            rightCam.cullingMask = cullingMask;


            if (stereoType == StereoType.Col | stereoType == StereoType.Row)
            {
                leftCam.targetTexture = leftCamRT;
                rightCam.targetTexture = rightCamRT;
                imageMaterial = RenderImage.material;
                RenderImage.enabled = true;
                imageMaterial.SetFloat("_Columns", Screen.width);
                imageMaterial.SetFloat("_Rows", Screen.height);
                imageMaterial.SetTexture("_LeftTex", leftCamRT);
                imageMaterial.SetTexture("_RightTex", rightCamRT);
                imageMaterial.SetTextureOffset("_LeftTex", new Vector2(0, 0));
                imageMaterial.SetTextureOffset("_RightTex", new Vector2(0, 0));
                imageMaterial.SetTextureScale("_LeftTex", new Vector2(1, 1));
                imageMaterial.SetTextureScale("_RightTex", new Vector2(1, 1));
            }
            else
            {
                // imageMaterial.SetTexture("_LeftTex", leftCamRT);
                // imageMaterial.SetTexture("_RightTex", rightCamRT);
                // imageMaterial.SetTextureOffset("_LeftTex", new Vector2(0, 0));
                // imageMaterial.SetTextureOffset("_RightTex", new Vector2(0, 0));
                // imageMaterial.SetTextureScale("_LeftTex", new Vector2(1, 1));
                // imageMaterial.SetTextureScale("_RightTex", new Vector2(1, 1));
                RenderImage.enabled = false;
            }
        }

    }

    public void CamRotationSet()
    {
        if (cam == null || leftCam == null || rightCam == null)
        {
            return;
        }
        Vector3 leftCamRot;
        Vector3 rightCamRot;
        // if (stereoType == StereoType.Col)
        // {
        //     leftCamRot = Vector3.up * IPA;
        //     rightCamRot = -Vector3.up * IPA;
        // }
        // else
        // {
        //     leftCamRot = -Vector3.right * IPA;
        //     rightCamRot = Vector3.right * IPA;
        // }
        leftCamRot = -Vector3.up * IPA;
        rightCamRot = Vector3.up * IPA;
        // Debug.Log("leftCamRot:" + leftCamRot);
        // Debug.Log("rightCamRot:" + rightCamRot);
        leftCam.transform.localEulerAngles = leftCamRot;
        rightCam.transform.localEulerAngles = rightCamRot;
    }

    void CamPositionSet()
    {
        if (cam == null || leftCam == null || rightCam == null)
        {
            return;
        }
        Vector3 leftCamPos;
        Vector3 rightCamPos;
        // if (stereoType == StereoType.Col)
        // {
        //     leftCamPos = Vector3.left * (IPD / 2) * 0.01f;
        //     rightCamPos = Vector3.right * (IPD / 2) * 0.01f;
        // }
        // else
        // {
        //     leftCamPos = Vector3.up * (IPD / 2) * 0.01f;
        //     rightCamPos = Vector3.down * (IPD / 2) * 0.01f;
        // }
        leftCamPos = Vector3.left * (IPD / 2) * 0.01f;
        rightCamPos = Vector3.right * (IPD / 2) * 0.01f;
        leftCam.transform.localPosition = leftCamPos;
        rightCam.transform.localPosition = rightCamPos;
    }

    void OnDisable()
    {
        IPDSlider.onValueChanged.RemoveAllListeners();
        IPASlider.onValueChanged.RemoveAllListeners();
        FOVSlider.onValueChanged.RemoveAllListeners();
        Active3DToggle.onValueChanged.RemoveAllListeners();
        verticalToggle.onValueChanged.RemoveAllListeners();
        ReleaseRT();
        Destroy(leftCam.gameObject);
        Destroy(rightCam.gameObject);
        Resources.UnloadUnusedAssets(); //free memory
    }

    void ReleaseRT()
    {

        if (leftCam == null || rightCam == null)
        {
            return;
        }
        leftCam.targetTexture = null;
        rightCam.targetTexture = null;
        leftCam.enabled = false;
        rightCam.enabled = false;
        //cam.cullingMask = cullingMask;
        cam.enabled = true;
        cam.nearClipPlane = nearClip;
        leftCam.ResetAspect();
        rightCam.ResetAspect();

        if (leftCamRT != null)
        {
            leftCamRT.Release();
        }
        if (rightCamRT != null)
        {
            rightCamRT.Release();
        }
        leftCamRT = null;
        rightCamRT = null;
#if UNITY_POST_PROCESSING_STACK_V2
        if (PPLayer)
            PPLayer.enabled = PPLayerStatus;
#endif
    }

    public Pipeline currentPipeline;
    public Pipeline GetCurrentPipeline()
    {
        if (GraphicsSettings.defaultRenderPipeline == null)
        {
            currentPipeline = Pipeline.BuildIn;
            return Pipeline.BuildIn;
        }
        else
        {
            if (GraphicsSettings.defaultRenderPipeline.name.Contains("HDRenderPipelineAsset"))
            {
                currentPipeline = Pipeline.HDRP;
            }
            else if (GraphicsSettings.defaultRenderPipeline.name.Contains("UniversalRenderPipelineAsset"))
            {
                currentPipeline = Pipeline.URP;
            }
        }
        return Pipeline.BuildIn;
    }

}

public enum Pipeline
{
    BuildIn,
    URP,
    HDRP
}
