using Castle.Core.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace CineTrackBE.Tests.Helpers
{
    public class MockHelper
    {
        //public static Mock<ILogger<T>> CreateLogger<T>() => new Mock<ILogger<T>>();

        //public static ILogger<T> CreateLoggerObject<T>() => Mock.Of<ILogger<T>>();

        //public static Mock<IConfiguration> CreateConfiguration(Dictionary<string, string> settings = null!)
        //{
        //    var config = new Mock<IConfiguration>();

        //    if (settings != null)
        //    {
        //        foreach (var setting in settings)
        //        {
        //            config.Setup(x => x[setting.Key]).Returns(setting.Value);
        //        }
        //    }

        //    return config;
        //}
    }
}
