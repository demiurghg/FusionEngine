using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Reflection;
using System.ComponentModel;

namespace Fusion.Core.Extensions {
	public static class Misc {

		/// <summary>
		/// https://en.wikibooks.org/wiki/Algorithm_Implementation/Strings/Longest_common_substring#Retrieve_the_Longest_Substring
		/// + special case when one of the args is null.
		/// </summary>
		/// <param name="str1"></param>
		/// <param name="str2"></param>
		/// <param name="sequence"></param>
		/// <returns></returns>
		public static int LongestCommonSubstring(string str1, string str2, out string sequence)
		{
			if (str1==null) {
				sequence = str2;
				return str2.Length;
			}
			if (str2==null) {
				sequence = str1;
				return str1.Length;
			}
			sequence = string.Empty;
			if (String.IsNullOrEmpty(str1) || String.IsNullOrEmpty(str2))
				return 0;

			int[,] num = new int[str1.Length, str2.Length];
			int maxlen = 0;
			int lastSubsBegin = 0;
			StringBuilder sequenceBuilder = new StringBuilder();

			for (int i = 0; i < str1.Length; i++)
			{
				for (int j = 0; j < str2.Length; j++)
				{
					if (str1[i] != str2[j])
						num[i, j] = 0;
					else
					{
						if ((i == 0) || (j == 0))
							num[i, j] = 1;
						else
							num[i, j] = 1 + num[i - 1, j - 1];

						if (num[i, j] > maxlen)
						{
							maxlen = num[i, j];
							int thisSubsBegin = i - num[i, j] + 1;
							if (lastSubsBegin == thisSubsBegin)
							{//if the current LCS is the same as the last time this block ran
								sequenceBuilder.Append(str1[i]);
							}
							else //this block resets the string builder if a different LCS is found
							{
								lastSubsBegin = thisSubsBegin;
								sequenceBuilder.Length = 0; //clear it
								sequenceBuilder.Append(str1.Substring(lastSubsBegin, (i + 1) - lastSubsBegin));
							}
						}
					}
				}
			}
			sequence = sequenceBuilder.ToString();
			return maxlen;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="type"></param>
		/// <returns></returns>
        public static object ConvertType (string value, Type type)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(type);

            return converter.ConvertFromInvariantString(value);
        }




		public static IList CreateList(Type type)
		{
			Type genericListType = typeof(List<>).MakeGenericType(type);
			return (IList)Activator.CreateInstance(genericListType);
		}


	
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		public static void Swap<T>(ref T lhs, ref T rhs)
		{
			T temp;
			temp = lhs;
			lhs = rhs;
			rhs = temp;
		}

		/// <summary>
		/// Same as Misc.Hash but without recursion and type reflection.
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public static int FastHash (params object[] args)
		{
			int hashCode = 0;
			unchecked {
				foreach( var arg in args ) {
					hashCode = (hashCode * 397) ^ arg.GetHashCode();
				}
			}
			return hashCode;
		}


		/// <summary>
		/// http://stackoverflow.com/questions/5450696/c-sharp-generic-hashcode-implementation-for-classes
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public static int Hash(params object[] args)
		{
			if (args == null)
			{
				return 0;
			}

			int num = 42;

			unchecked {
				foreach(var item in args) {

					if (ReferenceEquals(item, null)) { 
						//	...
					} else if (item is IEnumerable) {
						foreach (var subItem in (IEnumerable)item) {
							num = num * 37 + Hash(subItem);
						}
					}
					else {
						num = num * 37 + item.GetHashCode();
					}
				}
			}

			return num;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="targetObject"></param>
		/// <param name="propertyName"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool SetProperty<T> ( object targetObject, string propertyName, T value )
		{
			var type = targetObject.GetType();

			try {

				var pi = type.GetProperty( propertyName, typeof(T) );

				pi.SetValue( targetObject, value );

				return true;

			} catch ( Exception ) {
				return false;
			}
		}


		/// <summary>
		/// Copies property values from one object to another
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		public static void CopyPropertyValues(object source, object destination)
		{
			var destProperties = destination.GetType().GetProperties();

			foreach (var sourceProperty in source.GetType().GetProperties())
			{
				foreach (var destProperty in destProperties)
				{
					if (!destProperty.CanWrite) {
						continue;
					}

					if (destProperty.Name == sourceProperty.Name && destProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
					{
						destProperty.SetValue(destination, sourceProperty.GetValue(
							source, new object[] { }), new object[] { });

						break;
					}
				}
			}
		}



		/// <summary>
		/// Searches all loaded assemblies for all public subclasses of given type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static Type[] GetAllClassesWithAttribute<T>() where T : Attribute
		{	
			List<Type> types = new List<Type>();

			foreach ( var a in AppDomain.CurrentDomain.GetAssemblies() ) {

				foreach ( var t in a.GetTypes() ) {
					if (t.HasAttribute<T>()) {
						types.Add(t);						
					}
				}
			}

			return types.ToArray();
		}



		/// <summary>
		/// Searches all loaded assemblies for all public subclasses of given type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static Type[] GetAllSubclassesOf ( Type type, bool includeBaseClass = false )
		{	
			List<Type> types = new List<Type>();

			foreach ( var a in AppDomain.CurrentDomain.GetAssemblies() ) {

				foreach ( var t in a.GetTypes() ) {
					
					if (includeBaseClass && (t==type)) {
						types.Add(t);
					}

					if (t.IsSubclassOf( type )) {

						types.Add(t);						
					}
				}
			}

			return types.ToArray();
		}



		/// <summary>
		/// Saves object to Xml file 
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="type"></param>
		/// <param name="fileName"></param>
		static public string SaveObjectToXml ( object obj, Type type, Type[] extraTypes = null ) 
		{
			StringBuilder sb = new StringBuilder();
			
			XmlSerializer serializer = new XmlSerializer( type, extraTypes ?? new Type[0] );
			TextWriter textWriter = new StringWriter( sb );
			serializer.Serialize( textWriter, obj );
			textWriter.Close();

			return sb.ToString();
		}



		/// <summary>
		/// Loads object from Xml file
		/// </summary>
		/// <param name="type"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>
		static public object LoadObjectFromXml( Type type, string xmlText, Type[] extraTypes = null )
		{
			XmlSerializer serializer = new XmlSerializer( type, extraTypes ?? new Type[0] );

			using (TextReader textReader = new StringReader(xmlText) ) {
				object obj = serializer.Deserialize( textReader );
				return obj;
			}
		}




		/// <summary>
		/// Returns max enum value
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T MaxEnumValue<T>()
		{
			return (T)Enum.GetValues(typeof(T)).Cast<T>().Max();
		}
		  


		/// <summary>
		/// Build relative path from given full path, even wrong one :
		/// </summary>
		/// <param name="absolutePath"></param>
		/// <param name="relativeTo"></param>
		/// <returns></returns>
	    static public string RelativePath(string absolutePath, string relativeTo)
        {
            string[] absoluteDirectories = absolutePath.Split('\\');
            string[] relativeDirectories = relativeTo.Split('\\');

            //Get the shortest of the two paths
            int length = absoluteDirectories.Length < relativeDirectories.Length ? absoluteDirectories.Length : relativeDirectories.Length;

            //Use to determine where in the loop we exited
            int lastCommonRoot = -1;
            int index;

            //Find common root
            for (index = 0; index < length; index++)
                if (absoluteDirectories[index] == relativeDirectories[index])
                    lastCommonRoot = index;
                else
                    break;

            //If we didn't find a common prefix then throw
            if (lastCommonRoot == -1)
                throw new ArgumentException("Paths do not have a common base");

            //Build up the relative path
            StringBuilder relativePath = new StringBuilder();

            //Add on the ..
            for (index = lastCommonRoot + 1; index < absoluteDirectories.Length; index++)
                if (absoluteDirectories[index].Length > 0)
                    relativePath.Append("..\\");

            //Add on the folders
            for (index = lastCommonRoot + 1; index < relativeDirectories.Length - 1; index++)
                relativePath.Append(relativeDirectories[index] + "\\");
            relativePath.Append(relativeDirectories[relativeDirectories.Length - 1]);

            return relativePath.ToString();
        }

		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hex"></param>
		/// <returns></returns>
		public static byte[] HexStringToByte( string hex )
		{
			hex = hex.Replace("-", "");

			int numChars = hex.Length;
			byte[] bytes = new byte[numChars / 2];

			for (int i = 0; i < numChars; i += 2) {
				bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
			}
			return bytes;
		}



		/// <summary>
		/// Makes string signature from byte array. Example: [0xAA 0xBB] will be converted to "AA-BB".
		/// </summary>
		/// <param name="sig"></param>
		/// <returns></returns>
		public static string MakeStringSignature ( byte[] sig ) 
		{
			return BitConverter.ToString(sig);
		}
	}
}
