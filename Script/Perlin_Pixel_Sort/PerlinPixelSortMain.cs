using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]

public class PerlinPixelSortMain : MonoBehaviour
{
    public ComputeShader fragShader;

    int nx;
    RenderTexture target;
    Camera cam;
    float totalTime;
    [SerializeField]
    public Texture2D[] texture;
    int id = 0;
    public Slider scaleMultSli;
    int count = 0;
    float seed = 0;
    //WebCamTexture webCamTexture;

    //public Slider scaleMultSli;
    //public Slider transitionSli;

    // Start is called before the first frame update
    void Start()
    {
        //webCamTexture = new WebCamTexture();
        //Debug.Log(webCamtexture[id].deviceName);
        //webCamtexture[id].Play();
        //Texture2D texture_ = new Texture2D(webCamtexture[id].width, webCamtexture[id].height);
        //texture_.SetPixels32(webCamtexture[id].GetPixels32());
        //texture = texture_;
        
        //Load texture2D here!
        /*string[] guids1 = AssetDatabase.FindAssets("t:texture2D", new[] {"Assets/Sprite/PDLandscape-1920x1080" });

        texture = new Texture2D[guids1.Length];
        int i=0;
        foreach (string guid1 in guids1)
        {
            Debug.Log(AssetDatabase.GUIDToAssetPath(guid1));
            texture[i++] = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guid1));
        }*/
    }

    void Init()
    {
        cam = Camera.current;
        //var resizedTexture = new Texture2D(cam.pixelWidth, cam.pixelHeight);
        //Graphics.ConvertTexture(texture, resizedTexture);
        //texture = resizedTexture;
        if (target == null || target.width != texture[id].width || target.height != texture[id].height)
        {
            if (target != null)
            {
                target.Release();
                //srctexture[id].Release();
            }
            target = new RenderTexture(texture[id].width, texture[id].height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.Create();

            //webCamTexture = new WebCamTexture(1920, 1080);
            //webCamTexture.Play();
            //Debug.Log(webCamTexture.deviceName);
            //webCamTexture.Play();
            //Texture2D texture_ = new Texture2D(webCamTexture.width, webCamtexture[id].height);
            //texture_.SetPixels32(webCamtexture[id].GetPixels32());
            //texture = texture_;

            Texture resizedTexture = new Texture2D(1000, 1000);
            Graphics.ConvertTexture(texture[id], resizedTexture);
            Graphics.Blit(resizedTexture, target);
            
            //MonoBehaviour.Destroy(resizedTexture);

        }
        
    }

    //based on this site https://github.com/SebLague/Ray-Marching/blob/master/Assets/Scripts/SDF/Master.cs
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        totalTime += Time.deltaTime;// * Mathf.Pow(10.0f, transitionSli.value);


        for (int i = 0; i < 1; i++)
        {
            Init();


            int threadGroupsX = Mathf.CeilToInt(texture[id].width / 8.0f / 2.0f);
            int threadGroupsY = Mathf.CeilToInt(texture[id].height / 8.0f);
            //Graphics.CopyTexture(target, srcTexture);
            //fragShader.SetTexture(fragShader.FindKernel("CSMain"), "Source", srcTexture);
            fragShader.SetTexture(fragShader.FindKernel("CSMain"), "Result", target);
            fragShader.SetFloat("off", seed);
            fragShader.SetInt("iteration", count++);
            fragShader.SetInt("width", cam.pixelWidth);
            fragShader.SetFloat("scaleMult", Mathf.Pow(10.0f, scaleMultSli.value));
            fragShader.SetFloat("height", cam.pixelHeight);
            //            kernel id
            fragShader.Dispatch(fragShader.FindKernel("CSMain"), threadGroupsX, threadGroupsY, 1);
            if (target.width != cam.pixelHeight || target.height! != cam.pixelHeight)
            {
                Texture resizedTarget = new Texture2D(cam.pixelWidth, cam.pixelHeight);
                Graphics.ConvertTexture(target, resizedTarget);
                Graphics.Blit(resizedTarget, destination);
                MonoBehaviour.Destroy(resizedTarget);
            }
            else
            {
                Graphics.Blit(target, destination);
            }

        }
    
    }

    // Update is called once per frame
    void Update()
    {
        //totalTime += Time.deltaTime;
        if (Input.GetMouseButtonDown(0))
        {
            seed = Random.Range(-100000, 100000);
            Texture resizedTexture = new Texture2D(1920, 1080);
            id = (id + 1)%texture.Length;
            Graphics.ConvertTexture(texture[id], resizedTexture);
            Graphics.Blit(resizedTexture, target);
            MonoBehaviour.Destroy(resizedTexture);
        }
    }

    public void Youtube()
    {
        Application.OpenURL("https://www.youtube.com/channel/UCR8NgQAy5F5iz7lb4AI9uPw");
    }

    public void GooglePlay()
    {
        Application.OpenURL("https://play.google.com/store/apps/dev?id=8989861170574890555");
    }
}
