﻿using System.Linq.Expressions;
using AutoMapper;
using Moq;
using Streetcode.BLL.Dto.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Fact.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using Microsoft.EntityFrameworkCore.Query;
using FluentAssertions;
using FluentResults;

using FactEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Fact;
using ImageEntity = Streetcode.DAL.Entities.Media.Images.Image;
using StrecodeEntity = Streetcode.DAL.Entities.Streetcode.StreetcodeContent;

namespace Streetcode.XUnitTest.MediatRTests.StreetCode.Fact.Create
{
    public class CreateFactHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILoggerService> _mockLogger;

        public CreateFactHandlerTests()
        {
            _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILoggerService>();
        }

        [Fact]
        public async Task Handle_ValidCreateFactCommand_WithExistingImageAndStreetcode_ShouldSucceed()
        {
            // Arrange
            var request = GetValidCreateFactRequest();
            SetupMock(request);
            var handler = CreateHandler();
            var command = new CreateFactCommand(request);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await handler.Handle(command, cancellationToken);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_InvalidCreateFactCommand_WithNonexistentImage_ShouldReturnSingleError()
        {
            // Arrange
            var request = GetImageNotExistCreateFactRequest();
            var expectedErrorMsg = string.Format(Streetcode.BLL.Resources.Errors.CannotFindEntityErrors.CannotFindImageById, -1);
            SetupMock(request);
            var handler = CreateHandler();
            var command = new CreateFactCommand(request);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await handler.Handle(command, cancellationToken);

            // Assert
            result.Errors.Should().ContainSingle(e => e.Message == expectedErrorMsg);
        }

        [Fact]
        public async Task Handle_InvalidCreateFactCommand_WithNonexistentStreetcode_ShouldReturnSingleError()
        {
            // Arrange
            var request = GetStreetcodeNotExistCreateFactRequest();
            var expectedErrorMsg = string.Format(Streetcode.BLL.Resources.Errors.CannotFindEntityErrors.CannotFindStreetcodeById, -1);
            SetupMock(request);
            var handler = CreateHandler();
            var command = new CreateFactCommand(request);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await handler.Handle(command, cancellationToken);

            // Assert
            result.Errors.Should().ContainSingle(e => e.Message == expectedErrorMsg);
        }

        [Fact]
        public async Task Handle_AnyCreateFactCommand_WithAnyImageAndAnyStreetcode_ShouldReturnResultOfCorrectType()
        {
            // Arrange
            var request = GetValidCreateFactRequest();
            var expectedType = typeof(Result<CreateFactDto>);
            SetupMock(request);
            var handler = CreateHandler();
            var command = new CreateFactCommand(request);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await handler.Handle(command, cancellationToken);

            // Assert
            result.Should().BeOfType(expectedType);
        }

        private CreateFactHandler CreateHandler()
        {
            return new CreateFactHandler(
                repository: _mockRepositoryWrapper.Object,
                mapper: _mockMapper.Object,
                logger: _mockLogger.Object);
        }

        private void SetupMock(CreateFactDto request)
        {
            var image = request.ImageId switch
            {
                1 => new ImageEntity { Id = request.ImageId },
                _ => null,
            };

            var streetcode = request.StreetcodeId switch
            {
                1 => new StrecodeEntity { Id = request.StreetcodeId },
                _ => null,
            };

            var factEntity = new FactEntity
            {
                Id = 1,
            };

            _mockRepositoryWrapper
                .Setup(r => r.ImageRepository.GetFirstOrDefaultAsync(
                    AnyEntityPredicate<ImageEntity>(),
                    AnyEntityInclude<ImageEntity>()))
                .ReturnsAsync(image);

            _mockRepositoryWrapper
                .Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
                    AnyEntityPredicate<StrecodeEntity>(),
                    AnyEntityInclude<StrecodeEntity>()))
                .ReturnsAsync(streetcode);

            _mockRepositoryWrapper
                .Setup(r => r.FactRepository.Create(It.IsAny<FactEntity>()))
                .Returns(factEntity);

            _mockRepositoryWrapper
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            _mockMapper
                .Setup(m => m.Map<FactEntity>(It.Is<object>(obj => obj is CreateFactDto)))
                .Returns(factEntity);
        }

        private static CreateFactDto GetValidCreateFactRequest()
        {
            return new CreateFactDto(
                Title: "Title1",
                FactContent: "Fact content 1",
                ImageId: 1,
                StreetcodeId: 1);
        }

        private static CreateFactDto GetImageNotExistCreateFactRequest()
        {
            return new CreateFactDto(
                Title: "Title2",
                FactContent: "Fact content 2",
                ImageId: -1,
                StreetcodeId: 1);
        }

        private static CreateFactDto GetStreetcodeNotExistCreateFactRequest()
        {
            return new CreateFactDto(
                Title: "Title3",
                FactContent: "Fact content 3",
                ImageId: 1,
                StreetcodeId: -1);
        }

        private static Expression<Func<TEntity, bool>> AnyEntityPredicate<TEntity>()
        {
            return It.IsAny<Expression<Func<TEntity, bool>>>();
        }

        private static Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> AnyEntityInclude<TEntity>()
        {
            return It.IsAny<Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>>();
        }
    }
}