using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

/*

This is an attempt at a generic GameObject Pool system.

To Use:

Drop this file in your project, then any place you call `Instantiate(someGameObject, ...)`
instead call `GameObjectPooler.Get(someGameObject, ...)`

Any place you call `Destroy(someGameObject, ...)` instead call 
`GameObjectPoolers.Destroy(someGameObject, ...)`

At the moment it attempts to call `InitFromPool` in any Component
of the created GameObject when you call `Get` and it attempts to
call `OnDestroy` when you call `Destroy`

Note that all the usual caveats for GameObject pools exist. In particular
that `Awake` and `Start` will only be called once, the first time the 
GameObject is actually created. All other times when you call `Get`
they will not be called. Also that changes to any properties
remain is it's up to you to reset them to initial values either
in `InitFromPool` or some other way.

Another more obscure issue is if you call Destroy too many times.
With a normal GameObject that wouldn't matter as AFAIK it's just
a noop. With pools though you might end up killing someone else's
GameObject. Imagine you do this

     Destroy(someGameObject, 1.0);
     Destroy(someGameObject, 2.0);

With this in 1 second the game object will be put back in the pool.
It might then be gotten out of the pool to satisfy a new request
but another second later destroy is called again. Short of some kind
of handle system I don't know how I could easily work around that 
issue. Assuming other poolers have the same issue.

Also note that currently this pooler works on demand. That means
you don't give it a limit or an initial count so if there are no
instanced available a new instance will be created. Most poolers
create N instances up front.

I'd consider wrapping this with other classes if I wanted
that feature.

*/


#if false

public class GameObjectPooler : MonoBehaviour
{
  static public GameObjectPooler GetInstance()
  {
    if (self == null) {
      owner = new GameObject();
      owner.name = "GameObjectPoooler";
      self = owner.AddComponent<GameObjectPooler>();
    }
    return self;
  }
  
  static public GameObject Get(GameObject gameObjectToInstanciate)
  {
    return Get(gameObjectToInstanciate, Vector3.zero, Quaternion.identity);
  }

  static public GameObject Get(GameObject gameObjectToInstanciate, Vector3 position)
  {
    return Get(gameObjectToInstanciate, position, Quaternion.identity);
  }

  static public GameObject Get(GameObject gameObjectToInstanciate, Vector3 position, Quaternion rotation)
  {
    return Instantiate<GameObject>(gameObjectToInstanciate, position, rotation);
  }

  static public void Destroy(GameObject gameObject, float delay = 0.0f)
  {
    MonoBehaviour.Destroy(gameObject, delay);
  }

  static GameObject owner;
  static GameObjectPooler self;
}

#else

public sealed class IdentityEqualityComparer<T> : IEqualityComparer<T>
    where T : class
{
    public int GetHashCode(T value)
    {
        return RuntimeHelpers.GetHashCode(value);
    }

    public bool Equals(T left, T right)
    {
        return left == right; // Reference identity comparison
    }
}

public class GameObjectPooler : MonoBehaviour
{
  static public GameObjectPooler GetInstance()
  {
    if (self == null) {
      owner = new GameObject();
      owner.name = "GameObjectPoooler";
      self = owner.AddComponent<GameObjectPooler>();
    }
    return self;
  }

  public class GameObjectTracker
  {
    public GameObjectTracker(GameObject go, GameObject original)
    {
      name = go.name;
      gameObject = go;
      gameObjectWereInstantiatedFrom = original;
    }

    public GameObject gameObject;
    public GameObject gameObjectWereInstantiatedFrom;
    public float timeToDestroy = 0.0f;
    public bool toBeDestroyed = false;
    public bool destroyed = false;
    public string name;
  }

  static public GameObject Get(GameObject gameObjectToInstanciate)
  {
    return Get(gameObjectToInstanciate, Vector3.zero, Quaternion.identity);
  }

  static public GameObject Get(GameObject gameObjectToInstanciate, Vector3 position)
  {
    return Get(gameObjectToInstanciate, position, Quaternion.identity);
  }

  static public GameObject Get(GameObject gameObjectToInstanciate, Vector3 position, Quaternion rotation)
  {
    return GetInstance().GetImpl(gameObjectToInstanciate, position, rotation);
  }

  static public void Destroy(GameObject gameObject, float delay = 0.0f)
  {
    GetInstance().DestroyImpl(gameObject, delay);
  }

  Queue<GameObjectTracker> GetPool(GameObject gameObjectToInstanciate) 
  {
    Queue<GameObjectTracker> pool;
    if (!m_pools.TryGetValue(gameObjectToInstanciate, out pool)) 
    {
      pool = new Queue<GameObjectTracker>();
      m_pools.Add(gameObjectToInstanciate, pool);
    }
    return pool;
  }

  GameObject GetImpl(GameObject gameObjectToInstanciate, Vector3 position, Quaternion rotation)
  {
    Queue<GameObjectTracker> pool = GetPool(gameObjectToInstanciate);
    GameObjectTracker tracker;
    GameObject gameObject;
    if (pool.Count == 0)
    {
      gameObject = Instantiate<GameObject>(gameObjectToInstanciate, position, rotation);
      tracker = new GameObjectTracker(gameObject, gameObjectToInstanciate);
      m_tracked.Add(gameObject, tracker);
    }
    else
    {
      tracker = pool.Dequeue();
      gameObject = tracker.gameObject;
      gameObject.transform.localPosition = position;
      gameObject.transform.localRotation = rotation;
    }

    tracker.destroyed = false;
    tracker.toBeDestroyed = false;
    gameObject.SetActive(true);
    gameObject.SendMessage("InitFromPool", SendMessageOptions.DontRequireReceiver);
    return gameObject;
  } 

  void Put(GameObject gameObject)
  {
    GameObjectTracker tracker;
    if (!m_tracked.TryGetValue(gameObject, out tracker)) {
      Debug.LogError("no pool for: " + gameObject.name);
      return;
    }
    Queue<GameObjectTracker> pool = GetPool(tracker.gameObjectWereInstantiatedFrom);
    try {
      gameObject.SendMessage("OnDestroy", SendMessageOptions.DontRequireReceiver);
      gameObject.SetActive(false);
      gameObject.transform.parent = null;
    } 
    catch 
    {
      Debug.LogError("tried to use destroyed object: " + tracker.name);
      return;
    }
    tracker.destroyed = true;
    tracker.toBeDestroyed = false;
    pool.Enqueue(tracker);
  }

  public void DestroyImpl(GameObject gameObject, float delay = 0.0f)
  {
    float timeToDestroy = Time.time + delay;
    GameObjectTracker tracker;
    if (!m_tracked.TryGetValue(gameObject, out tracker))
    {
      Debug.LogError("object not tracked: " + gameObject.name);
      return;
    }

    if (tracker.destroyed)
    {
      return;
    }

    if (tracker.toBeDestroyed)
    {
      int ndx = m_toBeDestroyed.IndexOf(tracker);
      m_toBeDestroyed.RemoveAt(ndx);
    }

    tracker.timeToDestroy = timeToDestroy;
    int i = 0;
    for (; i < m_toBeDestroyed.Count; ++i) {
      if (timeToDestroy <= m_toBeDestroyed[i].timeToDestroy) {
        break;
      }
    }
    m_toBeDestroyed.Insert(i, tracker);
    tracker.toBeDestroyed = true;
  }

  public void Update()
  {
    while (m_toBeDestroyed.Count > 0)
    {
      GameObjectTracker tracker = m_toBeDestroyed[0];
      if (tracker.timeToDestroy > Time.time)
      {
        break;
      }

      m_toBeDestroyed.RemoveAt(0);
      Put(tracker.gameObject);
    }
  }

  Dictionary<GameObject, Queue<GameObjectTracker>> m_pools = new Dictionary<GameObject, Queue<GameObjectTracker>>(new IdentityEqualityComparer<GameObject>());

  // The inuse game objects. This maps a GameObject to the object it was instantiaed from.
  // This way we can put it back into the correct pool.
  Dictionary<UnityEngine.GameObject, GameObjectTracker> m_tracked = new Dictionary<UnityEngine.GameObject, GameObjectTracker>(new IdentityEqualityComparer<GameObject>());

  List<GameObjectTracker> m_toBeDestroyed = new List<GameObjectTracker>();

  static GameObject owner;
  static GameObjectPooler self;
}

#endif
