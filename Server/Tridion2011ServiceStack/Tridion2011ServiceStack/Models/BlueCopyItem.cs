using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tridion2011ServiceStack.Models
{
    public class BlueCopyItem
    {
        public string Title { get; set; }
        public string SourceTitle { get; set; }
        public string Uri { get; set; }
        public string Error { get; set; }
        public string Filename { get; set; }
    }
}