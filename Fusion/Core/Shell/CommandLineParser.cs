﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.ComponentModel;
using Fusion.Core.Extensions;

namespace Fusion.Core.Shell
{
    /// <summary>
    /// http://blogs.msdn.com/b/shawnhar/archive/2012/04/20/a-reusable-reflection-based-command-line-parser.aspx
    /// </summary>
    public class CommandLineParser
    {
        readonly Type optionsObjectType;

        Queue<PropertyInfo> requiredOptions = new Queue<PropertyInfo>();
        Dictionary<string, PropertyInfo> optionalOptions = new Dictionary<string, PropertyInfo>();

        List<string> requiredUsageHelp = new List<string>();
        List<string> optionalUsageHelp = new List<string>();

		readonly string name;


		public IEnumerable<string> RequiredUsageHelp { get { return requiredUsageHelp; } }
		public IEnumerable<string> OptionalUsageHelp { get { return optionalUsageHelp; } }


		/// <summary>
		/// Optional argument leading char. Could be '/' or '-'.
		/// </summary>
		public char OptionLeadingChar { get; set; }

        
		/// <summary>
		/// 
		/// </summary>
		/// <param name="optionsObjectType"></param>
		/// <param name="throwException"></param>
        public CommandLineParser( Type optionsObjectType, string name = null )
        {
			OptionLeadingChar	=	'/';

			this.name = name ?? Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().ProcessName);

            // Reflect to find what commandline options are available.
            foreach (PropertyInfo field in optionsObjectType.GetProperties()) {

				if ( GetAttribute<IgnoreAttribute>(field) != null ) {
					continue;
				}

                string fieldName = GetOptionName(field);
				string fieldDesc = GetOptionDescription(field);

                if (GetAttribute<RequiredAttribute>(field) != null) {
                    // Record a required option.
                    requiredOptions.Enqueue(field);

                    requiredUsageHelp.Add(string.Format("<{0}>", fieldName));

                } else {
                    // Record an optional option.
                    optionalOptions.Add(fieldName.ToLowerInvariant(), field);
  
                    optionalUsageHelp.Add(string.Format("{0,-20}{1}", OptionLeadingChar + fieldName + GetValueString(field), fieldDesc + GetEnumValues(field)));
                }
            }
        }


		/// <summary>
		/// http://stackoverflow.com/questions/298830/split-string-containing-command-line-parameters-into-string-in-c-sharp/298990#298990
		/// </summary>
		/// <param name="commandLine"></param>
		/// <returns></returns>
		public static IEnumerable<string> SplitCommandLine(string commandLine)
		{
			bool inQuotes = false;

			return commandLine.Split(c =>
									 {
										 if (c == '\"')
											 inQuotes = !inQuotes;

										 return !inQuotes && c == ' ';
									 })
							  .Select(arg => arg.Trim().TrimMatchingQuotes('\"'))
							  .Where(arg => !string.IsNullOrEmpty(arg));
		}





		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		/// <param name="args"></param>
        public void PrintError(string message, params object[] args)
        {
            Log.Error(message, args);
            Log.Error("");
            Log.Error("Usage: {0} {1}", name, string.Join(" ", requiredUsageHelp));

            if (optionalUsageHelp.Count > 0)
            {
                Log.Error("");

                foreach (string optional in optionalUsageHelp)
                {
                    Log.Error("    {0}", optional);
                }

                Log.Error("");
            }
        }



		/// <summary>
		/// 
		/// </summary>
		/// <param name="optionsObject"></param>
		/// <param name="args"></param>
		/// <param name="errorString"></param>
		/// <returns></returns>
		public bool TryParseCommandLine ( object optionsObject, string[] args, out string errorMessage )
		{
			errorMessage = default(string);

            // Parse each argument in turn.
            foreach ( string arg in args ) {
                if ( !ParseArgument( optionsObject, arg.Trim(), ref errorMessage ) ) {
                    return false;
                }
            }

            // Make sure we got all the required options.
            PropertyInfo missingRequiredOption = requiredOptions.FirstOrDefault(field => !IsList(field) || GetList(optionsObject,field).Count == 0);

            if (missingRequiredOption != null) {
                errorMessage = string.Format("Missing argument '{0}' - {1}", GetOptionName(missingRequiredOption), GetOptionDescription(missingRequiredOption));
                return false;
            }

            return true;
		}



		/// <summary>
		/// Parses given args.
		/// In case of error return false and prints error message.
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
        public void ParseCommandLine(object optionsObject, string[] args)
        {
			string errorMessage;

			if (!TryParseCommandLine( optionsObject, args, out errorMessage )) {
				throw new CommandLineParserException( errorMessage );
			}
        }



		/// <summary>
		/// 
		/// </summary>
		/// <param name="arg"></param>
		/// <returns></returns>
        bool ParseArgument(object optionsObject, string arg, ref string errorMessage )
        {
            if (arg.StartsWith( new string(OptionLeadingChar,1) ))
            {
                // Parse an optional argument.
                char[] separators = { ':' };

                string[] split = arg.Substring(1).Split(separators, 2, StringSplitOptions.None);

                string name = split[0];
                string value = (split.Length > 1) ? split[1].TrimMatchingQuotes('\"') : "true";

                PropertyInfo field;

                if (!optionalOptions.TryGetValue(name.ToLowerInvariant(), out field))
                {
                    errorMessage = string.Format("Unknown option '{0}'", name);
                    return false;
                }

                return SetOption(optionsObject, field, value, ref errorMessage);
            }
            else
            {
                // Parse a required argument.
                if (requiredOptions.Count == 0) {
                    errorMessage	=	"Too many arguments";
                    return false;
                }

                PropertyInfo field = requiredOptions.Peek();

                if (!IsList(field)) {
                    requiredOptions.Dequeue();
                }

                return SetOption(optionsObject, field, arg, ref errorMessage);
            }
        }



		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="field"></param>
		/// <param name="value"></param>
		/// <returns></returns>
        bool SetOption(object optionsObject, PropertyInfo field, string value, ref string errorMessage )
        {
            try
            {
                if (IsList(field))
                {
                    // Append this value to a list of options.
                    GetList(optionsObject,field).Add(ChangeType(value, ListElementType(field)));
                }
                else
                {
                    // Set the value of a single option.
                    field.SetValue(optionsObject, ChangeType(value, field.PropertyType));
                }

                return true;
            }
            catch
            {
                errorMessage = string.Format("Invalid value '{0}' for option '{1}'", value, GetOptionName(field));
                return false;
            }
        }



		/// <summary>
		/// 
		/// </summary>
		/// <param name="pi"></param>
		/// <returns></returns>
		string GetValueString ( PropertyInfo pi ) 
		{
			if (pi.PropertyType == typeof(bool)) {
				return "";
			}

			if (pi.PropertyType == typeof(string)) {
				return ":string";
			}

			if (pi.PropertyType == typeof(float)) {
				return ":float";
			}

			if (pi.PropertyType == typeof(int)) {
				return ":int";
			}

			if (pi.PropertyType.IsEnum) {	
				return ":enum";
			}
			return ":value";
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="pi"></param>
		/// <returns></returns>
		string GetEnumValues ( PropertyInfo pi ) 
		{
			if (pi.PropertyType.IsEnum) {	
				return " [" + string.Join(" ", Enum.GetNames(pi.PropertyType).Select(s => s.ToString().ToLower()).ToArray() ) + "]";
			}
			return "";
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="type"></param>
		/// <returns></returns>
        static object ChangeType(string value, Type type)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(type);

            return converter.ConvertFromInvariantString(value);
        }



		/// <summary>
		/// 
		/// </summary>
		/// <param name="field"></param>
		/// <returns></returns>
        static bool IsList(PropertyInfo field)
        {
            return typeof(IList).IsAssignableFrom(field.PropertyType);
        }



		/// <summary>
		/// 
		/// </summary>
		/// <param name="field"></param>
		/// <returns></returns>
        IList GetList(object optionsObject, PropertyInfo field)
        {
            return (IList)field.GetValue(optionsObject);
        }



		/// <summary>
		/// 
		/// </summary>
		/// <param name="field"></param>
		/// <returns></returns>
        static Type ListElementType(PropertyInfo field)
        {
            var interfaces = from i in field.PropertyType.GetInterfaces()
                             where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                             select i;

            return interfaces.First().GetGenericArguments()[0];
        }



		/// <summary>
		/// 
		/// </summary>
		/// <param name="field"></param>
		/// <returns></returns>
        static string GetOptionName(PropertyInfo field)
        {
            var nameAttribute = GetAttribute<NameAttribute>(field);

            if (nameAttribute != null)
            {
                return nameAttribute.Name;
            }
            else
            {
                return field.Name;
            }
        }



		/// <summary>
		/// 
		/// </summary>
		/// <param name="field"></param>
		/// <returns></returns>
        static string GetOptionDescription(PropertyInfo field)
        {
            var nameAttribute = GetAttribute<NameAttribute>(field);

            if (nameAttribute != null)
            {
                return nameAttribute.Description;
            }
            else
            {
                return "";
            }
        }



		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="provider"></param>
		/// <returns></returns>
        static T GetAttribute<T>(ICustomAttributeProvider provider) where T : Attribute
        {
            return provider.GetCustomAttributes(typeof(T), false).OfType<T>().FirstOrDefault();
        }



		/// <summary>
        /// Used on optionsObject fields to indicate which options are required.
		/// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public sealed class IgnoreAttribute : Attribute
        { 
        }


		/// <summary>
        /// Used on optionsObject fields to indicate which options are required.
		/// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public sealed class RequiredAttribute : Attribute
        { 
        }



		/// <summary>
        /// Used on an optionsObject field to rename the corresponding commandline option.
		/// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public sealed class NameAttribute : Attribute
        {
            public NameAttribute(string name )
            {
                this.Name = name;
				this.Description = "";
            }

            public NameAttribute(string name, string description )
            {
                this.Name = name;
				this.Description = description;
            }

            public string Name { get; private set; }
            public string Description { get; private set; }
        }
    }
}
