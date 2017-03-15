namespace xml2xliff
{
    using System.Collections.Generic;
    using Localization.Xliff.OM;
    using Localization.Xliff.OM.Extensibility;

    /// <summary>
    /// This class stores SrcXPathExtension extension information that is registered on extensible objects.
    /// </summary>
    public class SrcXPathExtension : IExtension
    {
        /// <summary>
        /// The name associated with this handler.
        /// </summary>
        public const string ExtensionName = "ExtensionXSrcXPathHandler";

        /// <summary>
        /// The list of attribute members.
        /// </summary>
        private List<IExtensionAttribute> attributes;

        /// <summary>
        /// The list of element and text data members.
        /// </summary>
        private List<ElementInfo> children;

        /// <summary>
        /// Initializes a new instance of the <see cref="SrcXPathExtension"/> class.
        /// </summary>
        public SrcXPathExtension()
        {
            this.attributes = new List<IExtensionAttribute>();
            this.children = new List<ElementInfo>();
        }

        #region Properties
        /// <summary>
        /// Gets a value indicating whether the data contains attribute members.
        /// </summary>
        public bool HasAttributes
        {
            get { return this.attributes.Count > 0; }
        }

        /// <summary>
        /// Gets a value indicating whether the data contains element or text members.
        /// </summary>
        public bool HasChildren
        {
            get { return this.children.Count > 0; }
        }

        /// <summary>
        /// Gets the name of the extension.
        /// </summary>
        public string Name
        {
            get { return SrcXPathExtension.ExtensionName; }
        }
        #endregion Properties

        #region Methods
        /// <summary>
        /// Adds an attribute member to the extension data.
        /// </summary>
        /// <param name="attribute">The attribute to add.</param>
        public void AddAttribute(IExtensionAttribute attribute)
        {
            this.attributes.Add(attribute);
        }

        /// <summary>
        /// Adds an element or text member to the extension data.
        /// </summary>
        /// <param name="child">The child to add.</param>
        public void AddChild(ElementInfo child)
        {
            this.children.Add(child);
        }

        /// <summary>
        /// Gets the attribute members.
        /// </summary>
        /// <returns>An enumeration of attribute members.</returns>
        public IEnumerable<IExtensionAttribute> GetAttributes()
        {
            return this.attributes;
        }

        /// <summary>
        /// Gets the element and text members.
        /// </summary>
        /// <returns>An enumeration of element and text members.</returns>
        public IEnumerable<ElementInfo> GetChildren()
        {
            return this.children;
        }
        #endregion Methods
    }

    public class SrcXPathAttribute : IExtensionAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SrcXPathAttribute"/> class to store attribute information.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="value">The value of the attribute.</param>
        public SrcXPathAttribute(IExtensionNameInfo name, string value)
        {
            this.LocalName = name.LocalName;
            this.Namespace = name.Namespace;
            this.Prefix = name.Prefix;
            this.Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SrcXPathAttribute"/> class to store attribute information.
        /// </summary>
        /// <param name="prefix">The Xml prefix of the attribute.</param>
        /// <param name="ns">The namespace of the attribute.</param>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="value">The value of the attribute.</param>
        public SrcXPathAttribute(string prefix, string ns, string name, string value)
        {
            this.LocalName = name;
            this.Namespace = ns;
            this.Prefix = prefix;
            this.Value = value;
        }

        /// <summary>
        /// Gets the local name of the member.
        /// </summary>
        public string LocalName { get; private set; }

        /// <summary>
        /// Gets the namespace of the member.
        /// </summary>
        public string Namespace { get; private set; }

        /// <summary>
        /// Gets the Xml prefix of the member.
        /// </summary>
        public string Prefix { get; private set; }

        /// <summary>
        /// Gets the information related to a member that stores attribute or text information.
        /// </summary>
        public string Value { get; private set; }
    }
}
