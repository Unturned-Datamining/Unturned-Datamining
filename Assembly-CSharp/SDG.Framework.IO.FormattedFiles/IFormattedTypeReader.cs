namespace SDG.Framework.IO.FormattedFiles;

public interface IFormattedTypeReader
{
    object read(IFormattedFileReader reader);
}
