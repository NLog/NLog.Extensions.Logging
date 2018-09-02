using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Xunit;

namespace NLog.Extensions.Hosting.Tests
{
    public class ExtensionMethodTests
    {
        [Fact]
        public void UseNLog_noParams_buildsAndExposesValidIServiceCollectiont()
        {
            var actual = new HostBuilder().UseNLog().Build();
            var serviceCount = actual.Services.GetServices<ILoggerFactory>().Count();

            Assert.NotNull(actual);
            Assert.Equal(2, serviceCount);
        }

        [Fact]
        public void UseNLog_withOptionsParam_buildsAndExposesValidIServiceCollection()
        {
            var someParam = new NLogProviderOptions {CaptureMessageProperties = false, CaptureMessageTemplates = false};

            var actual = new HostBuilder().UseNLog(someParam).Build();
            var serviceCount = actual.Services.GetServices<ILoggerFactory>().Count();

            Assert.NotNull(actual);
            Assert.Equal(2, serviceCount);
        }
    }
}