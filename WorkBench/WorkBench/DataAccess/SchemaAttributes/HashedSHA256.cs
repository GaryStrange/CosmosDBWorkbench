using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WorkBench.DataAccess.SchemaAttributes
{
    /// <summary>
    /// Protect the content of a json serializable field by hashing it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HashedSHA256<T>
    {
        /// <summary>
        /// Storage for field property.
        /// </summary>
        private T _objectToBeHashed;
        private string _hashedValue;

        /// <summary>
        /// When in the storage for the Value property is set, the hash of the value is also calculated.
        /// </summary>
        [JsonIgnore]
        public T Value
        {
            get { return _objectToBeHashed; }
            set { _objectToBeHashed = value; this.HashedValue = CryptographicHelper.SHA256(value); }
        }

        /// <summary>
        /// Hash for the Value property.
        /// </summary>
        public string HashedValue
        {
            get { return _hashedValue; }
            set { _hashedValue = value; }
        }

        /// <summary>
        /// Convert between the HashedSHA256 object and template type.
        /// </summary>
        /// <param name="source">A hashedSHA256 object</param>
        public static implicit operator T(HashedSHA256<T> source) => source.Value;

        /// <summary>
        /// Convert between the template type and HashedSHA256 object.
        /// </summary>
        /// <param name="source">An object of the generic type</param>
        public static implicit operator HashedSHA256<T>(T source)
        {
            return new HashedSHA256<T>() { Value = source };
        }

    }
}
