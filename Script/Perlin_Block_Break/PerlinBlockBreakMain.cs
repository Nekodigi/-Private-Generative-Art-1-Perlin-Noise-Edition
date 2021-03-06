using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]

public class PerlinBlockBreakMain : MonoBehaviour
{
    public ComputeShader fragShader;

    int n = 100000;
    RenderTexture target;
    Camera cam;
    float totalTime;
    public Slider scaleMultSli;
    public Slider transitionSli;
    bool first = true;

    ComputeBuffer particleBuffer;
    List<ComputeBuffer> buffersToDispose;
    Particle[] particles = null;

    // Start is called before the first frame update
    void Start()
    {
        first = true;
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
            particles = new Particle[1];
            particles[0] = new Particle()
            {
                //pos = new Vector2(Random.Range(-10, 10)+ cam.pixelWidth/2, Random.Range(-10, 10)+cam.pixelHeight/2),
                pos = new Vector2(cam.pixelWidth / 2, cam.pixelHeight / 2),
                enable = 1
            };
        }
        else
        {
            Particle[] particles_ = new Particle[particles.Length + 1];
            System.Array.Copy(particles, particles_, particles.Length);
            particles = particles_;
            particles[particles.Length - 1] = new Particle()
            {
                //pos = new Vector2(Random.Range(-10, 10)+ cam.pixelWidth/2, Random.Range(-10, 10)+cam.pixelHeight/2),
                pos = new Vector2(cam.pixelWidth / 2, cam.pixelHeight / 2),
                enable = 1
            };
        }
        particleBuffer = new ComputeBuffer(particles.Length, Particle.GetSize());
        particleBuffer.SetData(particles);
        fragShader.SetBuffer(fragShader.FindKernel("Particles"), "particles", particleBuffer);
        buffersToDispose.Add(particleBuffer);



    }

    //based on this site https://github.com/SebLague/Ray-Marching/blob/master/Assets/Scripts/SDF/Master.cs
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        for (int i = 0; i < 10; i++)
        {
            totalTime += 0.02f * Mathf.Pow(10.0f, transitionSli.value);
            buffersToDispose = new List<ComputeBuffer>();
            Init();

            CreateScene();


            fragShader.SetTexture(fragShader.FindKernel("CSMain"), "Result", target);
            fragShader.SetTexture(fragShader.FindKernel("Particles"), "Result", target);
            fragShader.SetFloat("time", totalTime);
            fragShader.SetFloat("height", cam.pixelHeight);
            fragShader.SetFloat("width", cam.pixelWidth);
            fragShader.SetFloat("scaleMult", Mathf.Pow(10.0f, scaleMultSli.value));
            //            kernel id

            if (Input.GetMouseButton(0) || first)
            {
                int threadGroupsX = Mathf.CeilToInt(cam.pixelWidth / 8.0f);
                int threadGroupsY = Mathf.CeilToInt(cam.pixelHeight / 8.0f);
                fragShader.Dispatch(fragShader.FindKernel("CSMain"), threadGroupsX, threadGroupsY, 1);
                first = false;
            }

            fragShader.Dispatch(fragShader.FindKernel("Particles"), Mathf.CeilToInt(particles.Length / 100.0f), 1, 1);


            Graphics.Blit(target, destination);

            //Debug.Log(particles.Length);

            //Particle[] particles = new Particle[n];
            //Debug.Log(particleBuffer);
            particleBuffer.GetData(particles);
            foreach (var buffer in buffersToDispose)
            {
                buffer.Dispose();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            int threadGroupsX = Mathf.CeilToInt(cam.pixelWidth / 8.0f);
            int threadGroupsY = Mathf.CeilToInt(cam.pixelHeight / 8.0f);
            fragShader.Dispatch(fragShader.FindKernel("CSMain"), threadGroupsX, threadGroupsY, 1);
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
        public int enable;
        public static int GetSize()
        {
            return sizeof(float) * 2 + sizeof(int);
        }
    }
}
