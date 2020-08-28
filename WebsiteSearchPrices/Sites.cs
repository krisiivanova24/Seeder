using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebsiteSearchPrices
{
    public class Sites
    {
        public Sites(string name, string url, string siteType, string price, string date)
        {
            this.Name = name;
            this.Url = url;
            this.SiteType = siteType;
            this.Price = price;
            this.Date = date;
        }

        public string Name { get;private set; }
        public string Url { get; private set; }
        public string SiteType { get; private set; }
        public string Price { get; set; }
        public string Date { get; private set; }

    }
}
