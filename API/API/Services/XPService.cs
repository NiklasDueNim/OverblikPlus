using DataAccess;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class XPService : IXPService
{
    private readonly AppDbContext _dbContext;
    
    public XPService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public int GetLevel(int xp)
    {
        if (xp >= 600) return 4;
        else if (xp >= 300) return 3;
        else if (xp >= 100) return 2;
        else return 1;
    }
    

    public int XpToNextLevel(int xp)
    {
        if (xp >= 600) return 0;
        else if (xp >= 300) return 600 - xp;
        else if (xp >= 100) return 300 - xp;
        else return 100 - xp;
    }

    public int XpProgresss(int xp)
    {
        if (xp >= 600) return 100;
        else if (xp >= 300) return ((xp - 300) * 100) / 300;
        else if (xp >= 100) return ((xp - 100) * 100) / 200;
        else return (xp * 100) / 100;
    }
}