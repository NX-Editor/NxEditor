namespace NxEditor.Plugin.Generics;

public interface ITransformable
{
    /// <summary>
    /// Transforms the raw data and returns the result
    /// </summary>
    /// <param name="raw"></param>
    /// <returns></returns>
    public Span<byte> Transform(Span<byte> raw);

    /// <summary>
    /// Runs the reverse transform and returns the result
    /// </summary>
    /// <param name="transformed"></param>
    /// <returns></returns>
    public Span<byte> FromTransformed(Span<byte> transformed);
}
