using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using SaraTouch.Service.Mapping;
using Xunit;

namespace SaraTouch.Tests.Mapping;

public class MappingProfileTests
{
    [Fact]
    public void MappingProfile_ConfigurationIsValid()
    {
        // נכשלת אם יש שדה ב-DTO שאף אחד לא ממפה אליו
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);
        config.AssertConfigurationIsValid();
    }
}