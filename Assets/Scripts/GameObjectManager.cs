using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A GameObject manager for the Instantiation and Destruction of GameObjects.
/// </summary>
public class GameObjectManager : MonoBehaviour {

    // Static instance of our class.
    public static GameObjectManager instance;

    // Queue of objects to be instantiated.
    private List<InstantiateQueue> instantiateQueue;

    // Queue of objects to be destroyed.
    private List<GameObject> destroyQueue;

    // Maximum time the loop can run.
    private float maxTime = 0.1f;


    // Load our static.
    void Awake() {
        instance = this;
    }


    // Create our lists.
	void Start () {
        instantiateQueue = new List<InstantiateQueue>();
        destroyQueue = new List<GameObject>();
	}

    /// <summary>Queue up an object to be instantiated.</summary>
    /// <param name="iq">The Object + prefab to be instantiated.</param>
    public void QueueInstantiate(InstantiateQueue iq) {
        instantiateQueue.Add(iq);
    }

    /// <summary>Queue up an object to be instantiated.</summary>
    /// <param name="go">The Object to be instantiated.</param>
    /// <param name="go">The prefab to be instantiated.</param>
    public void QueueInstantiate(GameObject go, GameObject pre, Vector3 pos, Quaternion quat) {
        QueueInstantiate(new InstantiateQueue(go, pre, pos, quat));
    }


    /// <summary>Queue up an object to be destroyed.</summary>
    /// <param name="go">The Object to be destroyed.</param>
    public void QueueDestroy(GameObject go) {
        go.active = false;
        destroyQueue.Add(go);
    }


    // Temp Use Variables for Update().
    private float startTime;
    private InstantiateQueue tempItem;
    private GameObject tempObject;

    void Update() {
        //Debug.Log("check0");
        //CheckQueues();
        //StartCoroutine("CheckQueues");

        startTime = Time.realtimeSinceStartup;
        //Debug.Log("check1");

        while ((instantiateQueue.Count + destroyQueue.Count) > 0) {
            //Debug.Log("check2");
            // Are we over our time limit for this frame?
            if (Time.realtimeSinceStartup - startTime >= maxTime) {
                return;
            }
            //Debug.Log("check3");
            // Check Queues for work to do.
            if (destroyQueue.Count > 0) {
                tempObject = destroyQueue[0];
                instantiateQueue.RemoveAt(0);
                GameObject.Destroy(tempObject);
            }else if (instantiateQueue.Count > 0) {
                tempItem = instantiateQueue[0];
                tempItem.myObject = (GameObject)Instantiate(tempItem.myPrefab, tempItem.myPosition, tempItem.myQuaternian);
                instantiateQueue.RemoveAt(0);
            }
        }
    }


	IEnumerable CheckQueues() {
        startTime = Time.realtimeSinceStartup;
        //Debug.Log("check1");

        while ((instantiateQueue.Count + destroyQueue.Count) > 0) {
            //Debug.Log("check2");
            // Are we over our time limit for this frame?
            if (Time.realtimeSinceStartup - startTime >= maxTime) {
                yield return 0;
            }
            //Debug.Log("check3");
            // Check Queues for work to do.
            if (instantiateQueue.Count > 0) {
                tempItem = instantiateQueue[0];
                tempItem.myObject = (GameObject)Instantiate(tempItem.myPrefab, tempItem.myPosition, tempItem.myQuaternian);
                instantiateQueue.Remove(tempItem);
            } else if (destroyQueue.Count > 0) {
                tempObject = destroyQueue[0];
                destroyQueue.Remove(tempObject);
                GameObject.Destroy(tempObject);
            }
        }

        yield return 0;
	}


    void OnGUI() {
        GUI.Label(new Rect( 10, 300, 100, 20), "InstQ: " + instantiateQueue.Count);
    }
}


/// <summary>
/// Struct to hold Instantiation details for the InstantiateQueue List.
/// </summary>
public struct InstantiateQueue {

    public GameObject myObject;
    public GameObject myPrefab;
    public Vector3 myPosition;
    public Quaternion myQuaternian; 


    /// <summary>New Instantiate Queue Item.</summary>
    /// <param name="obj">Object to hold the Instantiation.</param>
    /// <param name="pre">Prefab to be Instantiated.</param>
    /// <param name="pos">Position to be Instantiated at.</param>
    /// <param name="quat">Rotation to be Instantiated at.</param>
    public InstantiateQueue(GameObject obj, GameObject pre, Vector3 pos, Quaternion quat) {
        myObject = obj; 
        myPrefab = pre;
        myPosition = pos;
        myQuaternian = quat;
    }
}