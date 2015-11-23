using System.Collections.Generic;

namespace Jaunty
{
    /// <summary>
    /// Represents a set of altered properties
    /// </summary>
    /// <typeparam name="T">The type of object to perform alterations on</typeparam>
    public interface IMutator<out T>
    {
        /// <summary>
        /// Gets a list of property names whose values have been changed
        /// </summary>
        IEnumerable<string> Changeset { get; }
        /// <summary>
        /// Target object
        /// </summary>
        T Target { get; }
    }

    /// <summary>
    /// Mutator
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    public interface IMutator<out T, in U> : IMutator<T>
    {

    }


}
