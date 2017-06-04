using UnityEngine;
using System.Collections;

public class CharacterActionsOpponent : MonoBehaviour {

    private Animator anim;
    // Use this for initialization
    void Start()
    {
        // if(this.gameObject.name == "Character1")
        //this.gameObject.SetActive(false);
        anim = this.GetComponent<Animator>();
    }

    public void Idle()
    {
        anim.SetBool("Attack", false);
        anim.SetBool("SuperAttack", false);
        anim.SetBool("Die", false);
    }

    public void Attack()
    {
        //this.transform.parent.GetChild(0).gameObject.SetActive(true);
        anim.SetBool("Attack", true);
        anim.SetBool("SuperAttack", false);
        anim.SetBool("Die", false);
    }

    public void SuperAttack()
    {
        anim.SetBool("Attack", false);
        anim.SetBool("SuperAttack", true);
        anim.SetBool("Die", false);
    }

    public void Die()
    {
        anim.SetBool("Attack", false);
        anim.SetBool("SuperAttack", false);
        anim.SetBool("Die", true);
    }
    // Update is called once per frame
    void Update()
    {


    }
}
