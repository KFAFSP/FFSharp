using JetBrains.Annotations;

namespace FFSharp
{
    /// <summary>
    /// Enumeration of possible stream open modes.
    /// </summary>
    [PublicAPI]
    public enum StreamOpenMode
    {
        /// <summary>
        /// Stream is closed.
        /// </summary>
        Closed,
        /// <summary>
        /// Stream is open for input.
        /// </summary>
        Input,
        /// <summary>
        /// Stream is open for output.
        /// </summary>
        Output,
        /// <summary>
        /// Stream is bi-directional.
        /// </summary>
        BiDi
    }
}