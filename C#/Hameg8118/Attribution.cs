namespace Hameg8118
{
    /// <summary>
    /// Struct collecting information about attribution
    /// </summary>
    public struct Attribution
    {
        // <CONSTRUCTORS>
        // </CONSTRUCTORS>

        // <METHODS>

        /// <summary>
        /// Creates a line with delimited attribution information
        /// </summary>
        /// <returns>String representation of this struct</returns>
        public override string ToString()
        {
            return InternalName + Global.Delimiter + OriginalName + Global.Delimiter + Author + Global.Delimiter + Licence + Global.Delimiter + File + Global.Delimiter + URL;
        }

        // </METHODS>

        // <PROPERTIES>

        public string InternalName { get; set; } // the name used by this application 
        public string OriginalName { get; set; } // the name that author of the attributed contribution used
        public string Author { get; set; } // name or another identifier of the author
        public string Licence { get; set; } // license name
        public string File { get; set; } // if the contribution was in a form of a file, the name of the file
        public string URL { get; set; } // link to author`s web

        // </PROPERTIES>

        // <EVENT HANDLERS>
        // </EVENT HANDLERS>
    }
}
