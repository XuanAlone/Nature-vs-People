using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{

    [SerializeField] private Button PauseButton;
    [SerializeField] private Image PauseButtonImage;
    [SerializeField] private Sprite PauseImage;
    [SerializeField] private Sprite ContinueImage;

    public bool isPaused = false;

    void Start()
    {
        PauseButton.onClick.AddListener(GamePause);
        ChangeButtonImage();
    }

    public void GamePause ()
    {
        isPaused = !isPaused ;

        if (isPaused )
        {
            Time.timeScale = 0.0f;
        }
        else
        {
            Time.timeScale = 1.0f;
        }

        ChangeButtonImage();

    }

    public void ChangeButtonImage()
    {
        if ( PauseButtonImage != null )
        {
            PauseButtonImage.sprite = isPaused ? ContinueImage : PauseImage;
        }
    }
    

    public void OnDestory ()
    {
        Time.timeScale = 1.0f;
    }


    void Update()
    {
        
    }
}
