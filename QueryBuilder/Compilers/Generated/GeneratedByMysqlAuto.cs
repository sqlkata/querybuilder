﻿using System;
using System.Collections.Generic;
using System.Text;

namespace QueryBuilder.Compilers.Generated
{
    public sealed class GeneratedByMysqlAuto : GeneratedBy, IGeneratedBy
    {
        public GeneratedByMysqlAuto() 
            : base("SELECT LAST_INSERT_ID();", GeneratedByType.Last, "", "")
        {
        }
    }
}
