using System.Diagnostics;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using StromligningApp.Controllers;
using StromligningApp.Models;
using StromligningApp.Services;

namespace StromligningApp.Tests;

[TestClass]
public sealed class HomeControllerTests
{
    [TestMethod]
    public async Task Index_ReturnsPricesViewModelFromService()
    {
        var start = DateTimeOffset.Now.AddHours(1);
        var service = CreateService(
            $$"""[{"date":"{{start:O}}","price":1.25,"resolution":"15m"}]""");
        var controller = new HomeController(service);

        var result = await controller.Index(hoursBack: 0);

        var view = result as ViewResult;
        Assert.IsNotNull(view);
        var model = view.Model as PricesViewModel;
        Assert.IsNotNull(model);
        Assert.HasCount(1, model.Prices);
    }

    [TestMethod]
    public void Error_UsesCurrentActivityIdAndShowsRequestId()
    {
        var controller = CreateControllerWithHttpContext("trace-id");
        using var activity = new Activity("test").Start();

        var result = controller.Error() as ViewResult;

        Assert.IsNotNull(result);
        var model = result.Model as ErrorViewModel;
        Assert.IsNotNull(model);
        Assert.AreEqual(activity.Id, model.RequestId);
        Assert.IsTrue(model.ShowRequestId);
    }

    [TestMethod]
    public void Error_UsesTraceIdentifierWhenThereIsNoCurrentActivity()
    {
        var previousActivity = Activity.Current;
        Activity.Current = null;

        try
        {
            var controller = CreateControllerWithHttpContext("trace-id");

            var result = controller.Error() as ViewResult;

            Assert.IsNotNull(result);
            var model = result.Model as ErrorViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual("trace-id", model.RequestId);
        }
        finally
        {
            Activity.Current = previousActivity;
        }
    }

    [TestMethod]
    public void ErrorViewModel_DoesNotShowEmptyRequestId()
    {
        var model = new ErrorViewModel { RequestId = string.Empty };

        Assert.IsFalse(model.ShowRequestId);
    }

    private static HomeController CreateControllerWithHttpContext(string traceIdentifier)
    {
        var controller = new HomeController(CreateService("[]"));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                TraceIdentifier = traceIdentifier
            }
        };

        return controller;
    }

    private static StromligningService CreateService(string json)
    {
        var httpClient = new HttpClient(new StubHttpMessageHandler(json))
        {
            BaseAddress = new Uri("https://stromligning.dk/")
        };

        return new StromligningService(
            httpClient,
            new MemoryCache(new MemoryCacheOptions()),
            NullLogger<StromligningService>.Instance);
    }

    private sealed class StubHttpMessageHandler(string responseJson) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });
    }
}
