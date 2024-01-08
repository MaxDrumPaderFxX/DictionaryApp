using System;
using System.Collections.Generic;
using System.Text;

namespace DictionaryApp
{
    public class WordEntry
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public DateTime AddedDate { get; set; } = DateTime.Now;
    }
}
