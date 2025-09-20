/***********************************************************************
 * File: Program.cs
 * Project: Hangman Game (Console-Based)
 * Author: Alex Kachur
 * Date: February 19, 2025
 * Description: This file contains a professionally refactored console-based Hangman game.
 ***********************************************************************/

namespace HangmanGame
{
    #region Enums and Configuration

    /// <summary>
    /// Defines the difficulty levels for the game, ensuring type safety.
    /// </summary>
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard,
        Impossible
    }

    /// <summary>
    /// Represents the specific outcome of a letter guess attempt.
    /// </summary>
    public enum GuessResult
    {
        Correct,
        Incorrect,
        AlreadyGuessed,
        InvalidInput
    }

    /// <summary>
    /// Holds the configuration settings for a specific difficulty level.
    /// </summary>
    public class GameConfiguration
    {
        public Difficulty Difficulty { get; }
        public int MaxAttempts { get; }

        public GameConfiguration(Difficulty difficulty, int maxAttempts)
        {
            Difficulty = difficulty;
            MaxAttempts = maxAttempts;
        }
    }

    #endregion

    #region Core Game Logic

    /// <summary>
    /// Encapsulates the core state and logic of a single Hangman game instance.
    /// It is responsible for tracking the word, guesses, and attempts remaining.
    /// </summary>
    public class Hangman
    {
        private readonly string _wordToGuess;
        private readonly HashSet<char> _guessedLetters = new HashSet<char>();

        /// <summary>
        /// Gets the maximum number of incorrect attempts allowed.
        /// </summary>
        public int MaxAttempts { get; }

        /// <summary>
        /// Gets the number of incorrect attempts remaining.
        /// </summary>
        public int RemainingAttempts { get; private set; }

        /// <summary>
        /// Initializes a new instance of the Hangman game.
        /// </summary>
        /// <param name="wordProvider">The word provider to supply the secret word.</param>
        /// <param name="maxAttempts">The number of lives the player has.</param>
        public Hangman(WordProvider wordProvider, int maxAttempts)
        {
            if (wordProvider == null)
                throw new ArgumentNullException(nameof(wordProvider));

            _wordToGuess = wordProvider.GetWord().ToLower();
            MaxAttempts = maxAttempts;
            RemainingAttempts = maxAttempts;
        }

        /// <summary>
        /// Processes a player's letter guess and updates the game state.
        /// </summary>
        /// <param name="letter">The character guessed by the player.</param>
        /// <returns>A <see cref="GuessResult"/> indicating the outcome of the guess.</returns>
        public GuessResult GuessLetter(char letter)
        {
            letter = char.ToLower(letter);

            if (!char.IsLetter(letter))
            {
                return GuessResult.InvalidInput;
            }

            if (_guessedLetters.Contains(letter))
            {
                return GuessResult.AlreadyGuessed;
            }

            _guessedLetters.Add(letter);

            if (!_wordToGuess.Contains(letter))
            {
                RemainingAttempts--;
                return GuessResult.Incorrect;
            }

            return GuessResult.Correct;
        }

        /// <summary>
        /// Gets the current display state of the word, showing guessed letters and placeholders.
        /// </summary>
        /// <returns>A formatted string representing the word's progress (e.g., "_ p p _ _").</returns>
        public string GetWordProgress()
        {
            var progress = _wordToGuess.Select(c => _guessedLetters.Contains(c) ? c : '_');
            return string.Join(" ", progress);
        }

        /// <summary>
        /// Checks if the player has successfully guessed the word.
        /// </summary>
        public bool IsWin() => !_wordToGuess.Except(_guessedLetters).Any();

        /// <summary>
        /// Checks if the game has concluded (either by winning or losing).
        /// </summary>
        public bool IsGameOver() => IsWin() || RemainingAttempts <= 0;

        /// <summary>
        /// Reveals the secret word. Should only be called at the end of the game.
        /// </summary>
        public string RevealWord() => _wordToGuess;
    }

    #endregion

    #region Word Providers

    /// <summary>
    /// Defines the contract for any class that provides words for the Hangman game.
    /// This abstraction allows for different word sources (e.g., hardcoded lists, files, APIs).
    /// </summary>
    public abstract class WordProvider
    {
        public abstract string GetWord();
    }

    /// <summary>
    /// Provides words from hardcoded lists based on a specified difficulty level.
    /// </summary>
    public class DifficultyWordProvider : WordProvider
    {
        // Use a single static Random instance to ensure better random number generation.
        private static readonly Random _random = new Random();

        private readonly Dictionary<Difficulty, List<string>> _wordLists = new Dictionary<Difficulty, List<string>>
        {
            { Difficulty.Easy, new List<string> { "dog", "car", "sky", "book", "cup" } },
            { Difficulty.Medium, new List<string> { "computer", "guitar", "bicycle", "planet", "ocean" } },
            { Difficulty.Hard, new List<string> { "polymorphism", "abstraction", "encapsulation", "inheritance", "asynchronous" } }
        };

        private readonly Difficulty _difficulty;

        public DifficultyWordProvider(Difficulty difficulty)
        {
            _difficulty = difficulty;
        }

        /// <summary>
        /// Retrieves a random word corresponding to the selected difficulty.
        /// </summary>
        /// <returns>A randomly selected word as a string.</returns>
        public override string GetWord()
        {
            List<string> wordList = _difficulty switch
            {
                Difficulty.Easy => _wordLists[Difficulty.Easy],
                Difficulty.Medium => _wordLists[Difficulty.Medium],
                Difficulty.Hard => _wordLists[Difficulty.Hard],
                Difficulty.Impossible => _wordLists[Difficulty.Hard], // Impossible uses hard words
                _ => _wordLists[Difficulty.Easy], // Default fallback
            };
            return wordList[_random.Next(wordList.Count)];
        }
    }

    #endregion

    #region UI and Display

    /// <summary>
    /// Handles all console input and output, separating UI concerns from game logic.
    /// </summary>
    public static class ConsoleDisplay
    {
        private static readonly string[] _hangmanStages =
        {
                @"
                  +---+
                  |   |
                      |
                      |
                      |
                      |
            =========",
                @"
                  +---+
                  |   |
                  O   |
                      |
                      |
                      |
            =========",
                @"
                  +---+
                  |   |
                  O   |
                  |   |
                      |
                      |
            =========",
                @"
                  +---+
                  |   |
                  O   |
                 /|   |
                      |
                      |
            =========",
                @"
                  +---+
                  |   |
                  O   |
                 /|\  |
                      |
                      |
            =========",
                @"
                  +---+
                  |   |
                  O   |
                 /|\  |
                 /    |
                      |
            =========",
                @"
                  +---+
                  |   |
                  O   |
                 /|\  |
                 / \  |
                      |
            ========="
        };

        /// <summary>
        /// Displays the main menu and prompts for user selection.
        /// </summary>
        /// <returns>The user's menu choice as a string.</returns>
        public static string ShowMainMenu()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=================");
            Console.WriteLine("    HANGMAN");
            Console.WriteLine("=================");
            Console.ResetColor();
            Console.WriteLine("1. Play Game");
            Console.WriteLine("2. How to Play");
            Console.WriteLine("3. View Scoreboard");
            Console.WriteLine("4. Exit");
            Console.Write("\nChoose an option: ");
            return Console.ReadLine();
        }

        /// <summary>
        /// Displays the tutorial/instructions screen.
        /// </summary>
        public static void ShowTutorial()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=================");
            Console.WriteLine("   HOW TO PLAY");
            Console.WriteLine("=================");
            Console.ResetColor();
            Console.WriteLine("Hangman is a classic word guessing game.");
            Console.WriteLine("- A secret word is chosen based on the difficulty you select.");
            Console.WriteLine("- Guess one letter at a time to reveal the word.");
            Console.WriteLine("- Each incorrect guess adds a part to the hangman drawing.");
            Console.WriteLine("- Win by guessing the word before the drawing is complete!");
            WaitForAnyKey();
        }

        /// <summary>
        /// Displays the difficulty selection menu.
        /// </summary>
        /// <returns>The user's difficulty choice as a string.</returns>
        public static string ShowDifficultyMenu()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("======================");
            Console.WriteLine("  SELECT DIFFICULTY");
            Console.WriteLine("======================");
            Console.ResetColor();
            Console.WriteLine("1. Easy (6 lives, simple words)");
            Console.WriteLine("2. Medium (6 lives, moderate words)");
            Console.WriteLine("3. Hard (6 lives, advanced words)");
            Console.WriteLine("4. Impossible (1 life, hardest words)");
            Console.Write("\nChoice: ");
            return Console.ReadLine();
        }

        /// <summary>
        /// Displays the current state of the game board.
        /// </summary>
        /// <param name="game">The current Hangman game instance.</param>
        /// <param name="message">An optional message to display (e.g., result of the last guess).</param>
        public static void DrawGameBoard(Hangman game, string message = "")
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            int stage = game.MaxAttempts - game.RemainingAttempts;
            Console.WriteLine(_hangmanStages[Math.Min(stage, _hangmanStages.Length - 1)]);
            Console.ResetColor();

            Console.WriteLine($"\nWord: {game.GetWordProgress()}");
            Console.WriteLine($"Attempts Left: {game.RemainingAttempts}");

            if (!string.IsNullOrEmpty(message))
            {
                Console.WriteLine(message);
            }
        }

        /// <summary>
        /// Displays the final game over screen, indicating a win or loss.
        /// </summary>
        /// <param name="game">The completed Hangman game instance.</param>
        public static void ShowGameOverScreen(Hangman game)
        {
            Console.Clear();
            int stage = game.MaxAttempts - game.RemainingAttempts;
            Console.WriteLine(_hangmanStages[Math.Min(stage, _hangmanStages.Length - 1)]);

            if (game.IsWin())
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nCongratulations! You guessed the word: {game.RevealWord()}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nGame Over! The word was: {game.RevealWord()}");
            }
            Console.ResetColor();
            WaitForAnyKey();
        }

        /// <summary>
        /// Displays the scoreboard.
        /// </summary>
        /// <param name="scoreboard">The scoreboard instance to display.</param>
        public static void ShowScoreboard(Scoreboard scoreboard)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=================");
            Console.WriteLine("   SCOREBOARD");
            Console.WriteLine("=================");
            Console.ResetColor();
            Console.WriteLine($"Wins: {scoreboard.Wins}");
            Console.WriteLine($"Losses: {scoreboard.Losses}");
            WaitForAnyKey();
        }

        /// <summary>
        /// Prompts the user to press any key to continue.
        /// </summary>
        /// <param name="prompt">The message to display to the user.</param>
        private static void WaitForAnyKey(string prompt = "\nPress any key to continue...")
        {
            Console.WriteLine(prompt);
            Console.ReadKey();
        }
    }

    #endregion

    #region Application Flow and Control

    /// <summary>
    /// Manages player statistics (wins and losses).
    /// </summary>
    public class Scoreboard
    {
        public int Wins { get; private set; }
        public int Losses { get; private set; }

        public void RecordWin() => Wins++;
        public void RecordLoss() => Losses++;
    }

    /// <summary>
    /// Orchestrates a single game session, connecting the UI, game logic, and word source.
    /// </summary>
    public class GameController
    {
        private readonly Scoreboard _scoreboard;

        public GameController(Scoreboard scoreboard)
        {
            _scoreboard = scoreboard ?? throw new ArgumentNullException(nameof(scoreboard));
        }

        /// <summary>
        /// Starts and manages the entire lifecycle of a new game.
        /// </summary>
        public void PlayGame()
        {
            GameConfiguration config = SelectDifficulty();
            WordProvider wordProvider = new DifficultyWordProvider(config.Difficulty);
            Hangman game = new Hangman(wordProvider, config.MaxAttempts);

            GameLoop(game);

            ConsoleDisplay.ShowGameOverScreen(game);

            // Update scoreboard after the game is over
            if (game.IsWin())
            {
                _scoreboard.RecordWin();
            }
            else
            {
                _scoreboard.RecordLoss();
            }
        }

        /// <summary>
        /// Manages the primary game loop where the player makes guesses.
        /// </summary>
        private void GameLoop(Hangman game)
        {
            string message = "";
            while (!game.IsGameOver())
            {
                ConsoleDisplay.DrawGameBoard(game, message);

                Console.Write("\nGuess a letter: ");
                char guess = Console.ReadKey().KeyChar;

                GuessResult result = game.GuessLetter(guess);
                message = GetMessageForResult(result, guess);
            }
        }

        /// <summary>
        /// Prompts the user to select a difficulty and returns the corresponding configuration.
        /// </summary>
        private GameConfiguration SelectDifficulty()
        {
            string choice = ConsoleDisplay.ShowDifficultyMenu();
            return choice switch
            {
                "2" => new GameConfiguration(Difficulty.Medium, 6),
                "3" => new GameConfiguration(Difficulty.Hard, 6),
                "4" => new GameConfiguration(Difficulty.Impossible, 1),
                _ => new GameConfiguration(Difficulty.Easy, 6), // Default to Easy
            };
        }

        /// <summary>
        /// Generates a user-friendly, colored message based on the guess result.
        /// </summary>
        private string GetMessageForResult(GuessResult result, char letter)
        {
            // This is a bit of a trick to embed color codes in a string.
            // It's not the cleanest way, but avoids a complex UI handler for this simple case.
            switch (result)
            {
                case GuessResult.Correct:
                    Console.ForegroundColor = ConsoleColor.Green;
                    return $"Correct! '{letter}' is in the word.";
                case GuessResult.Incorrect:
                    Console.ForegroundColor = ConsoleColor.Red;
                    return $"Wrong! '{letter}' is not in the word.";
                case GuessResult.AlreadyGuessed:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    return $"You already guessed '{letter}'. Try another letter.";
                case GuessResult.InvalidInput:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    return "Invalid input. Please enter a letter.";
                default:
                    return "";
            }
        }
    }

    /// <summary>
    /// The main entry point for the application.
    /// It initializes services and runs the main application loop.
    /// </summary>
    class Program
    {
        private static readonly Scoreboard _scoreboard = new Scoreboard();
        private static readonly GameController _gameController = new GameController(_scoreboard);

        /// <summary>
        /// Main application entry point.
        /// </summary>
        static void Main(string[] args)
        {
            Console.Title = "Hangman Professional Edition";
            bool exit = false;

            while (!exit)
            {
                string choice = ConsoleDisplay.ShowMainMenu();

                switch (choice)
                {
                    case "1":
                        _gameController.PlayGame();
                        break;
                    case "2":
                        ConsoleDisplay.ShowTutorial();
                        break;
                    case "3":
                        ConsoleDisplay.ShowScoreboard(_scoreboard);
                        break;
                    case "4":
                        exit = true;
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid option. Press any key to try again...");
                        Console.ReadKey();
                        Console.ResetColor();
                        break;
                }
            }
        }
    }
    #endregion
}