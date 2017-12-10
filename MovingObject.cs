using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract classes are incomplete and must be implemented in the derived class.
/// </summary>
public abstract class MovingObject : MonoBehaviour {

    public float moveTime = 0.1f;
    public LayerMask blockingLayer;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rg2D;
    private float inverseMoveTime;


    // Use this for initialization
    //Virtual classes can be overriden by inheriting classes.
    protected virtual void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rg2D = GetComponent<Rigidbody2D>();
        inverseMoveTime = 1f / moveTime;
    }

    /// <summary>
    /// Return the bool value of the function and "hit" of type RaycastHit2D passed by reference
    /// </summary>
    /// <param name="xDir"></param>
    /// <param name="yDir"></param>
    /// <param name="hit"></param>
    /// <returns></returns>
    protected bool Move (int xDir, int yDir, out RaycastHit2D hit)
    {
        //the "z axis" data is discarded due to implecit conversion
        Vector2 start = transform.position;
        Vector2 end = start + new Vector2(xDir, yDir);

        //making sure the ray will not hit our own colldier
        boxCollider.enabled = false;
        //checking collision on blocking layer
        hit = Physics2D.Linecast(start, end, blockingLayer);
        boxCollider.enabled = true;
        //checking if anything was hit
        if (hit.transform == null)
        {
            StartCoroutine(SmoothMovement(end));
            return true;
        }
        return false;
    }

    /// <summary>
    /// Attempts to move the player to indicated position while xpecting to encounter a generic object.
    /// The generic object should be specified between <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">The type of component we expect the unit to interract with if blocked</typeparam>
    /// <param name="xDir"></param>
    /// <param name="yDir"></param>
    protected virtual void AttemptMove<T>(int xDir, int yDir)
        where T : Component
    {
        RaycastHit2D hit;
        //checking if we can move
        bool canMove = Move(xDir, yDir, out hit);
        //if nothing hit do nothing
        if (hit.transform == null)
            return;

        //getting the hit object components
        T hitComponent = hit.transform.GetComponent<T>();

        //deciding what to do with object
        if (!canMove && hitComponent != null)
            OnCantMove(hitComponent);
    }

    /// <summary>
    /// Moves units from one space to the next
    /// </summary>
    /// <param name="end">The position of the object we are moving towards</param>
    /// <returns></returns>
    protected IEnumerator SmoothMovement(Vector3 end)
    {
        //calculating remeining distance to move
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
        while (sqrRemainingDistance > float.Epsilon)
        {
            //finding new position proportionally close to "end" based on move time
            Vector3 newPosition = Vector3.MoveTowards(this.rg2D.position, end, inverseMoveTime * Time.deltaTime);
            //applying forces rigid body
            rg2D.MovePosition(newPosition);
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            yield return null;
        }
    }

    /// <summary>
    /// Defines what to do if object can't move while encountering generic parameter <typeparamref name="T"/>. 
    /// Also takes one parameter of type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="Component"></param>
    protected abstract void OnCantMove<T> (T Component)
        where T : Component;
}
