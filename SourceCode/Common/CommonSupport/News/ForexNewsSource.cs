using System;
using System.Collections.Generic;
using System.Text;

namespace CommonSupport
{
    /// <summary>
    /// News source for specific forex news.
    /// </summary>
    [NewsSource.NewsItemTypeAttribute(typeof(ForexNewsItem))]
    public class ForexNewsSource : NewsSource
    {
        /// <summary>
        /// 
        /// </summary>
        public ForexNewsSource()
        {
        }

        public override void OnUpdate()
        {
        }
    }
}
