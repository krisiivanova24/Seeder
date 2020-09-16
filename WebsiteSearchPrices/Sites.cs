using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebsiteSearchPrices
{
    public class Sites
    {
        public Sites(string name, string url, string siteType, string price, string date, string specialid)
        {
            this.Name = name;
            this.Url = url;
            this.SiteType = siteType;
            this.Price = price;
            this.Date = date;
            this.SpecialId = specialid;
        }

        public string Name { get;private set; }
        public string Url { get; private set; }
        public string SiteType { get; private set; }
        public string Price { get; set; }
        public string Date { get; private set; }
        public string SpecialId { get; private set; }

    }
}
