namespace Jds.NiceNotice;

internal class Either<TLeft, TRight>
{
  private readonly TLeft? _left;
  private readonly TRight? _right;

  public Either(TLeft left)
  {
    _left = left;
    IsLeft = true;
  }

  public Either(TRight right)
  {
    _right = right;
    IsRight = true;
  }

  public bool IsBottom => !IsLeft && !IsRight;
  public bool IsLeft { get; }
  public bool IsRight { get; }
  public TLeft LeftUnsafe => _left ?? throw new InvalidOperationException(message: "Either is not left.");
  public TRight RightUnsafe => _right ?? throw new InvalidOperationException(message: "Either is not right.");

  public Either<TLeft2, TRight2> BiBind<TLeft2, TRight2>(
    Func<TRight, Either<TLeft2, TRight2>> rightBinder,
    Func<TLeft, Either<TLeft2, TRight2>> leftBinder)
  {
    return IsLeft ? leftBinder(LeftUnsafe) : rightBinder(RightUnsafe);
  }

  public Either<TLeft, TRight2> Bind<TRight2>(Func<TRight, Either<TLeft, TRight2>> binder)
  {
    return IsLeft ? Eithers.Left<TLeft, TRight2>(LeftUnsafe) : binder(RightUnsafe);
  }

  public TRight FoldRight(Func<TLeft, TRight> leftMapper)
  {
    return IsLeft ? leftMapper(LeftUnsafe) : RightUnsafe;
  }

  public Either<TLeft, TRight2> Map<TRight2>(Func<TRight, TRight2> mapper)
  {
    return IsLeft ? Eithers.Left<TLeft, TRight2>(LeftUnsafe) : Eithers.Right<TLeft, TRight2>(mapper(RightUnsafe));
  }

  public Either<TLeft2, TRight> MapLeft<TLeft2>(Func<TLeft, TLeft2> mapper)
  {
    return IsLeft ? Eithers.Left<TLeft2, TRight>(mapper(LeftUnsafe)) : Eithers.Right<TLeft2, TRight>(RightUnsafe);
  }

  public TResult Match<TResult>(Func<TLeft, TResult> leftMapper, Func<TRight, TResult> rightMapper)
  {
    RequireNotBottom();

    return IsLeft ? leftMapper(LeftUnsafe) : rightMapper(RightUnsafe);
  }

  private void RequireNotBottom()
  {
    if (IsBottom)
    {
      throw new InvalidOperationException(message: "Either is neither left nor right (is bottom).");
    }
  }
}
