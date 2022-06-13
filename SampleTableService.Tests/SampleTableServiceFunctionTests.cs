using System.Net;
using System.Threading.Tasks;

using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace SampleTableService.Tests;

[TestClass]
public class SampleTableServiceFunctionTests
{
        private ILogger _logger;

        [TestInitialize]
        public void Init()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder => builder.AddConsole());
            serviceCollection.AddLogging(builder => builder.AddDebug());
            var loggerFactory = serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>();
            _logger = loggerFactory.CreateLogger<SampleTableServiceFunctionTests>();
        }

    [TestMethod]
    public async Task Delete404Test()
    {
        var httpResponseMock = new Mock<HttpResponse>();

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(ctx => ctx.Response).Returns(httpResponseMock.Object);

        var reqMock = new Mock<HttpRequest>();
        reqMock.Setup(req => req.HttpContext).Returns(httpContextMock.Object);

        reqMock.Setup(req => req.Headers).Returns(new HeaderDictionary());

        // 404 {"odata.error":{"code":"ResourceNotFound","message":{"lang":"en-US","value":"The specified resource does not exist.\nRequestId:19ab49c9-2002-001c-145f-7d77db000000\nTime:2022-06-11T06:47:53.2893442Z"}}}
        // 204 -> nocontent
        var mockResponse = new Mock<Azure.Response>();
        mockResponse.SetupGet(x => x.Status).Returns((int)HttpStatusCode.NotFound);
        mockResponse.SetupGet(x => x.Content).Returns(BinaryData.FromString("{\"odata.error\":{\"code\":\"ResourceNotFound\",\"message\":{\"lang\":\"en-US\",\"value\":\"The specified resource does not exist.\\nRequestId:19ab49c9-2002-001c-145f-7d77db000000\\nTime:2022-06-11T06:47:53.2893442Z\"}}}"));

        Mock<TableClient> tableClient = new Mock<TableClient>();
        tableClient.Setup(_ => _.DeleteEntityAsync(It.IsAny<string>(), It.IsAny<string>(), default, default))
            .ReturnsAsync(mockResponse.Object);

        var actionResult = await SampleTableServiceFunction.DeleteAsync(reqMock.Object, tableClient.Object, "lorem", "ipsum", _logger).ConfigureAwait(false);
        Assert.AreEqual(typeof(NotFoundResult), actionResult.GetType());
    }

    [TestMethod]
    public async Task Delete204Test()
    {
        var httpResponseMock = new Mock<HttpResponse>();

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(ctx => ctx.Response).Returns(httpResponseMock.Object);

        var reqMock = new Mock<HttpRequest>();
        reqMock.Setup(req => req.HttpContext).Returns(httpContextMock.Object);

        reqMock.Setup(req => req.Headers).Returns(new HeaderDictionary());

        // 404 {"odata.error":{"code":"ResourceNotFound","message":{"lang":"en-US","value":"The specified resource does not exist.\nRequestId:19ab49c9-2002-001c-145f-7d77db000000\nTime:2022-06-11T06:47:53.2893442Z"}}}
        // 204 -> nocontent
        var mockResponse = new Mock<Azure.Response>();
        mockResponse.SetupGet(x => x.Status).Returns((int)HttpStatusCode.NoContent);

        Mock<TableClient> tableClient = new Mock<TableClient>();
        tableClient.Setup(_ => _.DeleteEntityAsync(It.IsAny<string>(), It.IsAny<string>(), default, default))
            .ReturnsAsync(mockResponse.Object);

        var actionResult = await SampleTableServiceFunction.DeleteAsync(reqMock.Object, tableClient.Object, "lorem", "ipsum", _logger).ConfigureAwait(false);
        Assert.AreEqual(typeof(NoContentResult), actionResult.GetType());
    }

        [TestMethod]
    public async Task Add204Test()
    {
        var body = File.ReadAllText("./samples/company.json");

        var httpResponseMock = new Mock<HttpResponse>();

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(ctx => ctx.Response).Returns(httpResponseMock.Object);

        var reqMock = new Mock<HttpRequest>();
        reqMock.Setup(req => req.HttpContext).Returns(httpContextMock.Object);

        reqMock.Setup(req => req.Headers).Returns(new HeaderDictionary());
        var stream = new MemoryStream();
        using var streamWriter = new StreamWriter(stream);
        await streamWriter.WriteAsync(body).ConfigureAwait(false);
        await streamWriter.FlushAsync().ConfigureAwait(false);
        stream.Position = 0;
        reqMock.Setup(req => req.Body).Returns(stream);

        // 404 {"odata.error":{"code":"ResourceNotFound","message":{"lang":"en-US","value":"The specified resource does not exist.\nRequestId:19ab49c9-2002-001c-145f-7d77db000000\nTime:2022-06-11T06:47:53.2893442Z"}}}
        // 204 -> nocontent
        var mockResponse = new Mock<Azure.Response>();
        mockResponse.SetupGet(x => x.Status).Returns((int)HttpStatusCode.NoContent);

        Mock<TableClient> tableClient = new Mock<TableClient>();
        tableClient.Setup(_ => _.AddEntityAsync(It.IsAny<ITableEntity>(), default))
            .ReturnsAsync(mockResponse.Object);

        var actionResult = await SampleTableServiceFunction.AddAsync(reqMock.Object, tableClient.Object, _logger).ConfigureAwait(false);
        Assert.AreEqual(typeof(NoContentResult), actionResult.GetType());
    }
}