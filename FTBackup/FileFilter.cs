﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FTBackup
{

    class Filter
    {
        public string filter;
        public Regex ex;
    };

    public class FileFilter
    {
        private List<Filter> filters = new List<Filter>();

        public bool Match(String filename)
        {
            foreach (var f in filters)
                if (f.ex.IsMatch(filename))
                    return true;
            return false;
        }
        public void Add(string filter)
        {
            filters.Add(new Filter() { filter = filter, ex = new Regex(filter) });
        }
    }
}
