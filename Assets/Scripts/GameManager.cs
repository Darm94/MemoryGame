using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    GameObject card ;

    [SerializeField]
    Vector3[] cards ;

    [SerializeField]
    Texture2D[] images ;

    [SerializeField]
    float startX ;

    [SerializeField]
    float startY ;

    [SerializeField]
    float planeZ ;

    [SerializeField]
    float deltax = 1.1f;

    [SerializeField]
    float deltay = 1.1f;

    [SerializeField]
    int columns = 5;

    [SerializeField]
    int rows = 6;

    [SerializeField]
    GameObject winUI ;
    [SerializeField]
    GameObject loseUI ;

    [SerializeField]
    AudioSource matchedPairAudioSource ;

    [SerializeField] private AudioSource wrongPairAudioSource;
    [SerializeField] private AudioSource playLoopAudioSource;
    [SerializeField] private AudioSource alienAudioSource;
    int pairs ;

    InteractiveCard selectedCard1 ;
    InteractiveCard selectedCard2 ;

    //UI and custom rules
    [SerializeField] private float Playtime;
    [SerializeField] private Image TimeBar;
    private float remainingPlayTime;
    [SerializeField] private TextMeshProUGUI timeText;
    bool isGameOver ;
    private List<InteractiveCard> activeCards = new List<InteractiveCard>();
    
    //BackToStartScene
    [SerializeField] private SceneAsset StartScene ;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        remainingPlayTime = Playtime;
        timeText.text = remainingPlayTime.ToString();
        if (rows * columns != images.Length * 2)
        {
            Debug.LogWarning("Number of r*c is not equal to provided cards, quit...");
            return;
        }

        pairs = columns * rows / 2; //images.Length  its valid too

        //Random Sort
        System.Random random = new System.Random();
        images = images.OrderBy(x => random.Next()).ToArray();

        cards = new Vector3[rows * columns];

        float dx = startX;
        float dy = startY;

        int counter = 0;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                cards[counter++] = new Vector3(dx, dy, planeZ);
                dx += deltax;
            }
            dx = startX;
            dy += deltay;
        }

        cards = cards.OrderBy(x => random.Next()).ToArray();
        
        // Start creating (instantiate) cards, setting images etc
        counter = 0;

        int row = 0;

        foreach (Vector3 pos in cards)
        {
            GameObject go = Instantiate(card);

            go.SetActive(true); // WE ACTIVATE THE

            go.transform.position = pos;

            // We set card texture (using shader graph)
            go.GetComponent<MeshRenderer>().material.SetTexture("_MainTexture", images[row]);
            InteractiveCard interactiveCard = go.GetComponent<InteractiveCard>();
            
            // this event will be called on this class by the InteractiveCard
            go.GetComponent<InteractiveCard>().OnClicked += SelectedCard;

            // We set Interactive card cover image name
            go.GetComponent<InteractiveCard>().imageName = images[row].name;
            activeCards.Add(interactiveCard); // Aggiungi la carta alla lista
            
            counter++;

            if (counter % 2 == 0)
            {
                row++;
            }
            
        }
    }

    void Update()
    {
        if (!isGameOver)
        {
            remainingPlayTime -= Time.deltaTime;
            timeText.text = ((int)remainingPlayTime).ToString()+"s";
            TimeBar.fillAmount = remainingPlayTime / Playtime;

            //GameOver and And Of Loop
            if (remainingPlayTime < 0)
            {
                loseUI.SetActive(true);
                playLoopAudioSource.Stop();
                isGameOver = true;
                loseUI.GetComponent<AudioSource>().Play();
            }
        }
    }

    private void SelectedCard(InteractiveCard card, bool selected)
    {
        //Condizione separata per l' Alieno
        // Condizione di rimescolamento all'inizio, basata su card.isAlien()
        if (selected && card.isAlien())
        {
            alienAudioSource.Play();
            // Potresti anche voler resettare le carte selezionate finora
            if (selectedCard1) selectedCard1.ResetMe();
            if (selectedCard2) selectedCard2.ResetMe();
            selectedCard1 = null;
            selectedCard2 = null;
            RimescolaCarte();
            return; // Esci dalla funzione SelectedCard dopo il rimescolamento
        }
        
        //Se non ho selezionato la 1 card e viene selezionata
        if (selectedCard1 == null && selected)
        {
            selectedCard1 = card;
        }
        //se ho già selezionato la 1 e l' ho deselezionata
        else if (selectedCard1 == card && !selected)
        {
             selectedCard1.ResetMe();
            selectedCard1 = null;
        }
        //se ho già selezionato la 2 e l'  ho deselezionata
        else if (selectedCard2 != null && card == selectedCard2 && !selected)
        {
             selectedCard2.ResetMe();
            selectedCard2 = null;
        }
        //se non ho già selezionato la 2 e l' ho selezionata
        else if (selectedCard2 == null && card != selectedCard1 && selected)
        {
            selectedCard2 = card;

            if (selectedCard1.Compare(selectedCard2))
            {
                // ok match!
                matchedPairAudioSource.Play();
                
                activeCards.Remove(selectedCard1); // Rimuovi la prima carta dalla lista
                activeCards.Remove(selectedCard2); // Rimuovi la seconda carta dalla lista
                selectedCard1.HideAndDestroy();
                selectedCard2.HideAndDestroy();
                selectedCard1 = null;
                selectedCard2 = null;
                pairs--;

                if (pairs <= 1)
                {
                    // GameOver and WIN
                    winUI.SetActive(true);
                    playLoopAudioSource.Stop();
                    winUI.GetComponent<AudioSource>().Play();
                }
            }
            else
            {
                wrongPairAudioSource.Play();
                //flip back
                selectedCard1.ResetMe(); // flip back
                selectedCard2.ResetMe();
                
                //play error sound
                selectedCard1 = null;
                selectedCard2 = null;
            }
        }
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void BackToMainMenu()
    {
        SceneManager.LoadScene(StartScene.name);
    }
    
    private void RimescolaCarte()
    {
        System.Random random = new System.Random();
        activeCards = activeCards.OrderBy(x => random.Next()).ToList();

        // Riassegna le posizioni alle carte rimaste
        for (int i = 0; i < activeCards.Count; i++)
        {
            activeCards[i].transform.position = cards[i]; // Usa l'array di posizioni originale
            activeCards[i].ResetMe(); // Assicurati che le carte siano girate a faccia in giù
        }

        // Rimescola anche l'array delle posizioni per la prossima rimescolata
        cards = cards.OrderBy(x => random.Next()).ToArray();
    }
}
