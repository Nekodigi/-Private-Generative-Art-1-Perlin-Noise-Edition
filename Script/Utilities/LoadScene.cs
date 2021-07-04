using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Advertisements;

public class LoadScene : MonoBehaviour
{
    public string sceneName;
    // Start is called before the first frame update
    void Start()
    {
        Advertisement.Initialize("4108011", false);
    }

    public void Load()
    {
        SceneManager.LoadScene(sceneName);
        if (sceneName != "Perlin_Main" && Advertisement.IsReady()) {
            Advertisement.Show();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
