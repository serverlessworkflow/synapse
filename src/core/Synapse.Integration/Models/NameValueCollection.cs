using System.Collections;
using System.Collections.ObjectModel;

namespace Synapse.Integration.Models
{

    /// <summary>
    /// Represents an OData-compatible <see cref="IDictionary{TKey, TValue}"/> wrapper
    /// </summary>
    /// <typeparam name="TValue">The type of values held by the <see cref="NameValueCollection{TValue}"/></typeparam>
    /// <remarks>Taken from <see href="https://github.com/OData/WebApi/issues/438#issuecomment-129258269">Casimodo72's answer</see> on Github</remarks>
    [DataContract]
    public class NameValueCollection<TValue>
        : IDictionary<string, TValue>, ICollection
    {

        /// <summary>
        /// Initializes a new <see cref="NameValueCollection{TValue}"/>
        /// </summary>
        public NameValueCollection()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="NameValueCollection{TValue}"/>
        /// </summary>
        /// <param name="items">An <see cref="IDictionary{TKey, TValue}"/> containing the items the <see cref="NameValueCollection{TValue}"/> is made out of</param>
        public NameValueCollection(IDictionary<string, TValue> items)
        {
            this.Items = items.ToDictionary(kvp => kvp.Key, kvp => kvp.Value as object);
        }

        IDictionary<string, object> _Items;
        /// <summary>
        /// Gets the inner <see cref="IDictionary"/>
        /// </summary>
        /// <remarks>Must be Public to be picked up by OData</remarks>
        [DataMember]
        public IDictionary<string, object> Items
        {
            get
            {
                return _Items ?? (_Items = new Dictionary<string, object>());
            }
            set
            {
                this._Items = value;
            }
        }

        /// <inheritdoc/>
        public virtual TValue this[string key]
        {
            get { return (TValue)Items[key]; }
            set { Items[key] = value; }
        }

        /// <inheritdoc/>
        public virtual int Count
        {
            get { return Items.Count; }
        }

        /// <inheritdoc/>
        public virtual bool IsReadOnly
        {
            get { return Items.IsReadOnly; }
        }

        /// <inheritdoc/>
        public virtual ICollection<string> Keys
        {
            get { return Items.Keys; }
        }

        /// <summary>
        /// NOTE: This method will create a new ReadOnlyCollection based on the
        /// values of the underlying dictionary.
        /// </summary>
        /// <inheritdoc/>
        public virtual ICollection<TValue> Values => new ReadOnlyCollection<TValue>(Items.Values.Cast<TValue>().ToList());

        bool ICollection.IsSynchronized => ((ICollection)this.Items).IsSynchronized;

        object ICollection.SyncRoot => ((ICollection)this.Items).SyncRoot;

        /// <inheritdoc/>
        public virtual void Add(KeyValuePair<string, TValue> item)
        {
            Items.Add(Convert(item));
        }

        /// <inheritdoc/>
        public virtual void Add(string key, TValue value)
        {
            Items.Add(key, value);
        }

        /// <inheritdoc/>
        public virtual void Clear()
        {
            Items.Clear();
        }

        /// <inheritdoc/>
        public virtual bool Contains(KeyValuePair<string, TValue> item)
        {
            return Items.Contains(Convert(item));
        }

        /// <inheritdoc/>
        public virtual bool ContainsKey(string key)
        {
            return Items.ContainsKey(key);
        }

        /// <inheritdoc/>
        public virtual void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (Items.Count > array.Length - arrayIndex)
                throw new ArgumentException("The number of elements in the source dictionary " +
                    "is greater than the available space from arrayIndex to the end of the destination array.",
                    nameof(arrayIndex));
            var i = 0;
            foreach (var item in Items)
            {
                array[i] = Convert(item);
                i++;
            }
        }

        /// <inheritdoc/>
        public virtual bool Remove(KeyValuePair<string, TValue> item)
        {
            return Items.Remove(Convert(item));
        }

        /// <inheritdoc/>
        public virtual bool Remove(string key)
        {
            return Items.Remove(key);
        }

        /// <inheritdoc/>
        public virtual bool TryGetValue(string key, out TValue value)
        {
            object obj;
            if (Items.TryGetValue(key, out obj))
            {
                value = (TValue)obj;
                return true;
            }

            value = default(TValue);
            return false;
        }

        /// <inheritdoc/>
        public virtual IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            foreach (var item in Items)
                yield return Convert(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        static KeyValuePair<string, object> Convert(KeyValuePair<string, TValue> item)
        {
            return new KeyValuePair<string, object>(item.Key, item.Value);
        }

        static KeyValuePair<string, TValue> Convert(KeyValuePair<string, object> item)
        {
            return new KeyValuePair<string, TValue>(item.Key, (TValue)item.Value);
        }

        /// <inheritdoc/>
        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the specified <see cref="Dictionary{TKey, TValue}"/> int a new <see cref="NameValueCollection{TValue}"/>
        /// </summary>
        /// <param name="dictionary">The <see cref="Dictionary{TKey, TValue}"/> to convert</param>
        public static implicit operator NameValueCollection<TValue>(Dictionary<string, TValue> dictionary)
        {
            return new NameValueCollection<TValue>(dictionary);
        }

    }

}
