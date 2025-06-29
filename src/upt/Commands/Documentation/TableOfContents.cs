// Copyright Simone Livieri. All Rights Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// For terms of use, see LICENSE.txt

using System;
using System.Collections.Generic;
using System.Text;

namespace UnityPackageTool.Commands.Documentation;

sealed class TableOfContents
{
    readonly List<Item> m_Items = [];

    public int Count => m_Items.Count;

    public void AddItem(int index, string name, string href, string? topicHref = null)
        => m_Items.Add(new Item(index, name, href, topicHref, false));

    public void AddItem(string name, string href, string? topicHref = null)
        => m_Items.Add(new Item(m_Items.Count, name, href, topicHref, false));

    public void AddToc(string name, string href, string? topicHref = null)
        => m_Items.Add(new Item(m_Items.Count, name, href, topicHref, true));

    public override string ToString()
    {
        var sb = new StringBuilder();

        m_Items.Sort(CompareEntries);

        foreach (Item item in m_Items)
        {
            sb.Append("- name: ").AppendLine(item.Name);
            sb.Append(item.TocHref ? "  tocHref: " : "  href: ").AppendLine(item.Href);
            if (!string.IsNullOrEmpty(item.TopicHref))
                sb.Append("  topicHref: ").AppendLine(item.TopicHref);
        }

        return sb.ToString();

        static int CompareEntries(Item x, Item y)
        {
            return x.Index == y.Index
                       ? string.Compare(x.Name, y.Name, StringComparison.Ordinal)
                       : x.Index.CompareTo(y.Index);
        }
    }

    sealed class Item(int index, string name, string href, string? topicHref, bool tocHref)
    {
        public readonly int Index = index;
        public readonly string Name = name;
        public readonly string Href = href;
        public readonly bool TocHref = tocHref;
        public readonly string? TopicHref = topicHref;
    }
}
