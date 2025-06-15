using GlitchHunter.Constant;
using UnityEngine;

namespace GlitchHunter.Manager
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject playerPrefab;
        private float gameDuration = 300;
        private float mGameTime = 0;

        public float GameTime => mGameTime;
        public float GameDuration => gameDuration;

        public GameObject PlayerPrefab
        {
            get;
            set;
        }

        [HideInInspector] public bool IsGameStarted = false;
        [HideInInspector] public bool IsEquipped = false;
        [HideInInspector] public AudioSource AudioSource;


        public static GameManager Instance;

        private void Awake()
        {
            if (Instance == null) 
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            AudioSource = GetComponent<AudioSource>();
            SpawnPlayer();
        }

        private void Update()
        {
            if (IsGameStarted)
            {
                mGameTime += Time.deltaTime;
            }

            if (mGameTime >= gameDuration)
            {
                EndGame();
                return;
            }
        }

        private void SpawnPlayer()
        {
            GameObject obj = Instantiate(playerPrefab);
            PlayerPrefab = obj;
        }

        public void EndGame()
        {
            Debug.LogError("Game Over");
            GlitchHunterConstant.OnGameOver?.Invoke();
            IsGameStarted = false;
        }
    }
}
