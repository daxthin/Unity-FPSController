using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepsGenerator : MonoBehaviour
{
    private FootstepsManager m_footstepManager;

    private void Start()
    {
        m_footstepManager = gameObject.GetComponent<FootstepsManager>();
    }

    public void PlayFootsteps()
    {
        StartCoroutine(FootstepsSystem(m_footstepManager.Interval));
    }

    public IEnumerator FootstepsSystem(float timer)
    {
        int randomIndex = Random.Range(0, m_footstepManager.Footsteps.Length);

        m_footstepManager.AudioSource.clip = m_footstepManager.Footsteps[randomIndex];

        m_footstepManager.AudioSource.Play();

        m_footstepManager.IsWalking = true;

        yield return new WaitForSeconds(timer);

        m_footstepManager.IsWalking = false;
    }
}
