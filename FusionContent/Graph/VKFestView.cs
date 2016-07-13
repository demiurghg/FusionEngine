using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using City.ControlsClient.DomainClient.VKFest.JsonReader;
using City.Snapshot.Snapshot;
using City.UIFrames;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.Projections;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Fusion.Engine.Input;
using City.UIFrames.Converter;
using City.UIFrames.Converter.Models;
using City.UIFrames.FrameElement;
using City.Panel;
using City.UIFrames.Impl;
using Fusion.Engine.Graphics;

namespace City.ControlsClient.DomainClient
{
    public class VkFestView : AbstractPulseView<VkFestControl>
    {
		private PointsGisLayer globePostsVK;
        // UI Frame Post
        private int maxImageSize = 300;
        private List<WebPostFrame> frames;

		private Point previousMousePosition;
		private float maxAlpha = 0.6f;
		private Frame legendary_legend;

		private TilesGisLayer tiles;

        private GisLayerWrapper VkCurrentLayer;

        public VkFestView(VkFestControl instagramControl)
        {
            Control = instagramControl;
        }

        public override Frame AddControlsToUI()
        {
            if (!(Game.GameInterface is CustomGameInterface))
                return null;
            var ui = ((CustomGameInterface)Game.GameInterface).ui;
			
            //TODO new InstagramUI() -> extrat values
            var controlElements = Generator.getControlElement(new InstagramUI(), ui);
            return controlElements;
        }

        protected override void InitializeView()
        {
        }

        protected override void LoadView(ControlInfo controlInfo)
        {
            //Map
            ViewLayer.GlobeCamera.CameraDistance = GeoHelper.EarthRadius + 23;
			ViewLayer.GlobeCamera.GoToPlace(GlobeCamera.Places.SaintPetersburg_VO);

			ViewLayer.GlobeCamera.Parameters.MinCameraDistance = GeoHelper.EarthRadius + 1;
			ViewLayer.GlobeCamera.Parameters.MaxCameraDistance = GeoHelper.EarthRadius + 100;

			tiles = new TilesGisLayer(Game, ViewLayer.GlobeCamera) { IsVisible = true };
			tiles.SetMapSource(TilesGisLayer.MapSource.Dark);
			tiles.ZOrder = 2000;
            Layers.Add(new GisLayerWrapper(tiles));

			//Touch
			Game.Touch.Tap += p =>
			{
				bool clear = false;
				for (int i = 0; i < frames.Count; i++)
				{
					var f = frames[i];
					if (f.sendToHell)
					{
						f.Parent.Remove(f);
						f = null;
						clear = true;
					}
				}
				//if (clear) frames.Clear();
				onMouseClick(p.Position.X, p.Position.Y);
			};
			Game.Touch.Manipulate += OnManipulate;

            var frame = (Panel as GisPanel).Frame;
            frame.Click += (sender, args) => {
            	bool clear = false;
            	for (int i = 0; i < frames.Count; i++)
            	{
            		var f = frames[i];
            		if (f.sendToHell)
            		{
            			f.Parent.Remove(f);
            			f = null;
            			clear = true;
            		}
            	}
            	//if (clear) frames.Clear();
            	onMouseClick(args.X, args.Y);
            };

            //Data
            //VK
            initPosts();
            frames = new List<WebPostFrame>();
        }

        protected override ICommandSnapshot UpdateView(GameTime gameTime)
        {           
			if (Game.Keyboard.IsKeyDown(Keys.J))
            {
                frames.ForEach((x) => x.Parent.Remove(x));
            }
			updateCoorFrame();
			tiles.Update(gameTime);
            
            if (Control.newPostsVK != null)
            {
                Control.PostsVK.AddRange(Control.newPostsVK);
                initPosts();
            }
			return null;
        }

        private void initPosts()
        {
            var frame = (Panel as GisPanel).Frame;
            // Clear
            if (VkCurrentLayer != null)
            {
                Layers.Remove(VkCurrentLayer);
                frame.viewLayers.GisLayers.Remove(VkCurrentLayer.Layer);
            }
            if (Control.PostsVK == null || Control.PostsVK.Count == 0)
                return;

            // add new posts
            globePostsVK = new PointsGisLayer(Game, Control.PostsVK.Count)
            {
                ImageSizeInAtlas = new Vector2(128, 128),
                TextureAtlas = Game.Content.Load<Texture2D>("Train/station_circle.tga")
            };
            globePostsVK.ZOrder = 1000;
            var id = 0;
            foreach (var post in Control.PostsVK.FindAll(e=>e.geo!=null))
            {
                var coordinate = post.geo.coordinates.Split(' ');
                globePostsVK.PointsCpu[id] = new Gis.GeoPoint
                {
                    Lon = DMathUtil.DegreesToRadians(double.Parse(coordinate[1])),
                    Lat = DMathUtil.DegreesToRadians(double.Parse(coordinate[0])),
                    Color = new Color(255, 217, 0, maxAlpha),
                    Tex0 = new Vector4(0, 0, 0.05f, 0.0f)
                };
                id++;
            }
            VkCurrentLayer = new GisLayerWrapper(globePostsVK);
            frame.viewLayers.GisLayers.Add(VkCurrentLayer.Layer);
            Layers.Add(VkCurrentLayer);
            globePostsVK.UpdatePointsBuffer();

			if (frames != null)
			{
				for (int i = 0; i < frames.Count; i++)
				{
					var f = frames[i];
					f.Parent.Remove(f);
					f = null;
				}
				frames.Clear();				
			}
			var ui = ((CustomGameInterface)Game.GameInterface).ui;
			List<InstagramPost> instPost = new List<InstagramPost>();
			int idx = 0;
			if (Control.newPostsVK == null) return;
			foreach (var po in Control.newPostsVK)
			{

				var ins = new InstagramPost()
				{
					Likes = po?.likes?.count > 0 ? po.likes.count.ToString() : 0.ToString(),
					Text = po.text,
					TimeStamp = ConvertFromUnixTimestamp((double)po.date * 1000),
					Url = String.IsNullOrEmpty(po.photo_url) ? "" : po.photo_url,
				};
				instPost.Add(ins);
				var geopoint = globePostsVK.PointsCpu[globePostsVK.PointsCount - Control.newPostsVK.Count + idx];
				idx++;
				var cartesianCoor = GeoHelper.SphericalToCartesian(new DVector2(geopoint.Lon, geopoint.Lat));
				var screenPosition = ViewLayer.GlobeCamera.CartesianToScreen(cartesianCoor);
				Console.WriteLine(screenPosition);
				if (screenPosition.X < 0 || screenPosition.Y < 0)
				{
					continue;
				}
				var newFrame = FrameHelper.createWebPhotoFrameGeoTag(ui, (int)screenPosition.X, (int)screenPosition.Y, maxImageSize, maxImageSize, "inst_load", ins.Url, 0, instPost.ToArray(), geopoint);
				frames.Add(newFrame);
				frame.Add(newFrame);
			}

			// clear new posts
			Control.newPostsVK = null;
        }

        protected override void UnloadView()
        {
			//updateData();
		}

		private static DateTime ConvertFromUnixTimestamp(double timestamp)
		{
			DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
			return origin.AddMilliseconds(timestamp);
		}

		//action on mouse
		public void onMouseClick(int mouseX, int mouseY)
        {

            foreach (var f in frames)
			{
				if (f.GlobalRectangle.Contains(mouseX, mouseY)) return;
				Console.WriteLine("click on frame");
			}
			
			//if (frames.Count > 1) return;
			DVector2 mousePosition;

			ViewLayer.GlobeCamera.ScreenToSpherical( mouseX , mouseY, out mousePosition);
			var posts = new List<Post>();
			Gis.GeoPoint currentpost = new Gis.GeoPoint();

			if (globePostsVK != null && globePostsVK.IsVisible )
			{
				for (int i = 0; i < Control.PostsVK.Count; i++)
				{
					var post = globePostsVK.PointsCpu[i];
					if (post.Color.Alpha == 0) continue;
					var distance = GeoHelper.DistanceBetweenTwoPoints(mousePosition, new DVector2(post.Lon, post.Lat));
					if (distance < post.Tex0.Z)
					{
						if (!(Game.GameInterface is CustomGameInterface))
							return;

						posts.Add(Control.PostsVK[i]);
						currentpost = post;
					}
				}
			}			

			Log.Message(DMathUtil.RadiansToDegrees(mousePosition.X) + "");
			Log.Message(DMathUtil.RadiansToDegrees(mousePosition.Y) + "");

            var ui = ((CustomGameInterface)Game.GameInterface).ui;
            if (posts.Count > 0)
			{
				//for (int i = 0; i < frames.Count; i++)
				//{
				//	var f = frames[i];
				//	f.Parent.Remove(f);
				//	f = null;
				//}
				//frames.Clear();
                List<InstagramPost> instPost = new List<InstagramPost>();
			    foreach (var po in posts)
			    {
                    var ins = new InstagramPost()
                    {
                        Likes = po?.likes?.count > 0? po.likes.count.ToString() : 0.ToString(),
                        Text = po.text,
						TimeStamp = ConvertFromUnixTimestamp((double)po.date * 1000),
						Url = String.IsNullOrEmpty(po.photo_url) ? "" : po.photo_url,
					};
					instPost.Add(ins);
                }
				var frame = FrameHelper.createWebPhotoFrameGeoTag(ui, mouseX, mouseY, maxImageSize, maxImageSize, "inst_load", posts[0].photo_url, 0, instPost.ToArray(), currentpost);
				frames.Add(frame);
				var p = (Panel as GisPanel);
				p.Frame.Insert(0, frame);
			} 
			else
			{				
				for (int i = 0; i < frames.Count; i++)
				{
					var f = frames[i];
					f.Parent.Remove(f);
					f = null;
				}
				frames.Clear();
			}
		}

		string GetFileNameFromUrl(string url)
		{
			url = url.Replace(":", "");
			url = url.Replace("/", "");
			url = url.Replace(".", "");
			url = url.Remove(url.Length - 3);

			return url;
		}

//		public void changeSocialNetworksVisibility()
//        {
//			foreach (var c in rightPanel.socialFrameList.Children)
//			{
//				var checkbox = c as Checkbox;
//
//				switch (checkbox.Parent.Children.ToList().IndexOf(checkbox))
//				{
//					case 0:
//						globePostsVK.IsVisible = checkbox.IsChecked;
//						break;
//					case 1:
//						globePostsTwitter.IsVisible = checkbox.IsChecked;
//						break;
//					case 2:
//						globePostsInst.IsVisible = checkbox.IsChecked;
//						break;
//				};
//
//			}
//		}

//		public void changeSocialNetworksColor()
//		{
//			foreach (var c in rightPanel.filterColorFrameList.Children)
//			{
//				var checkbox = c as Checkbox;
//				if (!checkbox.IsChecked) continue;
//				switch (checkbox.Parent.Children.ToList().IndexOf(checkbox))
//				{
//					case 0:
//						{			
//							for (int i = 0; i < globePostsVK.PointsCount; i++)
//							{
//								globePostsVK.PointsCpu[i].Color = new Color(255, 217, 0);
//								globePostsVK.PointsCpu[i].Color.Alpha = maxAlpha;
//							};
//							for (int i = 0; i < globePostsInst.PointsCount; i++)
//							{
//								globePostsInst.PointsCpu[i].Color = new Color(255, 217, 0);
//								globePostsInst.PointsCpu[i].Color.Alpha = maxAlpha;
//							};
//							for (int i = 0; i < globePostsTwitter.PointsCount; i++)
//							{
//								globePostsTwitter.PointsCpu[i].Color = new Color(255, 217, 0);
//								globePostsTwitter.PointsCpu[i].Color.Alpha = maxAlpha;
//							};
//							globePostsVK.UpdatePointsBuffer();
//							globePostsInst.UpdatePointsBuffer();
//							globePostsTwitter.UpdatePointsBuffer();
//							legendary_legend.Visible = false;
//							legendImage.Visible = false;			
//							break;
//						}
//					case 1:
//						{
//							for (int i = 0; i < globePostsVK.PointsCount; i++)
//							{
//								globePostsVK.PointsCpu[i].Color = new Color(0, 105, 255);
//								globePostsVK.PointsCpu[i].Color.Alpha = maxAlpha;
//							};
//							for (int i = 0; i < globePostsInst.PointsCount; i++)
//							{
//								globePostsInst.PointsCpu[i].Color = new Color(250, 128, 40);
//								globePostsInst.PointsCpu[i].Color.Alpha = maxAlpha;
//							};
//							for (int i = 0; i < globePostsTwitter.PointsCount; i++)
//							{
//								globePostsTwitter.PointsCpu[i].Color = new Color(0, 255, 255);
//								globePostsTwitter.PointsCpu[i].Color.Alpha = maxAlpha;
//							};
//							globePostsVK.UpdatePointsBuffer();
//							globePostsInst.UpdatePointsBuffer();							
//							globePostsTwitter.UpdatePointsBuffer();
//							legendary_legend.Visible = true;
//							legendImage.Visible = true;
//							legendImage.Image = ((CustomGameInterface)Game.GameInterface).ui.Game.Content.Load<DiscTexture>(@"ui\Legend\lgnd_legend-socnet");
//							legendImage.Width = legendImage.Image.Width;
//							legendImage.Height = legendImage.Image.Height;
//							legendImage.X = (Panel as GisPanel).Frame.Width - rightPanel.Width - legendImage.Image.Width - 10 * ConstantFrameUI.gridUnits;
//							break;
//						}
//					case 2:
//						{
//							for (int i = 0; i < globePostsVK.PointsCount; i++)
//							{
//								var post = currentVKPosts[i];
//								globePostsVK.PointsCpu[i].Color.Alpha = 1;
//								if (post.Happiness > 0.5 || post.Polarity > 0)
//								{
//									globePostsVK.PointsCpu[i].Color = new Color(113, 185, 29);
//								}
//								else
//								{
//									if (post.Sadness > 0.5 || post.Polarity < 0)
//									{
//										globePostsVK.PointsCpu[i].Color = new Color(185, 38, 46);
//									} else
//									{
//										globePostsVK.PointsCpu[i].Color = new Color(165, 176, 184);
//										globePostsVK.PointsCpu[i].Color.Alpha = maxAlpha;
//									}
//								}
//							};
//							for (int i = 0; i < globePostsInst.PointsCount; i++)
//							{
//								var post = currentInstPosts[i];
//								globePostsInst.PointsCpu[i].Color.Alpha = 1;
//								if (post.Happiness > 0.5 || post.Polarity > 0)
//								{
//									globePostsInst.PointsCpu[i].Color = new Color(113, 185, 29);
//								}
//								else
//								{
//									if (post.Sadness > 0.5 || post.Polarity < 0)
//									{
//										globePostsInst.PointsCpu[i].Color = new Color(185, 38, 46);
//									}
//								}
//								
//
//								if (post.Neutral > 0.5 || (!(post.Happiness > 0.5 || post.Polarity > 0) && !(post.Sadness > 0.5 || post.Polarity < 0)))
//								{
//									globePostsInst.PointsCpu[i].Color = new Color(165, 176, 184);
//									globePostsInst.PointsCpu[i].Color.Alpha = maxAlpha;
//								}
//								
//							};
//							for (int i = 0; i < globePostsTwitter.PointsCount; i++)
//							{
//								var post = currentTwitterPosts[i];
//								globePostsTwitter.PointsCpu[i].Color.Alpha = 1;
//								if (post.Happiness > 0.5 || post.Polarity > 0)
//								{
//									globePostsTwitter.PointsCpu[i].Color = new Color(113, 185, 29);
//								}
//								else
//								{
//									if (post.Sadness > 0.5 || post.Polarity < 0)
//									{
//										globePostsTwitter.PointsCpu[i].Color = new Color(185, 38, 46);
//									}
//									else
//									{
//										globePostsTwitter.PointsCpu[i].Color = new Color(165, 176, 184);
//										globePostsTwitter.PointsCpu[i].Color.Alpha = maxAlpha;
//									}
//								}
//							};
//							globePostsVK.UpdatePointsBuffer();
//							globePostsInst.UpdatePointsBuffer();
//							globePostsTwitter.UpdatePointsBuffer();
//							legendary_legend.Visible = true;
//							legendImage.Visible = true;
//							legendImage.Image = ((CustomGameInterface)Game.GameInterface).ui.Game.Content.Load<DiscTexture>(@"ui\Legend\lgnd_legend-emot-mach");
//							legendImage.Width = legendImage.Image.Width;
//							legendImage.Height = legendImage.Image.Height;
//							legendImage.X = (Panel as GisPanel).Frame.Width - rightPanel.Width - legendImage.Image.Width - 10 * ConstantFrameUI.gridUnits;
//
//							break;
//						}
//					case 3:
//						{
//							for (int i = 0; i < globePostsVK.PointsCount; i++)
//							{
//								float values = currentVKPosts[i].User_Positive + currentVKPosts[i].User_Neutral + currentVKPosts[i].User_Negative;
//								if (values == 0)
//								{
//									globePostsVK.PointsCpu[i].Color = new Color(230, 190, 110);
//								}
//								else
//								{
//									globePostsVK.PointsCpu[i].Color = new Color(165, 176, 184);
//									if (currentVKPosts[i].User_Positive / values > 0.5)
//									{
//										globePostsVK.PointsCpu[i].Color = new Color(113, 185, 29);
//										globePostsVK.PointsCpu[i].Color.Alpha = 1;
//									}
//
//										if (currentVKPosts[i].User_Negative / values > 0.5)
//										{
//											globePostsVK.PointsCpu[i].Color = new Color(185, 38, 46);
//											globePostsVK.PointsCpu[i].Color.Alpha = 1;
//										}
//									
//								}								
//							};
//							for (int i = 0; i < globePostsInst.PointsCount; i++)
//							{
//								float values = currentInstPosts[i].User_Positive + currentInstPosts[i].User_Neutral + currentInstPosts[i].User_Negative;
//								if (values == 0)
//								{
//									globePostsInst.PointsCpu[i].Color = new Color(230, 190, 110);
//								}
//								else
//								{
//									globePostsInst.PointsCpu[i].Color =  new Color(165, 176, 184);
//									if (currentInstPosts[i].User_Positive / values > 0.5)
//									{
//										globePostsInst.PointsCpu[i].Color = new Color(113, 185, 29);
//										globePostsInst.PointsCpu[i].Color.Alpha = 1;
//									}
//									else
//									{
//										if (currentInstPosts[i].User_Negative / values > 0.5)
//										{
//											globePostsInst.PointsCpu[i].Color = new Color(185, 38, 46);
//											globePostsInst.PointsCpu[i].Color.Alpha = 1;
//										}
//									}
//								}								
//							};
//							for (int i = 0; i < globePostsTwitter.PointsCount; i++)
//							{
//								float values = currentTwitterPosts[i].User_Positive + currentTwitterPosts[i].User_Neutral + currentTwitterPosts[i].User_Negative;
//								if (values == 0)
//								{
//									globePostsTwitter.PointsCpu[i].Color = new Color(230, 190, 110);
//								} else
//								{
//									globePostsTwitter.PointsCpu[i].Color = new Color(165, 176, 184);
//									if ((float) (currentTwitterPosts[i].User_Positive / values) > 0.5)
//									{
//										globePostsTwitter.PointsCpu[i].Color = new Color(113, 185, 29);
//										globePostsTwitter.PointsCpu[i].Color.Alpha = 1;
//									}
//										if ((float) (currentTwitterPosts[i].User_Negative / values) > 0.5)
//										{
//											globePostsTwitter.PointsCpu[i].Color = new Color(185, 38, 46);
//											globePostsTwitter.PointsCpu[i].Color.Alpha = 1;
//										}
//									
//								}								
//							};
//							globePostsVK.UpdatePointsBuffer();
//							globePostsInst.UpdatePointsBuffer();
//							globePostsTwitter.UpdatePointsBuffer();
//							legendary_legend.Visible = true;
//							legendImage.Visible = true;
//							legendImage.Image = ((CustomGameInterface)Game.GameInterface).ui.Game.Content.Load<DiscTexture>(@"ui\Legend\lgnd_legend-emot-user");
//							legendImage.Width = legendImage.Image.Width;
//							legendImage.Height = legendImage.Image.Height;
//							legendImage.X = (Panel as GisPanel).Frame.Width - rightPanel.Width - legendImage.Image.Width - 10 * ConstantFrameUI.gridUnits;
//							break;
//						}
//				};
//			}
//		}

//		public void colorBySentiment()
//		{
//			for (int i = 0; i < currentVKPosts.Count; i++)
//			{
//				globePostsVK.PointsCpu[i].Color.Alpha = 0;
//			}
//
//			for (int i = 0; i < currentInstPosts.Count; i++)
//			{
//				globePostsInst.PointsCpu[i].Color.Alpha = 0;
//			}
//			for (int i = 0; i < currentTwitterPosts.Count; i++)
//			{
//				globePostsTwitter.PointsCpu[i].Color.Alpha = 0;
//			}
//			foreach (var c in rightPanel.filterFrameList.Children)
//			{		
//				var checkbox = c as Checkbox;
//				switch (checkbox.Parent.Children.ToList().IndexOf(checkbox))
//				{
//					case 0:
//						{							
//							for (int i = 0; i < currentVKPosts.Count; i++)
//							{
//								var post = currentVKPosts[i];
//								if (post.Happiness > 0.5 || post.Polarity > 0)
//								{
//									globePostsVK.PointsCpu[i].Color.Alpha = (checkbox.IsChecked) ? 1 : globePostsVK.PointsCpu[i].Color.Alpha;
//								}
//							}							
//							globePostsVK.UpdatePointsBuffer();
//							
//							for (int i = 0; i < currentInstPosts.Count; i++)
//							{
//								var post = currentInstPosts[i];
//								if (post.Happiness > 0.5 || post.Polarity > 0)
//								{
//									globePostsInst.PointsCpu[i].Color.Alpha = (checkbox.IsChecked) ? 1 : globePostsInst.PointsCpu[i].Color.Alpha;
//								}
//							}							
//							globePostsInst.UpdatePointsBuffer();
//
//							for (int i = 0; i < currentTwitterPosts.Count; i++)
//							{
//								var post = currentTwitterPosts[i];
//								if (post.Happiness > 0.5 || post.Polarity > 0)
//								{
//									globePostsTwitter.PointsCpu[i].Color.Alpha = (checkbox.IsChecked) ? 1 : globePostsTwitter.PointsCpu[i].Color.Alpha;
//								}
//							}
//							globePostsTwitter.UpdatePointsBuffer();
//							break;
//						}
//					case 1:
//						{							
//							for (int i = 0; i < currentInstPosts.Count; i++)
//							{
//								var post = currentInstPosts[i];
//								if ( post.Neutral > 0.5 || (!(post.Happiness > 0.5 || post.Polarity > 0) && !(post.Sadness > 0.5 || post.Polarity < 0)))
//								{
//									globePostsInst.PointsCpu[i].Color.Alpha = (checkbox.IsChecked) ? maxAlpha : globePostsInst.PointsCpu[i].Color.Alpha;
//								}
//							}
//							globePostsInst.UpdatePointsBuffer();
//							
//							
//							
//							for (int i = 0; i < currentVKPosts.Count; i++)
//							{
//								var post = currentVKPosts[i];
//								if (post.Neutral > 0.5 || (!(post.Happiness > 0.5 || post.Polarity > 0) && !(post.Sadness > 0.5 || post.Polarity < 0)))
//								{
//									 globePostsVK.PointsCpu[i].Color.Alpha = (checkbox.IsChecked) ? maxAlpha : globePostsVK.PointsCpu[i].Color.Alpha;
//								}
//							}
//							globePostsVK.UpdatePointsBuffer();
//
//							for (int i = 0; i < currentTwitterPosts.Count; i++)
//							{
//								var post = currentTwitterPosts[i];
//								if (post.Neutral > 0.5 || (!(post.Happiness > 0.5 || post.Polarity > 0) && !(post.Sadness > 0.5 || post.Polarity < 0)))
//								{
//									globePostsTwitter.PointsCpu[i].Color.Alpha = (checkbox.IsChecked) ? maxAlpha : globePostsTwitter.PointsCpu[i].Color.Alpha;
//								}
//							}
//							globePostsTwitter.UpdatePointsBuffer();
//							break;
//						}
//					case 2:
//						
//							for (int i = 0; i < currentVKPosts.Count; i++)
//							{
//								var post = currentVKPosts[i];
//								if (post.Sadness > 0.5 || post.Polarity < 0 && !(post.Happiness > 0.5 || post.Polarity > 0))
//								{
//									globePostsVK.PointsCpu[i].Color.Alpha = (checkbox.IsChecked) ? 1 : globePostsVK.PointsCpu[i].Color.Alpha;
//								}
//							}
//							globePostsVK.UpdatePointsBuffer();
//						
//						
//							for (int i = 0; i < currentInstPosts.Count; i++)
//							{
//								var post = currentInstPosts[i];
//								if (post.Sadness > 0.5 || post.Polarity < 0 && !(post.Happiness > 0.5 || post.Polarity > 0))
//								{
//									 globePostsInst.PointsCpu[i].Color.Alpha = (checkbox.IsChecked) ? 1 : globePostsInst.PointsCpu[i].Color.Alpha;
//								}
//							}
//							globePostsInst.UpdatePointsBuffer();
//
//						for (int i = 0; i < currentTwitterPosts.Count; i++)
//						{
//							var post = currentTwitterPosts[i];
//							if (post.Sadness > 0.5 || post.Polarity < 0 && !(post.Happiness > 0.5 || post.Polarity > 0)) 
//							{
//								globePostsTwitter.PointsCpu[i].Color.Alpha = (checkbox.IsChecked) ? 1 : globePostsTwitter.PointsCpu[i].Color.Alpha;
//							}
//						}
//						globePostsTwitter.UpdatePointsBuffer();
//
//						break;
//				};
//			}
//		}


//        private Frame CreateMapControls(FrameProcessor ui, int x, int y)
//		{
//			var listButton = new Frame(ui, x, y, 0, 0, "", Color.Zero)
//			{
//				Anchor = FrameAnchor.Right | FrameAnchor.Bottom
//			};
//			var textureSochi = Game.Content.Load<DiscTexture>(@"Sochi\cities-Sochi");
//			var textureAdler = Game.Content.Load<DiscTexture>(@"Sochi\cities-Adler");
//			var textureKrPol = Game.Content.Load<DiscTexture>(@"Sochi\cities-Krpol");
//			var texturePlus = Game.Content.Load<DiscTexture>(@"ui\map-btns-zoomin");
//			var textureMinus = Game.Content.Load<DiscTexture>(@"ui\map-btns-zoomout");
//
//			var listTexture = new[] { textureSochi, textureAdler, textureKrPol, texturePlus, textureMinus };
//
//			var listCityButton = new ListBox(ui, 0, 0, 0, 0, "", new Color(25, 25, 25, 255))
//			{
//				Anchor = FrameAnchor.Right | FrameAnchor.Bottom
//			};
//			listCityButton.addElement(FrameHelper.createButtonI(ui, 0, 0, ConstantFrameUI.mapButtonSize, ConstantFrameUI.mapButtonSize, "", textureSochi, textureSochi.Width, textureSochi.Height, Color.White, () => { ViewLayer.GlobeCamera.Pitch = -DMathUtil.DegreesToRadians(43.58191); ViewLayer.GlobeCamera.Yaw = DMathUtil.DegreesToRadians(39.749685); ViewLayer.GlobeCamera.CameraDistance = GeoHelper.EarthRadius + 27; }));
//			listCityButton.addElement(FrameHelper.createButtonI(ui, 0, 0, ConstantFrameUI.mapButtonSize, ConstantFrameUI.mapButtonSize, "", textureAdler, textureAdler.Width, textureAdler.Height, Color.White, () => { ViewLayer.GlobeCamera.Pitch = -DMathUtil.DegreesToRadians(43.417497); ViewLayer.GlobeCamera.Yaw = DMathUtil.DegreesToRadians(39.946433); ViewLayer.GlobeCamera.CameraDistance = GeoHelper.EarthRadius + 23; }));
//			listCityButton.addElement(FrameHelper.createButtonI(ui, 0, 0, ConstantFrameUI.mapButtonSize, ConstantFrameUI.mapButtonSize, "", textureKrPol, textureKrPol.Width, textureKrPol.Height, Color.White, () => { ViewLayer.GlobeCamera.Pitch = -DMathUtil.DegreesToRadians(43.668331); ViewLayer.GlobeCamera.Yaw = DMathUtil.DegreesToRadians(40.258084); ViewLayer.GlobeCamera.CameraDistance = GeoHelper.EarthRadius + 25; }));
//
//			var listMapButton = new ListBox(ui, 0, listCityButton.Y + listCityButton.Height +  offset * 12, 0, 0, "", new Color(25, 25, 25, 255))
//			{
//				Anchor = FrameAnchor.Right | FrameAnchor.Bottom
//			};
//			listMapButton.addElement(FrameHelper.createButtonI(ui, 0, 0, ConstantFrameUI.mapButtonSize, ConstantFrameUI.mapButtonSize, "", texturePlus, texturePlus.Width, texturePlus.Height, Color.White, () => { ViewLayer.GlobeCamera.CameraZoom(-0.3f); }));
//			listMapButton.addElement(FrameHelper.createButtonI(ui, 0, 0, ConstantFrameUI.mapButtonSize, ConstantFrameUI.mapButtonSize, "", textureMinus, textureMinus.Width, textureMinus.Height, Color.White, () => { ViewLayer.GlobeCamera.CameraZoom(0.3f); }));
//
//			listButton.Add(listCityButton);
//			listButton.Add(listMapButton);
//			listButton.Width = listCityButton.Width;
//			listButton.Height = listMapButton.Y + listMapButton.Height;
//			return listButton;
//		}

		private Frame activity_count;

		private Frame legendImage;
//		private void CreateIntefaceElements(FrameProcessor ui, int x, int y)
//		{
//			var frame = (Panel as GisPanel).Frame;
//			var texture = ui.Game.Content.Load<DiscTexture>("ui/help-btn");
//			var infoButton = new Button(ui, x, y, texture.Width, texture.Height, "", Color.Zero)
//			{
//				TextAlignment = Alignment.MiddleCenter,
//				Image = texture,
//				Anchor = FrameAnchor.Bottom | FrameAnchor.Left,
//			};
//			infoButton.Click += (s, a) =>
//			{
//				var infoPanel = new Button(ui, 0, 0, frame.Width, frame.Height, "", Color.Zero)
//				{
//					TextAlignment = Alignment.MiddleCenter,
//					HoverColor = new Color(ColorConstant.Biohazard.ToVector3(), 10),
//					BackColor = new Color(ColorConstant.Biohazard.ToVector3(), 50),
//				};
//				infoPanel.Click += (c, b) =>
//				{
//					infoPanel.Parent.Remove(infoPanel);
//				};
//				frame.Add(infoPanel);
//			};
//			//frame.Add(infoButton);
//
//
//			string activityString = "Пользовательских оценок:";
//			var activity_note = new Frame(ui, 8 * offset, 8 * offset, ConstantFrameUI.sfUltraLight32.MeasureString(activityString).Width, ConstantFrameUI.sfUltraLight32.LineHeight, activityString, Color.Zero)
//			{
//				Anchor = FrameAnchor.Top | FrameAnchor.Left,
//				Font = ConstantFrameUI.sfUltraLight32,
//				ForeColor = new Color(Color.White.ToVector3(), 0.5f),
//			};
//			frame.Add(activity_note);
//
//			activity_count = new Frame(ui, 8 * offset, activity_note.Y + activity_note.Height , 0, ConstantFrameUI.sfThin72.LineHeight, activityString, Color.Zero)
//			{
//				Anchor = FrameAnchor.Top | FrameAnchor.Left,
//				Font = ConstantFrameUI.sfThin72,
//				//Text = "" + evaluationNumber,
//			};
//			frame.Add(activity_count);
//
//			//var textureLegend = Game.Content.Load<DiscTexture>(@"ui\logo");
//			string legend = "Легенда:";
//			legendary_legend = new Frame(ui, (int)(frame.Width - rightPanel.Width - ConstantFrameUI.sfUltraLight32.MeasureString(legend).Width) - 10 * offset, offset * 8, ConstantFrameUI.sfUltraLight32.MeasureString(legend).Width, ConstantFrameUI.sfUltraLight32.MeasureString(legend).Height, legend, Color.Zero)
//			{
//				Anchor = FrameAnchor.Top | FrameAnchor.Right,
//				Font = ConstantFrameUI.sfUltraLight32,
//				ForeColor = new Color(Color.White.ToVector3(), 0.5f),
//			};
//
//			var image = ui.Game.Content.Load<DiscTexture>(@"ui\Legend\lgnd_legend-socnet");
//			legendImage = new Frame(ui, frame.Width - rightPanel.Width - image.Width - 10 * offset, legendary_legend.Y + legendary_legend.Height + offset * 8, image.Width, image.Height, "", Color.Zero)
//			{
//				Anchor = FrameAnchor.Top ,
//				Image = image,
//				ImageMode = FrameImageMode.Stretched,
//			};
//			frame.Add(legendary_legend);
//			frame.Add(legendImage);
//		}


		public void OnManipulate(TouchEventArgs p)
		{
			var f = ConstantFrameUI.GetHoveredFrame((Panel as GisPanel).Frame, p.Position);
			if (f != null)
			{
				ViewLayer.GlobeCamera.CameraZoom(MathUtil.Clamp(1.0f - p.ScaleDelta, -0.3f, 0.3f));

				if (p.IsEventEnd) return;

				if (p.IsEventBegin)
					previousMousePosition = p.Position;

				ViewLayer.GlobeCamera.MoveCamera(previousMousePosition, p.Position);
				previousMousePosition = p.Position;
			}
		}

		private void updateCoorFrame()
		{
            if(frames==null)
                return;
			foreach (var frame in frames)
			{
				var cartesianCoor = GeoHelper.SphericalToCartesian(new DVector2(frame.geoPoint.Value.Lon, frame.geoPoint.Value.Lat),
																		ViewLayer.GlobeCamera.EarthRadius);

				var screenPosition = ViewLayer.GlobeCamera.CartesianToScreen(cartesianCoor);
				frame.X = (int)screenPosition.X;
				frame.Y = (int)screenPosition.Y;
			}
		}
	}
}