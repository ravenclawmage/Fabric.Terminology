﻿namespace Fabric.Terminology.SqlServer.Persistence.Mapping
{
    using System;
    using System.Collections.Generic;

    using Fabric.Terminology.Domain.Models;
    using Fabric.Terminology.SqlServer.Models.Dto;

    internal abstract class ValueSetMapperBase
    {
        protected IValueSet Build(ValueSetDescriptionDto dto, IReadOnlyCollection<IValueSetCode> codes, int codeCount)
        {

            throw new NotImplementedException();

            //var valueSet = new ValueSet(
            //    dto.ValueSetID,
            //    dto.ValueSetUniqueID,
            //    dto.ValueSetOID,
            //    dto.ValueSetNM,
            //    dto.AuthoringSourceDSC,
            //    dto.PurposeDSC,
            //    dto.SourceDSC,
            //    dto.VersionDSC,
            //    codes)
            //{
            //    ValueSetCodesCount = codeCount
            //};

            //this.isCustomValue.Set(valueSet);

            //return valueSet;
        }
    }
}