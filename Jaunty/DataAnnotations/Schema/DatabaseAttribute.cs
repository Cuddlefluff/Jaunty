namespace Jaunty.DataAnnotations.Schema
{
    /// <summary>
    /// Designates a database the object should be read from. If not specified, it will assume the database selected by the connection
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple =false, Inherited = true)]
    public sealed class DatabaseAttribute : System.Attribute
    {
        private readonly string _database;

        /// <summary>
        /// Creates a new attribute which will define the name of the database this object resides in
        /// </summary>
        /// <param name="database"></param>
        public DatabaseAttribute(string database)
        {
            _database = database;
        }

        /// <summary>
        /// Gets the name of the database
        /// </summary>
        public string Database { get { return _database; } }

    }
}
