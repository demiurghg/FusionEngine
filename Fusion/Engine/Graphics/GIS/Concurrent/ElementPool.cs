using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Engine.Graphics.GIS.Concurrent
{
	public class ElementPool<T>
	{
		class ElementWrapper
		{
			public enum ElementStates
			{
				NotLoaded,
				Loading,
				Loaded,
				Error
			} 

			public T		Element;
			public float	LifeTime;
			public int		LoadingTries;
			public ElementStates State;

			public ElementWrapper(T elem)
			{
				Element = elem;
				State	= ElementStates.Loading;
			} 
		}

		Dictionary<string, ElementWrapper> memoryPool;

		public Func<string, T>		CreateNew;
		public Action<T>	Load;
		public Action<T>	DisposeElement;


		public ElementPool()
		{
			
		}


		public T GetElement(string elementId)
		{
			if (memoryPool.ContainsKey(elementId)) {
				return memoryPool[elementId].Element;
			}

			var element = CreateNew(elementId);
			memoryPool[elementId] = new ElementWrapper(element);



			return element;
		}
	}
}
