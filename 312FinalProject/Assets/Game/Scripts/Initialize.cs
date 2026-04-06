using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Initialize : MonoBehaviour
{
    IInitializable[] initObjects;

    private void Start()
    {
        initObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IInitializable>().ToArray();

        StartCoroutine(WaitForInitialization());
    }

    IEnumerator WaitForInitialization()
    {
        bool isInitialized = false;

        while (!isInitialized)
        {
            yield return new WaitForSeconds(0.5f);

            //Default: done initializing. Check if this is not true
            isInitialized = true;

            foreach(MonoBehaviour initObj in initObjects)
            {
                if (initObj is IInitializable init && !init.IsInitialized())
                {
                    //Object is not initialized, must wait another cycle 
                    isInitialized = false;
                    break;
                }
            }
        }

        //Everything is initialized; open main scene
        SceneManager.LoadScene("NetworkTest"); //Change scene name eventually
    }
}

