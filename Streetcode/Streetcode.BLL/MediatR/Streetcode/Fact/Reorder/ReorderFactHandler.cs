﻿using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.Reorder;

public sealed class ReorderFactHandler : IRequestHandler<ReorderFactCommand, Result<ReorderFactResponseDto>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public ReorderFactHandler(IRepositoryWrapper repositoryWrapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
    }

    public async Task<Result<ReorderFactResponseDto>> Handle(
        ReorderFactCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var arr = request.ReorderedIdArr;

        if (request.ReorderedIdArr is null || arr.Length == 0)
        {
            return FactIdArrIsNullOrEmpty(request);
        }

        var factsCount = (await _repositoryWrapper.FactRepository.GetAllAsync(f => f.StreetcodeId == request.StreetcodeId)).Count();

        if (factsCount == 0)
        {
            return CannotFindFactByStreetcodeId(request);
        }

        if (request.ReorderedIdArr.Length != factsCount)
        {
            return IncorrectIdsNumberTransferredInArray(request, arr.Length, factsCount);
        }

        for (int i = 0; i < arr.Length; i++)
        {
            var tmpFact = await _repositoryWrapper.FactRepository.GetFirstOrDefaultAsync(f => f.Id == arr[i] && f.StreetcodeId == request.StreetcodeId);
            if (tmpFact is null)
            {
                return IncorrectFactIdTransferredInArray(request, arr[i]);
            }
            else
            {
                tmpFact.Number = i + 1;
                _repositoryWrapper.FactRepository.Update(tmpFact);
            }
        }

        bool resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

        if (!resultIsSuccess)
        {
            return CannotUpdateFactsAfterReordering(request);
        }

        return Result.Ok(new ReorderFactResponseDto(true));
    }

    private Result<ReorderFactResponseDto> FactIdArrIsNullOrEmpty(ReorderFactRequestDto request)
    {
        var errorMsg = string.Format(Resources.Errors.ValidationErrors.Fact.ReorderFactErrors.IncomingFactIdArrIsNullOrEmpty);
        _logger.LogError(request, errorMsg);
        return Result.Fail(errorMsg);
    }

    private Result<ReorderFactResponseDto> CannotFindFactByStreetcodeId(ReorderFactRequestDto request)
    {
        var errorMsg = string.Format(Resources.Errors.CannotFindEntityErrors.CannotFindFactByStreetcodeId, request.StreetcodeId);
        _logger.LogError(request, errorMsg);
        return Result.Fail(errorMsg);
    }

    private Result<ReorderFactResponseDto> IncorrectIdsNumberTransferredInArray(ReorderFactRequestDto request, int idArrLength, int factsCount)
    {
        var errorMsg = string.Format(
            Resources.Errors.ValidationErrors.Fact.ReorderFactErrors.IncorrectIdsNumberInArray,
            idArrLength,
            factsCount,
            request.StreetcodeId);
        _logger.LogError(request, errorMsg);
        return Result.Fail(errorMsg);
    }

    private Result<ReorderFactResponseDto> IncorrectFactIdTransferredInArray(ReorderFactRequestDto request, int id)
    {
        var errorMsg = string.Format(Resources.Errors.ValidationErrors.Fact.ReorderFactErrors.IncorrectFactIdTransferredInArray, id, request.StreetcodeId);
        _logger.LogError(request, errorMsg);
        return Result.Fail(errorMsg);
    }

    private Result<ReorderFactResponseDto> CannotUpdateFactsAfterReordering(ReorderFactRequestDto request)
    {
        var errorMsg = string.Format(Resources.Errors.CannotUpdateEntityErrors.CannotUpdateNumberInFact);
        _logger.LogError(request, errorMsg);
        return Result.Fail(errorMsg);
    }
}