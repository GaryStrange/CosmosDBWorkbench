﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkBench.DataAccess
{
    public interface IPartitionedDocument
    {
        object PartitionKeyValue { get; }
    }
}
