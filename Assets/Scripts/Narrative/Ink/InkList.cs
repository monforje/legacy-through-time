using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace LegacyThroughTime.Narrative.Ink
{
    /// <summary>An element of an ink LIST: <c>Origin.Name</c>.</summary>
    public readonly struct InkListItem : IEquatable<InkListItem>
    {
        public readonly string Origin;
        public readonly string Name;

        public InkListItem(string origin, string name)
        {
            Origin = origin;
            Name = name;
        }

        public static InkListItem Parse(string fullName)
        {
            int dot = fullName.IndexOf('.');
            return dot < 0 ? new InkListItem(null, fullName) : new InkListItem(fullName.Substring(0, dot), fullName.Substring(dot + 1));
        }

        public bool IsNull => Origin == null && Name == null;
        public string FullName => (Origin ?? "?") + "." + Name;

        public bool Equals(InkListItem other) => Name == other.Name && Origin == other.Origin;
        public override bool Equals(object obj) => obj is InkListItem other && Equals(other);
        public override int GetHashCode() => (Name?.GetHashCode() ?? 0) + (Origin?.GetHashCode() ?? 0);
        public override string ToString() => FullName;
    }

    /// <summary>A LIST declaration: item name → integer value.</summary>
    sealed class ListDefinition
    {
        public readonly string Name;
        public readonly Dictionary<string, int> ValuesByName;
        public readonly Dictionary<InkListItem, int> Items;

        public ListDefinition(string name, Dictionary<string, int> valuesByName)
        {
            Name = name;
            ValuesByName = valuesByName;
            Items = new Dictionary<InkListItem, int>(valuesByName.Count);
            foreach (var kv in valuesByName) Items[new InkListItem(name, kv.Key)] = kv.Value;
        }

        public bool TryGetItemWithValue(int value, out InkListItem item)
        {
            foreach (var kv in ValuesByName)
            {
                if (kv.Value != value) continue;
                item = new InkListItem(Name, kv.Key);
                return true;
            }
            item = default;
            return false;
        }
    }

    /// <summary>All LIST declarations of a story plus a by-name cache of single-item lists.</summary>
    sealed class ListDefinitions
    {
        readonly Dictionary<string, ListDefinition> _byName = new Dictionary<string, ListDefinition>();
        readonly Dictionary<string, InkList> _singleItems = new Dictionary<string, InkList>();

        public static readonly ListDefinitions Empty = new ListDefinitions(Array.Empty<ListDefinition>());

        public ListDefinitions(IEnumerable<ListDefinition> defs)
        {
            foreach (var def in defs)
            {
                _byName[def.Name] = def;
                foreach (var kv in def.Items)
                {
                    var single = InkList.Single(kv.Key, kv.Value);
                    _singleItems[kv.Key.Name] = single;
                    _singleItems[kv.Key.FullName] = single;
                }
            }
        }

        public IEnumerable<ListDefinition> All => _byName.Values;
        public bool TryGet(string name, out ListDefinition def) => _byName.TryGetValue(name, out def);

        public InkList FindSingleItemList(string name) =>
            !string.IsNullOrWhiteSpace(name) && _singleItems.TryGetValue(name, out var list) ? list : null;
    }

    /// <summary>
    /// An ink LIST value: a finite set of items with integer values, remembering which
    /// LIST declarations ("origins") it is drawn from even when empty. Treated as immutable
    /// once built; every operation returns a new list. Items are kept in a dictionary so
    /// the enumeration order (visible via LIST_RANDOM) matches the reference runtime.
    /// </summary>
    public sealed class InkList : IEnumerable<KeyValuePair<InkListItem, int>>
    {
        readonly Dictionary<InkListItem, int> _items;
        List<string> _initialOrigins;

        public InkList() => _items = new Dictionary<InkListItem, int>();

        public InkList(InkList other)
        {
            _items = new Dictionary<InkListItem, int>(other._items);
            // Freeze the source's origins: if this copy later loses all its items it
            // must still know which LISTs it came from.
            var origins = other.OriginNames;
            if (origins != null) _initialOrigins = new List<string>(origins);
        }

        internal static InkList Single(InkListItem item, int value)
        {
            var l = new InkList();
            l._items.Add(item, value);
            return l;
        }

        internal static InkList WithOrigins(List<string> originNames)
        {
            var l = new InkList();
            if (originNames != null) l._initialOrigins = new List<string>(originNames);
            return l;
        }

        public int Count => _items.Count;
        public bool ContainsKey(InkListItem item) => _items.ContainsKey(item);
        internal void Add(InkListItem item, int value) => _items.Add(item, value);
        internal void Set(InkListItem item, int value) => _items[item] = value;
        internal void Remove(InkListItem item) => _items.Remove(item);

        public IEnumerator<KeyValuePair<InkListItem, int>> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>Origin list names: derived from items when non-empty, else the remembered ones.</summary>
        public List<string> OriginNames
        {
            get
            {
                if (_items.Count == 0) return _initialOrigins;
                var names = new List<string>(_items.Count);
                foreach (var kv in _items) names.Add(kv.Key.Origin);
                return names;
            }
        }

        internal List<ListDefinition> Origins(ListDefinitions defs)
        {
            var names = OriginNames;
            if (names == null) return null;
            var origins = new List<ListDefinition>(names.Count);
            foreach (var n in names)
            {
                defs.TryGet(n, out var def);
                if (!origins.Contains(def)) origins.Add(def);
            }
            return origins;
        }

        public KeyValuePair<InkListItem, int> MaxItem
        {
            get
            {
                var max = default(KeyValuePair<InkListItem, int>);
                foreach (var kv in _items)
                    if (max.Key.IsNull || kv.Value > max.Value) max = kv;
                return max;
            }
        }

        public KeyValuePair<InkListItem, int> MinItem
        {
            get
            {
                var min = default(KeyValuePair<InkListItem, int>);
                foreach (var kv in _items)
                    if (min.Key.IsNull || kv.Value < min.Value) min = kv;
                return min;
            }
        }

        internal InkList Inverse(ListDefinitions defs)
        {
            var result = new InkList();
            var origins = Origins(defs);
            if (origins == null) return result;
            foreach (var origin in origins)
            {
                if (origin == null) continue;
                foreach (var kv in origin.Items)
                    if (!_items.ContainsKey(kv.Key)) result._items.Add(kv.Key, kv.Value);
            }
            return result;
        }

        internal InkList All(ListDefinitions defs)
        {
            var result = new InkList();
            var origins = Origins(defs);
            if (origins == null) return result;
            foreach (var origin in origins)
            {
                if (origin == null) continue;
                foreach (var kv in origin.Items) result._items[kv.Key] = kv.Value;
            }
            return result;
        }

        public InkList Union(InkList other)
        {
            var result = new InkList(this);
            foreach (var kv in other._items) result._items[kv.Key] = kv.Value;
            return result;
        }

        public InkList Intersect(InkList other)
        {
            var result = new InkList();
            foreach (var kv in _items)
                if (other._items.ContainsKey(kv.Key)) result._items.Add(kv.Key, kv.Value);
            return result;
        }

        public InkList Without(InkList other)
        {
            var result = new InkList(this);
            foreach (var kv in other._items) result._items.Remove(kv.Key);
            return result;
        }

        /// <summary>Superset test (<c>?</c>). The empty list is contained by nothing.</summary>
        public bool Contains(InkList other)
        {
            if (other._items.Count == 0 || _items.Count == 0) return false;
            foreach (var kv in other._items)
                if (!_items.ContainsKey(kv.Key)) return false;
            return true;
        }

        public bool GreaterThan(InkList other)
        {
            if (Count == 0) return false;
            if (other.Count == 0) return true;
            return MinItem.Value > other.MaxItem.Value;
        }

        public bool GreaterThanOrEquals(InkList other)
        {
            if (Count == 0) return false;
            if (other.Count == 0) return true;
            return MinItem.Value >= other.MinItem.Value && MaxItem.Value >= other.MaxItem.Value;
        }

        public bool LessThan(InkList other)
        {
            if (other.Count == 0) return false;
            if (Count == 0) return true;
            return MaxItem.Value < other.MinItem.Value;
        }

        public bool LessThanOrEquals(InkList other)
        {
            if (other.Count == 0) return false;
            if (Count == 0) return true;
            return MaxItem.Value <= other.MaxItem.Value && MinItem.Value <= other.MinItem.Value;
        }

        public InkList MaxAsList() => Count > 0 ? Single(MaxItem.Key, MaxItem.Value) : new InkList();
        public InkList MinAsList() => Count > 0 ? Single(MinItem.Key, MinItem.Value) : new InkList();

        internal InkList SubRange(in Value min, in Value max)
        {
            if (Count == 0) return new InkList();
            int lo = 0, hi = int.MaxValue;
            if (min.Kind == ValueKind.Int) lo = min.I;
            else if (min.Kind == ValueKind.List && min.ListValue.Count > 0) lo = min.ListValue.MinItem.Value;
            if (max.Kind == ValueKind.Int) hi = max.I;
            else if (max.Kind == ValueKind.List && max.ListValue.Count > 0) hi = max.ListValue.MaxItem.Value;

            var result = WithOrigins(OriginNames);
            foreach (var kv in _items)
                if (kv.Value >= lo && kv.Value <= hi) result._items.Add(kv.Key, kv.Value);
            return result;
        }

        /// <summary>Set equality on items (values and origins are ignored, as in ink).</summary>
        public override bool Equals(object obj)
        {
            if (!(obj is InkList other) || other.Count != Count) return false;
            foreach (var kv in _items)
                if (!other._items.ContainsKey(kv.Key)) return false;
            return true;
        }

        public override int GetHashCode()
        {
            int h = 0;
            foreach (var kv in _items) h += kv.Key.GetHashCode();
            return h;
        }

        /// <summary>Item names ordered by value (ties by origin name), comma separated.</summary>
        public override string ToString()
        {
            if (_items.Count == 0) return "";
            var ordered = new List<KeyValuePair<InkListItem, int>>(_items);
            ordered.Sort((a, b) => a.Value == b.Value
                ? string.CompareOrdinal(a.Key.Origin, b.Key.Origin)
                : a.Value.CompareTo(b.Value));
            var sb = new StringBuilder();
            for (int i = 0; i < ordered.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(ordered[i].Key.Name);
            }
            return sb.ToString();
        }
    }
}
