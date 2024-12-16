using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ALIENS__2
{
    // < Player > This is the vessal in which the player acts through the game enviroment
    internal class PlayerShip : MovingAnimated2D
    {
        // < Shooting State > We need this public so we can sync up the charging and firing of the weapeons across objects
        public ShootingState State
        {
            get { return _shootingState; }
        }

        // < Velocity > We need this public so we can add it to the world rotation when the player brushes up against the edge of the playspace
        public Vector2 Velocity
        {
            get { return _velocity; }
        }

        // < Rotation > We need this public so we know what angle to draw the lazer object at
        public float Rotation
        {
            get { return _rotation; }
        }

        // < Position > We need this public so we know if the player brushes up against the edge of the playspace, as well as correctly spawn the lazer object
        public Vector2 Position
        {
            get { return _position; }
        }

        // < Lazer Line Of Sight > This is used to check if the player has made a sucessful hit with the lazer beam
        public Vector2 LazerEndPos
        {
            get { return _targetPoint; }
        }

        private MovingState _movementState;
        private ShootingState _shootingState;


        // < New Varibles > most of the new varibles added here are needed for the multiple animated sprites used in the ship. however there are also new ones related to movement and weapeons
        private Texture2D _engineSprite, _engineEffectSpriteSheet, _weaponSpriteSheet;
        private Rectangle _engineEffectSourceRectangle, _weaponSourceRectangle;
        private float _weaponFPS, _weaponUpdateTrigger, _shotCooldown, _inertia, _thrust;
        private Vector2 _direction, _targetPoint;


        public PlayerShip(Texture2D baseSprite, Texture2D engineSprite, Texture2D engineEffectSpriteSheet, Texture2D weaponSpriteSheet, float fps, Rectangle SrcRect, Rectangle rect, float scale) : base(baseSprite, fps, SrcRect, rect, scale)
        {
            _texture = baseSprite;
            _engineSprite = engineSprite;
            _engineEffectSpriteSheet = engineEffectSpriteSheet;
            _weaponSpriteSheet = weaponSpriteSheet;


            _engineEffectSourceRectangle = new Rectangle(0, 0, _engineEffectSpriteSheet.Width / 4, _engineEffectSpriteSheet.Height / 2);
            _weaponSourceRectangle = new Rectangle(0, 0, _weaponSpriteSheet.Height , _weaponSpriteSheet.Height);


            _framesPerSecond = fps;
            _weaponFPS = 16;
         

            _rotation = -1.6f; // We set rotation to -1.6 to make it draw to the right upon spawning
            _inertia = 0.95f;
            _thrust = 0f;
            _direction = new Vector2(1, 0); // We set direction to (1, 0) to make it point to the right upon spawning


            _shootingState = ShootingState.Idle;
            _movementState = MovingState.Idle;
        }

        public void updateme(KeyboardState kb, MouseState cm,GameTime gt)
        {
            // < Counters > These are needed for the animation updates for the sprites as well as cooldowns for the ship's lazer beam
            _updateTrigger += (float)gt.ElapsedGameTime.TotalSeconds * _framesPerSecond;
            _weaponUpdateTrigger += (float)gt.ElapsedGameTime.TotalSeconds * _weaponFPS;
            _shotCooldown -= (float)gt.ElapsedGameTime.TotalSeconds;


            // < Movement > This block in order. gets the direction to draw the object -> moves velocity towards direction relitive to thrust -> add velocity to position -> apply slowing effect
            _rotation = (float)Math.Atan2(_velocity.Y, _velocity.X);
            _velocity += (_direction * _thrust) * 0.01f;       
            _position += _velocity;
            _velocity *= _inertia;

            // < Weapeon Targeting > This block takes the direction the player is pointing and places a second point far out in front of the ship in order to draw a line between them to act as lazer beam hit registration
            _targetPoint = _direction;
            _targetPoint.Normalize();
            _targetPoint *= 700;
            _targetPoint += _position;


            // < Ship Contols > Thes statments control the player ships actions
            // ------- < Moving > when the movement key is pressed we keep adding to our thrust and change our state to moving to change the thruster animation
            if (kb.IsKeyDown(Keys.W) || cm.RightButton == ButtonState.Pressed)
            {
                _movementState = MovingState.Moving;
                _thrust += 0.01f;
                _engineEffectSourceRectangle.Y = _engineEffectSpriteSheet.Height / 2;

                _direction = new Vector2(
                    cm.X - _position.X,
                    cm.Y - _position.Y);
            }
            // ------- < Idle > when we dont press the movement key is pressed we keep adding drag to our thrust to slow the ship and change our state to idle to change the thruster animation
            if (kb.IsKeyUp(Keys.W) && cm.RightButton == ButtonState.Released)
            {
                _movementState = MovingState.Idle;
                _thrust *= _inertia;
                _engineEffectSourceRectangle.Y = 0;
            }
            // ------- < Shooting > when we press the fire button we start the cooldown of the weapeon and change the shooting state so we can fire the weapeon
            if ((kb.IsKeyDown(Keys.Space) || cm.LeftButton == ButtonState.Pressed) && _shotCooldown <= 0f)
            {
                _shootingState = ShootingState.Shooting;
                _shotCooldown = 1;
            }



            // < Clamps > These blocks are needed to keep the player ship within the designated play area
            if (_thrust > 0.25f)
            {
                _thrust = 0.25f;
            }
            if (_position.X < 80 + _texture.Width)
            {
                _position.X = 80 + _texture.Width;
            }
            if (_position.X > 560 - _texture.Width)
            {
                _position.X = 560 - _texture.Width;
            }
            if (_position.Y < 0 + (_texture.Height / 2))
            { 
                _position.Y = 0 + (_texture.Height / 2);
            }
            if (_position.Y > 480 - _texture.Height)
            {
                _position.Y = 480 - _texture.Height;
            }



            // < Movement Animation > This block controls when the thruster animation frame on the back of the player ship updates
            if (_updateTrigger >= 1)
            {
                _updateTrigger = 0;

                _engineEffectSourceRectangle.X += _engineEffectSourceRectangle.Width;

                // < Animation Control > Because the animation sprite sheet for the thrusters have two lines of diferent lenghts, we need this block here to control when the animation resets depending on the movement state 
                if (_movementState == MovingState.Idle && _engineEffectSourceRectangle.X == _engineEffectSpriteSheet.Width - (_engineEffectSpriteSheet.Width / 4))
                {
                    _engineEffectSourceRectangle.X = 0;
                }
                if (_engineEffectSourceRectangle.X >= _engineEffectSpriteSheet.Width)
                {
                    _engineEffectSourceRectangle.X = 0;
                }
            }


            // < Shooting Animation > This block controls when the shooting animation frame on the player ship updates
            if (_weaponUpdateTrigger >= 1)
            {
                _weaponUpdateTrigger = 0;

                // ------- < Animation Control > This control block makes sure the shooting animation can only play while we are in a shooting state. after finishing it will set back to idle ready for the player
                if (_shootingState == ShootingState.Shooting)
                {
                    _weaponSourceRectangle.X += _weaponSourceRectangle.Width;
                }
                if (_weaponSourceRectangle.X >= _weaponSpriteSheet.Width)
                {
                    _weaponSourceRectangle.X = 0;
                    _shootingState = ShootingState.Idle;
                }
            }
        }

        public override void drawme(SpriteBatch sb) 
        {
            sb.Draw(_weaponSpriteSheet, _position, _weaponSourceRectangle, Color.White, _rotation + 1.6f, _origin, _scale, SpriteEffects.None, 0);
            sb.Draw(_engineEffectSpriteSheet, _position, _engineEffectSourceRectangle, Color.White, _rotation + 1.6f, _origin, _scale, SpriteEffects.None, 0);
            sb.Draw(_engineSprite, _position, _sourceRectangle, Color.White, _rotation + 1.6f, _origin, _scale, SpriteEffects.None, 0);
            sb.Draw(_texture, _position, _sourceRectangle, Color.White, _rotation + 1.6f, _origin, _scale, SpriteEffects.None, 0);
        }
    }



    class LazerBeam : MovingAnimated2D
    {
        // < Tint > We need tint public so we know when to remove it once it has completly faded 
        public float Tint
        {
            get { return _tint; }
        }

        private float _tint;


        public LazerBeam(Texture2D txr, float fps, Rectangle srcRect, Rectangle rect, float scale, float rotation, Vector2 playerPos) : base(txr, fps, srcRect, rect, scale)
        {
            _rotation = rotation + 1.6f;
            _position = playerPos;
            _framesPerSecond = fps;
            _scale = scale;

            _sourceRectangle = new Rectangle(0, 0, _texture.Width / 4, _texture.Height);
            _origin = new Vector2(_texture.Width / 4 - 32, _texture.Height);
            _tint = 1f;
        }


        public void updateme(GameTime gt, float rotation , Vector2 playerPos)
        {
            // < Counters > These are for both for the sprite animation as well as the fade out
            _updateTrigger += (float)gt.ElapsedGameTime.TotalSeconds * _framesPerSecond;
            _tint -= (float)gt.ElapsedGameTime.TotalSeconds * 2;

            _rotation = rotation + 1.6f; // We add 1.6 to the rotation because the asset spritesheet is pointing up rather that the default right, this corrects that
            _position = playerPos;

            if (_updateTrigger >= 1)
            {
                _updateTrigger = 0;

                _sourceRectangle.X += _sourceRectangle.Width;

                if (_sourceRectangle.X == _texture.Width)
                {
                    _sourceRectangle.X = 0;
                }
            }
        }


        public override void drawme(SpriteBatch sb)
        {
            sb.Draw(_texture, _position, _sourceRectangle, Color.White * _tint, _rotation, _origin, _scale, SpriteEffects.None, 0);
        }
    }



    enum MovingState
    {
        Idle,
        Moving
    }



    enum ShootingState
    {
        Idle,
        Shooting
    }
}
