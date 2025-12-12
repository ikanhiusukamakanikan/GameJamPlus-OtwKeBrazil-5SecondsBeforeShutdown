using UnityEngine;
public class ButtonDetector : MonoBehaviour, IHistoryObject
{
    public int state = 0; // 0/1
    public GameObject doorObject;

    void Start()
    {
        updateDoor();
    }

    void Update()
    {
        updateDoor();
    }

    public HistoryData SaveState()
    {
        return new HistoryData { intValue = state };
    }

    public void LoadState(HistoryData data)
    {
        state = data.intValue;
        updateDoor();
        // update visual button di sini
    }

    public void buttonPressed()
    {
        state = 1;
        updateDoor();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Button Pressed");
            buttonPressed();
        }
    }

    public void updateDoor()
    {
        if (doorObject != null)
        {
            if (state == 1)
            {
                doorObject.SetActive(false);
            }
            else
            {
                doorObject.SetActive(true);
            }
        }
    }
}
