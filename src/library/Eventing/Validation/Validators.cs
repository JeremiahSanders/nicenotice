using Microsoft.Extensions.DependencyInjection;

namespace Jds.NiceNotice;

public static class Validators
{
  public static NoticeValidator<TEnterpriseEventBaseType> DataAnnotationsValidator<TEnterpriseEventBaseType>()
    where TEnterpriseEventBaseType : notnull
  {
    return new DataAnnotationsValidator<TEnterpriseEventBaseType>();
  }

  public static NoticeValidator<TEnterpriseEventBaseType> NoOpValidator<TEnterpriseEventBaseType>()
    where TEnterpriseEventBaseType : notnull
  {
    return new NoOpNoticeValidator<TEnterpriseEventBaseType>();
  }

  public static TypedNoticesBuilder<TEnterpriseEventBaseType> WithDataAnnotationsValidator<TEnterpriseEventBaseType>(
    this TypedNoticesBuilder<TEnterpriseEventBaseType> builder)
    where TEnterpriseEventBaseType : notnull
  {
    return builder.UseValidator(
      static _ => DataAnnotationsValidator<TEnterpriseEventBaseType>(),
      ServiceLifetime.Singleton
    );
  }

  public static TypedNoticesBuilder<TEnterpriseEventBaseType> WithNoValidation<TEnterpriseEventBaseType>(
    this TypedNoticesBuilder<TEnterpriseEventBaseType> builder)
    where TEnterpriseEventBaseType : notnull
  {
    return builder.UseValidator(
      static _ => NoOpValidator<TEnterpriseEventBaseType>(),
      ServiceLifetime.Singleton
    );
  }
}
