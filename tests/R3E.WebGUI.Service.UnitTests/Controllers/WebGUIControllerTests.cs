using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using R3E.WebGUI.Service.API.Controllers;
using R3E.WebGUI.Service.Core.Services;
using R3E.WebGUI.Service.Domain.Models;
using System.Text;
using Xunit;

namespace R3E.WebGUI.Service.UnitTests.Controllers;

public class WebGUIControllerTests
{
    private readonly Mock<IWebGUIService> _mockService;
    private readonly Mock<INeoRpcService> _mockNeoRpcService;
    private readonly Mock<IWebGUIGeneratorService> _mockGeneratorService;
    private readonly Mock<IContractConfigService> _mockConfigService;
    private readonly Mock<IStorageService> _mockStorageService;
    private readonly Mock<ILogger<WebGUIController>> _mockLogger;
    private readonly Mock<IValidator<DeployWebGUIRequest>> _mockValidator;
    private readonly WebGUIController _controller;

    public WebGUIControllerTests()
    {
        _mockService = new Mock<IWebGUIService>();
        _mockNeoRpcService = new Mock<INeoRpcService>();
        _mockGeneratorService = new Mock<IWebGUIGeneratorService>();
        _mockConfigService = new Mock<IContractConfigService>();
        _mockStorageService = new Mock<IStorageService>();
        _mockLogger = new Mock<ILogger<WebGUIController>>();
        _mockValidator = new Mock<IValidator<DeployWebGUIRequest>>();
        
        _controller = new WebGUIController(
            _mockService.Object,
            _mockNeoRpcService.Object,
            _mockGeneratorService.Object,
            _mockConfigService.Object,
            _mockStorageService.Object,
            _mockLogger.Object,
            _mockValidator.Object);
    }

    [Fact]
    public async Task DeployWebGUI_AnyRequest_ReturnsBadRequestWithDeprecationMessage()
    {
        // Arrange
        var request = new DeployWebGUIRequest
        {
            ContractAddress = "0x1234567890abcdef1234567890abcdef12345678",
            ContractName = "TestContract",
            Network = "testnet",
            DeployerAddress = "0xabcdef1234567890abcdef1234567890abcdef12",
            Description = "Test WebGUI",
            WebGUIFiles = CreateMockFiles()
        };

        // Act
        var result = await _controller.DeployWebGUI(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        var response = badRequestResult!.Value;
        
        response.Should().NotBeNull();
        var responseObj = response!.GetType();
        responseObj.GetProperty("error")!.GetValue(response).Should().Be("This endpoint is deprecated");
        responseObj.GetProperty("newEndpoint")!.GetValue(response).Should().Be("/api/webgui/deploy-from-manifest");
    }

    [Fact]
    public async Task DeployWebGUI_NoFiles_StillReturnsBadRequestWithDeprecationMessage()
    {
        // Arrange
        var request = new DeployWebGUIRequest
        {
            ContractAddress = "0x1234567890abcdef1234567890abcdef12345678",
            ContractName = "TestContract",
            WebGUIFiles = new FormFileCollection() // Empty collection
        };

        // Act
        var result = await _controller.DeployWebGUI(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        var response = badRequestResult!.Value;
        var responseObj = response!.GetType();
        responseObj.GetProperty("error")!.GetValue(response).Should().Be("This endpoint is deprecated");
    }

    [Fact]
    public async Task DeployWebGUI_ServiceWouldThrowException_StillReturnsBadRequestWithDeprecationMessage()
    {
        // Arrange
        var request = new DeployWebGUIRequest
        {
            ContractAddress = "0x1234567890abcdef1234567890abcdef12345678",
            ContractName = "TestContract",
            WebGUIFiles = CreateMockFiles()
        };

        // Note: Service is never called because endpoint returns deprecation message immediately

        // Act
        var result = await _controller.DeployWebGUI(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        var response = badRequestResult!.Value;
        var responseObj = response!.GetType();
        responseObj.GetProperty("error")!.GetValue(response).Should().Be("This endpoint is deprecated");
    }

    [Fact]
    public async Task SearchByContract_ValidAddress_ReturnsResults()
    {
        // Arrange
        var contractAddress = "0x1234567890abcdef1234567890abcdef12345678";
        var network = "testnet";
        var expectedResults = new List<ContractWebGUI>
        {
            new() { ContractAddress = contractAddress, Network = network }
        };

        _mockService.Setup(s => s.SearchByContractAddressAsync(contractAddress, network))
            .ReturnsAsync(expectedResults);

        // Act
        var result = await _controller.SearchByContract(contractAddress, network);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var results = okResult!.Value as IEnumerable<ContractWebGUI>;
        results.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchByContract_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var contractAddress = "0x1234567890abcdef1234567890abcdef12345678";
        
        _mockService.Setup(s => s.SearchByContractAddressAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Search error"));

        // Act
        var result = await _controller.SearchByContract(contractAddress);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetWebGUIInfo_ExistingSubdomain_ReturnsOkResult()
    {
        // Arrange
        var subdomain = "testcontract";
        var webGUI = new ContractWebGUI
        {
            Subdomain = subdomain,
            ContractName = "TestContract"
        };

        _mockService.Setup(s => s.GetBySubdomainAsync(subdomain))
            .ReturnsAsync(webGUI);

        // Act
        var result = await _controller.GetWebGUIInfo(subdomain);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var returnedWebGUI = okResult!.Value as ContractWebGUI;
        returnedWebGUI!.Subdomain.Should().Be(subdomain);
    }

    [Fact]
    public async Task GetWebGUIInfo_NonExistentSubdomain_ReturnsNotFound()
    {
        // Arrange
        var subdomain = "nonexistent";
        _mockService.Setup(s => s.GetBySubdomainAsync(subdomain))
            .ReturnsAsync((ContractWebGUI?)null);

        // Act
        var result = await _controller.GetWebGUIInfo(subdomain);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task ListWebGUIs_ValidParameters_ReturnsPagedResults()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;
        var network = "testnet";
        var pagedResult = new PagedResult<ContractWebGUI>
        {
            Items = new List<ContractWebGUI> { new() { ContractName = "Test" } },
            TotalCount = 1,
            Page = page,
            PageSize = pageSize
        };

        _mockService.Setup(s => s.ListWebGUIsAsync(page, pageSize, network))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.ListWebGUIs(page, pageSize, network);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var returnedResult = okResult!.Value as PagedResult<ContractWebGUI>;
        returnedResult!.Items.Should().HaveCount(1);
        returnedResult.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task UpdateWebGUI_AnyRequest_ReturnsBadRequestWithDeprecationMessage()
    {
        // Arrange
        var subdomain = "testcontract";
        var request = new UpdateWebGUIRequest
        {
            Description = "Updated description",
            WebGUIFiles = CreateMockFiles()
        };

        // Act
        var result = await _controller.UpdateWebGUI(subdomain, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        var response = badRequestResult!.Value;
        var responseObj = response!.GetType();
        responseObj.GetProperty("error")!.GetValue(response).Should().Be("This endpoint is deprecated");
    }

    private IFormFileCollection CreateMockFiles()
    {
        var files = new FormFileCollection();
        
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("index.html");
        mockFile.Setup(f => f.Length).Returns(1024);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(Encoding.UTF8.GetBytes("<html></html>")));
        
        files.Add(mockFile.Object);
        return files;
    }
}