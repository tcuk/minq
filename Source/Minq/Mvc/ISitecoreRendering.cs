﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minq.Mvc
{
	public interface ISitecoreRendering
	{
		SitecoreItemKey DataSourceKey
		{
			get;
		}

        SitecoreItemKey ItemKey
        {
            get;
        }

        IDictionary<string, string> Parameters
		{
			get;
		}
	}
}
