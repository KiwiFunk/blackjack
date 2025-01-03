﻿using BlackjackGame;

Cards[,] cardDeck = new Cards[4, 13];                       //Row = Suit, Column = Card Value
Random cardSelect = new Random();                           //Init a random function for selecting a card.
string? returnResult;                                       //Holds user input strings.

List<Player> players = new List<Player>();                  //Dealer is at index 0, players are at index 1 and up.     

int currentRound = 1;                                       //Incremented each round to track the current round.
int gameStatus = 0;                                         //Altered by GameEnd() to prompt replay, new game, or quit. 


///Game Start///

do
{                                                           //Outer loop for new game.
    BuildDeck();
    TitleScreen();
    InitializePlayers();
    do
    {                                                       //Loop for new round.                              
        Betting();
        DealCards();                                        //Deal Cards for each player. Two face up each. Deal Cards for dealer, 1 face up, 1 face down.
        for (int i = 1; i < players.Count; i++)             //Iterate though player loop, moving on to the next once they have either stood, or gone bust.
        {
            do
            {
                //Console.Clear();
                Console.WriteLine($"Round: {currentRound}. Current player: {players[i].playerName}");
                ShowHand(i);
                Console.WriteLine("Do you want to Hit, or Stand?");
                returnResult = Console.ReadLine();
                if (returnResult != null)
                {
                    if (returnResult.ToLower().Trim() == "stand")
                    {
                        Stand(i);
                    }
                    else if (returnResult.ToLower().Trim() == "hit")
                    {
                        Hit(i);
                    }
                    else Console.WriteLine("Invalid Input. Would you like to Hit or Stand?");
                }
            } while (players[i].inGame);                    //Repeat loop while player is not Bust, and hasn't Stood
        }
        DealerPlay();                                       //Once Every player has stood or busted, Dealer takes their turn.
        GameEnd();                                          //Prompt new round, new game, or quit.
    } while (gameStatus == 0);
} while (gameStatus == 1);


///Methods///

void GamePause() => Thread.Sleep(500);                      //Allows for globally adjusting the pause duration.

int TotalValue(int index)
{
    int totalValue = 0;
    foreach (var card in players[index].hand)
    {
        totalValue += card.value;
    }
    return totalValue;
}

void ShowHand(int currentPlayer)
{
    Console.WriteLine($"Your hand is currently: ");
    for (int i = 0; i < players[currentPlayer].hand.Count(); i++)
    {
        Console.Write($"{players[currentPlayer].hand[i].CardName}({players[currentPlayer].hand[i].value}), ");
    }
    Console.WriteLine("");
    Console.WriteLine($"For a total of {TotalValue(currentPlayer)}");
}

void CheckforAces(int player = 0)                           //If player index is given, use that. Else default to dealer index.
{
    if (!players[player].hasAce) return;                    //If player flagged for ace, run method, else return.

    int handTotal = TotalValue(player);

    foreach (var card in players[player].hand)
    {
        if (card.name == "Ace" && card.value == 1)
        {
            card.value = 11;
            handTotal += 10;
        }
    }

    foreach (var card in players[player].hand)
    {
        if (card.name == "Ace" && handTotal > 21)
        {
            card.value = 1;
            handTotal -= 10;
            if (handTotal <= 21) break;
        }
    }
    //Player can't be bust before using Hit(), and Hit() handles bust condition.
}

void BuildDeck()
{
    for (int i = 0; i < cardDeck.GetLength(0); i++)         //Populate our deck of cards on program start.
    {
        string[] names = new string[13] { "Ace", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Jack", "Queen", "King" };
        string suit = string.Empty;

        switch (i)
        {
            case 0:                                         //Arrays are 0-based.
                suit = "Clubs";
                break;
            case 1:
                suit = "Diamonds";
                break;
            case 2:
                suit = "Hearts";
                break;
            case 3:
                suit = "Spades";
                break;
        }

        for (int j = 0; j < cardDeck.GetLength(1); j++)
        {
            Cards card = new Cards();                       //Create a new instance of the Cards class for each array address.

            card.suit = suit;
            card.name = names[j];

            if (j > 9) card.value = 10;                     //In BlackJack, face cards count as 10
            else card.value = j + 1;

            card.inPlay = false;

            cardDeck[i, j] = card;                          //Assign the new card instace to the array address.
        }
    }
}

void TitleScreen()
{
    //Console.Clear();
    Console.WriteLine(@"
    ______ _            _      ___            _    
    | ___ \ |          | |    |_  |          | |   
    | |_/ / | __ _  ___| | __   | | __ _  ___| | __
    | ___ \ |/ _` |/ __| |/ /   | |/ _` |/ __| |/ /
    | |_/ / | (_| | (__|   </\__/ / (_| | (__|   < 
    \____/|_|\__,_|\___|_|\_\____/ \__,_|\___|_|\_\
                                                   ");
    Console.WriteLine("\t\tPress Any Key To Begin");
    Console.ReadKey();
}

void InitializePlayers()
{
    //Console.Clear();
    bool gameStart = false;

    Player dealer = new Player("DEALER");
    players.Add(dealer);

    Console.WriteLine("Please enter your Player Name(s) then hit return. Type Start to begin");
    do
    {
        returnResult = Console.ReadLine();
        if (returnResult != null && !String.IsNullOrEmpty(returnResult) && !String.IsNullOrWhiteSpace(returnResult))
        {
            if (returnResult.ToLower().Trim() == "start")
            {
                if (players.Count == 0)
                {
                    Console.WriteLine("You need to enter at least one name before starting!");
                }
                else
                {
                    gameStart = true;                       //Start the game
                    //Console.Clear();
                }
            }
            else
            {
                Player player = new Player(returnResult.ToUpper().Trim());
                players.Add(player);
                //Console.Clear();
                Console.WriteLine($"{returnResult.ToUpper()} Registered. Enter another name, or type start to begin!");
            }
        }
        else Console.WriteLine("Please enter a valid name!");
    } while (!gameStart);
}

void Betting()
{
    bool validEnty = false;
    double currentWager = 0.0;
    if (currentRound == 1)                                  //Check to see if game is a new game, or continuation
    {
        for (int i = 1; i < players.Count; i++)
        {
            players[i].bank[0] = 10.00;                     //Starting Bank balance   
            players[i].bank[1] = 0.00;                      //Current Bet Amount
        }
        if (players.Count <= 2) Console.WriteLine("You have been given $10.00 as a new player bonus!");             //If we only have 2 entries in players, we have one player[1] and the dealer[0]
        else Console.WriteLine("You have each been given a new player bonus of $10.00!");
        GamePause();
    }

    for (int i = 1; i < players.Count; i++)
    {
        validEnty = false;
        //Console.Clear();
        Console.WriteLine($"Current Player: {players[i].playerName}");
        Console.WriteLine($"You have ${players[i].bank[0]:N2},how much do you want to wager?");

        do
        {
            returnResult = Console.ReadLine();
            if (double.TryParse(returnResult, out currentWager))
            {
                if (currentWager <= players[i].bank[0])
                {
                    //Console.Clear();
                    Console.WriteLine($"{players[i].playerName} wagered ${currentWager:N2}! Good Luck!");
                    players[i].bank[1] = currentWager;
                    players[i].bank[0] -= currentWager;     //Remove wagered amount from bank balance.
                    validEnty = true;
                    GamePause();
                }
                else Console.WriteLine($"Your bank balance is only {players[i].bank[0]:N2}, please wager an amount you can afford!");
            }
            else Console.WriteLine("Please Enter a valid wager.");

        } while (!validEnty);
    }

    //Console.Clear();
    Console.WriteLine("All bets have been taken! Let the game begin!");
    GamePause();
    //Console.Clear();
}

Cards DrawCard(Cards[,] cardArray, int player = 0)          //Take in int for player index to use for assigning ace status.
{
    int suit = cardSelect.Next(0, 4);                       //Arrays are zero based. .Next upper bound is exclusive, so will never return 4.
    int card = cardSelect.Next(0, 13);
    Cards draw = cardArray[suit, card];
    bool validCard = false;

    do
    {
        if (!cardDeck[suit, card].inPlay)                   //If the selected array address is valid, replace it with 0, decrement total cards.
        {
            cardDeck[suit, card].inPlay = true;
            if (cardDeck[suit, card].name == "Ace")
            {
                players[player].hasAce = true;
            }
            validCard = true;
        }
        else                                                //If selected address .inPlay == true, select a new one.
        {
            suit = cardSelect.Next(0, 4);
            card = cardSelect.Next(0, 13);
            draw = cardArray[suit, card];
        }
    } while (!validCard);
    return draw;
}

void DealCards()
{
    players[0].hand.Add(DrawCard(cardDeck));
    players[0].hand.Add(DrawCard(cardDeck));
    CheckforAces();
    Console.WriteLine($"Dealer draws a {players[0].hand[1].CardName}({players[0].hand[1].value}) and a face down card!");
    Console.WriteLine();
    GamePause();

    //Now Assign Players
    for (int i = 1; i < players.Count(); i++)
    {
        players[i].hand.Add(DrawCard(cardDeck, i));
        players[i].hand.Add(DrawCard(cardDeck, i));
        CheckforAces(i);
        Console.WriteLine($"{players[i].playerName} drew a {players[i].hand[0].CardName}({players[i].hand[0].value}) and {players[i].hand[1].CardName}({players[i].hand[1].value}) for a total of {TotalValue(i)}!");
        Console.WriteLine();
        GamePause();
    }
}

void Hit(int currentPlayer)
{
    Console.WriteLine($"{players[currentPlayer].playerName} has decided to hit!");
    players[currentPlayer].hand.Add(DrawCard(cardDeck, currentPlayer));
    CheckforAces(currentPlayer);
    //Handle Bust condition. Remove player from current loop. Remove their bet from bank balance.
    if (TotalValue(currentPlayer) > 21)
    {
        Console.WriteLine($"BUST!! {players[currentPlayer].playerName} drew a {players[currentPlayer].hand.Last().CardName}({players[currentPlayer].hand.Last().value}) for a total of {TotalValue(currentPlayer)}!");
        players[currentPlayer].inGame = false;
        players[currentPlayer].isBust = true;
        //Wager was already taken from bank during Betting()
        BankruptCheck(currentPlayer);
        GamePause();
    }
    else
    {
        Console.WriteLine($"{players[currentPlayer].playerName} drew a {players[currentPlayer].hand.Last().CardName}({players[currentPlayer].hand.Last().value}) for a total of {TotalValue(currentPlayer)}!");
        GamePause();
    }
}

void Stand(int currentPlayer)
{
    //Console.Clear();
    Console.WriteLine($"{players[currentPlayer].playerName} has decided to stand! The total of their hand is {TotalValue(currentPlayer)}.");
    players[currentPlayer].inGame = false;
    GamePause();
}

void BankruptCheck(int currentPlayer)
{
    if (players[currentPlayer].bank[0] <= 0)
    {
        players[currentPlayer].isBankrupt = true;
    }
}

void DealerPlay()
{
    bool dealerPlaying = true;
    //Reveal the face down card.
    Console.WriteLine($"Dealer reveals their facedown card. Their cards are {players[0].hand[0].CardName}({players[0].hand[0].value}) and {players[0].hand[1].CardName}({players[0].hand[1].value}).");
    GamePause();
    do
    {
        if (TotalValue(0) < 17)                                         //Dealer Index is 0. If dealer total is < 17, they hit. If its >= 17 they stand.
        {
            players[0].hand.Add(DrawCard(cardDeck));
            CheckforAces();
            Console.WriteLine($"Dealer draws a {players[0].hand[players[0].hand.Count() - 1].CardName}({players[0].hand[players[0].hand.Count() - 1].value}) for a total of {TotalValue(0)}.");

            if (TotalValue(0) > 21)                                     //If dealer busts, every player who STOOD and didn't bust win.
            {
                Console.WriteLine("Dealer has gone Bust!");
                bool playerCashedOut = false;
                for (int i = 1; i < players.Count; i++)
                {
                    if (!players[i].isBust)
                    {
                        players[i].bank[0] += players[i].bank[1] * 2;
                        Console.WriteLine($"Congrats {players[i].playerName}, you win! Your bank balance is now ${players[i].bank[0]}!");
                        playerCashedOut = true;
                    }
                }
                if (!playerCashedOut) Console.WriteLine("All players went bust. Nobody wins this time.");
                dealerPlaying = false;
            }
        }
        else
        {
            //If dealer reaches a valid hand, total and compare to each valid players hand
            Console.WriteLine($"Dealer stands with a total of {TotalValue(0)}");
            dealerPlaying = false;
            Cashout();
        }
    } while (dealerPlaying);
}

void Cashout()
{
    //Console.Clear();
    int dealerTotal = TotalValue(0);
    Console.WriteLine($"Dealer total is {dealerTotal}.");
    for (int i = 1; i < players.Count; i++)
    {
        int playerTotal = TotalValue(i);
        //If player hand higher, they win. If equal, they only recieve their original bet amount. If less, they lose.
        if (playerTotal > dealerTotal && !players[i].isBust)
        {
            players[i].bank[0] += players[i].bank[1] * 2;
            Console.WriteLine($"Congrats {players[i].playerName}, you win! Your bank balance is now ${players[i].bank[0]}!");
        }

        else if (playerTotal == dealerTotal && !players[i].isBust)
        {
            Console.WriteLine($"Better than nothing {players[i].playerName}, you tie!");
            players[i].bank[0] += players[i].bank[1];
        }

        else if(!players[i].isBust)
        {
            Console.WriteLine($"Sorry {players[i].playerName}, you lose...");
            //Wager was already taken from bank during Betting()
        }
    }
    GamePause();
}

void GameEnd()
{
    ////Console.Clear();
    bool gameEnd = false;
    Console.WriteLine("Thanks for playing! Type 1 to start a new round, 2 for a New Game, or 3 to Quit!");
    do
    {
        returnResult = Console.ReadLine();
        if (returnResult != null)
        {
            if (returnResult.Trim() == "1")
            {
                gameStatus = 0;
                gameEnd = true;
                currentRound++;
                BuildDeck();
                int bankruptPlayers = 0;
                for (int i = players.Count() - 1; i >= 0; i--)                      //Iterate through players list in reverse to handle removal correctly.  
                {
                    players[i].ResetPlayerState();                                  //Reset player state for new round.
                    if (players[i].isBankrupt)                                      //If player is bankrupt, remove them from the game.
                    {
                        Console.WriteLine($"{players[i].playerName} is bankrupt and has been removed from the game.");
                        players.RemoveAt(i);
                        bankruptPlayers++;
                    }
                }
                if (bankruptPlayers >= players.Count())                                     //Dealer will never be bankrupt, so -1.
                {
                    gameStatus = 2;
                    Console.WriteLine("Sorry, there are no players left. Game Over!");          //Update to allow for new game(2) or quit(3) prompt.
                }
                else
                {
                    Console.WriteLine("New Round Starting!");
                    GamePause();
                    //Console.Clear();
                    Console.WriteLine($"Round {currentRound}. Please place your bets!");
                } 
            }
            else if (returnResult.Trim() == "2")
            {
                gameStatus = 1;
                players.Clear();                                                    //Better for memory management to clear the list, rather than create a new instance.  
                gameEnd = true;
            }
            else if (returnResult.Trim() == "3")
            {
                gameStatus = 2;
                gameEnd = true;
            }
            else Console.WriteLine("Invalid Input!");
        }
    } while (!gameEnd);
}