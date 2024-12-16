using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ALIENS__2
{
    // < Screen Transition > This object is used to fade between game screens to make it more seemless
    class Transition : StaticGraphic
    {
        // < Opacity > this is public so we can check when to change screen once it is complete
        public float Opacity
        {
            get { return _opacity; }
        }

        private bool _transitionComplete = false;
        private float _opacity = 0.001f;

        public Transition(Rectangle rectPos, Texture2D txr) : base(rectPos, txr)
        {
            _rectangle = rectPos;
            _texture = txr;
        }

        public void updateMe(GameTime gt)
        {
            // < Transition Proscess > This block will increace the opacity until it blocks the screen, then it will lower it until it is back to zero
            if (_transitionComplete == false)
                _opacity += (float)gt.ElapsedGameTime.TotalSeconds;
            else if (_transitionComplete == true)
                _opacity -= (float)gt.ElapsedGameTime.TotalSeconds;
            if (_opacity > 1.1f)
                _transitionComplete = true;
        }

        public void drawMe(SpriteBatch sb)
        {
            sb.Draw(_texture, _rectangle, Color.Black * _opacity);
        }
    }






    // < Invasion Bar > This element is displayed in the players UI for the game screen to show how much "Invasion" has occured
    class InvasionBar : StaticGraphic
    {
        // < New Varibles > The only change for this object is the source rectangle. only however because a static animated would be too complicated
        private Rectangle _sourceRectangle;

        public InvasionBar(Rectangle rectPos, Texture2D txr) : base (rectPos, txr)
        {
            _texture = txr;
            _rectangle = new Rectangle(rectPos.X - (rectPos.Width / 2), rectPos.Y, rectPos.Width, rectPos.Height);
            _sourceRectangle = rectPos;
        }

        public void updateMe(GameTime gt, int invasionMeter) 
        {
            if (invasionMeter < 5)
                _sourceRectangle = new Rectangle(0 + (168 * invasionMeter), 0, 168, 28);
        }

        public void drawMe(SpriteBatch sb)       
        {
            sb.Draw(
                _texture, 
                new Vector2(_rectangle.X, _rectangle.Y), 
                _sourceRectangle, 
                Color.White, 
                0, 
                Vector2.Zero, 
                1,
                SpriteEffects.None, 
                0f);
        }
    }






    // < Moving Prompt Keys > These objects are used when we need to change the position of the prompts during the runtime
    class movingPromptKeys : StaticAnimated2D
    {
        public movingPromptKeys(Texture2D spriteSheet, float fps, Rectangle SrcRect, Rectangle rect, float scale) : base(spriteSheet, fps, SrcRect, rect, scale)
        {
            _rectangle = rect;
            _position = new Vector2(rect.X, rect.Y);
            _sourceRectangle = new Rectangle(0, 0, SrcRect.Width, SrcRect.Height);
            _origin = new Vector2(SrcRect.Width / 2, SrcRect.Height / 2);
            _updateTrigger = 0;
            _framesPerSecond = fps;
            _scale = scale;
        }

        public void updateMe(GameTime gt, int selection)
        {
            // < Animation Updates > this statement runs through every update cycle checking if its ready to change the animation frame on the object
            _updateTrigger += (float)gt.ElapsedGameTime.TotalSeconds * _framesPerSecond;
            if (_updateTrigger >= 1)
            {
                _updateTrigger = 0;
                _sourceRectangle.X += _sourceRectangle.Width;

                // ------- < Animation Reset > once the animation reaches the end of the sheet it will be set back to the start
                if (_sourceRectangle.X >= _texture.Width)
                {
                    _sourceRectangle.X = 0;
                }
            }



            // < Position Clamp > this fixes the position of the object to specifyed locations that we can control from the main game screen
            if (selection == 0)
                _position.X = 255;
            if (selection == 1)
                _position.X = 320;
            if (selection == 2)
                _position.X = 385;
        }

        public void drawMe(SpriteBatch sb) 
        {
            _rectangle.X = (int)_position.X;
            _rectangle.Y = (int)_position.Y;

            sb.Draw(_texture, new Vector2(_rectangle.X, _rectangle.Y), _sourceRectangle, Color.White, 0, _origin, _scale, SpriteEffects.None, 0);
        }
    }







    [Serializable]
    public struct HighScoreData
    {
        public string[] PlayerName;
        public int[] Score;
        public int Count;


        public HighScoreData(int count)
        {
            PlayerName = new string[count];
            Score = new int[count];

            Count = count;
        }
    }



    class HighscoreManager
    {
        private HighScoreData _data;
        private string _filename;


        public HighScoreData Data
        { 
            get { return _data; } 
        }


        public HighscoreManager(string filename, int tableSize)
        {
            _filename = filename;
            _data = new HighScoreData(tableSize);

            // Check to see if the save exists
            if (!File.Exists(_filename))
            {
                setDefaultHighScores();
            }
            else
                LoadHighScores();
        }



        ~HighscoreManager()
        {
            // When the scoreManager get destroyed, save the Scores
            SaveHighScores();
        }


        // Just fill the table will some default values.
        private void setDefaultHighScores()
        {
            //If the file doesn't exist, make a "starter" one...
            // Create some data to save
            _data.PlayerName[0] = "###";
            _data.Score[0] = 0;

            _data.PlayerName[1] = "###";
            _data.Score[1] = 0;

            _data.PlayerName[2] = "###";
            _data.Score[2] = 0;

            _data.PlayerName[3] = "###";
            _data.Score[3] = 0;

            _data.PlayerName[4] = "###";
            _data.Score[4] = 0;

            _data.PlayerName[5] = "###";
            _data.Score[5] = 0;

            _data.PlayerName[6] = "###";
            _data.Score[6] = 0;

            _data.PlayerName[7] = "###";
            _data.Score[7] = 0;

            _data.PlayerName[8] = "###";
            _data.Score[8] = 0;

            _data.PlayerName[9] = "###";
            _data.Score[9] = 0;
        }



        // Tries to add a High Score - if the score isn't good enough, it won't add it
        public void MaybeAddHighScore(string playername, int score)
        {
            // We're using -1 to represent a "not found" position - this can happen if the score is LESS than all other scores in the table.
            int scoreIndex = -1;

            // Go through the table from the top (highest entry)
            for (int i = 0; i < _data.Count; i++)
            {
                // If the new score we've just been given is GREATER THAN the entry at the current position, we've found where the score should go.
                if (score > _data.Score[i])
                {
                    scoreIndex = i;     // So remember that position
                    break;              // and stop looking 
                }
            }

            // If an entry point was found (scoreIndex is not equal to -1)
            if (scoreIndex != -1)
            {
                //New high score should be inserted at "scoreIndex", so let's shunt everything below that down one. 
                for (int i = _data.Count - 1; i > scoreIndex; i--)
                {
                    _data.PlayerName[i] = _data.PlayerName[i - 1];
                    _data.Score[i] = _data.Score[i - 1];
                }

                // The insert the new score into the "scoreIndex" position
                _data.PlayerName[scoreIndex] = playername;
                _data.Score[scoreIndex] = score;
            }
        }


        // < INT to CHARACTER > this block is used to turn a number into a corasponding letter. 1->A, 4->D, 10->J, Ect.
        public string decodePlayerName(int[] playerNameINT)
        {
            string[] playerNameSTRING;
            playerNameSTRING = new string[playerNameINT.Length];
            string playerNameFULL = "";


            // < Decoder > We pull in the in list produced in the inital enter page of the game and change each number in the list to a string
            // < NOTE > I tried looking for some kind of already built in function to help me, but couldnt find anything so I did this.
            for (int i = 0; i < playerNameINT.Length; i++)
            {
                if (playerNameINT[i] == 1)
                    playerNameSTRING[i] = "a";
                if (playerNameINT[i] == 2)
                    playerNameSTRING[i] = "b";
                if (playerNameINT[i] == 3)
                    playerNameSTRING[i] = "c";
                if (playerNameINT[i] == 4)
                    playerNameSTRING[i] = "d";
                if (playerNameINT[i] == 5)
                    playerNameSTRING[i] = "e";
                if (playerNameINT[i] == 6)
                    playerNameSTRING[i] = "f";
                if (playerNameINT[i] == 7)
                    playerNameSTRING[i] = "g";
                if (playerNameINT[i] == 8)
                    playerNameSTRING[i] = "h";
                if (playerNameINT[i] == 9)
                    playerNameSTRING[i] = "i";
                if (playerNameINT[i] == 10)
                    playerNameSTRING[i] = "j";
                if (playerNameINT[i] == 11)
                    playerNameSTRING[i] = "k";
                if (playerNameINT[i] == 12)
                    playerNameSTRING[i] = "l";
                if (playerNameINT[i] == 13)
                    playerNameSTRING[i] = "m";
                if (playerNameINT[i] == 14)
                    playerNameSTRING[i] = "n";
                if (playerNameINT[i] == 15)
                    playerNameSTRING[i] = "o";
                if (playerNameINT[i] == 16)
                    playerNameSTRING[i] = "p";
                if (playerNameINT[i] == 17)
                    playerNameSTRING[i] = "q";
                if (playerNameINT[i] == 18)
                    playerNameSTRING[i] = "r";
                if (playerNameINT[i] == 19)
                    playerNameSTRING[i] = "s";
                if (playerNameINT[i] == 20)
                    playerNameSTRING[i] = "t";
                if (playerNameINT[i] == 21)
                    playerNameSTRING[i] = "u";
                if (playerNameINT[i] == 22)
                    playerNameSTRING[i] = "v";
                if (playerNameINT[i] == 23)
                    playerNameSTRING[i] = "w";
                if (playerNameINT[i] == 24)
                    playerNameSTRING[i] = "x";
                if (playerNameINT[i] == 25)
                    playerNameSTRING[i] = "y";
                if (playerNameINT[i] == 26)
                    playerNameSTRING[i] = "z";
            }
            // < Reassemble > this is where we reassembe the output into one string for display to the player and leaderboard
            playerNameFULL = playerNameSTRING[0] + playerNameSTRING[1] + playerNameSTRING[2];
            return playerNameFULL;
        }


        public void SaveHighScores()
        {
            FileStream stream;

            try // "try" means literally "try to do this section of code, but don't crash if it doesn't work, just skip down to "finally" instead.
            {
                // Open the file, creating it if necessary
                stream = File.Open(_filename, FileMode.OpenOrCreate);

                // Convert the object to XML data and put it in the stream
                XmlSerializer serializer = new XmlSerializer(typeof(HighScoreData));
                serializer.Serialize(stream, _data);

                // Close the file
                stream.Close();
            }
            catch (Exception error) // The code in "catch" is what happens if the "try" fails.
            {
                // You could add a message to your game letting the player know their scores cannot be saved.
                // We're just going to output some debug text though.
                Debug.WriteLine("Save has failed because of: " + error.Message);
            }
        }

        public void LoadHighScores()
        {
            FileStream stream;

            try
            {
                // Open the file - but read only mode!
                stream = File.Open(_filename, FileMode.OpenOrCreate, FileAccess.Read);
                // Read the data from the file
                XmlSerializer serializer = new XmlSerializer(typeof(HighScoreData));
                _data = (HighScoreData)serializer.Deserialize(stream);
            }
            catch (Exception error) // The code in "catch" is what happens if the "try" fails.
            {
                // You could add a message to your game letting the player know their scores cannot be saved.
                // We're just going to setup the defaults and output some debug text.
                setDefaultHighScores();
                Debug.WriteLine("Load has failed because of: " + error.Message);
            }
        }
    }







    public static class LazerLOS
    {
        /// Checks for an intersection point between two lines (l1 and l2) defined by their start and end points
        /// <returns>null if there wasn't an intersection, has a value if there was</returns>
        public static Vector2? LineIntersection(Vector2 l1Start, Vector2 l1End, Vector2 l2Start, Vector2 l2End)
        {
            // calculate the distance to intersection point
            var uA = ((l2End.X - l2Start.X) * (l1Start.Y - l2Start.Y) - (l2End.Y - l2Start.Y) * (l1Start.X - l2Start.X)) / ((l2End.Y - l2Start.Y) * (l1End.X - l1Start.X) - (l2End.X - l2Start.X) * (l1End.Y - l1Start.Y));
            var uB = ((l1End.X - l1Start.X) * (l1Start.Y - l2Start.Y) - (l1End.Y - l1Start.Y) * (l1Start.X - l2Start.X)) / ((l2End.Y - l2Start.Y) * (l1End.X - l1Start.X) - (l2End.X - l2Start.X) * (l1End.Y - l1Start.Y));

            // if uA and uB are between 0-1, lines are colliding
            if (uA >= 0 && uA <= 1 && uB >= 0 && uB <= 1)
            {
                // calculate the intersection point
                var intersectionPoint = new Vector2();
                intersectionPoint.X = l1Start.X + (uA * (l1End.X - l1Start.X));
                intersectionPoint.Y = l1Start.Y + (uA * (l1End.Y - l1Start.Y));

                return intersectionPoint;
            }
            return null;
        }



        /// Uses line intersection of the 4 border lines to find out if a line defined by its start and end points intersect a rectangle.
        /// <returns>A list of the points of intersection</returns>
        public static List<Vector2> LineHitsRect(Vector2 lineStart, Vector2 lineEnd, Rectangle rect)
        {
            var contactPoints = new List<Vector2>();

            // Check the left wall
            var line2Start = new Vector2(rect.Left, rect.Top);
            var line2End = new Vector2(rect.Left, rect.Bottom);
            var crossPoint = LineIntersection(lineStart, lineEnd, line2Start, line2End);
            if (crossPoint.HasValue)
                contactPoints.Add(crossPoint.Value);

            // Check the top wall
            line2Start = new Vector2(rect.Left, rect.Top);
            line2End = new Vector2(rect.Right, rect.Top);
            crossPoint = LineIntersection(lineStart, lineEnd, line2Start, line2End);
            if (crossPoint.HasValue)
                contactPoints.Add(crossPoint.Value);

            // Check the bottom wall
            line2Start = new Vector2(rect.Left, rect.Bottom);
            line2End = new Vector2(rect.Right, rect.Bottom);
            crossPoint = LineIntersection(lineStart, lineEnd, line2Start, line2End);
            if (crossPoint.HasValue)
                contactPoints.Add(crossPoint.Value);

            // Check the right wall
            line2Start = new Vector2(rect.Right, rect.Top);
            line2End = new Vector2(rect.Right, rect.Bottom);
            crossPoint = LineIntersection(lineStart, lineEnd, line2Start, line2End);
            if (crossPoint.HasValue)
                contactPoints.Add(crossPoint.Value);

            return contactPoints;
        }
    }
}
