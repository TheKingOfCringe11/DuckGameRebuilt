#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2022 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;

using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Microsoft.Xna.Framework
{
	public class DrawableGameComponent : GameComponent, IDrawable
	{
		#region Public Properties

		public GraphicsDevice GraphicsDevice
		{
			get
			{
				return this.Game.GraphicsDevice;
			}
		}

		public int DrawOrder
		{
			get
			{
				return _drawOrder;
			}
			set
			{
				if (_drawOrder != value)
				{
					_drawOrder = value;
					if (DrawOrderChanged != null)
					{
						DrawOrderChanged(this, null);
					}
					OnDrawOrderChanged(this, null);
				}
			}
		}

		public bool Visible
		{
			get
			{
				return _visible;
			}
			set
			{
				if (_visible != value)
				{
					_visible = value;
					if (VisibleChanged != null)
					{
						VisibleChanged(this, EventArgs.Empty);
					}
					OnVisibleChanged(this, EventArgs.Empty);
				}
			}
		}

		#endregion

		#region Private Variables

		private bool _initialized;
		private int _drawOrder;
		private bool _visible = true;

		#endregion

		#region Public Constructors

		public DrawableGameComponent(Game game) : base(game)
		{
		}

		#endregion

		#region Events

		public event EventHandler<EventArgs> DrawOrderChanged;
		public event EventHandler<EventArgs> VisibleChanged;

		#endregion

		#region Public Methods

		public override void Initialize()
		{
			if (!_initialized)
			{
				_initialized = true;

				IGraphicsDeviceService graphicsDeviceService = (IGraphicsDeviceService)
					Game.Services.GetService(typeof(IGraphicsDeviceService));
				if (graphicsDeviceService != null)
				{
					if (graphicsDeviceService.GraphicsDevice != null)
					{
						LoadContent();
					}
					else
					{
						graphicsDeviceService.DeviceCreated += OnDeviceCreated;
					}
				}
			}
		}

		#endregion

		#region Protected Methods

		protected override void Dispose(bool disposing)
		{
			if (_initialized)
			{
				UnloadContent();
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Private Methods

		private void OnDeviceCreated(object sender, EventArgs e)
		{
			LoadContent();
		}

		#endregion

		#region Public Virtual Methods

		public virtual void Draw(GameTime gameTime)
		{
		}

		#endregion

		#region Protected Virtual Methods

		protected virtual void LoadContent()
		{
		}

		protected virtual void UnloadContent()
		{
		}

		protected virtual void OnVisibleChanged(object sender, EventArgs args)
		{
		}

		protected virtual void OnDrawOrderChanged(object sender, EventArgs args)
		{
		}

		#endregion
	}
}
