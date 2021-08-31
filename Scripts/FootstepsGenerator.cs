using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepsGenerator : MonoBehaviour
{
    private FootstepsManager _footstepManager;

    private void Start()
    {
        _footstepManager = gameObject.GetComponent<FootstepsManager>();
    }

    public void PlayFootsteps()
    {
        StartCoroutine("FootstepsSystem", _footstepManager.Interval);
    }

    public IEnumerator FootstepsSystem(float timer)
    {
        int randomIndex = Random.Range(0, _footstepManager.Footsteps.Length);

        _footstepManager.AudioSource.clip = _footstepManager.Footsteps[randomIndex];

        _footstepManager.AudioSource.Play();

        _footstepManager.IsWalking = true;

        yield return new WaitForSeconds(timer);

        _footstepManager.IsWalking = false;
    }
}
