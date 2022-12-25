﻿using Microsoft.Xna.Framework;
using Nez.Sprites;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;
using Microsoft.Xna.Framework.Input;
using Nez.Tiled;


namespace Nez.Samples
{
	public class Caveman : Component, IUpdatable
	{
		public float MoveSpeed = 150;
		public float Gravity = 1000;
		public float JumpHeight = 16 * 5;

		SpriteAnimator _animator;
		TiledMapMover _mover;
		BoxCollider _boxCollider;
		TiledMapMover.CollisionState _collisionState = new TiledMapMover.CollisionState();
		Vector2 _velocity;
		ColliderTriggerHelper _triggerHelper;

		VirtualButton _jumpInput;
		VirtualIntegerAxis _xAxisInput;

		public override void OnAddedToEntity()
		{
			var texture = Entity.Scene.Content.LoadTexture("Content/Platformer/caveman.png");
			var sprites = Sprite.SpritesFromAtlas(texture, 32, 32);

			_boxCollider = Entity.GetComponent<BoxCollider>();
			_mover = Entity.GetComponent<TiledMapMover>();
			_animator = Entity.AddComponent(new SpriteAnimator(sprites[0]));

			// the TiledMapMover does not call ITriggerListener Methods on collision.
			// To achieve ITriggerListener calling, this ColliderTriggerHelper can be used.
			// See the Update() function below, to see how this helper can be used.
			_triggerHelper = new ColliderTriggerHelper(Entity);

			// extract the animations from the atlas. they are setup in rows with 8 columns
			_animator.AddAnimation("Walk", new[]
			{
				sprites[0],
				sprites[1],
				sprites[2],
				sprites[3],
				sprites[4],
				sprites[5]
			});

			_animator.AddAnimation("Run", new[]
			{
				sprites[8 + 0],
				sprites[8 + 1],
				sprites[8 + 2],
				sprites[8 + 3],
				sprites[8 + 4],
				sprites[8 + 5],
				sprites[8 + 6]
			});

			_animator.AddAnimation("Idle", new[]
			{
				sprites[16]
			});

			_animator.AddAnimation("Attack", new[]
			{
				sprites[24 + 0],
				sprites[24 + 1],
				sprites[24 + 2],
				sprites[24 + 3]
			});

			_animator.AddAnimation("Death", new[]
			{
				sprites[40 + 0],
				sprites[40 + 1],
				sprites[40 + 2],
				sprites[40 + 3]
			});

			_animator.AddAnimation("Falling", new[]
			{
				sprites[48]
			});

			_animator.AddAnimation("Hurt", new[]
			{
				sprites[64],
				sprites[64 + 1]
			});

			_animator.AddAnimation("Jumping", new[]
			{
				sprites[72 + 0],
				sprites[72 + 1],
				sprites[72 + 2],
				sprites[72 + 3]
			});

			SetupInput();
		}

		public override void OnRemovedFromEntity()
		{
			// deregister virtual input
			_jumpInput.Deregister();
			_xAxisInput.Deregister();
		}

		void SetupInput()
		{
			// setup input for jumping. we will allow z on the keyboard or a on the gamepad
			_jumpInput = new VirtualButton();
			_jumpInput.Nodes.Add(new VirtualButton.KeyboardKey(Keys.Z));
			_jumpInput.Nodes.Add(new VirtualButton.GamePadButton(0, Buttons.A));

			// horizontal input from dpad, left stick or keyboard left/right
			_xAxisInput = new VirtualIntegerAxis();
			_xAxisInput.Nodes.Add(new VirtualAxis.GamePadDpadLeftRight());
			_xAxisInput.Nodes.Add(new VirtualAxis.GamePadLeftStickX());
			_xAxisInput.Nodes.Add(new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.Left, Keys.Right));
		}

		void IUpdatable.Update()
		{
			// handle movement and animations
			var moveDir = new Vector2(_xAxisInput.Value, 0);
			string animation = null;

			if (moveDir.X < 0)
			{
				if (_collisionState.Below)
					animation = "Run";
				_animator.FlipX = true;
				_velocity.X = -MoveSpeed;
			}
			else if (moveDir.X > 0)
			{
				if (_collisionState.Below)
					animation = "Run";
				_animator.FlipX = false;
				_velocity.X = MoveSpeed;
			}
			else
			{
				_velocity.X = 0;
				if (_collisionState.Below)
					animation = "Idle";
			}

			if (_collisionState.Below && _jumpInput.IsPressed)
			{
				animation = "Jumping";
				_velocity.Y = -Mathf.Sqrt(2f * JumpHeight * Gravity);
			}

			if (!_collisionState.Below && _velocity.Y > 0)
				animation = "Falling";

			// apply gravity
			_velocity.Y += Gravity * Time.DeltaTime;

			// move
			_mover.Move(_velocity * Time.DeltaTime, _boxCollider, _collisionState);
			
			// Update the TriggerHelper. This will check if our collider intersects with a
			// trigger-collider and call ITriggerListener if necessary.
			_triggerHelper.Update();

			if (_collisionState.Below)
				_velocity.Y = 0;

			if (animation != null && !_animator.IsAnimationActive(animation))
				_animator.Play(animation);
		}
	}
}