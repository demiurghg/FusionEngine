﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Core.Shell;
using Fusion.Core.Utils;
using Fusion.Engine.Common;
using Fusion.Engine.Client;
using Fusion.Engine.Server;
using Fusion.Engine.Graphics;
using Fusion.Engine.Audio;
using Fusion.Engine.Input;
using System.IO;


namespace ShooterDemo.Entities {
	class Player : GameEntity {

		public string ClientID {
			get; private set;
		}


		public Matrix World { get; set; }



		
		/// <summary>
		/// Default constructor
		/// </summary>
		public Player ()
		{
			var world				=	Matrix.Identity;
			world.TranslationVector	=	Vector3.Up * 4;
			World	=	world;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="origin"></param>
		public Player ( SpawnParameters parameters, string clientId )
		{
			var world				=	Matrix.Identity;
			world.TranslationVector	=	Vector3.Up * 4;
			World	=	world;

			this.ClientID	=	clientId;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update ( GameTime gameTime )
		{
			throw new NotImplementedException();
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		public override void Read ( BinaryReader reader )
		{
			ClientID	=	reader.ReadString();
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="writer"></param>
		public override void Write ( BinaryWriter writer )
		{
			writer.Write( ClientID );
		}

	}
}
