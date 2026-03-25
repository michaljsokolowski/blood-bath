using UnityEngine;

public class AttackVisual : MonoBehaviour
{
    [Header("References")]
    public Transform player;          // Drag your player here

    [Header("Settings")]
    public float attackRadius = 5f;
    public float coneAngle    = 45f;
    public float chargeDuration = 1f; // How long the fill animation takes

    public Material _mat;
    private bool _charging;
    private float _chargeTimer;

    void Awake()
    {
        //_mat = GetComponent<>().material;
        // ensure we have a material instance (prefer renderer.material if none assigned in inspector)
        if (_mat == null)
        {
            var rend = GetComponent<Renderer>();
            if (rend != null)
                _mat = rend.material;
        }

        if (_mat != null)
        {
            _mat.SetFloat("_AttackRadius", attackRadius);
            _mat.SetFloat("_AttackAngle",    coneAngle); // shader expects _AttackAngle
            _mat.SetFloat("_AttackCharge",   0f);        // initialize charge
        }
        else
        {
            Debug.LogWarning("AttackVisual: No material assigned and no Renderer.material found.", this);
        }

        gameObject.SetActive(true); // Hidden until ability fires
    }

    void Update()
    {
        // Keep shader in sync with player transform every frame
        if (_mat != null)
        {
            // If attackRadius or coneAngle change at runtime keep shader updated
            _mat.SetFloat("_AttackRadius", attackRadius);
            _mat.SetFloat("_AttackAngle",  coneAngle);

            _mat.SetVector("playerPosition", player.position);
            _mat.SetVector("playerForward",  new Vector4(
                player.forward.x, 0f, player.forward.z, 0f));
        }

        //Animate fill amount if charging
        if (_charging)
        {
            _chargeTimer += Time.deltaTime;
            float fill = Mathf.Clamp01(_chargeTimer / chargeDuration);
            if (_mat != null)
                _mat.SetFloat("_AttackCharge", fill); // shader expects _AttackCharge

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
        if (_mat != null)
            _mat.SetFloat("_AttackCharge", 0f); // reset shader charge
        gameObject.SetActive(true);
    }

}