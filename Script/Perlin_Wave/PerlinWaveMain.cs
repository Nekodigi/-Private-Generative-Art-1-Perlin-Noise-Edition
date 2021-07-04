using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]

public class PerlinWaveMain : MonoBehaviour
{
    public ComputeShader fragShader;

    float particleSize = 1;//change later
    int nx;
    RenderTexture target;
    Camera cam;
    float totalTime;
    public int hist = 100;

    public Slider scaleMultSli;
    public Slider transitionSli;

    List<ComputeBuffer> buffersToDispose;
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
            nx = Mathf.CeilToInt(cam.pixelWidth / particleSize);
        }
    } 

    void CreateScene()
    {
        nx = Mathf.CeilToInt(cam.pixelWidth / particleSize);
        Particle[] particles = new Particle[nx*hist];
        for (int i=0; i<nx; i++)
        {
            for (int j = 0; j < hist; j++)
            {
                particles[i * hist + j] = new Particle()
                {
                    y = 0,
                    starty = 0,
                    count = j
                };
            }
        }
        ComputeBuffer particleBuffer = new ComputeBuffer(particles.Length, Particle.GetSize());
        particleBuffer.SetData(particles);
        fragShader.SetBuffer(fragShader.FindKernel("Particles"), "particles", particleBuffer);
        buffersToDispose.Add(particleBuffer);
    }

    //based on this site https://github.com/SebLague/Ray-Marching/blob/master/Assets/Scripts/SDF/Master.cs
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        totalTime += Time.deltaTime* Mathf.Pow(10.0f, transitionSli.value);
        buffersToDispose = new List<ComputeBuffer>();
        Init();
        CreateScene();

        int threadGroupsX = Mathf.CeilToInt(cam.pixelWidth / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(cam.pixelHeight / 8.0f);

        fragShader.SetTexture(fragShader.FindKernel("CSMain"), "Result", target);
        fragShader.SetTexture(fragShader.FindKernel("Particles"), "Result", target);
        fragShader.SetFloat("time", totalTime);
        fragShader.SetFloat("height", cam.pixelHeight);
        fragShader.SetFloat("scaleMult", Mathf.Pow(10.0f, scaleMultSli.value));
        //            kernel id
        fragShader.Dispatch(fragShader.FindKernel("CSMain"), threadGroupsX, threadGroupsY, 1);
        fragShader.Dispatch(fragShader.FindKernel("Particles"), Mathf.CeilToInt(nx/100.0f), hist, 1);


        Graphics.Blit(target, destination);

        foreach (var buffer in buffersToDispose)
        {
            buffer.Dispose();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //totalTime += Time.deltaTime;
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
        public float y;
        public float starty;
        public int count;
        public static int GetSize()
        {
            return sizeof(float) * 2 + sizeof(int) * 1;
        }
    }
}
