using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core.Managers
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance;

        public Sound[] sounds;
        [SerializeField] private AudioSource bgmSource;

        private bool isStartPlaying = false;
        private bool isMiddlePlaying = false;
        private bool playedStartSound = false;
        private Coroutine vacuumCoroutine;
        private float stepResetTime = 0.5f;
        private float alreadyResetTime = 0;
        private bool canStep = true;
        private bool firstStep = true;
        private bool _canPlayMusic;
        


        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            foreach (Sound s in sounds)
            {
                s.source = s.isBGM ? bgmSource : gameObject.AddComponent<AudioSource>();
                s.source.clip = s.clip;
                s.source.volume = s.volume;
                s.source.loop = s.loop;
                s.source.pitch = s.pitch;
                s.source.spatialBlend = s.spatialBlend;
            }

            StartCoroutine(PrewarmSounds());
        }

        private void OnEnable()
        {
            EventManager.StartFadeEnded += ChangeCanPlayMusicFlag;
            EventManager.CameraNeedToMove += ChangeCanPlayMusicFlag;
            EventManager.CameraFinishedMoving += ChangeCanPlayMusicFlag;
        }

        private void OnDisable()
        {
            EventManager.StartFadeEnded -= ChangeCanPlayMusicFlag;
            EventManager.CameraNeedToMove -= ChangeCanPlayMusicFlag;
            EventManager.CameraFinishedMoving -= ChangeCanPlayMusicFlag;
        }

        private void ChangeCanPlayMusicFlag()
        {
            _canPlayMusic = !_canPlayMusic;
        }

        private void Start()
        {
            Play("background");
        }

        private void Update()
        {
            if (!canStep)
            {
                alreadyResetTime += Time.deltaTime;
                if (alreadyResetTime >= stepResetTime)
                {
                    canStep = true;
                    alreadyResetTime = 0f; // <- Reset the timer here
                }
            }
        }


        public void Play(string name, bool randomPitch = false, bool isSpacial = false)
        {
            if (_canPlayMusic || name == "background" || name == "doorOpen")
            {
                Sound s = Array.Find(sounds, sound => sound.name == name);
                if (s == null)
                {
                    Debug.LogWarning($"Sound '{name}' not found!");
                    return;
                }

                if (randomPitch)
                {
                    s.source.pitch = Random.Range(0.5f, 1.3f);
                }

                s.source.clip = s.clip;
                s.source.Play();
            }
        } 
        public void PlayVacuumSound(string state)
        {
            if (_canPlayMusic)
            {
                if (state == "start" && !playedStartSound)
                {
                    playedStartSound = true;
                    isStartPlaying = false;
                    isMiddlePlaying = false;

                    if (vacuumCoroutine != null)
                        StopCoroutine(vacuumCoroutine);

                    vacuumCoroutine = StartCoroutine(PlayVacuumSequence());
                }
                else if (state == "stop" && playedStartSound)
                {
                    if (vacuumCoroutine != null)
                        StopCoroutine(vacuumCoroutine);

                    vacuumCoroutine = StartCoroutine(StopVacuumSequence());
                    playedStartSound = false;
                }
            }
        }

        private IEnumerator PlayVacuumSequence()
        {
            Sound startSound = Array.Find(sounds, s => s.name == "vacuumStart");
            Sound middleSound = Array.Find(sounds, s => s.name == "vacuumMiddle");

            if (startSound == null || middleSound == null)
            {
                Debug.LogWarning("Missing vacuumStart or vacuumMiddle sound!");
                yield break;
            }

            double dspTime = AudioSettings.dspTime;

            // Play vacuumStart
            startSound.source.PlayScheduled(dspTime);
            isStartPlaying = true;

            double startDuration = startSound.clip.length;

            // Schedule vacuumMiddle
            middleSound.source.loop = true;
            middleSound.source.PlayScheduled(dspTime + startDuration);
            

            // Wait until just before middle starts, update flags
            yield return new WaitForSeconds((float)startDuration - 1f);
            isStartPlaying = false;
            isMiddlePlaying = true;
        }

        private IEnumerator PrewarmSounds()
        {
            foreach (Sound s in sounds)
            {
                if (s.clip.loadState != AudioDataLoadState.Loaded)
                {
                    // Force loading into memory if needed
                    s.clip.LoadAudioData();
                    while (s.clip.loadState != AudioDataLoadState.Loaded)
                    {
                        yield return null; // Wait until it's ready
                    }
                }

                // Prewarm by playing silently and stopping
                s.source.volume = 0;
                s.source.Play();
                s.source.Stop();
                s.source.volume = s.volume; // Restore
            }
        }

        private IEnumerator StopVacuumSequence()
        {
            Sound startSound = Array.Find(sounds, s => s.name == "vacuumStart");
            Sound middleSound = Array.Find(sounds, s => s.name == "vacuumMiddle");
            Sound endSound = Array.Find(sounds, s => s.name == "vacuumEnd");

            if (startSound == null || middleSound == null || endSound == null)
            {
                Debug.LogWarning("Missing vacuum sound(s)!");
                yield break;
            }

            // Cancel scheduled play for vacuumMiddle to prevent it from starting if not started yet
            middleSound.source.SetScheduledEndTime(AudioSettings.dspTime);

            // Immediately stop vacuumStart if still playing
            if (isStartPlaying && startSound.source.isPlaying)
            {
                startSound.source.Stop();
                isStartPlaying = false;
            }

            // Immediately stop vacuumMiddle if playing
            if (isMiddlePlaying && middleSound.source.isPlaying)
            {
                middleSound.source.Stop();
                isMiddlePlaying = false;
            }

            // Play vacuumEnd immediately (no fade)
            endSound.source.volume = endSound.volume; // Ensure normal volume
            endSound.source.Play();

            yield return null;
        }

        public void PlayStep()
        {
            if (canStep)
            {
                if (firstStep)
                {
                    Play("firstStep");
                    firstStep = false;

                }
                else
                {
                    Play("secondStep");
                    firstStep = true;
                }
                Debug.Log("playing step sound");
                canStep = false;
            }
        }
    }
}
