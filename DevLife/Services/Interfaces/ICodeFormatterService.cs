using DevLife.Enums;

namespace DevLife.Services.Interfaces;

public interface ICodeFormatterService
{
    string FormatCode(string code, TechnologyStack techStack);
    string BeautifyCode(string code, TechnologyStack techStack);
}
