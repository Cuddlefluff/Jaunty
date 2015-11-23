using System.Collections.Generic;

namespace Jaunty
{
    /// <summary>
    /// Provides base for generating SQL code
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISqlBuilder<in T>
    {
        /// <summary>
        /// Gets an enumeration of all properties for the type T
        /// </summary>
        IEnumerable<DataProperty> Properties { get; }
        
        /// <summary>
        /// Generates SQL code for inserting a row
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        string InsertString();

        /// <summary>
        /// Generates SQL code for inserting a row using only the explicitly set values
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        string InsertString(IMutator<T> obj);
        
        /// <summary>
        /// Generates SQL code for deleting a row
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        string DeleteString();

        /// <summary>
        /// Generates SQL code for updating modified columns on an object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        string UpdateString(IMutator<T> obj);

        /// <summary>
        /// Generates SQL code for updating all columns on an object
        /// </summary>
        /// <returns></returns>
        string UpdateString();

        /// <summary>
        /// Finds a row by its identifier
        /// </summary>
        /// <param name="id">object containing properties to search for</param>
        /// <returns></returns>
        string FindByIdString(object id);
    }
}
