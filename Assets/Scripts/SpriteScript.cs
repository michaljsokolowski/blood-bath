using UnityEngine;

public class SpriteScript : MonoBehaviour
{
    private Camera _cam;
    private int _lastDirection = 1;

    void Start()
    {
        _cam = Camera.main;
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        if (horizontal > 0) _lastDirection = 1;
        else if (horizontal < 0) _lastDirection = -1;
    }

    void LateUpdate()  // <-- rotation applied AFTER everything else has moved
    {
        float yAngle = _lastDirection == 1
            ? _cam.transform.eulerAngles.y + 180f
            : _cam.transform.eulerAngles.y;

        transform.rotation = Quaternion.Euler(0f, yAngle, 0f);
    }
//komentarz, żeby zobaczyć czy vs code działa - karolinka tu była