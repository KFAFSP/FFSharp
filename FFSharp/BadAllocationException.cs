using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security.Permissions;

using JetBrains.Annotations;

namespace FFSharp
{
    /// <summary>
    /// Thrown when an unmanaged memory allocation fails.
    /// </summary>
    [PublicAPI]
    [Serializable]
    [ExcludeFromCodeCoverage]
    public sealed class BadAllocationException : Exception
    {
        const string C_Message = "Allocation of \"{0}\" failed.";

        /// <summary>
        /// Create a new <see cref="BadAllocationException"/> instance.
        /// </summary>
        /// <param name="ATypeName">The type name.</param>
        public BadAllocationException([CanBeNull] string ATypeName = null)
            : base(String.Format(C_Message, ATypeName ?? "void"))
        {
            TypeName = ATypeName ?? "void";
        }
        /// <summary>
        /// Create a nwe <see cref="BadAllocationException"/> instance.
        /// </summary>
        /// <param name="AType">
        /// The <see cref="Type"/> reference to get the <see cref="Type.FullName"/> from.
        /// </param>
        public BadAllocationException([NotNull] Type AType)
            : this(AType?.FullName)
        {
            Debug.Assert(
                AType != null,
                "Type is null.",
                "Providing a null type reference is discouraged because the resulting exception " +
                "will not provide any error information."
            );
        }

        /// <summary>
        /// Get the name of the type that failed to allocate.
        /// </summary>
        [NotNull]
        public string TypeName { get; }

        #region Serializable implementation
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        BadAllocationException(
            [NotNull] SerializationInfo AInfo,
            StreamingContext AContext)
            : base(AInfo, AContext)
        {
            TypeName = AInfo.GetString("TypeName");
        }

        /// <inheritdoc />
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo AInfo, StreamingContext AContext)
        {
            AInfo.AddValue("TypeName", TypeName);

            base.GetObjectData(AInfo, AContext);
        }
        #endregion
    }
}