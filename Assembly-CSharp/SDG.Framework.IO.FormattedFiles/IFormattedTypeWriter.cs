namespace SDG.Framework.IO.FormattedFiles;

public interface IFormattedTypeWriter
{
    void write(IFormattedFileWriter writer, object value);
}
