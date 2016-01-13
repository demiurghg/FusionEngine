﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Core.Content;
using Fusion.Engine.Common;


namespace Fusion.Core.Content {
	
	public class ContentManager : DisposableBase {

		class Item {
			public DateTime	LoadTime;
			public object	Object;
		}

		object lockObject =  new object();

		public readonly Game Game;
		Dictionary<string, Item> content;
		List<object> toDispose = new List<object>();
		List<ContentLoader> loaders;
		readonly string contentDirectory;



		/// <summary>
		/// Overloaded. Initializes a new instance of ContentManager. 
		/// </summary>
		/// <param name="game"></param>
		public ContentManager ( Game game, string contentDirectory = "Content" )
		{
			this.Game = game;

			this.contentDirectory = contentDirectory;

			content	=	new Dictionary<string,Item>();
			loaders	=	ContentLoader.GatherContentLoaders()
						.Select( clt => (ContentLoader)Activator.CreateInstance( clt ) )
						.ToList();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				Unload();
			}
			base.Dispose(disposing);
		}



		/// <summary>
		/// Gets path to cpecified content item. 
		/// Returns null if given object was not loaded by this content manager.
		/// </summary>
		/// <param name="contentItem"></param>
		/// <returns></returns>
		public string GetPathTo ( object contentItem )
		{
			return content
				.Where( c1 => c1.Value.Object == contentItem )
				.Select( c2 => c2.Key )
				.FirstOrDefault();
		}



		/// <summary>
		/// Determines whether the specified asset exists.
		/// </summary>
		/// <param name="assetPath"></param>
		/// <returns></returns>
		public bool Exists ( string assetPath )
		{
			if ( string.IsNullOrWhiteSpace(assetPath) ) {
				throw new ArgumentException("Asset path can not be null, empty or whitespace.");
			}

			return File.Exists( GetRealAssetFileName( assetPath ) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		string GetRealAssetFileName ( string assetPath )
		{
			if ( string.IsNullOrWhiteSpace(assetPath) ) {
				throw new ArgumentException("Asset path can not be null, empty or whitespace.");
			}

			//	take characters until dash '|' :
			assetPath = new string( assetPath.TakeWhile( ch => ch!='|' ).ToArray() );

			return Path.Combine(contentDirectory, ContentUtils.GetHashedFileName( assetPath, ".asset" ) );
		}



		/// <summary>
		/// Opens a stream for reading the specified asset.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public Stream OpenStream ( string assetPath )
		{
			var realName =  GetRealAssetFileName( assetPath );

			try {

				var stream	 =	AssetStream.OpenRead( realName );
				return stream;

			} catch ( IOException ioex ) {
				throw new IOException( string.Format("Could not open file: '{0}'\r\nHashed file name: '({1})'", assetPath, realName ), ioex );
			}
		}



		/// <summary>
		/// Gets loader for given type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		ContentLoader GetLoader( Type type )
		{
			// try get loader that absolutly meets desired type
			foreach ( var loader in loaders ) {
				if (loader.TargetType==type) {
					return loader;
				}
			}

			foreach ( var loader in loaders ) {
				if (type.IsSubclassOf( loader.TargetType ) ) {
					return loader;
				}
			}

			throw new ContentException( string.Format("Loader for type {0} not found", type ) );
		}



		/// <summary>
		/// Loads an asset that has been processed by the Content Pipeline.
		/// ContentManager.Unload will dispose all objects loaded by this method.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <returns></returns>
		public T Load<T> ( string assetPath )
		{
			if ( string.IsNullOrWhiteSpace(assetPath) ) {
				throw new ArgumentException("Asset path can not be null, empty or whitespace.");
			}

			Item item;

			//
			//	search for already loaded object :
			//
			lock (lockObject) {
				//	try to find object in dictionary :
				if ( content.TryGetValue( assetPath, out item ) ) {
				
					if (item.Object is T) {

						var time = File.GetLastWriteTime( GetRealAssetFileName( assetPath ) );

						if ( time > item.LoadTime ) {
							content.Remove(	assetPath );
						} else {
							return (T)item.Object;
						}
				
					} else {
						throw new ContentException( string.Format("'{0}' is not '{1}'", assetPath, typeof(T) ) );
					}
				}
			}


			ContentLoader loader	=	GetLoader( typeof(T) );


			//
			//	load content and add to dictionary :
			//
			Log.Message("Loading : {0}", assetPath );
			using (var stream = OpenStream(assetPath) ) {
				item = new Item() {
					Object		= loader.Load( this, stream, typeof(T), assetPath ),
					LoadTime	= File.GetLastWriteTime( GetRealAssetFileName( assetPath ) ),
				};
			}


			//
			//	check for content again and add it.
			//
			lock (lockObject) {
				Item anotherItem;

				//	put object to dispose list :
				toDispose.Add( item.Object );

				//	content item already loaded in another thread.
				if ( content.TryGetValue( assetPath, out anotherItem ) ) {
					if (item.Object is IDisposable) {
						(item.Object as IDisposable).Dispose();
					}

					return (T)anotherItem.Object;

				} else {
					content.Add( assetPath, item );
					return (T)item.Object;
				}
			}
		}



		/// <summary>
		/// This function resolves asset path using baseAssetPath and localAssetPath.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="baseAssetPath"></param>
		/// <param name="assetPath"></param>
		/// <returns></returns>
		public T Load<T>( string baseAssetPath, string localAssetPath )
		{
			var assetPath	=	Path.Combine( Path.GetDirectoryName( baseAssetPath ), localAssetPath );
			return Load<T>( assetPath );
		}


		/// <summary>
		/// Safe version of ContentManager.Load. If any exception occurs default object will be returned.
		/// ContentManager.Unload will not dispose default object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <param name="fallbackObject"></param>
		/// <returns></returns>
		public T Load<T>( string path, T defaultObject )
		{
			if ( string.IsNullOrWhiteSpace(path) ) {
				throw new ArgumentException("Asset path can not be null, empty or whitespace.");
			}

			try {
				return Load<T>(path);
			} catch ( Exception e ) {
				Log.Warning("Could not load {0} '{1}' : {2}", typeof(T).Name, path, e.Message);
				return defaultObject;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="baseAssetPath"></param>
		/// <param name="assetPath"></param>
		/// <returns></returns>
		public T Load<T>( string baseAssetPath, string localAssetPath, T defaultObject )
		{
			var assetPath	=	Path.Combine( Path.GetDirectoryName( baseAssetPath ), localAssetPath );
			return Load<T>( assetPath, defaultObject );
		}


		/// <summary>
		/// Safe version of ContentManager.Load. If any exception occurs it will try to load fallback object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <param name="fallbackObject"></param>
		/// <returns></returns>
		/*public T Load<T>( string path, string fallbackPath )
		{
			if ( string.IsNullOrWhiteSpace(path) ) {
				throw new ArgumentException("Asset path can not be null, empty or whitespace.");
			}

			try {
				return Load<T>(path);
			} catch ( Exception e ) {
				Log.Warning("Could not load {0} '{1}' : {2}", typeof(T).Name, path, e.Message);
				
				return Load<T>( fallbackPath );
			}
		}*/






		/// <summary>
		/// Disposes all data that was loaded by this ContentManager.
		/// </summary>
		public void Unload()
		{
			lock (lockObject) {
				Log.Message("Unloading content");

				foreach ( var item in toDispose ) {
					if ( item is DisposableBase ) {
						if ( (item as DisposableBase).IsDisposed ) {
							Log.Error("Item {0} has been disposed!", item.ToString() );
						}
					}
					if ( item is IDisposable ) {
						((IDisposable)item).Dispose();
					}
				}

				toDispose.Clear();
				content.Clear();
			}
		}
	}
}
