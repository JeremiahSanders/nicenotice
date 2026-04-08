namespace Jds.NiceNotice;

internal static class EitherExtensions
{
  public static TRight IfLeftThrow<TLeft, TRight>(this Either<TLeft, TRight> either) where TLeft : Exception
  {
    return either.IsLeft ? throw either.LeftUnsafe : either.RightUnsafe;
  }
}
