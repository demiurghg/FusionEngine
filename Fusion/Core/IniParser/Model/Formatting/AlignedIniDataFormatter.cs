using System;
using System.Collections.Generic;
using System.Text;
using Fusion.Core.IniParser.Model.Configuration;
using System.Linq;

namespace Fusion.Core.IniParser.Model.Formatting
{
    
    public class AlignedIniDataFormatter : IIniDataFormatter
    {
        IniParserConfiguration _configuration;
        
        #region Initialization
        public AlignedIniDataFormatter():this(new IniParserConfiguration()) {}
        
        public AlignedIniDataFormatter(IniParserConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");
            this.Configuration = configuration;
        }
        #endregion
        
        public virtual string IniDataToString(IniData iniData)
        {
            var sb = new StringBuilder();

            if (Configuration.AllowKeysWithoutSection)
            {
                // Write global key/value data
				if (iniData.Global.Any()) {
					var maxChars = iniData.Global.Max( key => key.KeyName.Length );
					var frmtPrfx = "{0,-" + maxChars.ToString() + "}";
	
					WriteKeyValueData(iniData.Global, sb, frmtPrfx);
				}
            }

            //Write sections
            foreach (SectionData section in iniData.Sections)
            {
                //Write current section
                WriteSection(section, sb);
            }

            return sb.ToString();
        }
        
        /// <summary>
        ///     Configuration used to write an ini file with the proper
        ///     delimiter characters and data.
        /// </summary>
        /// <remarks>
        ///     If the <see cref="IniData"/> instance was created by a parser,
        ///     this instance is a copy of the <see cref="IniParserConfiguration"/> used
        ///     by the parser (i.e. different objects instances)
        ///     If this instance is created programatically without using a parser, this
        ///     property returns an instance of <see cref=" IniParserConfiguration"/>
        /// </remarks>
        public IniParserConfiguration Configuration
        {
            get { return _configuration; }
            set { _configuration = value.Clone(); }
        }

        #region Helpers

        private void WriteSection(SectionData section, StringBuilder sb)
        {
            // Write blank line before section, but not if it is the first line
            if (sb.Length > 0) sb.AppendLine();

            // Leading comments
            WriteComments(section.LeadingComments, sb);

            //Write section name
            sb.AppendLine(string.Format("{0}{1}{2}", Configuration.SectionStartChar, section.SectionName, Configuration.SectionEndChar));

			if (section.Keys.Any()) {
				var maxChars = section.Keys.Max( key => key.KeyName.Length );
				var frmtPrfx = "{0,-" + maxChars.ToString() + "}";

				WriteKeyValueData(section.Keys, sb, frmtPrfx);
			}

            // Trailing comments
            WriteComments(section.TrailingComments, sb);
        }

        private void WriteKeyValueData(KeyDataCollection keyDataCollection, StringBuilder sb, string frmtPrfx )
        {

            foreach (KeyData keyData in keyDataCollection)
            {
                // Add a blank line if the key value pair has comments
                if (keyData.Comments.Count > 0) sb.AppendLine();

                // Write key comments
                WriteComments(keyData.Comments, sb);

                //Write key and value
                sb.AppendLine(string.Format(frmtPrfx + "{3}{1}{3}{2}", keyData.KeyName, Configuration.KeyValueAssigmentChar, keyData.Value, Configuration.AssigmentSpacer));
            }
        }

        private void WriteComments(List<string> comments, StringBuilder sb)
        {
            foreach (string comment in comments)
                sb.AppendLine(string.Format("{0}{1}", Configuration.CommentString, comment));
        }
        #endregion
        
    }
    
} 