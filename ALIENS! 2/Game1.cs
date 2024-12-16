using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ALIENS__2
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Rectangle _screen = new Rectangle(0, 0, 640, 580);

        private gameScreen _gameScreen = gameScreen.title, _targetScreen;

        private KeyboardState _currKey, _oldKey;
        private MouseState _currMouse, _oldMouse;

        private static readonly Random RNG = new Random();

        readonly string HighScoresFilename = "highscores.lst";
        HighscoreManager scoreManager;


        // < Menu Objects > ------------------------------------------------------------------
        StaticGraphic Background, Moon, CreditProfiles;
        StaticAnimated2D LeftMousePrompt, RightMousePrompt, WKeyPrompt, SpacebarPrompt, LeftArrowPrompt, RightArrowPrompt;
        StaticAnimated2D MovementExample, ShootingExample, ExplosionExample;
        movingPromptKeys UpArrowPrompt, DownArrowPrompt;
        Texture2D _overlay;


        // < Main Gameplay Objects > ------------------------------------------------------------------
        MovingAnimated2D Planet;
        PlayerShip Player;
        List<LazerBeam> Lazer;
        List<Alien> Aliens;
        List<Human> Humans;


        // < Misc Game Objects > ------------------------------------------------------------------
        Transition transition;
        InvasionBar InvasionBar;


        // < Fonts > ------------------------------------------------------------------
        SpriteFont _titleFont, _bodyFont, _secondaryBodyFont;


        // < Audio > ------------------------------------------------------------------
        Song _menuMusic, _gameMusic;
        SoundEffect _lazerSFX, _interactSFX, _alienDeathSFX, _warningSFX, _thrusterSFX;
        SoundEffectInstance _alienDeathInst, _thrusterInst;


        // < Core Functions > ------------------------------------------------------------------
        private float _gameRunTime;
        private bool _firstTimeOpening = true, _transitioning;


        // < Main Gameplay Functions > ------------------------------------------------------------------
        private float _worldRotation, _shotCharge, _shotCooldown, _alienSpawnCounter = 3;
        private int _invasionMeter, _maxAlienSpawnCounter = 10, _numberOfAliens;
        private bool _weapeonFiring;


        // < Misc Gameplay Functions > ------------------------------------------------------------------
        private int _score, _initalSelect;
        private bool _gamePaused, _gameOver, _sentNewTableEntry;
        private string _playerNameInput = "AAA";
        private int[] playerInitals = new int[3] { 1, 1, 1 };
        private const int NUMBEROFPEOPLE = 15;


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = _screen.Width;
            _graphics.PreferredBackBufferHeight = _screen.Height;
            _graphics.ApplyChanges();

            Lazer = new List<LazerBeam>();
            Aliens = new List<Alien>();
            Humans = new List<Human>();

            scoreManager = new HighscoreManager(HighScoresFilename, 10);


            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _overlay = Content.Load<Texture2D>("overlayPixel");

            // < Basic Objects > ------------------------------------------------------------------
            Background = new StaticGraphic(
                new Rectangle(0, 0, _screen.Width, _screen.Height),
                Content.Load<Texture2D>("Sprites/Background"));

            Moon = new StaticGraphic(
                new Rectangle(_screen.Width / 6, _screen.Height / 6, 50, 50),
                Content.Load<Texture2D>("Sprites/Moon"));

            CreditProfiles = new StaticGraphic(
                new Rectangle(50, 50, 80, 480),
                Content.Load<Texture2D>("Sprites/Misc/CreditProfiles/Credits"));



            // < Advanced Objects > ------------------------------------------------------------------
            InvasionBar = new InvasionBar(
                new Rectangle((_screen.Width / 2) - 12, 40, 168, 28),
                Content.Load<Texture2D>("Sprites/InvasionBar"));
            Planet = new MovingAnimated2D(
                Content.Load<Texture2D>("Sprites/Planet"),
                0.5f,
                new Rectangle(0, 0, 500, 500),
                new Rectangle(
                    _screen.Width / 2,
                    _screen.Height + 300,
                    500,
                    500),
                1.7f);


            // < UI Button Prompts > ------------------------------------------------------------------
            // ------- < Basic Prompts > ------------------------------------------------------------------
            RightMousePrompt = new StaticAnimated2D(Content.Load<Texture2D>("Sprites/Misc/Button Prompts/RightMousePrompt"), 1, new Rectangle(0, 0, 16, 16), new Rectangle(120, 100, 16, 16),2);
            LeftMousePrompt = new StaticAnimated2D(Content.Load<Texture2D>("Sprites/Misc/Button Prompts/LeftMousePrompt"), 1, new Rectangle(0, 0, 16, 16), new Rectangle(225, 235, 16, 16), 2);
            WKeyPrompt = new StaticAnimated2D(Content.Load<Texture2D>("Sprites/Misc/Button Prompts/WKeyPrompt"), 1, new Rectangle(0, 0, 16, 16), new Rectangle(200, 100, 16, 16), 2);
            SpacebarPrompt = new StaticAnimated2D(Content.Load<Texture2D>("Sprites/Misc/Button Prompts/SpacebarPrompt"), 1, new Rectangle(0, 0, 48, 16), new Rectangle(340, 235, 48, 16), 2);
            MovementExample = new StaticAnimated2D(Content.Load<Texture2D>("Sprites/Misc/Button Prompts/MovementSpriteSheet"), 15, new Rectangle(0, 0, 285, 160), new Rectangle(320, 160, 16, 16), 1);
            ShootingExample = new StaticAnimated2D(Content.Load<Texture2D>("Sprites/Misc/Button Prompts/ShootSpriteSheet"), 8, new Rectangle(0, 0, 320, 290), new Rectangle(320, 320, 16, 16), 0.65f);
            ExplosionExample = new StaticAnimated2D(Content.Load<Texture2D>("Sprites/Alien Ships/Destruction/Alien0"), 8, new Rectangle(0, 0, 64, 64), new Rectangle(320, 450, 16, 16), 1f);
            LeftArrowPrompt = new StaticAnimated2D(Content.Load<Texture2D>("Sprites/Misc/Button Prompts/LeftArrowKeyPrompt"), 1, new Rectangle(0, 0, 16, 16), new Rectangle(200, 415, 16, 16), 2);
            RightArrowPrompt = new StaticAnimated2D(Content.Load<Texture2D>("Sprites/Misc/Button Prompts/RightArrowKeyPrompt"), 1, new Rectangle(0, 0, 16, 16), new Rectangle(440, 415, 16, 16), 2);
            // ------- < Advanced Prompts > ------------------------------------------------------------------
            UpArrowPrompt = new movingPromptKeys(Content.Load<Texture2D>("Sprites/Misc/Button Prompts/UpArrowKeyPrompt"), 1,  new Rectangle(0, 0, 16, 16), new Rectangle(255, 350, 16, 16), 2);
            DownArrowPrompt = new movingPromptKeys(Content.Load<Texture2D>("Sprites/Misc/Button Prompts/DownArrowKeyPrompt"), 1, new Rectangle(0, 0, 16, 16), new Rectangle(255, 470, 16, 16), 2);


            // < Fonts > ------------------------------------------------------------------
            _titleFont = Content.Load<SpriteFont>("Fonts/TitleFont");
            _bodyFont = Content.Load<SpriteFont>("Fonts/BodyFont");
            _secondaryBodyFont = Content.Load<SpriteFont>("Fonts/SecondaryBodyFont");


            // < Music > ------------------------------------------------------------------
            _menuMusic = Content.Load<Song>("Music/menu");
            _gameMusic = Content.Load<Song>("Music/game");


            // < Sound Effects > ------------------------------------------------------------------
            _alienDeathSFX = Content.Load<SoundEffect>("SFX/alienHitSound");
            _warningSFX = Content.Load<SoundEffect>("SFX/warningSound");
            _interactSFX = Content.Load<SoundEffect>("SFX/interactSound");
            _lazerSFX = Content.Load<SoundEffect>("SFX/shootSound");
            _thrusterSFX = Content.Load<SoundEffect>("SFX/thrusterSound");


            // < Sound Effect Instances > ------------------------------------------------------------------
            _alienDeathInst = _alienDeathSFX.CreateInstance();
            _alienDeathInst.Volume = 0.5f;
            _thrusterInst = _thrusterSFX.CreateInstance();
            _thrusterInst.Volume = 0.2f; 
        }

        

        protected override void Update(GameTime gameTime)
        {
            _gameRunTime += (float)gameTime.ElapsedGameTime.TotalSeconds;


            // < Mouse & Keyboard Updater > This is for getting the current state of the mouse and keyboard for use throught the program
            _currKey = Keyboard.GetState();
            _currMouse = Mouse.GetState();

            MediaPlayer.Volume = 0.15f;



            // < Splash Screen Checker > this check is to so see if the player is opening the game for the first time, if true it will take them first to the info screen for the game 
            if (_firstTimeOpening == true)
            {
                _gameScreen = gameScreen.help;
            }



            // < Switchboard > This controls what screen is displayed on the screen 
            switch (_gameScreen)
            {
                case gameScreen.title:
                    updateTitle(gameTime);
                    break;

                case gameScreen.game:
                    updateGame(gameTime);
                    break;

                case gameScreen.highScores:
                    updateHighScores(gameTime);
                    break;

                case gameScreen.help:
                    updateHelp(gameTime);
                    break;


                case gameScreen.credits:
                    updateCredits(gameTime);
                    break;
            }



            // < Screen Transition > This handles the logic related to the transition when the player changes between screens
            if (_transitioning == true)
            {
                transition.updateMe(gameTime);

                if (transition.Opacity <= 0)
                {
                    _transitioning = false;
                }
            }



            // < Mouse & Keyboard State Store > This stores the states for use in the program to check if the player is still pressing a key or button
            _oldKey = _currKey;
            _oldMouse = _currMouse;

            base.Update(gameTime);
        }



        void updateTitle(GameTime gameTime)
        {
            // < Exit > A simple exit command to leave the game
            if (_currKey.IsKeyDown(Keys.Escape) == true)
                Exit();



            // < Media Player Loop > This checks if the background has stopped, if true it will start to play it again
            if (MediaPlayer.State == MediaState.Stopped)
            {
                MediaPlayer.Play(_menuMusic);
            }



            // < Menu Controls > This set of statements controls what screen the player will go once a button has been pressed
            // ------- < Game Start Controls >
            if (_currKey.IsKeyDown(Keys.Space) == true && _oldKey.IsKeyUp(Keys.Space) && _transitioning == false)
            {
                screenTransition();
                _targetScreen = gameScreen.game;
            }
            // ------- < Highscores Screen Controls >
            if (_currKey.IsKeyDown(Keys.D1) == true && _oldKey.IsKeyUp(Keys.D1) && _transitioning == false)
            {
                screenTransition();
                _targetScreen = gameScreen.highScores;
            }
            // ------- < Help Screen Controls >
            if (_currKey.IsKeyDown(Keys.D2) == true && _oldKey.IsKeyUp(Keys.D2) && _transitioning == false)
            {
                screenTransition();
                _targetScreen = gameScreen.help;
            }
            // ------- < Credits Screen Controls >
            if (_currKey.IsKeyDown(Keys.D3) == true && _oldKey.IsKeyUp(Keys.D3) && _transitioning == false)
            {
                screenTransition();
                _targetScreen = gameScreen.credits;
            }
            // ------- < Transition Logic > this sends the player to the selected screen depending on what button the pressed, if its the game screen it make sure to reset everything related to it
            if (_transitioning == true)
            {
                if (transition.Opacity >= 1)
                {
                    _gameScreen = _targetScreen;

                    if (_targetScreen == gameScreen.game)
                    {
                        resetGame();
                    }
                }
            }



            // < Planet Update > This is to update the animation on the planet
            Planet.updateme(gameTime, _worldRotation);
        }



        void updateGame(GameTime gameTime)
        {
            // < Media Player Loop > This checks if the background has stopped, if true it will start to play it again
            if (MediaPlayer.State == MediaState.Stopped)
            {
                MediaPlayer.Play(_gameMusic);
            }



            // < Pause Menu Controls & Logic > This deals with checking if the player pauses the game and if the then exit the game from the pause menu
            // ------- < Pause/Unpause Button > this check when the player hits the pause button but also makes usre the player isnt sitting on the game over screen
            if (_currKey.IsKeyDown(Keys.Escape) && _oldKey.IsKeyUp(Keys.Escape) && _gameOver == false)
            {
                _gamePaused = !_gamePaused;
                _thrusterInst.Stop();
            }
            if (_gamePaused == true && _gameOver == false)
            {
                // ------- < Pause Menu Exit Button >
                if (_currKey.IsKeyDown(Keys.Space) == true && _oldKey.IsKeyUp(Keys.Space) && _transitioning == false)
                {
                    screenTransition();
                    _targetScreen = gameScreen.title;
                }
                // ------- < Pause Menu Exit Transition > This to send the player to the title screen and stop the music at the correct time
                if (_transitioning == true)
                {
                    if (transition.Opacity >= 1)
                    {
                        _gameScreen = _targetScreen;
                        MediaPlayer.Stop();
                    }
                }
            }



            // < Pause Check > This make sure that the game isnt paused so that objects arnt updating when they shouldnt
            if (_gamePaused == false)
            {
                _shotCharge -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                _shotCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                _alienSpawnCounter -= (float)gameTime.ElapsedGameTime.TotalSeconds;



                // < Simple Animation Update > This is for updating these objects animations at their specified intervals
                Planet.updateme(gameTime, _worldRotation);
                InvasionBar.updateMe(gameTime, _invasionMeter);



                // < Humans Update >  This update deals with updating all the Human game objects. be it their movement, rotation, animation, and state
                for (int i = 0; i < Humans.Count; i++)
                {
                    Humans[i].updateme(gameTime, _worldRotation);
                }



                // < Lazer Logic > This runs through the operations related to lazer objects
                for (int i = 0; i < Lazer.Count; i++)
                {
                    // ------- < Lazer Update > This update deals with updating all the Human game objects. be it their movement, rotation, animation, and state
                    Lazer[i].updateme(gameTime, Player.Rotation, Player.Position);

                    // ------- < Lazer Remove > Once the lazer as faded from view it gets removed for sake of unessisary objects
                    if (Lazer[i].Tint <= 0)
                    {
                        Lazer.RemoveAt(i);
                    }
                }



                // < Aliens Logic > This runs through the operations related to Aliens objects
                for (int i = 0; i < Aliens.Count; i++)
                {
                    bool abductionConfirm = false;



                    // < Human Collision Check > 
                    for (int j = 0; j < Humans.Count; j++)
                    {
                        if (Aliens[i].Hitbox.Intersects(Humans[j].Hitbox))
                        {
                            Humans.RemoveAt(j);
                            abductionConfirm = true;
                        }
                    }



                    // < Aliens Update > This update deals with updating all the Alien game objects. be it their movement, rotation, animation, and state
                    // ------- < Lazer Check > This if statement is to meke sure there is lazer being fired whilst the alien is being updated to make sure it isnt sent a NULL
                    if (Lazer.Count > 0)
                        Aliens[i].updateme(gameTime, _worldRotation, abductionConfirm, Player.Position, Player.LazerEndPos);
                    // ------- < Lazer Default > This only sends if there is no lazer currently being fired to avoid an error
                    else
                        Aliens[i].updateme(gameTime, _worldRotation, abductionConfirm, new Vector2(0,0), new Vector2(0, 0));



                    // < Aliens Dying/Dead State Check > This simply checks when the aliens change their state from Alive -> Dying -> Dead, and executes what nessisary at those points
                    // ------- < Alien Dying > Plays the alien explosion SFX when it is hit whith a lazer
                    if (Aliens[i].LifeState == lifeState.Dying)
                    {
                        _alienDeathInst.Play();
                    }
                    // ------- < Alien Dead > Once the alien played its death animation to completion it is removed from the list and points are added to the player score
                    if (Aliens[i].LifeState == lifeState.Dead)
                    {
                        Aliens.RemoveAt(i);
                        _score += 100;
                        break;
                    }



                    // < Sucessful Abduction Check > This checks if the Alien is far enough from the planet for the player to do anything. once true it will remove the alien and add to the invasion meter
                    if (Aliens[i].DistanceFromPlanet > 920 && Aliens[i].AbductionState == true)
                    {
                        Aliens.RemoveAt(i);
                        _invasionMeter += 1;
                        if (_gameOver == false)
                            _warningSFX.Play();
                    }
                }



                // < Player Update >  This update deals with updating all the Player object. be it their movement, rotation, animation, and state. also checking that the game isnt over before updating them
                if (_gameOver == false)
                    Player.updateme(_currKey, _currMouse, gameTime);



                // < Player Movement SFX > These controls when the player thrusters sound plays and stops
                // ------- < Movment SFX Play > 
                if (_currKey.IsKeyDown(Keys.W) || _currMouse.RightButton == ButtonState.Pressed)
                {
                    _thrusterInst.Play();
                }
                // ------- < Movment SFX Stop > 
                if (_currKey.IsKeyUp(Keys.W) && _currMouse.RightButton == ButtonState.Released)
                {
                    _thrusterInst.Stop();
                }



                // < Weapeon Controls & logic > set of statements check for weapeon firings and deals with the Charging -> Firing -> Cooldown process
                // ------- < Weapeon Charge > this detects the player pressing the fire button making sure the weapeon is off cooldown, if true is starts the countdown to firing the lazer
                if ((_currKey.IsKeyDown(Keys.Space) || _currMouse.LeftButton == ButtonState.Pressed) && _shotCooldown <= 0 && _weapeonFiring == false && _gameOver == false)
                {
                    _shotCharge = 0.4f;
                    _weapeonFiring = true;

                }
                // ------- < Weapeon Firing > once the weapen has been charged it spawns a Lazer object in front of the player ship and starts the cooldown process
                if (_shotCharge <= 0 && _weapeonFiring == true)
                {
                    _weapeonFiring = false;
                    _shotCooldown = 1;
                    _lazerSFX.Play();

                    Lazer.Add(new LazerBeam(
                           Content.Load<Texture2D>("Sprites/Player Ship/Weapons/Zapper Beam"),
                           16,
                           new Rectangle(0, 0, 10, 10),
                           new Rectangle(0, 0, 0, 0),
                           0.50f,
                           Player.Rotation,
                           Player.Position));
                }
                


                // < Human Count Top-Up > this check to make sure the number of human objects in play match the number we want, if this is false it will add human objects until it matches
                if (Humans.Count < NUMBEROFPEOPLE)
                {
                    Humans.Add(new Human(
                    Content.Load<Texture2D>("Sprites/walk"),
                    6,
                    new Rectangle(0, 0, 80, 80),
                    new Rectangle(_screen.Width / 2, 445, 80, 80),
                    1));
                }



                // < Alien Spawner > once the countdown reaches 0 we spawn the new wave of aliens whilst also adding more per new wave
                if (_alienSpawnCounter <= 0)
                {
                    _alienSpawnCounter = _maxAlienSpawnCounter;
                    _numberOfAliens++;

                    while (Aliens.Count < _numberOfAliens)
                    {
                        int random = RNG.Next(0, 4);
                        Aliens.Add(new Alien(
                            Content.Load<Texture2D>("Sprites/Alien Ships/Base/Alien" + random),
                            Content.Load<Texture2D>("Sprites/Alien Ships/Destruction/Alien" + random),
                            Content.Load<Texture2D>("Sprites/Alien Ships/Engine/Alien" + random),
                            Content.Load<Texture2D>("Sprites/Alien Ships/warningSign"),
                            10,
                            new Rectangle(0, 0, 64, 64),
                            new Rectangle(_screen.Width / 2, -50, 20, 20),
                            1));
                    }
                }



                // < Game End Checks > Once the invasion meter has been filled the game is ended and the player can no longer make inputs to the ship
                if (_invasionMeter == 4)
                {
                    _gameOver = true;
                    // ------- < Game Score Check > this is to check wether the player qualifys for the highscore board, if true we will display to the player controls to input their initals
                    if (_score < scoreManager.Data.Score[9])
                    {
                        _sentNewTableEntry = true;
                    }
                }



                // < End Game Controls & Logic > These set of statements control the scoreboard entry screen as well as the game over screen
                if (_gameOver == true)
                {
                    // ------- < Player Inital Input screen > If the players score is elegable for the Highscores table, the player is given the option to input their initals
                    if (_sentNewTableEntry == false)
                    {
                        // ------- // ------- < Input Decoder > This is used to change the INT value of the number wheel the player interacts with into a corasponding number A-Z -> 1-26
                        _playerNameInput = scoreManager.decodePlayerName(playerInitals);



                        // ------- // ------- < Confirm Initals > this input sends the initals inputed by the player to the highscore table and saves it
                        if (_currKey.IsKeyDown(Keys.Enter) == true && _oldKey.IsKeyUp(Keys.Enter))
                        {
                            scoreManager.MaybeAddHighScore(_playerNameInput, _score);
                            scoreManager.SaveHighScores();
                            _sentNewTableEntry = true;
                        }
                        // ------- // ------- < Change Input Left > this input changes what letter the player can modify to the left & wraps around if its to the farthest left
                        if (_currKey.IsKeyDown(Keys.Left) == true && _oldKey.IsKeyUp(Keys.Left))
                        {
                            _initalSelect--;
                            _interactSFX.Play();
                            if (_initalSelect < 0)
                                _initalSelect = 2;
                        }
                        // ------- // ------- < Change Input Right > this input changes what letter the player can modify to the right & wraps around if its to the farthest right
                        if (_currKey.IsKeyDown(Keys.Right) == true && _oldKey.IsKeyUp(Keys.Right))
                        {
                            _initalSelect++;
                            _interactSFX.Play();
                            if (_initalSelect > 2)
                                _initalSelect = 0;
                        }
                        // ------- // ------- < Change Letter Up > this input changes the selected letter up by one, eg. A -> B. This is also combined with a wraper if it it alread the smallest entry
                        if (_currKey.IsKeyDown(Keys.Up) == true && _oldKey.IsKeyUp(Keys.Up))
                        {
                            playerInitals[_initalSelect]++;
                            _interactSFX.Play();
                            if (playerInitals[_initalSelect] > 26)
                                playerInitals[_initalSelect] = 1;
                        }
                        // ------- // ------- < Change Letter Down > this input changes the selected letter up by one, eg. B -> A. This is also combined with a wraper if it it alread the smallest entry
                        if (_currKey.IsKeyDown(Keys.Down) == true && _oldKey.IsKeyUp(Keys.Down))
                        {
                            playerInitals[_initalSelect]--;
                            _interactSFX.Play();
                            if (playerInitals[_initalSelect] < 1)
                                playerInitals[_initalSelect] = 26;
                        }


                        // ------- // ------- < Prompt Updates > These are simple updates for the button prompts and their animations
                        LeftArrowPrompt.updateme(gameTime);
                        RightArrowPrompt.updateme(gameTime);
                        UpArrowPrompt.updateMe(gameTime, _initalSelect);
                        DownArrowPrompt.updateMe(gameTime, _initalSelect);
                    }
                    // ------- < Game Over Screen Controls > if the player enterd their initals for the scoreboard they will be brought here, if they where not elegible they will be brought right here
                    else if (_sentNewTableEntry == true)
                    {
                        // ------- // ------- < Screen Select > these statements control the end game screen and where the player will go after the game ends
                        // ------- // ------- // ------- < Go To Title Screen >
                        if (_currKey.IsKeyDown(Keys.Escape) == true && _oldKey.IsKeyUp(Keys.Escape) && _transitioning == false)
                        {
                            screenTransition();
                            _targetScreen = gameScreen.title;
                        }
                        // ------- // ------- // ------- < Restart Game >
                        if (_currKey.IsKeyDown(Keys.Space) == true && _oldKey.IsKeyUp(Keys.Space) && _transitioning == false)
                        {
                            screenTransition();
                            _targetScreen = gameScreen.game;
                        }
                        // ------- // ------- // ------- < Screen Transition > once the transition has completly coverd the screenm it changes the screen so the player cant see it
                        if (_transitioning == true)
                        {
                            if (transition.Opacity >= 1)
                            {
                                _gameScreen = _targetScreen;
                                MediaPlayer.Stop();

                                // ------- // ------- // ------- // ------- < Game Screen Check > if the player hit to play the game again we must make sure the game is completly reset before start
                                if (_gameScreen == gameScreen.game)
                                {
                                    resetGame();
                                }
                            }
                        }
                    }                  
                }

                // < World Rotation > When the player presses up against eather side of the play area we start to rotate the game based on the player speed
                // ------- < World Rotate Left >
                if (Player.Position.X < 130 && _gameOver == false)
                {
                    _worldRotation -= Player.Velocity.X * 0.005f;
                }
                // ------- < World Rotate Left >
                if (Player.Position.X > 505 && _gameOver == false)
                {
                    _worldRotation -= Player.Velocity.X * 0.005f;
                }
            }          
        }



        void updateHelp(GameTime gameTime)
        {
            // < Media Player Loop > This checks if the background has stopped, if true it will start to play it again
            if (MediaPlayer.State == MediaState.Stopped)
            {
                MediaPlayer.Play(_menuMusic);
            }



            // < Return > This input simply bright the player back to the title screen
            if ((_currKey.IsKeyDown(Keys.D2) == true && _oldKey.IsKeyUp(Keys.D2)) || (_currKey.IsKeyDown(Keys.Space) && _oldKey.IsKeyUp(Keys.Space) && _firstTimeOpening == true) && _transitioning == false)
            {
                screenTransition();
                _targetScreen = gameScreen.title;
            }
            // ------- < Transition Logic > this sends the player to the selected screen
            if (_transitioning == true)
            {
                if (transition.Opacity >= 1)
                {
                    _gameScreen = _targetScreen;
                    _firstTimeOpening = false;
                }
            }



            // < Animation Updates > This block is to update the animations for each of these prompts
            Planet.updateme(gameTime);
            LeftMousePrompt.updateme(gameTime);
            RightMousePrompt.updateme(gameTime);
            WKeyPrompt.updateme(gameTime);
            SpacebarPrompt.updateme(gameTime);
            MovementExample.updateme(gameTime);
            ShootingExample.updateme(gameTime);
            ExplosionExample.updateme(gameTime);
        }
    


        void updateHighScores(GameTime gameTime)
        {
            // < Media Player Loop > This checks if the background has stopped, if true it will start to play it again
            if (MediaPlayer.State == MediaState.Stopped)
            {
                MediaPlayer.Play(_menuMusic);
            }


            // < Return > This input simply bright the player back to the title screen
            if (_currKey.IsKeyDown(Keys.D1) == true && _oldKey.IsKeyUp(Keys.D1) && _transitioning == false)
            {
                screenTransition();
                _targetScreen = gameScreen.title;
            }
            // ------- < Transition Logic > this sends the player to the selected screen
            if (_transitioning == true)
            {
                if (transition.Opacity >= 1)
                {
                    _gameScreen = _targetScreen;
                }
            }


            // < Planet Update > This is to update the animation on the planet
            Planet.updateme(gameTime, _worldRotation);
        }



        void updateCredits(GameTime gameTime) 
        {
            // < Media Player Loop > This checks if the background has stopped, if true it will start to play it again
            if (MediaPlayer.State == MediaState.Stopped)
            {
                MediaPlayer.Play(_menuMusic);
            }


            // < Return > This input simply bright the player back to the title screen
            if (_currKey.IsKeyDown(Keys.D3) == true && _oldKey.IsKeyUp(Keys.D3) && _transitioning == false)
            {
                screenTransition();
                _targetScreen = gameScreen.title;
            }
            // ------- < Transition Logic > this sends the player to the selected screen
            if (_transitioning == true)
            {
                if (transition.Opacity >= 1)
                {
                    _gameScreen = _targetScreen;
                }
            }


            // < Planet Update > This is to update the animation on the planet
            Planet.updateme(gameTime, _worldRotation);
        }



        // < Game Varibles/Object Reset > This is where we reset all the objects and varibles related to the game by re-building it in its entirety
        void resetGame()
        {
            _numberOfAliens = 0;
            _alienSpawnCounter = 3;
            _maxAlienSpawnCounter = 10;

            _score = 0;
            _invasionMeter = 0;
            _worldRotation = 0;

            _gamePaused = false;
            _gameOver = false;
            _sentNewTableEntry = false;

            MediaPlayer.Stop();

            Humans.Clear();
            Aliens.Clear();

            while (Humans.Count < NUMBEROFPEOPLE)
            {
                Humans.Add(new Human(
                Content.Load<Texture2D>("Sprites/walk"),
                6,
                new Rectangle(0, 0, 80, 80),
                new Rectangle(_screen.Width / 2, 445, 80, 80),
                1));
            }

            Planet = new MovingAnimated2D(
                Content.Load<Texture2D>("Sprites/Planet"),
                0.5f,
                new Rectangle(0, 0, 500, 500),
                new Rectangle(
                    _screen.Width / 2,
                    _screen.Height + 300,
                    500,
                    500),
                1.7f);

            Player = new PlayerShip(
                Content.Load<Texture2D>("Sprites/Player Ship/Hull/Full health"),
                Content.Load<Texture2D>("Sprites/Player Ship/Engines/Engine"),
                Content.Load<Texture2D>("Sprites/Player Ship/Engines/Engine Effects"),
                Content.Load<Texture2D>("Sprites/Player Ship/Weapons/Zapper"),
                10,
                new Rectangle(0, 0, 48, 48),
                new Rectangle(_screen.Width / 2, _screen.Height / 2, 10, 10),
                1);
        }



        // < Game Transition > This is function we call when we want to fade between main screens to make them more seemless
        void screenTransition()
        {
            transition = new Transition(_screen, Content.Load<Texture2D>("overlayPixel"));
            _transitioning = true;
            _interactSFX.Play();
        }



        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            // < Switchboard > This controls what screen is displayed on the screen 
            switch (_gameScreen)
            {
                case gameScreen.title:
                    drawTitle(gameTime);
                    break;

                case gameScreen.game:
                    drawGame(gameTime);
                    break;

                case gameScreen.highScores:
                    drawHighScores(gameTime);
                    break;

                case gameScreen.help:
                    drawHelp(gameTime);
                    break;

                case gameScreen.credits:
                    drawCredits(gameTime);
                    break;
            }


            // < Transition > this draws the transition slide only when we have hit a button used for navigation
            if (_transitioning == true)
                transition.drawMe(_spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }



        void drawTitle(GameTime gameTime)
        {
            // < Background Objects > These are static background objects for the backdrop of the screen
            Background.drawme(_spriteBatch);
            Planet.drawme(_spriteBatch);
            Moon.drawme(_spriteBatch);



            // < Title Page Text > These blocks are for displaying the text for the page. each one has a colour line and a shadow line drawn behind it
            // ------- < Title >
            Vector2 titleSize = _titleFont.MeasureString("ALIENS!2");
            _spriteBatch.DrawString(_titleFont, "ALIENS!2", new Vector2((_screen.Width / 2) - (titleSize.X / 2), (_screen.Height / 3) - (titleSize.Y / 2)), Color.Black);
            _spriteBatch.DrawString(_titleFont, "ALIENS!2", new Vector2((_screen.Width / 2) - (titleSize.X / 2) + 6, (_screen.Height / 3) - (titleSize.Y / 2) + 6), Color.GreenYellow);

            // ------- < Start Prompt > this line only draws every other second so its given a flashing apperence
            if ((int)_gameRunTime % 2 == 0)
            {
                Vector2 startSize = _bodyFont.MeasureString("press SPACE to START!");
                _spriteBatch.DrawString(_bodyFont, "press SPACE to START!", new Vector2((_screen.Width / 2) - (startSize.X / 2), (_screen.Height / 2) - (startSize.Y / 2)), Color.Black);
                _spriteBatch.DrawString(_bodyFont, "press SPACE to START!", new Vector2((_screen.Width / 2) - (startSize.X / 2) + 3, (_screen.Height / 2) - (startSize.Y / 2) + 3), Color.GreenYellow);
            }

            // ------- < Scoreboard Prompt > 
            Vector2 scoreSize = _secondaryBodyFont.MeasureString("press 1 for SCOREBOARD");
            _spriteBatch.DrawString(_secondaryBodyFont, "press 1 for SCOREBOARD", new Vector2((_screen.Width / 2) - (scoreSize.X / 2), ((_screen.Height / 2) + (_screen.Height / 3)) - (scoreSize.Y / 2)), Color.Black);
            _spriteBatch.DrawString(_secondaryBodyFont, "press 1 for SCOREBOARD", new Vector2((_screen.Width / 2) - (scoreSize.X / 2) + 3, ((_screen.Height / 2) + (_screen.Height / 3)) - (scoreSize.Y / 2) + 3), Color.GreenYellow);

            // ------- < Help Prompt >
            Vector2 helpSize = _secondaryBodyFont.MeasureString("press 2 for HELP");
            _spriteBatch.DrawString(_secondaryBodyFont, "press 2 for HELP", new Vector2((_screen.Width / 2) - (helpSize.X / 2), ((_screen.Height / 2) + (_screen.Height / 3) + 40) - (helpSize.Y / 2)), Color.Black);
            _spriteBatch.DrawString(_secondaryBodyFont, "press 2 for HELP", new Vector2((_screen.Width / 2) - (helpSize.X / 2) + 3, ((_screen.Height / 2) + (_screen.Height / 3) + 40) - (helpSize.Y / 2) + 3), Color.GreenYellow);

            // ------- < Help Prompt >
            Vector2 creditSize = _secondaryBodyFont.MeasureString("press 3 for CREDITS");
            _spriteBatch.DrawString(_secondaryBodyFont, "press 3 for CREDITS", new Vector2((_screen.Width / 2) - (creditSize.X / 2), ((_screen.Height / 2) + (_screen.Height / 3) + 80) - (creditSize.Y / 2)), Color.Black);
            _spriteBatch.DrawString(_secondaryBodyFont, "press 3 for CREDITS", new Vector2((_screen.Width / 2) - (creditSize.X / 2) + 3, ((_screen.Height / 2) + (_screen.Height / 3) + 80) - (creditSize.Y / 2) + 3), Color.GreenYellow);

            // ------- < Credit >
            _spriteBatch.DrawString(_secondaryBodyFont, "Vega Games", new Vector2(0, 0), Color.Black);
            _spriteBatch.DrawString(_secondaryBodyFont, "Vega Games", new Vector2(3, 3), Color.GreenYellow);
        }



        void drawGame(GameTime gameTime)
        {
            // < Background Objects > These are semi-static background objects for the backdrop of the screen
            Background.drawme(_spriteBatch);
            Planet.drawme(_spriteBatch);
            Moon.drawme(_spriteBatch);



            // < Game Object > These are where all the main game objects are drawn
            for (int i = 0; i < Humans.Count; i++)
                Humans[i].drawme(_spriteBatch);

            for (int i = 0; i < Aliens.Count; i++)
                Aliens[i].drawme(_spriteBatch);

            for (int i = 0; i < Lazer.Count; i++)
                Lazer[i].drawme(_spriteBatch);

            Player.drawme(_spriteBatch);



            // < In-Game UI > This is for displaying the player information whilst playing the game
            if (_gameOver == false)
            {
                // ------- < Invasion-O-Meter > This displays how much invasion score has gone up in bar form
                InvasionBar.drawMe(_spriteBatch);


                // ------- < Pause Menu Text > These blocks are for displaying the text for the page. each one has a colour line and a shadow line drawn behind it
                // ------- // ------- < Invasion Bar Headliner > 
                Vector2 invasionSize = _bodyFont.MeasureString("Invasion!");
                _spriteBatch.DrawString(_bodyFont, "Invasion!", new Vector2((_screen.Width / 2) - (invasionSize.X / 2), 20 - (invasionSize.Y / 2)), Color.Black);
                _spriteBatch.DrawString(_bodyFont, "Invasion!", new Vector2((_screen.Width / 2) - (invasionSize.X / 2) + 3, 20 - (invasionSize.Y / 2) + 3), Color.GreenYellow);


                // ------- // ------- < Player Score > This constantly displays the score to the player
                Vector2 scoreSize = _secondaryBodyFont.MeasureString("SCORE:" + _score);
                _spriteBatch.DrawString(_secondaryBodyFont, "SCORE:" + _score, new Vector2((_screen.Width / 2) - (scoreSize.X / 2), 80 - (scoreSize.Y / 2)), Color.Black);
                _spriteBatch.DrawString(_secondaryBodyFont, "SCORE:" + _score, new Vector2((_screen.Width / 2) - (scoreSize.X / 2) + 3, 80 - (scoreSize.Y / 2) + 3), Color.GreenYellow);
            }



            // < Pause Menu > this is only displayed whilst the game is paused
            if (_gamePaused == true)
            {
                // ------- < Overlay > This dims the game screen behind the pause menu to make it easier to read 
                _spriteBatch.Draw(_overlay, _screen, Color.Black * 0.5f);


                // ------- < Pause Menu Text > These blocks are for displaying the text for the page. each one has a colour line and a shadow line drawn behind it
                // ------- // ------- < Headliner > 
                Vector2 titleSize = _titleFont.MeasureString("Paused");
                _spriteBatch.DrawString(_titleFont, "Paused", new Vector2((_screen.Width / 2) - (titleSize.X / 2), (_screen.Height / 3) - (titleSize.Y / 2)), Color.Black);
                _spriteBatch.DrawString(_titleFont, "Paused", new Vector2((_screen.Width / 2) - (titleSize.X / 2) + 6, (_screen.Height / 3) - (titleSize.Y / 2) + 6), Color.GreenYellow);


                // ------- // ------- < Exit Prompt >
                Vector2 exitSize = _secondaryBodyFont.MeasureString("press SPACE to EXIT to TITLE");
                _spriteBatch.DrawString(_secondaryBodyFont, "press SPACE to EXIT to TITLE", new Vector2((_screen.Width / 2) - (exitSize.X / 2), ((_screen.Height / 2) + (_screen.Height / 3)) - (exitSize.Y / 2)), Color.Black);
                _spriteBatch.DrawString(_secondaryBodyFont, "press SPACE to EXIT to TITLE", new Vector2((_screen.Width / 2) - (exitSize.X / 2) + 3, ((_screen.Height / 2) + (_screen.Height / 3)) - (exitSize.Y / 2) + 3), Color.GreenYellow);


                // ------- // ------- < Resume Prompt >
                Vector2 resumeSize = _secondaryBodyFont.MeasureString("press ESCAPE to RESUME");
                _spriteBatch.DrawString(_secondaryBodyFont, "press ESCAPE to RESUME", new Vector2((_screen.Width / 2) - (resumeSize.X / 2), ((_screen.Height / 2) + (_screen.Height / 3) + 40) - (resumeSize.Y / 2)), Color.Black);
                _spriteBatch.DrawString(_secondaryBodyFont, "press ESCAPE to RESUME", new Vector2((_screen.Width / 2) - (resumeSize.X / 2) + 3, ((_screen.Height / 2) + (_screen.Height / 3) + 40) - (resumeSize.Y / 2) + 3), Color.GreenYellow);
            }



            // < Game Over Screens > 
            if (_gameOver == true)
            {
                // ------- < Overlay > This dims the game screen behind the pause menu to make it easier to read 
                _spriteBatch.Draw(_overlay, _screen, Color.Black * 0.5f);


                // ------- < Headliner > 
                Vector2 endSize = _titleFont.MeasureString("GAME OVER");
                _spriteBatch.DrawString(_titleFont, "GAME OVER", new Vector2((_screen.Width / 2) - (endSize.X / 2), (_screen.Height / 3) - (endSize.Y / 2)), Color.Black);
                _spriteBatch.DrawString(_titleFont, "GAME OVER", new Vector2((_screen.Width / 2) - (endSize.X / 2) + 6, (_screen.Height / 3) - (endSize.Y / 2) + 6), Color.GreenYellow);


                // ------- < Initals Entry Text > These blocks are for displaying the text for the page. each one has a colour line and a shadow line drawn behind it
                if (_sentNewTableEntry == false)
                {
                    // ------- // ------- < Initals Input Prompt >
                    Vector2 scoreSize = _secondaryBodyFont.MeasureString("HIGHSCORE! - Input any three letters");
                    _spriteBatch.DrawString(_secondaryBodyFont, "HIGHSCORE! - Input any three letters", new Vector2((_screen.Width / 2) - (scoreSize.X / 2), (_screen.Height / 2) - (scoreSize.Y / 2)), Color.Black);
                    _spriteBatch.DrawString(_secondaryBodyFont, "HIGHSCORE! - Input any three letters", new Vector2((_screen.Width / 2) - (scoreSize.X / 2) + 3, (_screen.Height / 2) - (scoreSize.Y / 2) + 3), Color.GreenYellow);


                    // ------- // ------- < Player Initals >
                    Vector2 nameSize = _titleFont.MeasureString(_playerNameInput);
                    _spriteBatch.DrawString(_titleFont, _playerNameInput, new Vector2((_screen.Width / 2) - (nameSize.X / 2), 125 + (_screen.Height / 2) - (nameSize.Y / 2)), Color.Black);
                    _spriteBatch.DrawString(_titleFont, _playerNameInput, new Vector2((_screen.Width / 2) - (nameSize.X / 2) + 3, 125 + (_screen.Height / 2) - (nameSize.Y / 2) + 3), Color.GreenYellow);


                    // ------- // ------- < Selected Inital Underline > this line only draws under the inital that has been currently selected to change
                    _spriteBatch.DrawString(_titleFont, "_", new Vector2(225 + (_initalSelect * 65), 140 + (_screen.Height / 2) - (nameSize.Y / 2)), Color.Black);
                    _spriteBatch.DrawString(_titleFont, "_", new Vector2(225 + (_initalSelect * 65) + 3, 140 + (_screen.Height / 2) - (nameSize.Y / 2) + 3), Color.GreenYellow);


                    // ------- // ------- < Confirmation Prompt >
                    Vector2 resumeSize = _secondaryBodyFont.MeasureString("press ENTER to CONFIRM");
                    _spriteBatch.DrawString(_secondaryBodyFont, "press ENTER to CONFIRM", new Vector2((_screen.Width / 2) - (resumeSize.X / 2), ((_screen.Height / 2) + (_screen.Height / 3) + 40) - (resumeSize.Y / 2)), Color.Black);
                    _spriteBatch.DrawString(_secondaryBodyFont, "press ENTER to CONFIRM", new Vector2((_screen.Width / 2) - (resumeSize.X / 2) + 3, ((_screen.Height / 2) + (_screen.Height / 3) + 40) - (resumeSize.Y / 2) + 3), Color.GreenYellow);


                    // ------- // ------- < Input prompts > These are prompts for the player to follow to that can input their initals into the game
                    LeftArrowPrompt.drawme(_spriteBatch);
                    RightArrowPrompt.drawme(_spriteBatch);
                    UpArrowPrompt.drawme(_spriteBatch);
                    DownArrowPrompt.drawme(_spriteBatch);
                }
                // ------- < Game Over Text > These blocks are for displaying the text for the page. each one has a colour line and a shadow line drawn behind it
                else if (_sentNewTableEntry == true)
                {
                    // ------- // ------- < Player Score >
                    Vector2 scoreSize = _bodyFont.MeasureString("Score:" + _score);
                    _spriteBatch.DrawString(_bodyFont, "Score:" + _score, new Vector2((_screen.Width / 2) - (scoreSize.X / 2), (_screen.Height / 2) - (scoreSize.Y / 2)), Color.Black);
                    _spriteBatch.DrawString(_bodyFont, "Score:" + _score, new Vector2((_screen.Width / 2) - (scoreSize.X / 2) + 3, (_screen.Height / 2) - (scoreSize.Y / 2) + 3), Color.GreenYellow);

                    // ------- // ------- < Reset Game Prompt >
                    Vector2 exitSize = _secondaryBodyFont.MeasureString("press SPACE to PLAY AGAIN");
                    _spriteBatch.DrawString(_secondaryBodyFont, "press SPACE to PLAY AGAIN", new Vector2((_screen.Width / 2) - (exitSize.X / 2), ((_screen.Height / 2) + (_screen.Height / 3)) - (exitSize.Y / 2)), Color.Black);
                    _spriteBatch.DrawString(_secondaryBodyFont, "press SPACE to PLAY AGAIN", new Vector2((_screen.Width / 2) - (exitSize.X / 2) + 3, ((_screen.Height / 2) + (_screen.Height / 3)) - (exitSize.Y / 2) + 3), Color.GreenYellow);

                    // ------- // ------- < Exit Game Prompt >
                    Vector2 resumeSize = _secondaryBodyFont.MeasureString("press ESCAPE to EXIT to TITLE");
                    _spriteBatch.DrawString(_secondaryBodyFont, "press ESCAPE to EXIT to TITLE", new Vector2((_screen.Width / 2) - (resumeSize.X / 2), ((_screen.Height / 2) + (_screen.Height / 3) + 40) - (resumeSize.Y / 2)), Color.Black);
                    _spriteBatch.DrawString(_secondaryBodyFont, "press ESCAPE to EXIT to TITLE", new Vector2((_screen.Width / 2) - (resumeSize.X / 2) + 3, ((_screen.Height / 2) + (_screen.Height / 3) + 40) - (resumeSize.Y / 2) + 3), Color.GreenYellow);
                }
            }
        }



        void drawHighScores(GameTime gameTime)
        {
            // < Background Objects > These are semi-static background objects for the backdrop of the screen
            Background.drawme(_spriteBatch);
            Planet.drawme(_spriteBatch);
            Moon.drawme(_spriteBatch);


            // < Scoreboard Page Text > These blocks are for displaying the text for the page. each one has a colour line and a shadow line drawn behind it
            // ------- < Title >
            Vector2 titleSize = _bodyFont.MeasureString("Highscores");
            _spriteBatch.DrawString(_bodyFont, "Highscores", new Vector2((_screen.Width / 2) - (titleSize.X / 2), 30 - (titleSize.Y / 2)), Color.Black);
            _spriteBatch.DrawString(_bodyFont, "Highscores", new Vector2((_screen.Width / 2) - (titleSize.X / 2) + 3, 30 - (titleSize.Y / 2) + 3), Color.GreenYellow);

            // ------- < Scoreboard > This functions goes through the scoreboard entrys drawing them line by line
            for (int i = 0; i < scoreManager.Data.Count; i++)
            {
                Vector2 tableSize = _secondaryBodyFont.MeasureString((i + 1) + ". " + scoreManager.Data.PlayerName[i] + " - " + scoreManager.Data.Score[i]);
                
                _spriteBatch.DrawString(
                    _secondaryBodyFont, 
                    (i + 1) + ". " + scoreManager.Data.PlayerName[i] + " - " + scoreManager.Data.Score[i], 
                    new Vector2((_screen.Width / 2) - (tableSize.X / 2), (80) - (tableSize.Y / 2) + (40 * i)),
                    Color.Black);

                _spriteBatch.DrawString(
                    _secondaryBodyFont,
                    (i + 1) + ". " + scoreManager.Data.PlayerName[i] + " - " + scoreManager.Data.Score[i],
                    new Vector2((_screen.Width / 2) - (tableSize.X / 2) + 3, (80) - (tableSize.Y / 2) + (40 * i) + 3),
                    Color.GreenYellow);
            }

            // ------- < Return Prompt >
            Vector2 scoreSize = _secondaryBodyFont.MeasureString("press 1 to RETURN");
            _spriteBatch.DrawString(_secondaryBodyFont, "press 1 to RETURN", new Vector2((_screen.Width / 2) - (scoreSize.X / 2), 60 + ((_screen.Height / 2) + (_screen.Height / 3)) - (scoreSize.Y / 2)), Color.Black);
            _spriteBatch.DrawString(_secondaryBodyFont, "press 1 to RETURN", new Vector2((_screen.Width / 2) - (scoreSize.X / 2) + 3, 60 + ((_screen.Height / 2) + (_screen.Height / 3)) - (scoreSize.Y / 2) + 3), Color.GreenYellow);
        }



        void drawHelp(GameTime gameTime)
        {
            // < Background Objects > These are semi-static background objects for the backdrop of the screen
            Background.drawme(_spriteBatch);
            Planet.drawme(_spriteBatch);
            Moon.drawme(_spriteBatch);



            // < Prompt Objects > These the prompt keys for players to know which buttons do what
            LeftMousePrompt.drawme(_spriteBatch);
            RightMousePrompt.drawme(_spriteBatch);
            WKeyPrompt.drawme(_spriteBatch);
            SpacebarPrompt.drawme(_spriteBatch);
            MovementExample.drawme(_spriteBatch);
            ShootingExample.drawme(_spriteBatch);
            ExplosionExample.drawme(_spriteBatch);



            // < Help Page Text > These blocks are for displaying the text for the page. each one has a colour line and a shadow line drawn behind it
            // ------- < Movement Info > 
            Vector2 moveSize = _secondaryBodyFont.MeasureString("press   or   to MOVE towards the MOUSE");
            _spriteBatch.DrawString(_secondaryBodyFont, "press   or   to MOVE towards the MOUSE", new Vector2((_screen.Width / 2) - (moveSize.X / 2), ((_screen.Height / 6)) - (moveSize.Y / 2)), Color.Black);
            _spriteBatch.DrawString(_secondaryBodyFont, "press   or   to MOVE towards the MOUSE", new Vector2((_screen.Width / 2) - (moveSize.X / 2) + 3, ((_screen.Height / 6)) - (moveSize.Y / 2) + 3), Color.GreenYellow);

            // ------- < Shooting Info > 
            Vector2 shootSize = _secondaryBodyFont.MeasureString("press   or       to Shoot");
            _spriteBatch.DrawString(_secondaryBodyFont, "press   or       to Shoot", new Vector2((_screen.Width / 2) - (shootSize.X / 2), ((_screen.Height / 2.5f)) - (shootSize.Y / 2)), Color.Black);
            _spriteBatch.DrawString(_secondaryBodyFont, "press   or       to Shoot", new Vector2((_screen.Width / 2) - (shootSize.X / 2) + 3, ((_screen.Height / 2.5f)) - (shootSize.Y / 2) + 3), Color.GreenYellow);

            // ------- < Alien Info > 
            Vector2 killSize = _secondaryBodyFont.MeasureString("DESTROY the ALIENS to stop them");
            _spriteBatch.DrawString(_secondaryBodyFont, "DESTROY the ALIENS to stop them", new Vector2((_screen.Width / 2) - (killSize.X / 2), ((_screen.Height / 1.5f)) - (killSize.Y / 2)), Color.Black);
            _spriteBatch.DrawString(_secondaryBodyFont, "DESTROY the ALIENS to stop them", new Vector2((_screen.Width / 2) - (killSize.X / 2) + 3, ((_screen.Height / 1.5f)) - (killSize.Y / 2) + 3), Color.GreenYellow);

            // ------- < Return Prompt > 
            // ------- // ------- < Splash Text > if its the first time the player opens the game make sure the text displays a CONTINUE message
            if (_firstTimeOpening == true)
            {
                Vector2 helpSize = _secondaryBodyFont.MeasureString("press Space to Continue to TITLE");
                _spriteBatch.DrawString(_secondaryBodyFont, "press Space to Continue to TITLE", new Vector2((_screen.Width / 2) - (helpSize.X / 2), ((_screen.Height / 2) + (_screen.Height / 3) + 40) - (helpSize.Y / 2)), Color.Black);
                _spriteBatch.DrawString(_secondaryBodyFont, "press Space to Continue to TITLE", new Vector2((_screen.Width / 2) - (helpSize.X / 2) + 3, ((_screen.Height / 2) + (_screen.Height / 3) + 40) - (helpSize.Y / 2) + 3), Color.GreenYellow);
            }
            // ------- // ------- < Regular Text > if the player acessed the page the normal way, text will show a RETURN message
            else
            {
                Vector2 helpSize = _secondaryBodyFont.MeasureString("press 2 to RETURN");
                _spriteBatch.DrawString(_secondaryBodyFont, "press 2 to RETURN", new Vector2((_screen.Width / 2) - (helpSize.X / 2), ((_screen.Height / 2) + (_screen.Height / 3) + 40) - (helpSize.Y / 2)), Color.Black);
                _spriteBatch.DrawString(_secondaryBodyFont, "press 2 to RETURN", new Vector2((_screen.Width / 2) - (helpSize.X / 2) + 3, ((_screen.Height / 2) + (_screen.Height / 3) + 40) - (helpSize.Y / 2) + 3), Color.GreenYellow);
            }         
        }


        void drawCredits(GameTime gameTime)
        {
            // < Background Objects > These are semi-static background objects for the backdrop of the screen
            Background.drawme(_spriteBatch);
            Planet.drawme(_spriteBatch);
            Moon.drawme(_spriteBatch);
            CreditProfiles.drawme(_spriteBatch);

            // < Credits Page Text > These blocks are for displaying the text for the page. each one has a colour line and a shadow line drawn behind it
            // ------- < Title >
            Vector2 titleSize = _bodyFont.MeasureString("Itch.io Asset Credits");
            _spriteBatch.DrawString(_bodyFont, "Itch.io Asset Credits", new Vector2((_screen.Width / 2) - (titleSize.X / 2), 25 - (titleSize.Y / 2)), Color.Black);
            _spriteBatch.DrawString(_bodyFont, "Itch.io Asset Credits", new Vector2((_screen.Width / 2) - (titleSize.X / 2) + 3, 25 - (titleSize.Y / 2) + 3), Color.GreenYellow);
            
            
            // ------- < Credits >
            Vector2 creditsSize = _secondaryBodyFont.MeasureString("Deep-Fold - Planet and Moon\n\n\n\n" +
                "Goose Ninja - Game Music\n\n\n\n" +
                "Penzilla - Humans\n\n\n\n" +
                "Foozle - Player & Alien Ships\n\n\n\n" +
                "PiiiXL - Background\n\n\n\n" +
                "Dream Mix - Button Prompts");
            
            _spriteBatch.DrawString
                (_secondaryBodyFont,
                "Deep-Fold - Planet and Moon\n\n\n\n" +
                "Goose Ninja - Game Music\n\n\n\n" +
                "Penzilla - Humans\n\n\n\n" +
                "Foozle - Player & Alien Ships\n\n\n\n" +
                "PiiiXL - Background\n\n\n\n" +
                "Dream Mix - Button Prompts", 
                new Vector2(50 + (_screen.Width / 2) - (creditsSize.X / 2), 290 - (creditsSize.Y / 2)), 
                Color.Black);

            _spriteBatch.DrawString(
                _secondaryBodyFont,
                "Deep-Fold - Planet and Moon\n\n\n\n" +
                "Goose Ninja - Game Music\n\n\n\n" +
                "Penzilla - Humans\n\n\n\n" +
                "Foozle - Player & Alien Ships\n\n\n\n" +
                "PiiiXL - Background\n\n\n\n" +
                "Dream Mix - Button Prompts",
                new Vector2(50 + (_screen.Width / 2) - (creditsSize.X / 2) + 3, 290 - (creditsSize.Y / 2) + 3), 
                Color.GreenYellow);
            
            
            // ------- < Return Prompt >
            Vector2 returnSize = _secondaryBodyFont.MeasureString("press 3 to RETURN");
            _spriteBatch.DrawString(_secondaryBodyFont, "press 3 to RETURN", new Vector2((_screen.Width / 2) - (returnSize.X / 2), ((_screen.Height / 2) + (_screen.Height / 3) + 60) - (returnSize.Y / 2)), Color.Black);
            _spriteBatch.DrawString(_secondaryBodyFont, "press 3 to RETURN", new Vector2((_screen.Width / 2) - (returnSize.X / 2) + 3, ((_screen.Height / 2) + (_screen.Height / 3) + 60) - (returnSize.Y / 2) + 3), Color.GreenYellow);
        }
    }
}



// < Game State > This is how we control the currnet screen that is displayed to the player
enum gameScreen
{
    title,
    game,
    help,
    highScores,
    credits
}