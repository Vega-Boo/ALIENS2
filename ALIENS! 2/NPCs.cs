using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ALIENS__2
{
    class Alien : MovingAnimated2D
    {
        // < Hitbox > We need this pubic so we can check when the alien ship colides with a human on the ground
        public Rectangle Hitbox
        {
            get { return _rectangle; }
        }

        // < Abduction state > We need this public so we know if the alien ship that is off screen has picked up human
        public bool AbductionState
        {
            get { return _abductionState; }
        }

        // < Life State > We need this public so we know when to remove an alien object from the list after it has died
        public lifeState LifeState
        {
            get { return _lifeState; }
        }

        // < Distance > We need this public so we know if its far enought from the planet to count as a sucessful abduction
        public float DistanceFromPlanet
        {
            get { return _distanceToPlanet; }
        }

        private MovementState _movementState;
        private lifeState _lifeState;

        private Texture2D _deathSpriteSheet, _engineSpriteSheet, _warningSignTexture;
        private Rectangle _engineSourceRectangle, _deathSourceRectangle;
        private Vector2 _rotationPoint, _direction;
        private float _distanceToPlanet, _speed, _oldGlobalRotate, _localRotate, _oldLocalRotate;
        private Color _warningColor;
        private bool _abductionState;

        private static readonly Random RNG = new Random();

        public Alien(Texture2D spriteSheet, Texture2D deathSpriteSheet, Texture2D EngineSpriteSheet, Texture2D warningSignTexture, float fps, Rectangle ScrRect, Rectangle rect, float scale) : base(spriteSheet, fps, ScrRect, rect, scale)
        {
            _texture = spriteSheet;
            _engineSpriteSheet = EngineSpriteSheet;
            _deathSpriteSheet = deathSpriteSheet;
            _warningSignTexture = warningSignTexture;
            _framesPerSecond = fps;
            _sourceRectangle = ScrRect;
            _engineSourceRectangle = ScrRect;
            _deathSourceRectangle = ScrRect;
            _rectangle = rect;
            _scale = scale;

            
            _position = new Vector2(_rectangle.X, _rectangle.Y);
            _rotation = 1.53f;
            _rotationPoint = new Vector2(320, 880);
            _speed = 0.5f;          
            _warningColor = Color.Yellow;
            _localRotate = RNG.Next(0, 6) + (float)RNG.NextDouble(); // This random is what places them around the planet when they spawn


            _lifeState = lifeState.Alive;
            _movementState = MovementState.Moving;
        }


        public void updateme(GameTime gt, float globalRotate, bool abductionConfirm, Vector2 lazerStart, Vector2 LazerEnd)
        {
            // < Animation Trigger > This is used to keep the animation frames updating
            _updateTrigger += (float)gt.ElapsedGameTime.TotalSeconds * _framesPerSecond;


            // < Hitbox Update > This is to keep the alien hitbox inline whith its current position
            _rectangle.X = (int)_position.X - (_rectangle.Width / 2);
            _rectangle.Y = (int)_position.Y - (_rectangle.Height / 2);


            // < Abduction State > when the alien collides with a human we cange the state so we can start the second phase of the aliens life cycle
            if (abductionConfirm == true)
                _abductionState = true;


            // < Movement Rotation > this block keeps the alien ship in the correct position when we move around the planet
            _distanceToPlanet = (_rotationPoint - _position).Length();
            _position = rotateAroundPlanet(globalRotate);

           
            // < Alien Stages >
            // ------- < State 1 > Stage one we are traveling towards the planet, maing sure to turn off the engine when we collide with it
            if (_abductionState == false)
            {
                _rotation = pointToPlanet();

                if (_distanceToPlanet > 430)
                    _position += _direction * _speed;
                if (_distanceToPlanet <= 430)
                    _movementState = MovementState.Idle;
                if (_distanceToPlanet <= 425)
                    _position -= _direction * _speed;
            }
            // ------- < State 2 > For state two we are traveling in the opposite direction until we are off the edge of the screen
            if (_abductionState == true)
            {
                _movementState = MovementState.Moving;
                _position -= _direction * _speed;
                _rotation = pointAwayFromPlanet();
            }


            // < Lazer Hit Registration > This function checks whether if the alien sutatined a hit while the player is firing the lazer beam. if true is starts the dying process
            var collisions = LazerLOS.LineHitsRect(lazerStart, LazerEnd, _rectangle);
            if (collisions.Count > 0)
            {
                _lifeState = lifeState.Dying;
            }


            // < Thruster Animation > This is played on a loop whilst the alien is moving towards or away from the planet
            if (_lifeState == lifeState.Alive)
            {
                if (_updateTrigger >= 1)
                {
                    _updateTrigger = 0;
                    _engineSourceRectangle.X += _engineSourceRectangle.Width;

                    if (_engineSourceRectangle.X == _engineSpriteSheet.Width)
                    {
                        _engineSourceRectangle.X = 0;
                    }
                }
            }
            // < Death Animation > When the alien is hit by the lazer it plays the death animation here. when it finishes it swaps to dead and is removed
            if (_lifeState == lifeState.Dying)
            {
                if (_updateTrigger >= 1)
                {
                    _updateTrigger = 0;
                    _deathSourceRectangle.X += _deathSourceRectangle.Width;

                    if (_deathSourceRectangle.X == _deathSpriteSheet.Width)
                    {
                        _deathSourceRectangle.X = 0;
                        _lifeState = lifeState.Dead;
                    }
                }
            }
        }



        // < Moving To Planet > this function in needed to get the alien to travel in the direction of the planet
        private float pointToPlanet()
        {
            _direction = _rotationPoint - _position;
            _direction.Normalize();
            return (float)Math.Atan2(_direction.X, -_direction.Y);
        }
        // < Moving Away from Planet > this function in needed to get the alien to travel in the opposite direction of the planet
        private float pointAwayFromPlanet()
        {
            _direction = _rotationPoint - _position;
            _direction.Normalize();
            return (float)Math.Atan2(-_direction.X, _direction.Y);
        }
        // < Rotate Alien Position > This function is needed to acuratlly move the alien around the planet while retaining the same distance
        private Vector2 rotateAroundPlanet(float newGlobalRotate)
        {
            float globalRotate = (newGlobalRotate - _oldGlobalRotate) + (_localRotate - _oldLocalRotate);

            Vector2 newPosition = new Vector2(_position.X - _rotationPoint.X, _position.Y - _rotationPoint.Y);
            newPosition.X = (newPosition.X * (float)Math.Cos(globalRotate)) - (newPosition.Y * (float)Math.Sin(globalRotate));
            newPosition.Y = (newPosition.X * (float)Math.Sin(globalRotate)) + (newPosition.Y * (float)Math.Cos(globalRotate));
            newPosition = new Vector2(newPosition.X + _rotationPoint.X, newPosition.Y + _rotationPoint.Y);

            _oldGlobalRotate = newGlobalRotate;
            _oldLocalRotate = _localRotate;

            return newPosition;
        }


        public override void drawme(SpriteBatch sb)
        {

            if (_lifeState == lifeState.Alive)
            {
                if (_movementState == MovementState.Moving)
                    sb.Draw(_engineSpriteSheet, _position, _engineSourceRectangle, Color.White, _rotation, _origin, _scale, SpriteEffects.None, 0);
                sb.Draw(_texture, _position, _sourceRectangle, Color.White, _rotation, _origin, _scale, SpriteEffects.None, 0);
            }
            if (_lifeState == lifeState.Dying)
            {
                sb.Draw(_deathSpriteSheet, _position, _deathSourceRectangle, Color.White, _rotation, _origin, _scale, SpriteEffects.None, 0);
            }


            // < Warning Icon > This Draws the warning icon on the edge of the screen relitive to where the alien is off-screen
            if (_position.X < 0 || _position.X > 640 || _position.Y > 580 || _position.Y < 0)
                sb.Draw(
                    _warningSignTexture,
                    new Vector2(
                        MathHelper.Clamp(_position.X, 0, 640 - (int)(_warningSignTexture.Width * 0.08f)),
                        MathHelper.Clamp(_position.Y, 0, 540 - (int)(_warningSignTexture.Height * 0.08f))),
                    new Rectangle(0, 0, 534, 470),
                    _warningColor,
                    0, _origin,
                    0.08f,
                    SpriteEffects.None,
                    0);

            // < Warning Icon Change > This changes the warning icon's colour when the alien gets to certain stages indicating its severity 
            if (_movementState == MovementState.Idle)
                _warningColor = Color.OrangeRed;
            if (_abductionState == true)
                _warningColor = Color.Red;
        }
    }

    enum MovementState
    {
        Idle,
        Moving
    }

    enum lifeState
    {
        Alive,
        Dying,
        Dead
    }



    class Human : MovingAnimated2D
    {
        // < Hitbox > We need this public so we can check when this hitbox touches the alien hitbox
        public Rectangle Hitbox
        {
            get { return _rectangle; }
        }


        private Vector2 _pointDirection, _rotationPoint;
        private float _oldGlobalRotate, _localRotate, _oldLocalRotate, _distanceToPlanet, _moveCounter, _speed;
        private bool _isAirborn;

        private static readonly Random RNG = new Random();

        public Human(Texture2D texture, float fps, Rectangle SrcRect, Rectangle rect, float scale) : base(texture, fps, SrcRect, rect, scale)
        {
            _texture = texture;
            _framesPerSecond = fps;
            _sourceRectangle = SrcRect;
            _rectangle = rect;
            _scale = scale;


            _rotationPoint = new Vector2(320, 880);
            _localRotate = RNG.Next(0,6) + (float)RNG.NextDouble(); // This random is what places them around the planet when they spawn
            _moveCounter = RNG.Next(3, 15); // this dictates how long the human will walk in a given direction before changing


            // < Random Direction > This simply flips a coin to decide what direction the human will travel once it is spawned
            switch (RNG.Next(2))
            {
                case 0:
                    _speed = -0.001f;
                    _sourceRectangle.Y = 0;
                    break;
                case 1:
                    _speed = 0.001f;
                    _sourceRectangle.Y = 80;
                    break;
            }
        }



        public override void updateme(GameTime gt, float globalRotate)
        {
            _updateTrigger += (float)gt.ElapsedGameTime.TotalSeconds * _framesPerSecond;
            _moveCounter -= (float)gt.ElapsedGameTime.TotalSeconds;

            _rectangle = new Rectangle((int)_position.X - 5, (int)_position.Y - 5, 10, 10);
            _rotation = pointToPlanet();
            _position = rotateAroundPlanet(globalRotate);


            // < Movement Timer > This controls the toggle between the left and right movement once the counter reches zero
            if (_moveCounter <= 0)
            {
                _moveCounter = RNG.Next(3, 15);
                _speed *= -1;
                if (_speed < 0)
                    _sourceRectangle.Y = 0;
                if (_speed > 0)
                    _sourceRectangle.Y = 80;
            }
            if (_isAirborn == false)
            {
                _localRotate += _speed;
            }


            _distanceToPlanet = (_position - _rotationPoint).Length();


            // < Distance Clamp > This is so that humans dont stray too far into, or off the planet 
            if (_distanceToPlanet < 425)
            {
                _position -= _pointDirection * 1;
            }
            if (_distanceToPlanet > 430)
            {
                _position += _pointDirection * 1;
                _isAirborn = true;
            }
            else
            {
                _isAirborn = false;
            }



            // < Walking Animation > This is needed for the humans walking animation cycle
            if (_updateTrigger >= 1)
            {
                _updateTrigger = 0;

                if (_isAirborn == false)
                {
                    _sourceRectangle.X += _sourceRectangle.Width;
                }

                if (_sourceRectangle.X == _texture.Width)
                {
                    _sourceRectangle.X = 0;
                }
            }
        }



        // < Pointing To Planet > this function in needed to get the human have its feet always pointing at the ground
        private float pointToPlanet()
        {
            _pointDirection = _rotationPoint - _position;
            _pointDirection.Normalize();
            return (float)Math.Atan2(_pointDirection.X, -_pointDirection.Y);
        }
        // < Rotate Human Position > This function is needed to acuratlly move the Human around the planet while retaining the same distance
        private Vector2 rotateAroundPlanet(float newGlobalRotate)
        {
            float globalRotate = (newGlobalRotate - _oldGlobalRotate) + (_localRotate - _oldLocalRotate);

            Vector2 newPosition = new Vector2(_position.X - _rotationPoint.X, _position.Y - _rotationPoint.Y);
            newPosition.X = (newPosition.X * (float)Math.Cos(globalRotate)) - (newPosition.Y * (float)Math.Sin(globalRotate));
            newPosition.Y = (newPosition.X * (float)Math.Sin(globalRotate)) + (newPosition.Y * (float)Math.Cos(globalRotate));
            newPosition = new Vector2(newPosition.X + _rotationPoint.X, newPosition.Y + _rotationPoint.Y);

            _oldGlobalRotate = newGlobalRotate;
            _oldLocalRotate = _localRotate;

            return newPosition;
        }

        public override void drawme(SpriteBatch sb)
        {
            sb.Draw(_texture, _position, _sourceRectangle, Color.White, _rotation, _origin, _scale, SpriteEffects.FlipVertically, 0);
        }
    }

    enum isAirborn
    {
        True,
        False
    }
}

