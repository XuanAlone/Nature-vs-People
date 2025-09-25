using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonOnClick : MonoBehaviour
{
    
    public ImprovedLineSegmentController improvedLineSegmentController;
    public Button button;

    // Start is called before the first frame update
    void Start()
    {
        improvedLineSegmentController=FindAnyObjectByType<ImprovedLineSegmentController>();
        improvedLineSegmentController.OnCreateSegmentButtonClicked();
        //improvedLineSegmentController.UpdateButtonInteractivity();
    }
        // Update is called once per frame
        void Update()
    {
        
    }
}
