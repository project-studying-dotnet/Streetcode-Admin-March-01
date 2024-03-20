﻿using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Art.GetAll;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.Media.Art
{
    public class GetAllArtTest
    {
        private Mock<IRepositoryWrapper> _mockRepositoryWrapper;
        private Mock<IMapper> _mockMapper;
        private readonly Mock<ILoggerService> _mockLogger;

        public GetAllArtTest()
        {
            _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILoggerService>();
        }

        [Fact]
        public async Task Handle_ReturnsOkResult_WithListOfArts_WhenArtFound()
        {
            // Arrange
            MockRepositorySetupReturnsData();
            MockMapperSetup();

            var handler = new GetAllArtsHandler(
                _mockRepositoryWrapper.Object,
                _mockMapper.Object,
                _mockLogger.Object);

            // Act
            var result = await handler.Handle(new GetAllArtsQuery(), CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Handle_ReturnsFailedResult_WhenArtsNotFound()
        {
            // Arrange
            MockRepositorySetupReturnsNull();
            MockMapperSetup();

            var handler = new GetAllArtsHandler(
                _mockRepositoryWrapper.Object,
                _mockMapper.Object,
                _mockLogger.Object);

            // Act
            var result = await handler.Handle(new GetAllArtsQuery(), CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
        }

        [Fact]
        public async Task Handle_LogsError_WhenArtNotFound()
        {
            // Arrange
            MockRepositorySetupReturnsNull();
            MockMapperSetup();

            var handler = new GetAllArtsHandler(
                _mockRepositoryWrapper.Object,
                _mockMapper.Object,
                _mockLogger.Object);

            // Act
            var result = await handler.Handle(new GetAllArtsQuery(), CancellationToken.None);

            // Assert
            _mockLogger.Verify(
                logger => logger.LogError(
                    It.IsAny<GetAllArtsQuery>(),
                    It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsCollectionOfCorrectCount_WhenArtsFound()
        {
            // Arrange
            var mockVideo = GetArtList();
            var expectedCount = mockVideo.Count;

            MockRepositorySetupReturnsData();
            MockMapperSetup();

            var handler = new GetAllArtsHandler(
                _mockRepositoryWrapper.Object,
                _mockMapper.Object,
                _mockLogger.Object);

            // Act
            var result = await handler.Handle(new GetAllArtsQuery(), CancellationToken.None);
            var actualCount = result.Value.Count();

            // Assert
            Assert.Equal(expectedCount, actualCount);
        }

        [Fact]
        public async Task Handle_MapperShouldMapOnlyOnce_WhenArtsFound()
        {
            // Arrange
            MockRepositorySetupReturnsData();
            MockMapperSetup();

            var handler = new GetAllArtsHandler(
                _mockRepositoryWrapper.Object,
                _mockMapper.Object,
                _mockLogger.Object);

            // Act
            var result = await handler.Handle(new GetAllArtsQuery(), CancellationToken.None);

            // Assert
            _mockMapper.Verify(
                mapper =>
                mapper.Map<IEnumerable<ArtDTO>>(It.IsAny<IEnumerable<Streetcode.DAL.Entities.Media.Images.Art>>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnCollectionOfArtDto_WhenArtsFound()
        {
            // Arrange
            MockRepositorySetupReturnsData();
            MockMapperSetup();

            var handler = new GetAllArtsHandler(
                _mockRepositoryWrapper.Object,
                _mockMapper.Object,
                _mockLogger.Object);

            // Act
            var result = await handler.Handle(new GetAllArtsQuery(), CancellationToken.None);

            // Assert
            Assert.IsType<List<ArtDTO>>(result.Value);
        }

        [Fact]
        public async Task Handle_ReturnsFailedResult_WhenArtsAreNull()
        {
            // Arrange
            MockRepositorySetupReturnsNull();

            var handler = new GetAllArtsHandler(
                _mockRepositoryWrapper.Object,
                _mockMapper.Object,
                _mockLogger.Object);

            // Act
            var result = await handler.Handle(new GetAllArtsQuery(), CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
        }

        [Fact]
        public async Task Handle_ShouldLogCorrectErrorMessage_WhenArtsAreNull()
        {
            // Arrange
            MockRepositorySetupReturnsNull();

            var handler = new GetAllArtsHandler(
                _mockRepositoryWrapper.Object,
                _mockMapper.Object,
                _mockLogger.Object);

            var expectedError = "Cannot find any arts";

            // Act
            var result = await handler.Handle(new GetAllArtsQuery(), CancellationToken.None);

            // Assert
            Assert.Equal(expectedError, result.Errors.First().Message);
        }

        private static List<Streetcode.DAL.Entities.Media.Images.Art> GetArtList()
        {
            return new List<Streetcode.DAL.Entities.Media.Images.Art>
            {
                new ()
                {
                    Id = 1,
                    Title = "Art Title 1",
                    Description = "Art Description 1",
                    ImageId = 1,
                    StreetcodeArts = new List<StreetcodeArt>(),
                },
                new ()
                {
                    Id = 2,
                    Title = "Art Title 2",
                    Description = "Art Description 2",
                    ImageId = 2,
                    StreetcodeArts = new List<StreetcodeArt>(),
                },
                new ()
                {
                    Id = 3,
                    Title = "Art Title 3",
                    Description = "Art Description 3",
                    ImageId = 3,
                    StreetcodeArts = new List<StreetcodeArt>(),
                },
            };
        }

        private static List<ArtDTO> GetArtDtoList()
        {
            return new List<ArtDTO>
            {
                new ()
                {
                    Id = 1,
                    Title = "Art Title 1",
                    Description = "Art Description 1",
                    ImageId = 1,
                    Image = new ImageDTO(),
                },
                new ()
                {
                    Id = 2,
                    Title = "Art Title 2",
                    Description = "Art Description 2",
                    ImageId = 2,
                    Image = new ImageDTO(),
                },
                new ()
                {
                    Id = 3,
                    Title = "Art Title 3",
                    Description = "Art Description 3",
                    ImageId = 3,
                    Image = new ImageDTO(),
                },
            };
        }

        private void MockMapperSetup()
        {
            _mockMapper.Setup(x => x
                .Map<IEnumerable<ArtDTO>>(It.IsAny<IEnumerable<Streetcode.DAL.Entities.Media.Images.Art>>()))
                .Returns(GetArtDtoList());
        }

        private void MockRepositorySetupReturnsData()
        {
            _mockRepositoryWrapper.Setup(x => x.ArtRepository
                .GetAllAsync(
                    It.IsAny<Expression<Func<Streetcode.DAL.Entities.Media.Images.Art, bool>>>(),
                    It.IsAny<Func<IQueryable<Streetcode.DAL.Entities.Media.Images.Art>,
                IIncludableQueryable<Streetcode.DAL.Entities.Media.Images.Art, object>>>()))
                .ReturnsAsync(GetArtList());
        }

        private void MockRepositorySetupReturnsNull()
        {
            _mockRepositoryWrapper.Setup(x => x.ArtRepository
                .GetAllAsync(
                    It.IsAny<Expression<Func<Streetcode.DAL.Entities.Media.Images.Art, bool>>>(),
                    It.IsAny<Func<IQueryable<Streetcode.DAL.Entities.Media.Images.Art>,
                IIncludableQueryable<Streetcode.DAL.Entities.Media.Images.Art, object>>>()))
                .ReturnsAsync((IEnumerable<Streetcode.DAL.Entities.Media.Images.Art>?)null);
        }
    }
}