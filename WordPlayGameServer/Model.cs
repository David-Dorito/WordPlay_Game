using System;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;

namespace WordPlayGameServer
{
    public sealed class Player
    {
        public string Username { get; set; }
        public string ConnectionId { get; set; }
    }

    public sealed class Party : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string? Name { get; set; }

        private string hostName;
        public string HostName
        {
            get { return hostName; }
            set
            {
                hostName = value;
                if (string.IsNullOrEmpty(Name))
                    Name = $"{value}'s Party";
            }
        }

        public string? Description { get; set; } = "";

        public int MaxPlayerCount { get; set; }

        private List<Player> players;
        public List<Player> Players
        {
            get { return players; }
            set
            {
                players = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Players)));
            }
        }

        public int PlayerCount { get; set; } = 0;

        private bool englishOnly;
        public bool EnglishOnly
        {
            get { return englishOnly; }
            set
            {
                englishOnly = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EnglishOnly)));
            }
        }
        public string EnglishOnlyString { get; set; } = "";

        public int? Id { get; set; }

        public List<string> Messages { get; set; } = new List<string>();

        public bool InGame { get; set; } = false;

        public Party()
        {
            this.PropertyChanged += PropertyChangedHandler;
            if (string.IsNullOrEmpty(Name))
                Name = $"{HostName}'s Party";
            if (Players == null)
                Players = new List<Player>();
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Players))
                PlayerCount = Players.Count;
            if (e.PropertyName == nameof(EnglishOnly))
                if (EnglishOnly)
                    EnglishOnlyString = "English only";
                else EnglishOnlyString = "";
        }
    }

    public sealed class InformationPackage
    {
        public List<Party> PartiesList { get; set; }

        public int TotalPlayerCount { get; set; }
        public int WaitingPlayerCount { get; set; }
        public int TotalPartyCount { get; set; }
        public int WaitingPartyCount { get; set; }
    }

    public sealed class GameRound : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        
        public int Id { get; set; }
        
        public List<Player> RemainingPlayers { get; set; }

        private List<Player> players { get; set; } = new List<Player>();
        public List<Player> Players
        {
            get { return players; }
            set
            {
                players = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Players)));
            }
        }
        public List<Player> Imposters { get; set; } = new List<Player>();
        public List<Player> ThoseWhoKnow { get; set; } = new List<Player>();
        public List<Player> TurnOrder { get; set; }
        public List<string> Votes { get; set; } = new List<string>();
        public List<Player> VotePlayerList { get; set; } = new List<Player>();

        public string SecretWord { get; set; }
        public string SelectedHint { get; set; } = "";

        public bool HintsEnabled { get; set; }
        public int PlayerCount { get; set; }

        public List<string> GameMessages { get; set; } = new List<string>();

        public GameRound()
        {
            this.PropertyChanged += PropertyChangedHandler;
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Players))
            { 
                PlayerCount = Players.Count;
            }
        }
    }

    public sealed class WordResult
    {
        public string word { get; set; }
        public int score { get; set; }
    }

    public sealed class GameWords
    {
        public static string[] ValidNouns { get; } = new string[]
        {
            "apple", "banana", "chair", "desk", "school", "teacher", "student", "book", "pencil", "pen", "paper", "eraser", "backpack", "bus", "car", "bicycle", "motorcycle", "train", "airplane",
            "helicopter", "boat", "ship", "submarine", "rocket", "planet", "moon", "star", "galaxy", "universe", "sun", "earth", "mars", "jupiter", "saturn", "venus", "mercury", "pluto", "asteroid", "comet",
            "meteor", "cloud", "rain", "snow", "hail", "storm", "thunder", "lightning", "rainbow", "wind", "fog", "ice", "fire", "water", "ocean", "river", "lake", "pond", "stream", "waterfall",
            "beach", "sand", "rock", "stone", "pebble", "mountain", "hill", "valley", "forest", "jungle", "desert", "island", "cave", "cliff", "volcano", "earthquake", "tsunami", "avalanche", "tornado",
            "whirlpool", "wave", "dog", "cat", "rabbit", "hamster", "horse", "cow", "pig", "sheep", "goat", "chicken", "duck", "goose", "turkey", "parrot", "eagle", "hawk", "owl",
            "penguin", "flamingo", "shark", "whale", "dolphin", "octopus", "squid", "crab", "lobster", "seal", "otter", "frog", "toad", "snake", "lizard", "crocodile", "alligator", "turtle", "tortoise",
            "butterfly", "bee", "wasp", "ant", "spider", "fly", "mosquito", "ladybug", "beetle", "dragonfly", "grasshopper", "cricket", "moth", "worm", "slug", "snail", "fish", "goldfish", "salmon",
            "trout", "tuna", "bear", "lion", "tiger", "cheetah", "leopard", "jaguar", "panther", "wolf", "fox", "coyote", "deer", "moose", "elk", "bison", "buffalo", "camel", "zebra",
            "giraffe", "hippopotamus", "rhinoceros", "monkey", "gorilla", "chimpanzee", "orangutan", "lemur", "kangaroo", "koala", "panda", "sloth", "armadillo", "hedgehog", "bat", "rat", "mouse", "squirrel", "chipmunk",
            "beaver", "porcupine", "ferret", "unicorn", "dragon", "phoenix", "mermaid", "troll", "goblin", "orc", "elf", "giant", "fairy", "witch", "wizard", "knight", "princess", "king", "queen",
            "castle", "tower", "fortress", "bridge", "gate", "wall", "fence", "house", "apartment", "building", "skyscraper", "shop", "store", "mall", "market", "restaurant", "cafe", "hospital", "library",
            "museum", "theater", "cinema", "stadium", "arena", "court", "field", "park", "garden", "playground", "zoo", "aquarium", "farm", "barn", "garage", "workshop", "factory", "office", "studio",
            "lab", "hotel", "pizza", "burger", "hotdog", "sandwich", "taco", "burrito", "sushi", "ramen", "pasta", "spaghetti", "noodles", "salad", "steak", "chicken", "beef", "pork", "lamb",
            "shrimp", "ice", "cream", "cake", "cookie", "brownie", "muffin", "donut", "pancake", "waffle", "candy", "chocolate", "gum", "popcorn", "chips", "pretzel", "cracker", "pie", "custard",
            "pudding", "marshmallow", "cereal"
        };

        public static string[] InvalidNouns { get; } = new string[]
        {
            "guinea pig", "sea lion", "school bus", "bus stop", "police station", "fire station", "post office", "nintendo switch", "hollow knight", "among us", "animal crossing", "pac-man",
            "call of duty", "black widow", "doctor strange", "star wars", "luke skywalker", "darth vader", "millennium falcon", "death star", "harry potter", "hermione granger", "ron weasley", "gryffindor",
            "slytherin", "hufflepuff", "ravenclaw", "broomstick", "diagon alley", "butterbeer", "jurassic park", "t-rex", "aircraft carrier", "hot air balloon", "pacific ocean", "atlantic ocean", "indian ocean",
            "arctic ocean", "north america", "south america", "mount everest", "grand canyon", "niagara falls", "amazon rainforest", "sahara desert", "united states", "statue of liberty", "eiffel tower",
            "big ben", "great wall of china", "central park", "fish fillet"
        };

        public static string[] Adjectives { get; } = new string[]
        {
            "happy", "sad", "angry", "excited", "bored", "tired", "sleepy", "hungry", "thirsty", "scared", "brave", "strong", "weak", "fast", "slow", "quiet", "loud", "bright", "dark", "colorful",
            "red", "blue", "green", "yellow", "purple", "orange", "black", "white", "pink", "brown", "big", "small", "tall", "short", "fat", "thin", "clean", "dirty", "hot", "cold",
            "warm", "cool", "soft", "hard", "rough", "smooth", "shiny", "dull", "new", "old", "young", "fresh", "dry", "wet", "sweet", "sour", "bitter", "salty", "friendly", "mean",
            "kind", "rude", "polite", "naughty", "nice", "crazy", "funny", "serious", "lazy", "busy", "quiet", "noisy", "peaceful", "dangerous", "safe", "clean", "messy", "strong", "weak",
            "brave", "cowardly", "smart", "stupid", "clever", "foolish", "happy", "sad", "angry", "calm", "nervous", "excited", "shy", "bold", "friendly", "mean", "cheerful", "gloomy", "gentle",
            "harsh", "polite", "rude", "honest", "dishonest", "loyal", "unloyal", "curious", "bored", "eager", "relaxed", "scared", "confident", "shy", "brilliant", "dull", "bright", "dark", "colorful",
            "plain", "beautiful", "ugly", "handsome", "pretty", "cute", "tall", "short", "fat", "thin", "strong", "weak", "fast", "slow", "heavy", "light", "hard", "soft", "rough", "smooth",
            "hot", "cold", "warm", "cool", "wet", "dry", "clean", "dirty", "full", "empty", "young", "old", "new", "ancient", "fresh", "stale", "sweet", "sour", "bitter", "salty", "spicy",
            "healthy", "sick", "rich", "poor", "famous", "unknown", "happy", "sad", "angry", "excited", "bored", "tired", "sleepy", "hungry", "thirsty", "scared", "brave", "strong", "weak",
            "magical", "mysterious", "legendary", "heroic", "fantastic", "amazing", "awesome", "great", "wonderful", "terrible", "awful", "horrible", "fun", "boring", "interesting", "exciting", "dull", "colorful",
            "plain", "bright", "dark", "shiny", "dull", "soft", "hard", "rough", "smooth", "warm", "cold", "hot", "cool", "wet", "dry", "clean", "dirty", "fresh", "stale", "sweet", "sour",
            "bitter", "salty", "spicy", "healthy", "sick", "rich", "poor", "famous", "unknown", "happy", "sad", "angry", "excited", "bored", "tired", "sleepy", "hungry", "thirsty", "scared",
            "brave", "strong", "weak", "fast", "slow", "quiet", "loud", "friendly", "mean", "kind", "rude", "polite", "naughty", "nice", "crazy", "funny", "serious", "lazy", "busy", "quiet",
            "noisy", "peaceful", "dangerous", "safe", "clean", "messy", "strong", "weak", "brave", "cowardly", "smart", "stupid", "clever", "foolish", "happy", "sad", "angry", "calm", "nervous",
            "excited", "shy", "bold", "friendly", "mean", "cheerful", "gloomy", "gentle", "harsh", "polite", "rude", "honest", "dishonest", "loyal", "unloyal", "curious", "bored", "eager", "relaxed",
            "scared", "confident", "shy", "brilliant", "dull", "bright", "dark", "colorful", "plain", "beautiful", "ugly", "handsome", "pretty", "cute", "tall", "short", "fat", "thin", "strong",
            "weak", "fast", "slow", "heavy", "light", "hard", "soft", "rough", "smooth", "hot", "cold", "warm", "cool", "wet", "dry", "clean", "dirty", "full", "empty", "young", "old",
            "new", "ancient", "fresh", "stale", "sweet", "sour", "bitter", "salty", "spicy", "healthy", "sick", "rich", "poor", "famous", "unknown", "magical", "mysterious", "legendary", "heroic", "fantastic",
            "amazing", "awesome", "great", "wonderful", "terrible", "awful", "horrible", "fun", "boring", "interesting", "exciting", "dull", "colorful", "plain", "bright", "dark", "shiny", "dull",
            "soft", "hard", "rough", "smooth", "warm", "cold", "hot", "cool", "wet", "dry", "clean", "dirty", "fresh", "stale", "sweet", "sour", "bitter", "salty", "spicy", "healthy", "sick",
            "rich", "poor", "famous", "unknown"
        };
    }
}
