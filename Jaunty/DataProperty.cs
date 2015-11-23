
namespace Jaunty
{
    /// <summary>
    /// Contains metadata relating to the mapping of a property to a column
    /// </summary>
    public sealed class DataProperty
    {
        /// <summary>
        /// Gets the name of this property
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the column name this property is mapped to
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// Gets or sets the object name this property relates to (table or view)
        /// </summary>
        public string ObjectName { get; set; }
        /// <summary>
        /// Gets or sets the name of the database this property and object belongs in
        /// </summary>
        public string Database { get; set; }
        /// <summary>
        /// Gets or sets the schema this property belongs to
        /// </summary>
        public string Schema { get; set; }
        /// <summary>
        /// Gets or sets the order index for this property
        /// </summary>
        public int Order { get; set; }
        /// <summary>
        /// Gets or sets the type name for the data this property is mapped to
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// Gets or sets flags relating to how this property should be enumerated
        /// </summary>
        public PropertyFlags Flags { get; set; }

        /// <summary>
        /// Creates a new mapping for a property
        /// </summary>
        /// <param name="name">Name of the property</param>
        /// <param name="columnName">Column this property is mapped to</param>
        /// <param name="flags">Flags specifying behavior for the column</param>
        public DataProperty(string name, string columnName, PropertyFlags flags)
        {
            Name = name;
            ColumnName = columnName;
            Flags = flags;
        }

        /// <summary>
        /// Creates a mapping for a property
        /// </summary>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        public DataProperty(string name, PropertyFlags flags) : this(name, name, flags)
        {

        }

        /// <summary>
        /// Returns the column name of the property enclosed in square brackets
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ColumnName;
        }

        /// <summary>
        /// Gets the name of the property prepended with an at-sign
        /// </summary>
        /// <returns></returns>
        public string GetParameterName()
        {
            return string.Concat("@", Name);
        }

    }
}
