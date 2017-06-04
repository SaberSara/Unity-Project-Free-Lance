using UnityEngine;
using System.Collections;

public class ScrollController : MonoBehaviour {

    private StatManager st;
    private float startPoint = -0.2403329f;
    private float endPoint = 12.59404f;
    private float newPosition = 0f;

    // Use this for initialization
    void Start () {
        st = (StatManager)FindObjectOfType(typeof(StatManager));
        newPosition = startPoint + (0.32f * (st.sc.getLevelsUnlocked() - 1));
        this.GetComponent<UISlider>().value = 1-(newPosition/endPoint);
	}

    // Update is called once per frame
    void Update()
    {

    }
}
