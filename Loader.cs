using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Check is GameManager instance exists. (if not creates it)
/// </summary>
public class Loader : MonoBehaviour {

    public GameObject gameManager;

	void Awake ()
    {
        if (GameManager.instance == null)
            Instantiate(gameManager);
	}
}
