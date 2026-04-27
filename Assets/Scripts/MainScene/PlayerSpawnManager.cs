using UnityEngine;
using System.Collections;
using System.Reflection;

public class PlayerSpawnManager : MonoBehaviour
{
    // Floor is at -13.99. Setting spawn to -13.9 to be slightly above for safety.
    [SerializeField] private Vector3 m_spawnPosition = new Vector3(-174.7f, -13.9f, 201.8f);
    [SerializeField] private float m_eyeLevel = 1.6f;
    
    private CharacterController m_cc;
    private Rigidbody m_rb;
    private bool m_stabilized = false;

    private void OnValidate()
    {
        FixComponents();
    }

    private void FixComponents()
    {
        m_cc = GetComponent<CharacterController>();
        if (m_cc != null)
        {
            m_cc.height = 2.0f;
            m_cc.radius = 0.5f;
            m_cc.center = new Vector3(0, 1.0f, 0);
            m_cc.skinWidth = 0.08f;
        }

        m_rb = GetComponent<Rigidbody>();
        if (m_rb != null)
        {
            m_rb.isKinematic = true;
            m_rb.useGravity = false;
        }

        var camTransform = transform.Find("FirstPersonCharacter");
        if (camTransform != null)
        {
            camTransform.localPosition = new Vector3(0, m_eyeLevel, 0);
        }
    }

    private void Awake()
    {
        FixComponents();
        transform.position = m_spawnPosition;
        if (m_cc != null) m_cc.enabled = false;
        
        var fps = GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();
        if (fps != null) fps.enabled = false;
    }

    private IEnumerator Start()
    {
        // Wait for physics and hierarchy to stabilize
        float elapsed = 0;
        while (elapsed < 1.0f)
        {
            transform.position = m_spawnPosition;
            if (m_cc != null) m_cc.enabled = false;
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        var fps = GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();
        if (fps != null)
        {
            // Reset internal velocity to prevent falling through floor on the first frame
            ResetFPSControllerState(fps);
            fps.enabled = true;
        }

        if (m_cc != null) m_cc.enabled = true;
        
        m_stabilized = true;
        Debug.Log($"[PlayerSpawnManager] Player stabilized at {transform.position}. Eye Level: {m_eyeLevel}");
    }

    private void ResetFPSControllerState(MonoBehaviour fps)
    {
        try
        {
            // Use reflection to reset m_MoveDir to zero
            FieldInfo moveDirField = fps.GetType().GetField("m_MoveDir", BindingFlags.Instance | BindingFlags.NonPublic);
            if (moveDirField != null)
            {
                moveDirField.SetValue(fps, Vector3.zero);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[PlayerSpawnManager] Could not reset FPSController velocity: " + e.Message);
        }
    }

    private void LateUpdate()
    {
        if (!m_stabilized)
        {
            transform.position = m_spawnPosition;
        }
    }
}

