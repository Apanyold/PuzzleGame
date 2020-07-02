using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    #region Variables

    [SerializeField] private Audio m_audio = new Audio();

    [SerializeField] private Score m_score;

    private LevelParameters m_level;

    private int m_currentLevel;

    private Field m_field;

    private List<List<Token>> m_tokensByTypes;

    private static Controller m_instance;

    [SerializeField] private Color[] m_tokenColors;

    public Color[] TokenColors
    {
        get
        {
            return m_tokenColors;
        }

        set
        {
            m_tokenColors = value;

        }
    }
    public static Controller Instance
    {
        get
        {
            if (m_instance == null)
            {
                var controller = Instantiate(Resources.Load("Prefabs/Controller")) as GameObject;

                m_instance = controller.GetComponent<Controller>();
            }
            return m_instance;
        }
    }
    public List<List<Token>> TokensByTypes { get => m_tokensByTypes; set => m_tokensByTypes = value; }
    public Field Field { get => m_field; set => m_field = value; }
    public int CurrentLevel { get => m_currentLevel; set => m_currentLevel = value; } 
    public LevelParameters Level { get => m_level; set => m_level = value; }
    public Score Score { get => m_score; set => m_score = value; }
    public Audio Audio { get => m_audio; set => m_audio = value; }

    #endregion

    private void Start()
    {

        Audio.PlayMusic(true);

        InitializeLevel();

        DataStorage.LoadGame();
    }

    private void Awake()
    {
        Screen.fullScreen = false;

        if (m_instance == null)
        {
            m_instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (m_instance != this) Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        Audio.SourceMusic = gameObject.AddComponent<AudioSource>();
        Audio.SourceRandomPitchSFX = gameObject.AddComponent<AudioSource>();
        Audio.SourceSFX = gameObject.AddComponent<AudioSource>();

        DataStorage.LoadOptions();
    }

    private Color[]MakeColors(int count)
    {
        Color[] result = new Color[count];

        float colorStep = 1f / (count + 1);

        float hue = 0f;
        float saturation = 0.5f;
        float value = 1f;

        for(int i = 0; i< count; i++)
        {
            float newHue = hue + (colorStep * i);

            result[i] = Color.HSVToRGB(newHue,saturation,value);
        }
        return result;
    }

    public void TurnDone()
    {
        Audio.PlaySound("Drop");

        if (IsAllTokensConnected())
        {
            Destroy(m_field.gameObject);

            Debug.Log("WIN!");

            Audio.PlaySound("Victory");

            if (m_level.Turns > 0)
            {
                m_level.Turns--;
            }

            Score.AddLevelBonus();

            Hud.Instance.CountScore(m_level.Turns);

            m_currentLevel++;
        }
        else
        {
            Debug.Log("Continue!");
        }
    }

    public bool IsAllTokensConnected()
    {
        for (var i = 0; i < TokensByTypes.Count; i++)
        {
            if (IsTokensConnected(TokensByTypes[i]) == false)
            {
                return false;
            }
        }
        return true;
    }

    private bool IsTokensConnected(List<Token> tokens)
    {
        if (tokens.Count == 0)
        {
            return true;
        }

        List<Token> connectedTokens = new List<Token>();

        connectedTokens.Add(tokens[0]);

        bool moved = true;

        while (moved)
        {
            moved = false;

            for (int i = 0; i < connectedTokens.Count; i++)
            {
                for (int j = 0; j < tokens.Count; j++)
                {
                    if (IsTokensNear(tokens[j], connectedTokens[i]))
                    {
                        if (connectedTokens.Contains(tokens[j]) == false)
                        {
                            connectedTokens.Add(tokens[j]);
                            moved = true;
                        }
                    }
                }
            }
        }

        if (tokens.Count == connectedTokens.Count)
        {
            return true;
        }
        return false;
    }

    private bool IsTokensNear(Token first, Token second)
    {
        if ((int)first.transform.position.x == (int)second.transform.position.x + 1 ||
                 (int)first.transform.position.x == (int)second.transform.position.x - 1)
        {
            if ((int)first.transform.position.y == (int)second.transform.position.y)
            {
                return true;
            }
        }

        if ((int)first.transform.position.y == (int)second.transform.position.y + 1 ||
                   (int)first.transform.position.y == (int)second.transform.position.y - 1)

        {
            if ((int)first.transform.position.x == (int)second.transform.position.x)
            {
                return true;
            }
        }
        return false;
    }

    public void InitializeLevel()
    {
        m_level = new LevelParameters(m_currentLevel);

        TokenColors = MakeColors(m_level.TokenTypes);

        TokensByTypes = new List<List<Token>>();

        for (int i = 0; i < m_level.TokenTypes; i++)
        {
            TokensByTypes.Add(new List<Token>());
        }

        m_field = Field.Create(m_level.FieldSize, m_level.FreeSpace);
    }

    public void Reset()
    {
        CurrentLevel = 1;

        Score.CurrentScore = 0;

        Level.Turns = 0;

        Destroy(m_field.gameObject);

        DataStorage.SaveOptions();

        InitializeLevel();
    }
}
