using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Initialize : MonoBehaviour
{
    IInitializable[] initObjects;

    private void Start()
    {
        //Get all objects in the scene that implement IInitializable
        initObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).
            OfType<IInitializable>().
            ToArray();

        //Wait for all initializable objects to be initialized
        StartCoroutine(WaitForInitialization());
    }

    IEnumerator WaitForInitialization()
    {
        bool isInitialized = false;

        while (!isInitialized)
        {
            //Wait for next init cheeck
            yield return new WaitForSeconds(0.5f);

            //Default: done initializing. Check if this is not true
            isInitialized = true;

            foreach(MonoBehaviour initObj in initObjects)
            {
                //Check if the object is initialized
                if (initObj is IInitializable init && !init.IsInitialized())
                {
                    //Object is not initialized, must wait another cycle 
                    isInitialized = false;
                    break;
                }
            }
        }

        //Everything is initialized; open main scene
        SceneManager.LoadScene("MainMenu"); //Change scene name eventually
    }
}

