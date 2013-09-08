using System;
using System.Collections.Generic;
using System.Text;
using CommonSupport;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace ForexPlatform
{
    /// <summary>
    /// Implements a site-specific news source, parsing dataDelivery from the bloomberg.com
    /// </summary>
    [NewsSource.NewsItemType(typeof(RssNewsItem))]
    public class BloombergNewsSource : NewsSource
    {
        volatile bool _upading = false;

        /// <summary>
        /// Cache if items for the last few days, with titles.
        /// </summary>
        Dictionary<string, RssNewsItem> _latestNewsItemsTitles = new Dictionary<string, RssNewsItem>();

        Dictionary<string, string> _channelsAddresses = new Dictionary<string, string>();

        const string BaseAddress = "http://www.bloomberg.com/";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="platform"></param>
        public BloombergNewsSource()
        {
            this.Name = "Bloomberg News";
            this.Address = BaseAddress;
            Description = "Bloomberg economic news source.";

            _channels.Clear();
            _channelsItems.Clear();

            AddChannel("Exclusive", "http://www.bloomberg.com/news/exclusive/");
            AddChannel("Worldwide", "http://www.bloomberg.com/news/worldwide/");

            AddChannel("Markets Stocks", "http://www.bloomberg.com/news/markets/stocks.html");
            AddChannel("Markets Bonds", "http://www.bloomberg.com/news/markets/bonds.html");
            AddChannel("Markets Commodities", "http://www.bloomberg.com/news/markets/commodities.html");
            AddChannel("Markets Currencies", "http://www.bloomberg.com/news/markets/currencies.html");
            AddChannel("Markets Emerging", "http://www.bloomberg.com/news/markets/emerging_markets.html");
            AddChannel("Markets Energy", "http://www.bloomberg.com/news/markets/energy.html");
            AddChannel("Markets Funds", "http://www.bloomberg.com/news/markets/funds.html");
            AddChannel("Markets Municipal Bonds", "http://www.bloomberg.com/news/markets/muni_bonds.html");

            AddChannel("Industries Consumer", "http://www.bloomberg.com/news/industries/consumer.html");
            AddChannel("Industries Energy", "http://www.bloomberg.com/news/industries/energy.html");
            AddChannel("Industries Finance", "http://www.bloomberg.com/news/industries/finance.html");
            AddChannel("Industries Health Care", "http://www.bloomberg.com/news/industries/health_care.html");
            AddChannel("Industries Insurance", "http://www.bloomberg.com/news/industries/insurance.html");
            AddChannel("Industries Real Estate", "http://www.bloomberg.com/news/industries/real_estate.html");
            AddChannel("Industries Technology", "http://www.bloomberg.com/news/industries/technology.html");
            AddChannel("Industries Transportation", "http://www.bloomberg.com/news/industries/transportation.html");

            AddChannel("Economy", "http://www.bloomberg.com/news/economy/");
            AddChannel("Politics", "http://www.bloomberg.com/news/politics/politics.html");
            AddChannel("Investment", "http://www.bloomberg.com/news/moreinvest.html");
        }

        /// <summary>
        /// Helper. Not thread safe.
        /// </summary>
        void AddChannel(string name, string uri)
        {
            _channelsAddresses.Add(name, uri);
            base.AddChannel(name, true);
        }

        /// <summary>
        /// Intercept call to gether locally items for filtering.
        /// </summary>
        public override void AddItems(NewsItem[] items)
        {
            lock (this)
            {
                foreach (RssNewsItem item in items)
                {
                    if ((DateTime.Now - item.DateTime) < TimeSpan.FromDays(7)
                        && _latestNewsItemsTitles.ContainsKey(item.Title) == false)
                    {// Gather items from the last 3 days.
                        _latestNewsItemsTitles.Add(item.Title, item);
                    }
                }
            }

            base.AddItems(items);
        }

        /// <summary>
        /// Helper. Download HTML and generate a HTMLAgilityPack document from it.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        static HtmlDocument GenerateDocument(string uri)
        {
            HtmlDocument document = null;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(uri);

                webRequest.Timeout = 25000;
                webRequest.MaximumAutomaticRedirections = 10;
                webRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0b; Windows NT5.0)";

                document = new HtmlAgilityPack.HtmlDocument();
                using (WebResponse response = webRequest.GetResponse())
                {
                    if (response.ContentLength == 0)
                    {
                        SystemMonitor.OperationError("Failed to obtain web page stream [" + uri + "].");
                        return null;
                    }

                    using (Stream receiveStream = response.GetResponseStream())
                    {// Load document to HtmlAgilityPack document structure.
                        document.Load(receiveStream, Encoding.UTF8);
                    }
                }
            }
            catch (WebException ex)
            {
                SystemMonitor.OperationError(ex.Message);
            }

            return document;
        }

        /// <summary>
        /// Result is new items found on page.
        /// </summary>
        /// <param name="uri"></param>
        List<RssNewsItem> ProcessPage(string uri, int channelId)
        {
            List<RssNewsItem> result = new List<RssNewsItem>();

            HtmlDocument document = GenerateDocument(uri);
            if (document == null)
            {
                return result;
            }
            
            foreach (HtmlNode node in document.DocumentNode.SelectNodes("//a[@class]"))
            {
                if (node.ParentNode.Name == "p" &&
                    node.ParentNode.Attributes["class"] != null
                    && node.ParentNode.Attributes["class"].Value == "summ")
                {
                    string itemTitle = node.ChildNodes[0].InnerText;

                    lock (this)
                    {
                        if (_latestNewsItemsTitles.ContainsKey(itemTitle))
                        {// News already listed.
                            continue;
                        }

                        RssNewsItem item = CreateNewsItem(node, true);
                        if (item != null)
                        {
                            _latestNewsItemsTitles.Add(itemTitle, item);
                            item.ChannelId = channelId;
                            result.Add(item);
                        }
                    }
                }
            }

            return result;
        }


        
        /// <summary>
        /// Helper. node.ChildNodes[0].InnerText + ">>" + node.Attributes["href"].Value + "; " + Environment.NewLine;
        /// </summary>
        /// <param name="node"></param>
        /// <param name="fetchDate"></param>
        /// <returns></returns>
        RssNewsItem CreateNewsItem(HtmlNode node, bool fetchDateAndDetails)
        {

            RssNewsItem item = new RssNewsItem(this);
            item.Author = "Bloomberg";
            item.Comments = "";

            if (node.ParentNode.Name == "p" && node.ParentNode.ChildNodes[2].Name == "#text")
            {// Description available in parent.
                item.Description = GeneralHelper.RepairHTMLString(node.ParentNode.ChildNodes[2].InnerText);
                item.Description = item.Description.Replace("\n", " ");
            }
            else
            {
                item.Description = "";
            }

            item.Link = new Uri(BaseAddress + node.Attributes["href"].Value);
            item.Title = node.ChildNodes[0].InnerText;

            if (fetchDateAndDetails)
            {
                HtmlDocument document = GenerateDocument(item.Link.AbsoluteUri);
                if (document == null)
                {
                    return null;
                }

                HtmlNodeCollection nodes = document.DocumentNode.SelectNodes("//i");

                foreach (HtmlNode iNode in nodes)
                {
                    string dateTimeInfo = iNode.ChildNodes[0].InnerText;
                    DateTime time = GeneralHelper.ParseDateTimeWithZone(dateTimeInfo.Replace("Last Updated:", ""));
                    item.DateTime = time;
                }
            }
            
            return item;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void OnUpdate()
        {
            if (_upading)
            {
                return;
            }

            int channelId = 0;
            string[] names;
            lock (this)
            {
                names = GeneralHelper.EnumerableToArray<string>(_channelsAddresses.Keys);
            }

            foreach (string channelName in names)
            {
                string uri;
                lock (this)
                {
                    uri = _channelsAddresses[channelName];
                }

                if (IsChannelEnabled(channelName))
                {
                    List<RssNewsItem> items = ProcessPage(uri, channelId);
                    base.AddItems(items.ToArray());
                }
                channelId++;
            }
        }
    }
}
