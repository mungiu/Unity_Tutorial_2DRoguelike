using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour {

    public Sprite dmgSprite;            //dispalyed when player hit the wall
    public int hp = 4;
    public AudioClip chopSound1;
    public AudioClip chopSound2;

    private SpriteRenderer spriteRenderer;

	void Awake ()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
	}
	
	/// <summary>
    /// Applies wall damage and animation.
    /// </summary>
    /// <param name="loss"></param>
	public void DamagedWall(int loss)
    {
        spriteRenderer.sprite = dmgSprite;
        hp -= loss;
        SoundManager.instance.RandomizeSfx(chopSound1, chopSound2);
        if (hp <= 0)
            gameObject.SetActive(false);
	}
}
