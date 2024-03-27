﻿using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode.TextContent.Fact;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.Update
{
    public record FactForUpdateQuery(FactForUpdateDto UpdateRequest) : IRequest<Result<FactForUpdateDto>>;
}
