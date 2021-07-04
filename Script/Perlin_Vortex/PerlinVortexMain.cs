using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]

public class PerlinVortexMain : MonoBehaviour
{
    public ComputeShader fragShader;

    int n = 100000;
    RenderTexture target;
    Camera cam;
    float totalTime;
    public Slider scaleMultSli;
    public Slider transitionSli;
    

    ComputeBuffer particleBuffer;
    List<ComputeBuffer> buffersToDispose;
    Particle[] particles = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Init()
    {
        cam = Camera.current;
        if (target == null || target.width != cam.pixelWidth || target.height != cam.pixelHeight)
        {
            if (target != null)
            {
                target.Release();
            }
            target = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.Create();
        }
    } 

    void CreateScene()
    {
        if (particles == null)
        {

            particles = new Particle[n];
            for (int i = 0; i < n; i++)
            {
                particles[i] = new Particle()
                {
                    pos = new Vector2(Random.Range(0, cam.pixelWidth), Random.Range(0, cam.pixelHeight)),
                    //pos = new Vector2(Random.Range(0, 100), Random.Range(0, 100)),
                    speed = 1f
                };
            }
        }
        particleBuffer = new ComputeBuffer(particles.Length, Particle.GetSize());
        particleBuffer.SetData(particles);
        fragShader.SetBuffer(fragShader.FindKernel("Particles"), "particles", particleBuffer);
        buffersToDispose.Add(particleBuffer);



    }

    //based on this site https://github.com/SebLague/Ray-Marching/blob/master/Assets/Scripts/SDF/Master.cs
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        buffersToDispose = new List<ComputeBuffer>();
        Init();
        if (Input.GetMouseButton(0))
        {
            fragShader.SetFloats("touchPos", Input.mousePosition.x, Input.mousePosition.y);
        }
        else
        {
            fragShader.SetFloats("touchPos", -1, -1);
        }
        CreateScene();

        int threadGroupsX = Mathf.CeilToInt(cam.pixelWidth / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(cam.pixelHeight / 8.0f);

        fragShader.SetTexture(fragShader.FindKernel("CSMain"), "Result", target);
        fragShader.SetTexture(fragShader.FindKernel("Particles"), "Result", target);
        fragShader.SetFloat("time", totalTime);
        fragShader.SetFloat("height", cam.pixelHeight);
        fragShader.SetFloat("width", cam.pixelWidth);
        fragShader.SetFloat("scaleMult", Mathf.Pow(10.0f, scaleMultSli.value));
        //fragShader.SetFloat("speedMult", Mathf.Pow(10.0f, speedMultSli.value));
        //            kernel id
        fragShader.Dispatch(fragShader.FindKernel("CSMain"), threadGroupsX, threadGroupsY, 1);
        fragShader.Dispatch(fragShader.FindKernel("Particles"), Mathf.CeilToInt(n/ 100), 1, 1);


        Graphics.Blit(target, destination);



        //Particle[] particles = new Particle[n];
        //Debug.Log(particleBuffer);
        particleBuffer.GetData(particles);
        foreach (var buffer in buffersToDispose)
        {
            buffer.Dispose();
        }
    }

    // Update is called once per frame
    void Update()
    {
        totalTime += Time.deltaTime * Mathf.Pow(10.0f, transitionSli.value);
        if (Input.GetMouseButtonDown(0))
        {
            particles = new Particle[n];
            for (int i = 0; i < n; i++)
            {
                particles[i] = new Particle()
                {
                    pos = new Vector2(Random.Range(0, cam.pixelWidth), Random.Range(0, cam.pixelHeight)),
                    speed = 1
                };
            }
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

    struct Particle
    {
        public Vector2 pos;
        public float speed;
        public static int GetSize()
        {
            return sizeof(float) * 3;
        }
    }
}
