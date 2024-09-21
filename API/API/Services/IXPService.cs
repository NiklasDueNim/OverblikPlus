namespace API.Services;

public interface IXPService
{
    int GetLevel(int xp);
    int XpToNextLevel(int xp);
    int XpProgresss(int xp);
}