using UnityEngine;

public class AttackVisual : MonoBehaviour
{
    [Header("References")]
    public Transform player;          // Drag your player here

    [Header("Settings")]
    public float attackRadius = 5f;
    public float coneAngle    = 45f;
    public float chargeDuration = 1f; // How long the fill animation takes

    private Material _mat;
    private bool _charging;
    private float _chargeTimer;

    void Awake()
    {
        _mat = GetComponent<Renderer>().material;
        _mat.SetFloat("_AttackRadius", attackRadius);
        _mat.SetFloat("_ConeAngle",    coneAngle);
        gameObject.SetActive(true); // Hidden until ability fires
    }

    void Update()
    {
        // Keep shader in sync with player transform every frame
        _mat.SetVector("playerPosition", player.position);
        _mat.SetVector("playerForward",  new Vector4(
            player.forward.x, 0f, player.forward.z, 0f));

        // Animate fill amount if charging
        if (_charging)
        {
            _chargeTimer += Time.deltaTime;
            float fill = Mathf.Clamp01(_chargeTimer / chargeDuration);
            _mat.SetFloat("_FillAmount", fill);

            if (fill >= 1f)
            {
                _charging = false;
                gameObject.SetActive(true);
            }
        }
    }

    // Call this when the ability starts
    public void StartCharge()
    {
        _chargeTimer = 0f;
        _charging    = true;
        _mat.SetFloat("_FillAmount", 0f);
        gameObject.SetActive(true);
    }
}