namespace Jds.NiceNotice;

internal static class Eithers
{
  public static Either<Exception, T> Try<T>(Func<T> func)
  {
    try
    {
      return Right<Exception, T>(func());
    }
    catch (Exception e)
    {
      return Left<Exception, T>(e);
    }
  }

  public static async Task<Either<Exception, T>> TryAsync<T>(Func<Task<T>> func)
  {
    try
    {
      return Right<Exception, T>(await func());
    }
    catch (Exception e)
    {
      return Left<Exception, T>(e);
    }
  }

  public static Either<TLeft, TRight> Left<TLeft, TRight>(TLeft left)
  {
    return new Either<TLeft, TRight>(left);
  }

  public static Either<TLeft, TRight> Right<TLeft, TRight>(TRight right)
  {
    return new Either<TLeft, TRight>(right);
  }

  public static (List<TLeft> lefts, List<TRight> rights) Partition<TLeft, TRight>(
    this IReadOnlyList<Either<TLeft, TRight>> eithers)
  {
    List<TLeft> lefts = eithers.Where(static e => e.IsLeft).Select(static e => e.LeftUnsafe).ToList();
    List<TRight> rights = eithers.Where(static e => e.IsRight).Select(static e => e.RightUnsafe).ToList();

    return (lefts, rights);
  }
}
