#region Copyright (c) 2011-2023 Technosoftware GmbH. All rights reserved
//-----------------------------------------------------------------------------
// Copyright (c) 2011-2023 Technosoftware GmbH. All rights reserved
// Web: https://www.technosoftware.com 
// 
// The source code in this file is covered under a dual-license scenario:
//   - Owner of a purchased license: SCLA 1.0
//   - GPL V3: everybody else
//
// SCLA license terms accompanied with this source code.
// See SCLA 1.0: https://technosoftware.com/license/Source_Code_License_Agreement.pdf
//
// GNU General Public License as published by the Free Software Foundation;
// version 3 of the License are accompanied with this source code.
// See https://technosoftware.com/license/GPLv3License.txt
//
// This source code is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE.
//-----------------------------------------------------------------------------
#endregion Copyright (c) 2011-2023 Technosoftware GmbH. All rights reserved

#region Using Directives
using System;
using System.Collections;
using System.Runtime.Serialization;
#endregion

namespace Technosoftware.DaAeHdaClient
{
    /// <summary>
    /// A read only dictionary class which can be used to expose arrays as properties of classes.
    /// </summary>
    [Serializable]
    public class OpcWriteableDictionary : IDictionary, ISerializable
    {
        #region Fields
        private Hashtable dictionary_ = new Hashtable();
        private Type keyType_;
        private Type valueType_;
        #endregion

        #region Protected Interface
        /// <summary>
        /// Creates a collection that wraps the specified array instance.
        /// </summary>
        protected OpcWriteableDictionary(IDictionary dictionary, Type keyType, Type valueType)
        {
            // set default key/value types.
            keyType_ = keyType ?? typeof(object);
            valueType_ = valueType ?? typeof(object);

            // copy dictionary.
            Dictionary = dictionary;
        }

        /// <summary>
        /// The dictionary instance exposed by the collection.
        /// </summary>
        protected virtual IDictionary Dictionary
        {
            get => dictionary_;

            set
            {
                // copy dictionary.
                if (value != null)
                {
                    // verify that current keys of the dictionary are the correct type.
                    if (keyType_ != null)
                    {
                        foreach (var element in value.Keys)
                        {
                            ValidateKey(element, keyType_);
                        }
                    }

                    // verify that current values of the dictionary are the correct type.
                    if (valueType_ != null)
                    {
                        foreach (var element in value.Values)
                        {
                            ValidateValue(element, valueType_);
                        }
                    }

                    dictionary_ = new Hashtable(value);
                }
                else
                {
                    dictionary_ = new Hashtable();
                }
            }
        }

        /// <summary>
        /// The type of objects allowed as keys in the dictionary.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        protected Type KeyType
        {
            get => keyType_;

            set
            {
                // verify that current keys of the dictionary are the correct type.
                foreach (var element in dictionary_.Keys)
                {
                    ValidateKey(element, value);
                }

                keyType_ = value;
            }
        }

        /// <summary>
        /// The type of objects allowed as values in the dictionary.
        /// </summary>
        protected Type ValueType
        {
            get => valueType_;

            set
            {
                // verify that current values of the dictionary are the correct type.
                foreach (var element in dictionary_.Values)
                {
                    ValidateValue(element, value);
                }

                valueType_ = value;
            }
        }

        /// <summary>
        /// Throws an exception if the key is not valid for the dictionary.
        /// </summary>
        protected virtual void ValidateKey(object element, Type type)
        {
            if (element == null)
            {
                throw new ArgumentException(string.Format(INVALID_VALUE, null, "key"));
            }

            if (!type.IsInstanceOfType(element))
            {
                throw new ArgumentException(string.Format(INVALID_TYPE, element.GetType(), "key"));
            }
        }

        /// <summary>
        /// Throws an exception if the value is not valid for the dictionary.
        /// </summary>
        protected virtual void ValidateValue(object element, Type type)
        {
            if (element != null)
            {
                if (!type.IsInstanceOfType(element))
                {
                    throw new ArgumentException(string.Format(INVALID_TYPE, element.GetType(), "value"));
                }
            }
        }

        /// <remarks/>
        protected const string INVALID_VALUE = "The {1} '{0}' cannot be added to the dictionary.";
        /// <remarks/>
        protected const string INVALID_TYPE = "A {1} with type '{0}' cannot be added to the dictionary.";
        #endregion

        #region ISerializable Members
        /// <summary>
        /// A set of names for fields used in serialization.
        /// </summary>
        private class Names
        {
            internal const string Count = "CT";
            internal const string Key = "KY";
            internal const string Value = "VA";
            internal const string KeyType = "KT";
            internal const string ValueValue = "VT";
        }

        /// <summary>
        /// Construct a server by de-serializing its OpcUrl from the stream.
        /// </summary>
        protected OpcWriteableDictionary(SerializationInfo info, StreamingContext context)
        {
            keyType_ = (Type)info.GetValue(Names.KeyType, typeof(Type));
            valueType_ = (Type)info.GetValue(Names.ValueValue, typeof(Type));

            var count = (int)info.GetValue(Names.Count, typeof(int));

            dictionary_ = new Hashtable();

            for (var ii = 0; ii < count; ii++)
            {
                var key = info.GetValue(Names.Key + ii.ToString(), typeof(object));
                var value = info.GetValue(Names.Value + ii.ToString(), typeof(object));

                if (key != null)
                {
                    dictionary_[key] = value;
                }
            }
        }

        /// <summary>
        /// Serializes a server into a stream.
        /// </summary>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(Names.KeyType, keyType_);
            info.AddValue(Names.ValueValue, valueType_);
            info.AddValue(Names.Count, dictionary_.Count);

            var ii = 0;

            var enumerator = dictionary_.GetEnumerator();

            while (enumerator.MoveNext())
            {
                info.AddValue(Names.Key + ii.ToString(), enumerator.Key);
                info.AddValue(Names.Value + ii.ToString(), enumerator.Value);

                ii++;
            }
        }
        #endregion

        #region IDictionary Members
        /// <summary>
        /// Gets a value indicating whether the IDictionary is read-only.
        /// </summary>
        public virtual bool IsReadOnly => false;

        /// <summary>
        /// Returns an IDictionaryEnumerator for the IDictionary.
        /// </summary>
        public virtual IDictionaryEnumerator GetEnumerator()
        {
            return dictionary_.GetEnumerator();
        }

        /// <summary>
        /// Gets or sets the element with the specified key. 
        /// </summary>
        public virtual object this[object key]
        {
            get => dictionary_[key];

            set
            {
                ValidateKey(key, keyType_);
                ValidateValue(value, valueType_);
                dictionary_[key] = value;
            }
        }

        /// <summary>
        /// Removes the element with the specified key from the IDictionary.
        /// </summary>
        public virtual void Remove(object key)
        {
            dictionary_.Remove(key);
        }

        /// <summary>
        /// Determines whether the IDictionary contains an element with the specified key.
        /// </summary>
        public virtual bool Contains(object key)
        {
            return dictionary_.Contains(key);
        }

        /// <summary>
        /// Removes all elements from the IDictionary.
        /// </summary>
        public virtual void Clear()
        {
            dictionary_.Clear();
        }

        /// <summary>
        /// Gets an ICollection containing the values in the IDictionary.
        /// </summary>
        public virtual ICollection Values => dictionary_.Values;

        /// <summary>
        /// Adds an element with the provided key and value to the IDictionary.
        /// </summary>
        public virtual void Add(object key, object value)
        {
            ValidateKey(key, keyType_);
            ValidateValue(value, valueType_);
            dictionary_.Add(key, value);
        }

        /// <summary>
        /// Gets an ICollection containing the keys of the IDictionary.
        /// </summary>
        public virtual ICollection Keys => dictionary_.Keys;

        /// <summary>
        /// Gets a value indicating whether the IDictionary has a fixed size.
        /// </summary>
        public virtual bool IsFixedSize => false;

        #endregion

        #region ICollection Members
        /// <summary>
        /// Indicates whether access to the ICollection is synchronized (thread-safe).
        /// </summary>
        public virtual bool IsSynchronized => false;

        /// <summary>
        /// Gets the number of objects in the collection.
        /// </summary>
        public virtual int Count => dictionary_.Count;

        /// <summary>
        /// Copies the objects to an Array, starting at a the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination for the objects.</param>
        /// <param name="index">The zero-based index in the Array at which copying begins.</param>
        public virtual void CopyTo(Array array, int index)
        {
            dictionary_?.CopyTo(array, index);
        }

        /// <summary>
        /// Indicates whether access to the ICollection is synchronized (thread-safe).
        /// </summary>
        public virtual object SyncRoot => this;
        #endregion

        #region IEnumerable Members
        /// <summary>
        /// Returns an enumerator that can iterate through a collection.
        /// </summary>
        /// <returns>An IEnumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region ICloneable Members
        /// <summary>
        /// Creates a deep copy of the collection.
        /// </summary>
        public virtual object Clone()
        {
            var clone = (OpcWriteableDictionary)MemberwiseClone();

            // clone contents of hashtable.
            var dictionary = new Hashtable();

            var enumerator = dictionary_.GetEnumerator();

            while (enumerator.MoveNext())
            {
                dictionary.Add(OpcConvert.Clone(enumerator.Key), OpcConvert.Clone(enumerator.Value));
            }

            clone.dictionary_ = dictionary;

            // return clone.
            return clone;
        }
        #endregion

    }
}
