using MySql.Data.MySqlClient;
using System.IO;
using System.Media;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CyberSecurity_Bot
{
    public partial class MainWindow : Window
    {
        //this is the main window of the application, where all the UI elements and logic are handled.
        const string DB_SERVER = "localhost";
        const string DB_NAME = "bimo_bot";
        const string DB_USER = "root";
        const string DB_PASSWORD = "HARRYPOTTER@11__2@@";   

        // Returns the MySQL connection string based on the constants defined above
        private static string GetConnectionString()
        {
            return $"Server={DB_SERVER};Database={DB_NAME};User ID={DB_USER};Password={DB_PASSWORD};";
        }

        // Tests the DB connection and returns true if it works
        private bool DatabaseIsAvailable()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(GetConnectionString()))
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        // GENERAL VARIABLES

        string userName = "";
        string lastTopic = "";
        string userInterest = "";
        int emptyInputCount = 0;
        Random random = new Random();
        string activeTab = "chat";
        bool dbConnected = false;


        // ACTIVITY LOG

        /*here is where we store the activity log entries in memory.
        Each entry is a string with a timestamp and description of the action.*/
        List<string> activityLog = new List<string>();
        int logDisplayLimit = 10;

        //this allows us to add a new action to the activity log, with a timestamp, and keep only the most recent 50 entries.
        private void LogAction(string action)
        {
            string time = DateTime.Now.ToString("HH:mm");
            string entry = "[" + time + "] " + action;
            activityLog.Insert(0, entry);
            if (activityLog.Count > 50)
                activityLog.RemoveAt(activityLog.Count - 1);
        }

        //TASK CLASS

        /*this class represents a task in the application, with properties for ID, title,
        description, reminder, completion status, and creation time.*/
        private class CyberTask
        {
            public int Id { get; set; } //this is the unique identifier for the task, assigned by the database.
            public string Title { get; set; } = string.Empty; //this is the title of the task, entered by the user.
            public string Description { get; set; } = string.Empty; //this is the description of the task, entered by the user.
            public string Reminder { get; set; } = string.Empty; //this is the reminder for the task, entered by the user.
            public bool IsComplete { get; set; }//this indicates whether the task has been marked as complete by the user.
            public string CreatedAt { get; set; } = string.Empty; //this is the timestamp of when the task was created,
                                                                  //assigned by the database.
        }

        // QUIZ DATA

        //this class represents a quiz question, with properties for the question text, answer options,
        //the index of the correct answer, and an explanation for the correct answer.
        private class QuizQuestion
        {
            public string Question { get; set; } //this is the text of the quiz question.
            public List<string> Options { get; set; } //this is a list of answer options for the question,
                                                      //typically 2-4 choices.
            public int Correct { get; set; } //this is the index of the correct answer in the Options list (0-based).
            public string Explanation { get; set; } //this is a brief explanation of why the correct answer is correct,
                                                    //shown after the user answers.
        }

        //this dictionary holds all the quiz questions, with the question text as the key
        //and the QuizQuestion object as the value.

        List<QuizQuestion> allQuestions = new List<QuizQuestion>
        {
            new QuizQuestion {
                Question    = "What should you do if you receive an email asking for your password?",
                Options     = new List<string> { "A) Reply with your password", "B) Delete the email", "C) Report it as phishing", "D) Ignore it" },
                Correct     = 2,
                Explanation = "✅ Correct! Reporting phishing emails helps protect everyone. Legitimate companies never ask for passwords by email."
            },
            new QuizQuestion {
                Question    = "TRUE or FALSE: Using the same password on multiple websites is safe.",
                Options     = new List<string> { "TRUE", "FALSE" },
                Correct     = 1,
                Explanation = "✅ FALSE! If one site is hacked, attackers try your password everywhere. Always use unique passwords."
            },
            new QuizQuestion {
                Question    = "What does 2FA stand for?",
                Options     = new List<string> { "A) Two Free Accounts", "B) Two-Factor Authentication", "C) Twice-Failed Attempt", "D) Two-File Access" },
                Correct     = 1,
                Explanation = "✅ Two-Factor Authentication adds a second layer of security beyond just your password."
            },
            new QuizQuestion {
                Question    = "TRUE or FALSE: A VPN is useful for protecting your data on public Wi-Fi.",
                Options     = new List<string> { "TRUE", "FALSE" },
                Correct     = 0,
                Explanation = "✅ TRUE! A VPN encrypts your internet traffic, keeping your data safe on public networks."
            },
            new QuizQuestion {
                Question    = "Which of these is a sign of a phishing email?",
                Options     = new List<string> { "A) Personalised greeting with your name", "B) Urgent language and suspicious links", "C) Sent from a known friend", "D) Contains your account number" },
                Correct     = 1,
                Explanation = "✅ Correct! Urgency and suspicious links are classic phishing red flags."
            },
            new QuizQuestion {
                Question    = "What is the 3-2-1 backup rule?",
                Options     = new List<string> { "A) 3 devices, 2 passwords, 1 cloud", "B) 3 copies, 2 media types, 1 offsite", "C) 3 scans, 2 firewalls, 1 VPN", "D) 3 users, 2 backups, 1 restore" },
                Correct     = 1,
                Explanation = "✅ Correct! 3 copies of data, on 2 different media types, with 1 stored offsite."
            },
            new QuizQuestion {
                Question    = "TRUE or FALSE: Free VPN apps are always safe to use.",
                Options     = new List<string> { "TRUE", "FALSE" },
                Correct     = 1,
                Explanation = "✅ FALSE! Many free VPNs sell your browsing data. Choose a trusted paid VPN with a no-logs policy."
            },
            new QuizQuestion {
                Question    = "What is malware?",
                Options     = new List<string> { "A) A type of firewall", "B) A secure email server", "C) Harmful software designed to damage or steal data", "D) A backup tool" },
                Correct     = 2,
                Explanation = "✅ Correct! Malware includes viruses, ransomware, and spyware — all designed to harm your device or steal data."
            },
            new QuizQuestion {
                Question    = "TRUE or FALSE: Software updates should be delayed as long as possible.",
                Options     = new List<string> { "TRUE", "FALSE" },
                Correct     = 1,
                Explanation = "✅ FALSE! Updates patch security vulnerabilities. Delaying them leaves your system exposed to known attacks."
            },
            new QuizQuestion {
                Question    = "Which password is the STRONGEST?",
                Options     = new List<string> { "A) password123", "B) John1990", "C) Tr0ub4dor&3!", "D) qwerty" },
                Correct     = 2,
                Explanation = "✅ Correct! Tr0ub4dor&3! is long, uses mixed cases, numbers, and a symbol — very hard to crack."
            },
            new QuizQuestion {
                Question    = "What should you do if a caller claims to be from your bank and asks for your PIN?",
                Options     = new List<string> { "A) Give it to them quickly", "B) Hang up and call the bank directly", "C) Email it to them later", "D) Read it slowly" },
                Correct     = 1,
                Explanation = "✅ Correct! Banks never ask for your PIN. Hang up and call the official number on your card."
            },
            new QuizQuestion {
                Question    = "TRUE or FALSE: Social engineering attacks target technology, not people.",
                Options     = new List<string> { "TRUE", "FALSE" },
                Correct     = 1,
                Explanation = "✅ FALSE! Social engineering manipulates PEOPLE into revealing information or doing things they should not."
            }
        };

        List<QuizQuestion> currentQuizQuestions = new List<QuizQuestion>();
        int currentQuestionIndex = 0;
        int quizScore = 0;
        bool quizAnswered = false;

        // RESPONSE ARRAYS
        //these arrays hold the responses for each quiz category, providing tips and explanations.

        string[] passwordResponses =
        {
            "Use a password with at least 12 characters. Mix uppercase, lowercase, numbers and symbols. Never reuse passwords! 🔐",
            "Try a passphrase — three random words like 'BloodMoon$Castle9'. Long and easy to remember! 🛡️",
            "A password manager like Bitwarden stores all your passwords. You only remember one master password! 🔑"
        };
        string[] phishingResponses =
        {
            "Phishing is fake emails pretending to be real companies. Never click unexpected links — go to the site directly! 🎣",
            "Watch for spelling mistakes, urgency, or requests for personal info in emails. Classic phishing signs! ⚠️",
            "Always check the sender's actual email address. Scammers hide behind legitimate-looking names. 🔍"
        };
        string[] scamResponses =
        {
            "If someone asks for gift cards or Bitcoin — it is a scam! No real company asks for payment this way. 🚨",
            "Scams create urgency: 'Act now or lose your account!'. Take a breath and verify first! 🛑",
            "If a deal seems to good to be true, it probably is. Report scams to the SAPS. 🛡️"
        };
        string[] privacyResponses =
        {
            "Check your social media privacy settings. Not everyone needs to see your address or daily routine. 🔒",
            "Only give apps the permissions they need. A calculator does not need your camera! 📱",
            "Be careful what you share. Scammers use personal details to steal your identity. 🌐"
        };
        string[] malwareResponses =
        {
            "Malware steals your data or damages your device. Install trusted antivirus and keep it updated! 🦠",
            "Only download from official websites. Pirated software almost always contains hidden malware! 💀",
            "Signs: slow device, strange pop-ups, programs opening alone. Run a scan immediately! ⚡"
        };
        string[] twoFAResponses =
        {
            "2FA means password PLUS a phone code. Even if hackers steal your password, they cannot get in! 🛡️",
            "Use an authenticator app like Google Authenticator — harder to intercept than SMS! 📲",
            "Enable 2FA on email, banking, and social media. Takes a minute and blocks most attacks! 🔑"
        };
        string[] vpnResponses =
        {
            "A VPN hides your internet activity and protects you on public Wi-Fi. Use one at coffee shops! 🔒",
            "Free VPNs often sell your data. Choose a paid, trusted VPN with a no-logs policy! 💡",
            "A VPN is a great layer of protection, especially on public internet connections. 🌐"
        };
        string[] backupResponses =
        {
            "Follow the 3-2-1 rule: 3 copies, 2 different devices, 1 in the cloud. 💾",
            "Test your backups regularly. A backup you have never tested might not work! ✅",
            "If ransomware hits, a backup means you do not have to pay criminals. Keep one offline! 🛡️"
        };

        // CONSTRUCTOR

        //here is the constructor for the MainWindow class. It initializes the UI components, plays a greeting sound
        public MainWindow()
        {
            InitializeComponent();
            PlayGreeting();

            // Test the database connection when the app opens
            dbConnected = DatabaseIsAvailable();

            NameTextBox.Focus();
        }

        //if the greeting sound file is embedded in the application, it plays it. If not, it looks for a local file named
        //"greeting.wav" and plays that instead.
        private void PlayGreeting()
        {
            try
            {
                string resourceName = "CyberSecurity_Bot.greeting.wav";
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream(resourceName);

                if (stream != null)
                {
                    new SoundPlayer(stream).Play();
                    return;
                }

                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "greeting.wav");
                if (File.Exists(filePath))
                    new SoundPlayer(filePath).Play();
            }
            catch 
            { }
        }


        // NAME ENTRY

        //this event handler is triggered when the user presses a key in the NameTextBox.
        private void NameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) StartChat();
        }

        //this event handler is triggered when the user clicks the Start button. It calls the StartChat method.
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartChat();
        }

        //this method starts the chat session. It checks if the user has entered a name, formats it, and updates
        //the UI to show the chat interface. It also displays a welcome message and the database connection status.
        private void StartChat()
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Please enter your name to begin! 🦇");
                return;
            }

            string rawName = NameTextBox.Text.Trim();
            userName = char.ToUpper(rawName[0]) + rawName.Substring(1).ToLower();

            NamePanel.Visibility = Visibility.Collapsed;
            UserInput.IsEnabled = true;
            SendBtn.IsEnabled = true;

            UserBadgeText.Text = "👤 " + userName;
            UserBadge.Visibility = Visibility.Visible;

            // Shows the welcome message and DB connection status
            string dbStatus = dbConnected
                ? " MySQL database connected — tasks will be saved!"
                : " MySQL not connected — tasks saved in memory only.\n   (Check your password in the code and run the SQL setup file)";

            AddBotMessage(
                " Welcome, " + userName + "! I am BIMO — your Cybersecurity Awareness Assistant.\n\n" +
                " Chat: Ask me about cybersecurity topics\n" +
                " Tasks: Add and manage your security tasks\n" +
                " Quiz: Test your cybersecurity knowledge\n" +
                " Log: See everything BIMO has done\n\n" +
                dbStatus
            );

            LogAction("Session started — user: " + userName +
                      (dbConnected ? " | DB: connected" : " | DB: not connected"));

            UserInput.Focus();
        }

        // TAB NAVIGATION

        //this method shows the specified tab and hides the others. It also updates the tab button
        //colors to indicate which tab is active.
        private void ShowTab(string tab)
        {
            activeTab = tab;

            ChatPanel.Visibility = Visibility.Collapsed;
            TasksPanel.Visibility = Visibility.Collapsed;
            QuizPanel.Visibility = Visibility.Collapsed;
            LogPanel.Visibility = Visibility.Collapsed;

            Color inactive = Color.FromRgb(0xCC, 0x00, 0x33);
            Color active = Color.FromRgb(0xFF, 0x2D, 0x55);

            TabChat.Background = new SolidColorBrush(inactive);
            TabTasks.Background = new SolidColorBrush(inactive);
            TabQuiz.Background = new SolidColorBrush(inactive);
            TabLog.Background = new SolidColorBrush(inactive);

            if (tab == "chat")
            {
                ChatPanel.Visibility = Visibility.Visible;
                TabChat.Background = new SolidColorBrush(active);
            }
            else if (tab == "tasks")
            {
                TasksPanel.Visibility = Visibility.Visible;
                TabTasks.Background = new SolidColorBrush(active);
                RefreshTaskList();
            }
            else if (tab == "quiz")
            {
                QuizPanel.Visibility = Visibility.Visible;
                TabQuiz.Background = new SolidColorBrush(active);
            }
            else if (tab == "log")
            {
                LogPanel.Visibility = Visibility.Visible;
                TabLog.Background = new SolidColorBrush(active);
                RefreshLogPanel();
            }
        }

        //these event handlers are triggered when the user clicks on the tab buttons. They call the ShowTab method
        private void TabChat_Click(object sender, RoutedEventArgs e) => ShowTab("chat");
        private void TabTasks_Click(object sender, RoutedEventArgs e) => ShowTab("tasks");
        private void TabQuiz_Click(object sender, RoutedEventArgs e) => ShowTab("quiz");
        private void TabLog_Click(object sender, RoutedEventArgs e) => ShowTab("log");


        // CHAT INPUT

        //this event handler is triggered when the user presses a key in the UserInput TextBox.
        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) HandleUserInput();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            HandleUserInput();
        }

        private void TopicButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            string full = btn.Content.ToString();
            string keyword = full.Substring(full.IndexOf(' ') + 1).Trim();
            AddUserMessage(keyword);
            RespondToInput(keyword);
        }

        private void HandleUserInput()
        {
            string input = UserInput.Text.Trim();
            UserInput.Clear();

            if (string.IsNullOrWhiteSpace(input))
            {
                emptyInputCount++;
                if (emptyInputCount >= 3)
                {
                    AddBotMessage("🦇 I notice you are not typing anything, " + userName + ". Tap a chip or type 'help'!");
                    emptyInputCount = 0;
                }
                else
                    AddBotMessage("Please type something, " + userName + "! I am here to help. 🩸");
                return;
            }

            emptyInputCount = 0;
            AddUserMessage(input);
            RespondToInput(input);
        }


        // NLP SIMULATION
        //NLP is basically keyword recognition in this context. The following methods check if the user's input
        //contains certain keywords to determine the intent.

        //this method checks if the input string contains any of the specified keywords.
        //It returns true if a match is found.
        private bool ContainsAny(string input, params string[] keywords)
        {
            foreach (string kw in keywords)
                if (input.Contains(kw)) return true;
            return false;
        }

        private bool IsAddTaskRequest(string lower)
        {
            return ContainsAny(lower, "add task", "add a task", "new task", "create task",
                "remind me", "set a reminder", "set reminder", "can you remind", "please remind");
        }

        private bool IsViewTaskRequest(string lower)
        {
            return ContainsAny(lower, "show tasks", "view tasks", "my tasks", "list tasks",
                "what tasks", "see my tasks", "show my tasks", "what have you done");
        }

        private bool IsLogRequest(string lower)
        {
            return ContainsAny(lower, "activity log", "show log", "view log", "show activity",
                "what have you done for me", "recent actions", "history");
        }

        private bool IsQuizRequest(string lower)
        {
            return ContainsAny(lower, "quiz", "mini game", "minigame", "test me", "start quiz",
                "play quiz", "game", "questions", "cyber quiz");
        }

        // SECTION 12: MAIN CHAT RESPONSE LOGIC

        //this method processes the user's input, checks for keywords, and generates appropriate responses.
        private void RespondToInput(string input)
        {
            string lower = input.ToLower().Trim();

            if (input.Length > 300)
            {
                AddBotMessage("That message is very long, " + userName + "! Please keep it shorter. 🦇");
                return;
            }

            if (ContainsAny(lower, "exit", "quit", "bye", "goodbye"))
            {
                AddBotMessage("Farewell, " + userName + "! Stay safe online. 🦇🩸");
                LogAction("Session ended by " + userName);
                UserInput.IsEnabled = false;
                SendBtn.IsEnabled = false;
                return;
            }

            if (lower == "help" || lower == "commands")
            {
                AddBotMessage(
                    " BIMO COMMANDS:\n\n" +
                    " password · phishing · scam · privacy\n" +
                    " malware · 2fa · vpn · backup\n\n" +
                    " Tasks:  'add task'  ·  'show tasks'\n" +
                    " Quiz:   'start quiz'\n" +
                    " Log:    'show activity log'\n\n" +
                    " Also:   'how are you' · 'tell me more'\n" +
                    "           'I am interested in [topic]'\n\n" +
                    "🦇 Type 'bye' to exit"
                );
                LogAction("Help menu displayed");
                return;
            }

            if (IsAddTaskRequest(lower))
            {
                AddBotMessage("Sure! Switching to the Tasks tab so you can add your task. 📋\n\nFill in the title, description, and an optional reminder!");
                LogAction("NLP: add task request from " + userName);
                ShowTab("tasks");
                return;
            }

            if (IsViewTaskRequest(lower))
            {
                AddBotMessage("Switching to the Tasks tab to show your tasks, " + userName + "! 📋");
                LogAction("NLP: view tasks request from " + userName);
                ShowTab("tasks");
                return;
            }

            if (IsLogRequest(lower))
            {
                if (activityLog.Count == 0)
                {
                    AddBotMessage("No actions have been logged yet, " + userName + ". Start chatting, adding tasks, or taking the quiz! 🦇");
                }
                else
                {
                    string summary = "📜 Here is a summary of recent actions:\n\n";
                    int count = Math.Min(5, activityLog.Count);
                    for (int i = 0; i < count; i++)
                        summary += (i + 1) + ". " + activityLog[i] + "\n";
                    if (activityLog.Count > 5)
                        summary += "\n...and more. Switch to the Log tab to see all!";
                    AddBotMessage(summary);
                }
                LogAction("Activity log viewed via chat");
                return;
            }

            if (IsQuizRequest(lower))
            {
                AddBotMessage("Let's test your cybersecurity knowledge! 🎮\nSwitching to the Quiz tab...");
                LogAction("NLP: quiz request from " + userName);
                ShowTab("quiz");
                return;
            }

            if (ContainsAny(lower, "how are you", "how r u"))
            {
                string[] r = { "I am doing great, " + userName + "! Ready to guard you. 🦇", "Excellent! All cybersecurity fangs are sharp! 🩸", "Feeling ready, " + userName + "! Let's keep you safe. 🦇" };
                AddBotMessage(r[random.Next(r.Length)]);
                return;
            }

            if (ContainsAny(lower, "your purpose", "what are you", "who are you"))
            {
                AddBotMessage("I am BIMO 🦇 — your Cybersecurity Awareness Assistant! Passwords, phishing, tasks, quiz, and more. Your digital bodyguard! 🩸");
                return;
            }

            if (ContainsAny(lower, "hello", "hi", "hey", "greetings"))
            {
                string[] g = { "Greetings, " + userName + "! Let's keep you safe. 🦇", "Hello, " + userName + "! How can I protect you today? 🩸", "Hey " + userName + "! Cyber threats won't get you on my watch! 🛡️" };
                AddBotMessage(g[random.Next(g.Length)]);
                return;
            }

            if (ContainsAny(lower, "thank you", "thanks", "thx"))
            {
                AddBotMessage("You are very welcome, " + userName + "! Stay safe and vigilant. 🦇");
                return;
            }

            if (ContainsAny(lower, "i am interested in", "i'm interested in"))
            {
                int idx = lower.IndexOf("interested in");
                string topic = input.Substring(idx + 13).Trim().TrimEnd('.', '!', '?');
                userInterest = topic;
                AddBotMessage("🩸 Noted! I will remember you are interested in " + topic + ", " + userName + ".");
                LogAction("Memory stored: " + userName + " interested in " + topic);
                return;
            }

            if (ContainsAny(lower, "what do you remember", "my interest"))
            {
                AddBotMessage(userInterest != ""
                    ? " I remember you are interested in: " + userInterest + ", " + userName + "!"
                    : "I know your name is " + userName + "! You have not told me your interests yet. 🦇");
                return;
            }

            if (ContainsAny(lower, "tell me more", "another tip", "more details", "what else"))
            {
                if (lastTopic == "")
                    AddBotMessage("Ask about a topic first — like 'password' or 'phishing'. 🦇");
                else
                    AddBotMessage("🩸 Here is another tip about " + lastTopic + ":\n\n" + GetRandomResponse(lastTopic));
                return;
            }

            if (ContainsAny(lower, "worried", "scared", "anxious", "afraid", "unsafe", "nervous"))
            {
                AddBotMessage("It is okay to feel worried, " + userName + ". 🦇\nKnowledge is your best defence! Ask me about phishing, scam, or password. 🩸");
                return;
            }

            if (ContainsAny(lower, "frustrated", "confused", "don't understand", "difficult", "overwhelming"))
            {
                AddBotMessage("I hear you, " + userName + "! Let's slow down. 🦇\nTap one of the topic chips and I will explain simply. 🩸");
                return;
            }

            // this section checks for specific cybersecurity topics and provides tips accordingly.
            // It also logs the action for tracking.
            if (ContainsAny(lower, "password", "passphrase"))
            {
                lastTopic = "password";
                AddBotMessage(CheckSentiment(lower) + GetRandomResponse("password"));
                AddBotMessage(" Type 'tell me more' for another password tip!");
                LogAction("Tip given: password");
                return;
            }
            if (ContainsAny(lower, "phishing", "fake email", "suspicious email", "email scam"))
            {
                lastTopic = "phishing";
                AddBotMessage(CheckSentiment(lower) + GetRandomResponse("phishing"));
                AddBotMessage(" Type 'tell me more' for another phishing tip!");
                LogAction("Tip given: phishing");
                return;
            }
            if (ContainsAny(lower, "scam", "fraud", "con trick"))
            {
                lastTopic = "scam";
                AddBotMessage(CheckSentiment(lower) + GetRandomResponse("scam"));
                AddBotMessage(" Type 'tell me more' for another scam tip!");
                LogAction("Tip given: scam");
                return;
            }
            if (ContainsAny(lower, "privacy", "personal data", "personal information"))
            {
                lastTopic = "privacy";
                AddBotMessage(CheckSentiment(lower) + GetRandomResponse("privacy"));
                AddBotMessage(" Type 'tell me more' for another privacy tip!");
                LogAction("Tip given: privacy");
                return;
            }
            if (ContainsAny(lower, "malware", "virus", "antivirus", "spyware", "ransomware"))
            {
                lastTopic = "malware";
                AddBotMessage(CheckSentiment(lower) + GetRandomResponse("malware"));
                AddBotMessage(" Type 'tell me more' for another malware tip!");
                LogAction("Tip given: malware");
                return;
            }
            if (ContainsAny(lower, "2fa", "two factor", "two-factor", "mfa", "authentication"))
            {
                lastTopic = "2fa";
                AddBotMessage(CheckSentiment(lower) + GetRandomResponse("2fa"));
                AddBotMessage(" Type 'tell me more' for another 2FA tip!");
                LogAction("Tip given: 2FA");
                return;
            }
            if (ContainsAny(lower, "vpn", "public wifi", "public wi-fi", "virtual private"))
            {
                lastTopic = "vpn";
                AddBotMessage(CheckSentiment(lower) + GetRandomResponse("vpn"));
                AddBotMessage(" Type 'tell me more' for another VPN tip!");
                LogAction("Tip given: VPN");
                return;
            }
            if (ContainsAny(lower, "backup", "back up", "back-up", "cloud storage"))
            {
                lastTopic = "backup";
                AddBotMessage(CheckSentiment(lower) + GetRandomResponse("backup"));
                AddBotMessage(" Type 'tell me more' for another backup tip!");
                LogAction("Tip given: backup");
                return;
            }

            // Default fallback
            string[] defaults = {
                "I did not catch that, " + userName + ". 🦇 Try 'password', 'phishing', or 'help'.",
                "That is outside my expertise, " + userName + ". Ask about cybersecurity topics! 🩸",
                "Not sure about that one, " + userName + ". Tap a chip or type 'help'. 🦇"
            };
            AddBotMessage(defaults[random.Next(defaults.Length)]);
        }

        // TASK ASSISTANT WITH MYSQL

        //this event handler is triggered when the user clicks the Add Task button. It collects the task details,
        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            string title = TaskTitleBox.Text.Trim();
            string desc = TaskDescBox.Text.Trim();
            string reminder = TaskReminderBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Please enter a task title! 🦇");
                return;
            }

            if (string.IsNullOrWhiteSpace(desc)) desc = "No description provided.";
            if (string.IsNullOrWhiteSpace(reminder)) reminder = "No reminder set";

            if (dbConnected)
            {
                // Saves to MySQL database
                SaveTaskToDatabase(title, desc, reminder);
            }
            else
            {
                // No DB — show a message but won't do anything 
                MessageBox.Show(
                    "⚠ MySQL is not connected.\n\n" +
                    "To save tasks:\n" +
                    "1. Run bimo_database_setup.sql in MySQL Workbench\n" +
                    "2. Update DB_PASSWORD in the code\n" +
                    "3. Restart the application",
                    "Database Not Connected");
            }

            // Clear the form
            TaskTitleBox.Text = "";
            TaskDescBox.Text = "";
            TaskReminderBox.Text = "";

            // Refresh the list from the database
            RefreshTaskList();
        }

        // Saves a new task to the MySQL database
        private void SaveTaskToDatabase(string title, string description, string reminder)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(GetConnectionString()))
                {
                    conn.Open();

                    // SQL INSERT statement — adds the task to the tasks table
                    string sql = "INSERT INTO tasks (title, description, reminder, is_complete) " +
                                 "VALUES (@title, @description, @reminder, 0)";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        // Use parameters to prevent SQL injection
                        cmd.Parameters.AddWithValue("@title", title);
                        cmd.Parameters.AddWithValue("@description", description);
                        cmd.Parameters.AddWithValue("@reminder", reminder);
                        cmd.ExecuteNonQuery();
                    }
                }

                LogAction("Task saved to DB: '" + title + "'" +
                          (reminder != "No reminder set" ? " (Reminder: " + reminder + ")" : ""));

                MessageBox.Show(" Task saved to database!\n\nTitle: " + title + "\nReminder: " + reminder);
            }
            catch (Exception ex)
            {
                MessageBox.Show(" Could not save to database.\n\nError: " + ex.Message +
                                "\n\nCheck your MySQL connection settings.");
                LogAction("DB error saving task: " + ex.Message);
            }
        }

        // Loads all tasks from MySQL and displays them
        private void RefreshTaskList()
        {
            TaskListPanel.Children.Clear();

            // Update the DB status bar
            DbStatusText.Text = dbConnected
                ? " Connected to MySQL database: bimo_bot"
                : " Not connected to MySQL — run setup SQL and check password";

            if (!dbConnected)
            {
                TextBlock noDb = new TextBlock();
                noDb.Text = " MySQL not connected.\n\nTo set up the database:\n" +
                                       "1. Open MySQL Workbench\n" +
                                       "2. Run bimo_database_setup.sql\n" +
                                       "3. Update DB_PASSWORD in the code\n" +
                                       "4. Restart the app";
                noDb.Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x00, 0x22));
                noDb.FontSize = 12;
                noDb.FontFamily = new FontFamily("Segoe UI");
                noDb.TextWrapping = TextWrapping.Wrap;
                noDb.Margin = new Thickness(10, 20, 10, 0);
                TaskListPanel.Children.Add(noDb);
                return;
            }

            // Load tasks from database
            List<CyberTask> loadedTasks = LoadTasksFromDatabase();

            if (loadedTasks.Count == 0)
            {
                TextBlock empty = new TextBlock();
                empty.Text = "No tasks yet! Add your first cybersecurity task above. 🦇";
                empty.Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x00, 0x22));
                empty.FontSize = 13;
                empty.FontFamily = new FontFamily("Segoe UI");
                empty.Margin = new Thickness(0, 20, 0, 0);
                empty.HorizontalAlignment = HorizontalAlignment.Center;
                TaskListPanel.Children.Add(empty);
                return;
            }

            // Display each task as a card
            foreach (CyberTask task in loadedTasks)
            {
                Border card = new Border();
                card.Background = task.IsComplete
                                       ? new SolidColorBrush(Color.FromRgb(0xFF, 0xE0, 0xE8))
                                       : Brushes.White;
                card.BorderBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x8A));
                card.BorderThickness = new Thickness(1.5);
                card.CornerRadius = new CornerRadius(10);
                card.Padding = new Thickness(12, 10, 12, 10);
                card.Margin = new Thickness(0, 0, 0, 8);

                StackPanel inner = new StackPanel();

                // Title
                TextBlock titleLabel = new TextBlock();
                titleLabel.Text = (task.IsComplete ? "✅ " : "📌 ") + task.Title;
                titleLabel.Foreground = new SolidColorBrush(Color.FromRgb(0xCC, 0x00, 0x33));
                titleLabel.FontSize = 13;
                titleLabel.FontWeight = FontWeights.Bold;
                titleLabel.FontFamily = new FontFamily("Segoe UI");
                inner.Children.Add(titleLabel);

                // Description
                TextBlock descLabel = new TextBlock();
                descLabel.Text = task.Description;
                descLabel.Foreground = new SolidColorBrush(Color.FromRgb(0x44, 0x00, 0x11));
                descLabel.FontSize = 12;
                descLabel.FontFamily = new FontFamily("Segoe UI");
                descLabel.TextWrapping = TextWrapping.Wrap;
                descLabel.Margin = new Thickness(0, 4, 0, 4);
                inner.Children.Add(descLabel);

                // Reminder
                TextBlock remLabel = new TextBlock();
                remLabel.Text = " Reminder: " + task.Reminder;
                remLabel.Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0x2D, 0x55));
                remLabel.FontSize = 11;
                remLabel.FontFamily = new FontFamily("Segoe UI");
                inner.Children.Add(remLabel);

                // Created date
                TextBlock dateLabel = new TextBlock();
                dateLabel.Text = " Added: " + task.CreatedAt;
                dateLabel.Foreground = new SolidColorBrush(Color.FromRgb(0xAA, 0x44, 0x55));
                dateLabel.FontSize = 10;
                dateLabel.FontFamily = new FontFamily("Segoe UI");
                dateLabel.Margin = new Thickness(0, 2, 0, 6);
                inner.Children.Add(dateLabel);

                // Action buttons
                StackPanel buttons = new StackPanel();
                buttons.Orientation = Orientation.Horizontal;
                buttons.Margin = new Thickness(0, 6, 0, 0);

                if (!task.IsComplete)
                {
                    Button completeBtn = MakeActionButton(" Complete", "#28a745", task.Id, CompleteTask_Click);
                    buttons.Children.Add(completeBtn);
                }

                Button deleteBtn = MakeActionButton(" Delete", "#ff2d55", task.Id, DeleteTask_Click);
                buttons.Children.Add(deleteBtn);

                inner.Children.Add(buttons);
                card.Child = inner;
                TaskListPanel.Children.Add(card);
            }
        }

        // Loads tasks from the MySQL database and returns a list
        private List<CyberTask> LoadTasksFromDatabase()
        {
            List<CyberTask> result = new List<CyberTask>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(GetConnectionString()))
                {
                    conn.Open();

                    // SELECT all tasks ordered by newest first
                    string sql = "SELECT id, title, description, reminder, is_complete, " +
                                 "DATE_FORMAT(created_at, '%d %b %Y %H:%i') AS created_at " +
                                 "FROM tasks ORDER BY created_at DESC";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CyberTask task = new CyberTask
                            {
                                Id = reader.GetInt32("id"),
                                Title = reader.GetString("title"),
                                Description = reader.GetString("description"),
                                Reminder = reader.GetString("reminder"),
                                IsComplete = reader.GetBoolean("is_complete"),
                                CreatedAt = reader.GetString("created_at")
                            };
                            result.Add(task);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(" Could not load tasks from database.\n\nError: " + ex.Message);
                LogAction("DB error loading tasks: " + ex.Message);
            }

            return result;
        }

        // Creates a styled action button for task cards
        private Button MakeActionButton(string label, string hexColour, int taskId, RoutedEventHandler handler)
        {
            byte r = Convert.ToByte(hexColour.Substring(1, 2), 16);
            byte g = Convert.ToByte(hexColour.Substring(3, 2), 16);
            byte b = Convert.ToByte(hexColour.Substring(5, 2), 16);

            Button btn = new Button();
            btn.Content = label;
            btn.Tag = taskId;
            btn.Margin = new Thickness(0, 0, 8, 0);
            btn.Cursor = Cursors.Hand;
            btn.Click += handler;
            btn.Background = new SolidColorBrush(Color.FromRgb(r, g, b));
            btn.Foreground = Brushes.White;
            btn.BorderThickness = new Thickness(0);
            btn.Padding = new Thickness(10, 4, 10, 4);
            btn.FontSize = 11;
            btn.FontWeight = FontWeights.Bold;
            btn.FontFamily = new FontFamily("Segoe UI");
            return btn;
        }

        // Marks a task as complete in the database
        private void CompleteTask_Click(object sender, RoutedEventArgs e)
        {
            int taskId = (int)((Button)sender).Tag;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(GetConnectionString()))
                {
                    conn.Open();

                    // UPDATE the is_complete column to 1 
                    string sql = "UPDATE tasks SET is_complete = 1 WHERE id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", taskId);
                        cmd.ExecuteNonQuery();
                    }
                }

                LogAction("Task #" + taskId + " marked as complete in DB");
            }
            catch (Exception ex)
            {
                MessageBox.Show(" Could not update task.\n\nError: " + ex.Message);
            }

            RefreshTaskList();
        }

        // Deletes a task from the database permanently
        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            int result = MessageBox.Show(
                "Are you sure you want to delete this task?\nThis cannot be undone.",
                "Delete Task", MessageBoxButton.YesNo) == MessageBoxResult.Yes ? 1 : 0;

            if (result != 1) return;

            int taskId = (int)((Button)sender).Tag;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(GetConnectionString()))
                {
                    conn.Open();

                    // DELETE the row from the tasks table
                    string sql = "DELETE FROM tasks WHERE id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", taskId);
                        cmd.ExecuteNonQuery();
                    }
                }

                LogAction("Task #" + taskId + " deleted from DB");
            }
            catch (Exception ex)
            {
                MessageBox.Show(" Could not delete task.\n\nError: " + ex.Message);
            }

            RefreshTaskList();
        }

        // QUIZ

        //this event handler is triggered when the user clicks the Start Quiz button.
        //It initializes the quiz state, shuffles the questions, and displays the first question.
        private void StartQuiz_Click(object sender, RoutedEventArgs e)
        {
            currentQuizQuestions = ShuffleQuestions(allQuestions);
            currentQuestionIndex = 0;
            quizScore = 0;
            quizAnswered = false;

            QuizStartPanel.Visibility = Visibility.Collapsed;
            QuizResultsPanel.Visibility = Visibility.Collapsed;
            QuizQuestionPanel.Visibility = Visibility.Visible;

            LogAction("Quiz started by " + userName);
            ShowQuestion();
        }

        //this method shuffles the list of quiz questions using the Fisher-Yates shuffle algorithm.
        private List<QuizQuestion> ShuffleQuestions(List<QuizQuestion> source)
        {
            List<QuizQuestion> copy = new List<QuizQuestion>(source);
            for (int i = copy.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                QuizQuestion temp = copy[i];
                copy[i] = copy[j];
                copy[j] = temp;
            }
            return copy;
        }

        //this method displays the current quiz question and its answer options.
        //It also updates the progress and score text.
        private void ShowQuestion()
        {
            quizAnswered = false;
            FeedbackBorder.Visibility = Visibility.Collapsed;
            NextQuestionBtn.Visibility = Visibility.Collapsed;

            QuizQuestion q = currentQuizQuestions[currentQuestionIndex];

            QuizProgressText.Text = "Question " + (currentQuestionIndex + 1) + " of " + currentQuizQuestions.Count;
            QuizScoreText.Text = "Score: " + quizScore;
            QuestionText.Text = q.Question;

            AnswerButtonsPanel.Children.Clear();

            for (int i = 0; i < q.Options.Count; i++)
            {
                Button btn = new Button();
                btn.Content = q.Options[i];
                btn.Tag = i;
                btn.Margin = new Thickness(0, 0, 0, 8);
                btn.Cursor = Cursors.Hand;
                btn.Click += Answer_Click;
                btn.Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xB3, 0xC1));
                btn.Foreground = new SolidColorBrush(Color.FromRgb(0x44, 0x00, 0x11));
                btn.BorderBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0x2D, 0x55));
                btn.BorderThickness = new Thickness(1);
                btn.Padding = new Thickness(12, 8, 12, 8);
                btn.FontSize = 13;
                btn.FontFamily = new FontFamily("Segoe UI");
                btn.HorizontalContentAlignment = HorizontalAlignment.Left;
                AnswerButtonsPanel.Children.Add(btn);
            }
        }

        //this event handler is triggered when the user clicks an answer button.
        private void Answer_Click(object sender, RoutedEventArgs e)
        {
            if (quizAnswered) return;
            quizAnswered = true;

            Button clicked = (Button)sender;
            int chosen = (int)clicked.Tag;
            QuizQuestion q = currentQuizQuestions[currentQuestionIndex];
            bool isCorrect = (chosen == q.Correct);

            if (isCorrect) quizScore++;

            foreach (Button btn in AnswerButtonsPanel.Children)
            {
                int idx = (int)btn.Tag;
                if (idx == q.Correct)
                    btn.Background = new SolidColorBrush(Color.FromRgb(0x28, 0xA7, 0x45));
                else if (idx == chosen && !isCorrect)
                    btn.Background = new SolidColorBrush(Color.FromRgb(0xFF, 0x2D, 0x55));
            }

            FeedbackText.Text = isCorrect ? q.Explanation : "❌ Not quite!\n\n" + q.Explanation;
            FeedbackBorder.Visibility = Visibility.Visible;
            NextQuestionBtn.Visibility = Visibility.Visible;
            QuizScoreText.Text = "Score: " + quizScore;

            LogAction("Quiz Q" + (currentQuestionIndex + 1) + " — " + (isCorrect ? "correct" : "incorrect"));
        }

        //this event handler is triggered when the user clicks the Next Question button.
        private void NextQuestion_Click(object sender, RoutedEventArgs e)
        {
            currentQuestionIndex++;
            if (currentQuestionIndex >= currentQuizQuestions.Count)
                ShowResults();
            else
                ShowQuestion();
        }

        private void ShowResults()
        {
            QuizQuestionPanel.Visibility = Visibility.Collapsed;
            QuizResultsPanel.Visibility = Visibility.Visible;

            int total = currentQuizQuestions.Count;
            int pct = (quizScore * 100) / total;

            ResultsScore.Text = "You scored " + quizScore + " out of " + total + " (" + pct + "%)";

            if (pct >= 80)
            {
                ResultsEmoji.Text = "🏆";
                ResultsFeedback.Text = "Great job! You are a cybersecurity pro! 🦇";
            }
            else if (pct >= 50)
            {
                ResultsEmoji.Text = "👍";
                ResultsFeedback.Text = "Good effort! Keep learning to stay safe online! 🩸";
            }
            else
            {
                ResultsEmoji.Text = "📚";
                ResultsFeedback.Text = "Keep learning! Chat with BIMO to improve. 🦇";
            }

            QuizProgressText.Text = "Quiz complete!";
            LogAction("Quiz completed by " + userName + " — score: " + quizScore + "/" + total + " (" + pct + "%)");
        }

        // ACTIVITY LOG PANEL

        //this method refreshes the activity log panel, displaying the most recent actions and providing a
        //"Show more" button if there are additional entries.
        private void RefreshLogPanel()
        {
            LogListPanel.Children.Clear();

            if (activityLog.Count == 0)
            {
                TextBlock empty = new TextBlock();
                empty.Text = "No actions logged yet. Start chatting, adding tasks, or taking the quiz! 🦇";
                empty.Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x00, 0x22));
                empty.FontSize = 13;
                empty.FontFamily = new FontFamily("Segoe UI");
                empty.TextWrapping = TextWrapping.Wrap;
                empty.Margin = new Thickness(0, 20, 0, 0);
                empty.HorizontalAlignment = HorizontalAlignment.Center;
                LogListPanel.Children.Add(empty);
                LogCountText.Text = "No actions yet";
                return;
            }

            //this section determines how many log entries to display based on the logDisplayLimit and the total number of entries.
            int showCount = Math.Min(logDisplayLimit, activityLog.Count);

            //the for loop iterates through the log entries to display them in the panel.
            for (int i = 0; i < showCount; i++)
            {
                Border entry = new Border();
                entry.Background = (i % 2 == 0) ? Brushes.White : new SolidColorBrush(Color.FromRgb(0xFF, 0xF0, 0xF3));
                entry.BorderBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xB3, 0xC1));
                entry.BorderThickness = new Thickness(0, 0, 0, 1);
                entry.Padding = new Thickness(10, 8, 10, 8);

                TextBlock text = new TextBlock();
                text.Text = (i + 1) + ". " + activityLog[i];
                text.Foreground = new SolidColorBrush(Color.FromRgb(0x44, 0x00, 0x11));
                text.FontSize = 12;
                text.FontFamily = new FontFamily("Segoe UI");
                text.TextWrapping = TextWrapping.Wrap;

                entry.Child = text;
                LogListPanel.Children.Add(entry);
            }

            //this section adds a "Show more" button if there are more log entries than the current display limit.
            if (activityLog.Count > logDisplayLimit)
            {
                Button showMore = new Button();
                showMore.Content = "Show more (" + (activityLog.Count - logDisplayLimit) + " more actions)";
                showMore.Background = new SolidColorBrush(Color.FromRgb(0xFF, 0x2D, 0x55));
                showMore.Foreground = Brushes.White;
                showMore.BorderThickness = new Thickness(0);
                showMore.Padding = new Thickness(12, 8, 12, 8);
                showMore.FontSize = 12;
                showMore.FontFamily = new FontFamily("Segoe UI");
                showMore.Margin = new Thickness(0, 8, 0, 0);
                showMore.Cursor = Cursors.Hand;
                showMore.Click += ShowMoreLog_Click;
                LogListPanel.Children.Add(showMore);
            }

            LogCountText.Text = "Showing " + showCount + " of " + activityLog.Count + " actions";
        }

        private void ShowMoreLog_Click(object sender, RoutedEventArgs e)
        {
            logDisplayLimit += 10;
            RefreshLogPanel();
        }

        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Clear all activity log entries?", "Clear Log", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                activityLog.Clear();
                logDisplayLimit = 10;
                RefreshLogPanel();
            }
        }

        // HELPER METHODS

        //this method returns a random response string based on the specified cybersecurity topic.
        private string GetRandomResponse(string topic)
        {
            if (topic == "password") return passwordResponses[random.Next(passwordResponses.Length)];
            if (topic == "phishing") return phishingResponses[random.Next(phishingResponses.Length)];
            if (topic == "scam") return scamResponses[random.Next(scamResponses.Length)];
            if (topic == "privacy") return privacyResponses[random.Next(privacyResponses.Length)];
            if (topic == "malware") return malwareResponses[random.Next(malwareResponses.Length)];
            if (topic == "2fa") return twoFAResponses[random.Next(twoFAResponses.Length)];
            if (topic == "vpn") return vpnResponses[random.Next(vpnResponses.Length)];
            if (topic == "backup") return backupResponses[random.Next(backupResponses.Length)];
            return "I do not have tips on that topic yet!";
        }

        //the method checks the user's input for certain keywords to determine their
        //sentiment and returns an appropriate response prefix.
        private string CheckSentiment(string lower)
        {
            if (ContainsAny(lower, "worried", "scared", "afraid", "nervous"))
                return "It sounds like you are worried, " + userName + ". Here is what you need to know:\n\n";
            if (ContainsAny(lower, "curious", "interested", "want to know", "wondering"))
                return "Great question, " + userName + "! Here is what you should know:\n\n";
            if (ContainsAny(lower, "frustrated", "confused", "don't understand"))
                return "Let me explain this simply, " + userName + ":\n\n";
            return "";
        }


        // ADD CHAT MESSAGES TO SCREEN

        //this method adds a user message to the chat panel, styling it as a right-aligned bubble with a specific color.
        private void AddUserMessage(string text)
        {
            StackPanel row = new StackPanel();
            row.Orientation = Orientation.Horizontal;
            row.HorizontalAlignment = HorizontalAlignment.Right;
            row.Margin = new Thickness(60, 4, 8, 4);

            Border bubble = new Border();
            bubble.Background = new SolidColorBrush(Color.FromRgb(0xFF, 0x2D, 0x55));
            bubble.BorderThickness = new Thickness(0);
            bubble.CornerRadius = new CornerRadius(16, 16, 2, 16);
            bubble.Padding = new Thickness(12, 8, 12, 8);
            bubble.MaxWidth = 280;

            TextBlock tb = new TextBlock();
            tb.Text = text;
            tb.Foreground = Brushes.White;
            tb.FontSize = 13;
            tb.FontFamily = new FontFamily("Segoe UI");
            tb.TextWrapping = TextWrapping.Wrap;

            bubble.Child = tb;
            row.Children.Add(bubble);
            MessagesPanel.Children.Add(row);
            ChatScroller.ScrollToBottom();
        }

        //this method adds a bot message to the chat panel, styling it as a left-aligned bubble with an avatar and
        //specific colors.
        private void AddBotMessage(string text)
        {
            StackPanel row = new StackPanel();
            row.Orientation = Orientation.Horizontal;
            row.HorizontalAlignment = HorizontalAlignment.Left;
            row.Margin = new Thickness(8, 4, 60, 4);
            row.VerticalAlignment = VerticalAlignment.Top;

            Border avatar = new Border();
            avatar.Width = 32;
            avatar.Height = 32;
            avatar.CornerRadius = new CornerRadius(16);
            avatar.Background = new SolidColorBrush(Color.FromRgb(0xFF, 0x2D, 0x55));
            avatar.BorderBrush = new SolidColorBrush(Color.FromRgb(0xCC, 0x00, 0x33));
            avatar.BorderThickness = new Thickness(2);
            avatar.VerticalAlignment = VerticalAlignment.Bottom;
            avatar.Margin = new Thickness(0, 0, 6, 0);
            TextBlock bat = new TextBlock();
            bat.Text = "🦇";
            bat.FontSize = 16;
            bat.HorizontalAlignment = HorizontalAlignment.Center;
            bat.VerticalAlignment = VerticalAlignment.Center;
            avatar.Child = bat;

            Border bubble = new Border();
            bubble.Background = Brushes.White;
            bubble.BorderBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x8A));
            bubble.BorderThickness = new Thickness(1.5);
            bubble.CornerRadius = new CornerRadius(2, 16, 16, 16);
            bubble.Padding = new Thickness(12, 8, 12, 8);
            bubble.MaxWidth = 280;

            StackPanel inner = new StackPanel();

            TextBlock name = new TextBlock();
            name.Text = "🦇 BIMO";
            name.Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0x2D, 0x55));
            name.FontSize = 11;
            name.FontWeight = FontWeights.Bold;
            name.FontFamily = new FontFamily("Segoe UI");
            name.Margin = new Thickness(0, 0, 0, 4);

            TextBlock msg = new TextBlock();
            msg.Text = text;
            msg.Foreground = new SolidColorBrush(Color.FromRgb(0x44, 0x00, 0x11));
            msg.FontSize = 13;
            msg.FontFamily = new FontFamily("Segoe UI");
            msg.TextWrapping = TextWrapping.Wrap;

            inner.Children.Add(name);
            inner.Children.Add(msg);
            bubble.Child = inner;

            row.Children.Add(avatar);
            row.Children.Add(bubble);
            MessagesPanel.Children.Add(row);
            ChatScroller.ScrollToBottom();
        }
    } 
} 